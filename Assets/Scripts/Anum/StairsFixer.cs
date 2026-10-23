#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor helper: Tools > Fix Stairs
// Makes every staircase on the planets easy and safe to climb and joins the rocket stairs to the passages:
//  - each rocket door gets one staircase, centred on the doorway with its top step flush with the cabin
//    floor. Where a jet bridge passage leads to the door, the stairs start from the passage deck (less to
//    climb) and a level walkway in the deck's own material fills the gap between the end of the passage
//    and the foot of the stairs, so there is no drop to fall into. Without a passage, the stairs rise from
//    the ground. A staircase already by the door is turned, moved and resized to fit; a door without one
//    gets a stray staircase (one that leads to no door); strays left over are switched off (not deleted)
//  - fitted stairs are at least as wide as the doorway (1.6 m at least) and no steeper than MaxAngle
//  - every staircase gets an invisible ramp collider along its step edges, so players walk up smoothly
//  - waist-high glass railings (steel posts and handrail, glass panels that stop anyone falling) along both
//    sides of every staircase, both sides and the open ends of every walkway, and any side of a passage deck
//    that has no wall
// Everything new goes in "<rocket port>/Stairs and passages"; a rerun switches the earlier one off as
// "Stairs and passages (off)" and builds it again. Only existing materials are used. Measured with raycasts
// against the scene's colliders. Ctrl+Z undoes it. Save the scene.
public static class StairsFixer
{
    const string UndoName = "Fix Stairs";
    const string GroupName = "Stairs and passages";
    const float MaxAngle = 30f;
    const float MinWidth = 1.6f;
    const float DoorReach = 6f;     // a staircase whose top is this close to a door belongs to it
    const float RampThickness = 0.2f;
    const float RailHeight = 1.05f;
    const string Mats = "Assets/Prefabs/Anum/Materials/";
    static readonly string[] Roots = { "Earth", "Astronomy Planet", "Tech Planet" };
    static Material mSteel, mGlass;
    static Dictionary<Transform, Transform> groups;
    static string log;

    class Stairs
    {
        public Transform t;
        public bool big, used;
    }

    class Door
    {
        public Transform room;
        public string name;
        public Vector3 inward, across, outer; // outer: middle of the doorway's outside face, at sill height
        public float sill, ground, width;
        public bool hasDeck;
        public Bounds deck;                   // the passage deck that leads to this door
        public Material deckMat;
    }

    [MenuItem("Tools/Fix Stairs")]
    public static void Fix()
    {
        mSteel = AssetDatabase.LoadAssetAtPath<Material>(Mats + "Activity2_ValvePlate.mat");
        mGlass = AssetDatabase.LoadAssetAtPath<Material>(Mats + "Activity2_Glass.mat");
        if (mSteel == null || mGlass == null) { Debug.LogError("[StairsFixer] ERROR: Activity2_ValvePlate or Activity2_Glass is missing. Nothing changed."); return; }

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();
        log = "";
        groups = new Dictionary<Transform, Transform>();
        Physics.SyncTransforms();

        var stairs = new List<Stairs>();
        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (System.Array.IndexOf(Roots, root.name) < 0 || !root.activeInHierarchy) continue;
            // Earlier runs: ramps straight under the rocket ports and the old group are switched off first,
            // so nothing old gets in the way of the measuring
            foreach (Transform t in root.GetComponentsInChildren<Transform>(false))
            {
                if (t == null || t.parent == null) continue;
                bool oldRamp = t.name.EndsWith(" ramp") && t.name.Contains("Staircase") && t.GetComponent<BoxCollider>() != null && t.childCount == 0;
                if (oldRamp || t.name == GroupName)
                {
                    Undo.RecordObject(t.gameObject, UndoName);
                    t.gameObject.SetActive(false);
                    if (t.name == GroupName) t.gameObject.name = GroupName + " (off)";
                }
            }
        }
        Physics.SyncTransforms();
        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (System.Array.IndexOf(Roots, root.name) < 0 || !root.activeInHierarchy) continue;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(false))
            {
                if (!t.name.StartsWith("Staircase") || StepCount(t) < 2) continue;
                Bounds b; Vector3 dir;
                if (!Measure(t, out b, out dir)) { log += "\n - " + PathOf(t) + ": steps do not run along a world axis, left as it is."; continue; }
                stairs.Add(new Stairs { t = t, big = b.size.y > 4f });
            }
        }
        List<Door> doors = Doors();

        // 1. Each door takes the staircase whose top is nearest to it, or else the nearest stray one
        int fitted = 0, moved = 0, flat = 0, walkways = 0;
        foreach (Door d in doors)
        {
            Stairs pick = null;
            float best = DoorReach;
            foreach (Stairs s in stairs)
            {
                if (s.used || s.big) continue;
                float dist = Flat(TopEdge(s.t) - d.outer).magnitude;
                if (dist < best) { best = dist; pick = s; }
            }
            if (d.sill - d.ground < 0.25f)
            {
                if (pick != null) pick.used = true;
                log += "\n - " + d.name + ": door is level with what is in front of it, no stairs needed.";
                flat++;
                continue;
            }
            bool stray = false;
            if (pick == null)
            {
                best = float.MaxValue;
                foreach (Stairs s in stairs)
                {
                    if (s.used || s.big || NearAnyDoor(s.t, doors)) continue;
                    float dist = Flat(TopEdge(s.t) - d.outer).magnitude;
                    if (dist < best) { best = dist; pick = s; }
                }
                stray = pick != null;
            }
            if (pick == null) { log += "\n - " + d.name + ": no staircase to spare for this door."; continue; }
            pick.used = true;
            if (FitToDoor(pick.t, d)) walkways++;
            fitted++;
            if (stray) moved++;
        }

        // 2. Strays that lead nowhere are switched off
        int off = 0;
        foreach (Stairs s in stairs)
        {
            if (s.used || s.big || NearAnyDoor(s.t, doors)) continue;
            Undo.RecordObject(s.t.gameObject, UndoName);
            s.t.gameObject.SetActive(false);
            off++;
            log += "\n - " + PathOf(s.t) + ": leads to no door, switched off.";
        }

        // 3. A smooth ramp and railings on every staircase still in use
        int ramps = 0;
        foreach (Stairs s in stairs)
        {
            if (!s.t.gameObject.activeInHierarchy) continue;
            if (Ramp(s.t)) ramps++;
            StairRailings(s.t);
        }

        // 4. Railings along any side of a passage deck without a wall
        int deckRails = 0;
        foreach (Door d in doors) if (d.hasDeck) deckRails += DeckRailings(d);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[StairsFixer] Done. " + doors.Count + " rocket doors: " + fitted + " staircases fitted (" + moved + " moved over from nowhere), "
            + walkways + " walkways joining a passage to its stairs, " + flat + " doors needing no stairs; " + off + " stray staircases switched off; "
            + ramps + " smooth ramps; glass railings on every staircase and walkway, and on " + deckRails + " open passage sides." + log
            + "\nEverything new is under '<rocket port>/" + GroupName + "'. Nothing was deleted. Save the scene to keep the changes. Ctrl+Z undoes this.");
    }

    // ================================================================ doors

    static List<Door> Doors()
    {
        var list = new List<Door>();
        var rooms = new HashSet<Transform>();
        var decks = new List<Renderer>();
        foreach (Transform t in Object.FindObjectsOfType<Transform>(false))
            if (t.name == "Cube (21)" && t.parent != null && t.parent.name.StartsWith("Bridge") && t.GetComponent<Renderer>() != null) decks.Add(t.GetComponent<Renderer>());

        foreach (RocketSystem rs in Object.FindObjectsOfType<RocketSystem>(false))
        {
            Transform door = FindChild(rs.transform, "Gate");
            Renderer r = door != null ? door.GetComponent<Renderer>() : null;
            if (r == null || !door.gameObject.activeInHierarchy) continue;
            Transform room = door;
            while (room != null && room != rs.transform && !room.name.StartsWith("Prefab_RocketRoom")) room = room.parent;
            if (room == null || room == rs.transform || !rooms.Add(room)) continue;

            // The closed door fills the doorway: thin across its thickness, which points into the cabin
            Bounds db = r.bounds;
            Bounds rb;
            if (!RenderBounds(room, door, out rb)) continue;
            bool thinX = db.size.x < db.size.z;
            Vector3 toRoom = Flat(rb.center - db.center);
            Vector3 inward = thinX ? new Vector3(Mathf.Sign(toRoom.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(toRoom.z));
            Vector3 across = thinX ? Vector3.forward : Vector3.right;
            float half = (thinX ? db.size.x : db.size.z) * 0.5f;

            var d = new Door { room = room, name = PathOf(rs.transform), inward = inward, across = across, width = thinX ? db.size.z : db.size.x };
            // Cabin floor just inside the doorway
            RaycastHit hit;
            Vector3 inside = db.center + inward * (half + 0.5f);
            d.sill = Down(inside + Vector3.up * 0.5f, db.size.y + 1f, null, null, out hit) ? hit.point.y : db.min.y;
            d.outer = new Vector3(db.center.x, d.sill, db.center.z) - inward * half;
            // Ground in front of the door, a few metres out
            d.ground = Down(d.outer - inward * 3f + Vector3.up * 0.5f, 40f, null, room, out hit) ? hit.point.y : d.sill;

            // A passage deck that runs toward this door, ending up to 15 m short of it, roughly in line with it
            // and not above the cabin floor: the stairs start from it
            float bestGap = 15f;
            foreach (Renderer dr in decks)
            {
                Bounds k = dr.bounds;
                float nearEnd = Vector3.Dot(k.center, inward) + Mathf.Abs(Vector3.Dot(k.extents, inward));
                float gap = Vector3.Dot(d.outer, inward) - nearEnd;
                float lateral = Mathf.Abs(Vector3.Dot(k.center - d.outer, across));
                if (gap < -0.5f || gap > bestGap || lateral > Mathf.Abs(Vector3.Dot(k.extents, across)) + 2f) continue;
                if (k.max.y > d.sill + 0.1f || k.max.y < d.sill - 6f) continue;
                bestGap = gap;
                d.hasDeck = true;
                d.deck = k;
                d.deckMat = dr.sharedMaterial;
                d.ground = k.max.y;
            }
            list.Add(d);
        }
        return list;
    }

    static bool NearAnyDoor(Transform s, List<Door> doors)
    {
        foreach (Door d in doors) if (Flat(TopEdge(s) - d.outer).magnitude < DoorReach) return true;
        return false;
    }

    // ================================================================ fitting

    // Returns true when it also built a walkway from a passage deck to the stairs
    static bool FitToDoor(Transform s, Door d)
    {
        Undo.RecordObject(s, UndoName);
        Bounds b; Vector3 dir;
        Measure(s, out b, out dir);

        // Climb straight in through the door
        float turn = Vector3.SignedAngle(dir, d.inward, Vector3.up);
        if (Mathf.Abs(turn) > 1f)
        {
            Vector3 pivot = b.center;
            s.rotation = Quaternion.AngleAxis(turn, Vector3.up) * s.rotation;
            s.position = pivot + Quaternion.AngleAxis(turn, Vector3.up) * (s.position - pivot);
            Physics.SyncTransforms();
        }

        // At least as wide as the doorway
        Measure(s, out b, out dir);
        float width = Mathf.Abs(Vector3.Dot(b.size, d.across));
        float want = Mathf.Clamp(d.width + 0.4f, MinWidth, 3.5f);
        if (width > 0.05f && width < want) { ScaleWorld(s, d.across, want / width); Physics.SyncTransforms(); }

        // Rise from the passage deck (or the ground at the foot of the stairs) to the sill, no steeper than
        // MaxAngle. Without a deck the foot moves when the stairs stretch, so measure the ground there twice
        float ground = d.ground;
        for (int pass = 0; pass < 2; pass++)
        {
            Measure(s, out b, out dir);
            float rise0 = b.size.y, run0 = Along(b.size, dir);
            float rise = d.sill - ground;
            if (rise0 < 0.05f || run0 < 0.05f || rise < 0.25f) break;
            float run = Mathf.Max(run0 * rise / rise0, rise / Mathf.Tan(MaxAngle * Mathf.Deg2Rad));
            ScaleWorld(s, Vector3.up, rise / rise0);
            ScaleWorld(s, dir, run / run0);
            Physics.SyncTransforms();
            Place(s, d);
            if (d.hasDeck) continue;
            RaycastHit hit;
            Measure(s, out b, out dir);
            Vector3 foot = FootPoint(b, dir) - dir * 0.3f;
            ground = Down(new Vector3(foot.x, d.sill + 0.5f, foot.z), 40f, s, d.room, out hit) ? hit.point.y : ground;
        }
        Place(s, d);
        if (PrefabUtility.IsPartOfPrefabInstance(s)) PrefabUtility.RecordPrefabInstancePropertyModifications(s);

        Measure(s, out b, out dir);
        float angle = Mathf.Atan2(b.size.y, Along(b.size, dir)) * Mathf.Rad2Deg;
        log += "\n - " + PathOf(s) + " -> door of " + d.name + ": " + Mathf.Abs(Vector3.Dot(b.size, d.across)).ToString("0.0") + " m wide, rises "
            + b.size.y.ToString("0.00") + " m over " + Along(b.size, dir).ToString("0.0") + " m (" + angle.ToString("0") + " degrees) from the "
            + (d.hasDeck ? "passage deck" : "ground") + ", top step level with the cabin floor";

        // Walkway from the end of the passage deck to the foot of the stairs, with railings
        bool walkway = false;
        if (d.hasDeck)
        {
            float deckEnd = Vector3.Dot(d.deck.center, d.inward) + Mathf.Abs(Vector3.Dot(d.deck.extents, d.inward));
            float footAt = Vector3.Dot(FootPoint(b, dir), d.inward);
            if (footAt > deckEnd + 0.05f)
            {
                Walkway(s, d, b, deckEnd, footAt);
                walkway = true;
                log += "; a " + (footAt - deckEnd).ToString("0.0") + " m walkway joins it to the passage";
            }
        }
        log += ".";
        return walkway;
    }

    // Top step's edge on the outer face of the doorway, centred on it, top surface at the sill
    static void Place(Transform s, Door d)
    {
        Bounds b; Vector3 dir;
        Measure(s, out b, out dir);
        Vector3 top = TopEdge(s);
        Vector3 move = d.inward * (Vector3.Dot(d.outer - top, d.inward))
            + d.across * Vector3.Dot(d.outer - b.center, d.across)
            + Vector3.up * (d.sill - b.max.y);
        s.position += move;
        Physics.SyncTransforms();
    }

    // Stretch along a world direction by changing whichever local axis points that way
    static void ScaleWorld(Transform s, Vector3 worldDir, float k)
    {
        Vector3[] axes = { s.right, s.up, s.forward };
        int a = 0;
        for (int i = 1; i < 3; i++) if (Mathf.Abs(Vector3.Dot(axes[i], worldDir)) > Mathf.Abs(Vector3.Dot(axes[a], worldDir))) a = i;
        Vector3 ls = s.localScale;
        ls[a] *= k;
        s.localScale = ls;
    }

    // ================================================================ walkway and railings

    // Level deck from the passage's end to the foot of the stairs, as wide as both, in the passage's material
    static void Walkway(Transform s, Door d, Bounds stairs, float from, float to)
    {
        Transform g = Group(s);
        float lo = Mathf.Min(Lateral(d.deck.min, d), Lateral(d.deck.max, d), Lateral(stairs.min, d), Lateral(stairs.max, d));
        float hi = Mathf.Max(Lateral(d.deck.min, d), Lateral(d.deck.max, d), Lateral(stairs.min, d), Lateral(stairs.max, d));
        float dlo = Mathf.Min(Lateral(d.deck.min, d), Lateral(d.deck.max, d)), dhi = Mathf.Max(Lateral(d.deck.min, d), Lateral(d.deck.max, d));
        float slo = Mathf.Min(Lateral(stairs.min, d), Lateral(stairs.max, d)), shi = Mathf.Max(Lateral(stairs.min, d), Lateral(stairs.max, d));
        float top = d.deck.max.y, len = to - from + 0.1f;
        Vector3 c = d.inward * ((from + to) * 0.5f) + d.across * ((lo + hi) * 0.5f);
        c.y = top - 0.1f;
        Box(g, s.name + " walkway", d.deckMat != null ? d.deckMat : mSteel, c, Quaternion.LookRotation(d.inward), new Vector3(hi - lo, 0.2f, len), true);

        // Railings down both sides, and across the end beside the stairs where the walkway is wider than them
        Vector3 A(float along, float lat) { Vector3 p = d.inward * along + d.across * lat; p.y = top; return p; }
        Railing(g, s.name + " walkway railing", A(from, lo + 0.05f), A(to, lo + 0.05f));
        Railing(g, s.name + " walkway railing", A(from, hi - 0.05f), A(to, hi - 0.05f));
        if (slo - lo > 0.3f) Railing(g, s.name + " walkway railing", A(to, lo + 0.05f), A(to, slo));
        if (hi - shi > 0.3f) Railing(g, s.name + " walkway railing", A(to, shi), A(to, hi - 0.05f));
        // and where the walkway is wider than the passage, across the passage end
        if (dlo - lo > 0.3f) Railing(g, s.name + " walkway railing", A(from, lo + 0.05f), A(from, dlo));
        if (hi - dhi > 0.3f) Railing(g, s.name + " walkway railing", A(from, dhi), A(from, hi - 0.05f));
    }

    static float Lateral(Vector3 p, Door d) { return Vector3.Dot(p, d.across); }

    // Sloping railings along both sides of a staircase, from its foot to its top
    static void StairRailings(Transform s)
    {
        Bounds b; Vector3 dir;
        if (!Measure(s, out b, out dir)) return;
        Transform g = Group(s);
        Vector3 across = Vector3.Cross(Vector3.up, dir).normalized;
        float halfW = Mathf.Abs(Vector3.Dot(b.size, across)) * 0.5f;
        Vector3 foot = FootPoint(b, dir);
        Vector3 top = new Vector3(b.center.x, b.max.y, b.center.z) + dir * (Along(b.size, dir) * 0.5f);
        foreach (float side in new float[] { -1f, 1f })
            Railing(g, s.name + " railing", foot + across * side * (halfW - 0.05f), top + across * side * (halfW - 0.05f));
    }

    // Any long side of a passage deck that has no wall gets a railing
    static int DeckRailings(Door d)
    {
        Transform g = null;
        int n = 0;
        Bounds k = d.deck;
        float len = Mathf.Abs(Vector3.Dot(k.size, d.inward));
        float y = k.max.y;
        foreach (float side in new float[] { -1f, 1f })
        {
            Vector3 edge = k.center + d.across * side * (Mathf.Abs(Vector3.Dot(k.extents, d.across)) - 0.08f);
            edge.y = y + 0.6f;
            Vector3 half = d.inward * (len * 0.4f) + d.across * 0.12f + Vector3.up * 0.25f;
            Vector3 size = new Vector3(Mathf.Abs(half.x), Mathf.Abs(half.y), Mathf.Abs(half.z));
            bool wall = false;
            foreach (Collider c in Physics.OverlapBox(edge, size, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                if (!c.transform.name.Contains("railing")) { wall = true; break; }
            if (wall) continue;
            if (g == null) g = GroupFor(d.room);
            Vector3 a = edge - d.inward * (len * 0.5f), b = edge + d.inward * (len * 0.5f);
            a.y = b.y = y;
            Railing(g, "Passage railing", a, b);
            n++;
        }
        return n;
    }

    // A waist-high glass railing from a to b (a and b on the walking surface; they may differ in height):
    // steel posts about every 1.4 m, a steel handrail, and glass panels whose colliders stop anyone falling
    static void Railing(Transform g, string name, Vector3 a, Vector3 b)
    {
        Vector3 v = b - a;
        float len = v.magnitude;
        if (len < 0.2f) return;
        Transform r = new GameObject(name).transform;
        Undo.RegisterCreatedObjectUndo(r.gameObject, UndoName);
        r.SetParent(g, false);
        r.position = (a + b) * 0.5f;
        Vector3 flat = Flat(v);
        Vector3 across = flat.sqrMagnitude > 0.0001f ? Vector3.Cross(Vector3.up, flat.normalized) : Vector3.right;
        Vector3 up = Vector3.Cross(v.normalized, across).normalized;
        if (up.y < 0f) up = -up;
        Quaternion along = Quaternion.LookRotation(v.normalized, up);

        int posts = Mathf.Max(1, Mathf.CeilToInt(len / 1.4f));
        for (int i = 0; i <= posts; i++)
        {
            Vector3 p = a + v * ((float)i / posts);
            Box(r, "Post", mSteel, p + Vector3.up * (RailHeight * 0.5f), Quaternion.LookRotation(across), new Vector3(0.06f, RailHeight, 0.06f), false);
        }
        Box(r, "Handrail", mSteel, (a + b) * 0.5f + Vector3.up * RailHeight, along, new Vector3(0.07f, 0.05f, len + 0.06f), false);
        Box(r, "Glass", mGlass, (a + b) * 0.5f + Vector3.up * (RailHeight * 0.5f + 0.04f), along, new Vector3(0.02f, RailHeight - 0.12f, len), true);
    }

    // Invisible box collider laid along the step edges, from the ground at the foot to the top step's far edge
    static bool Ramp(Transform s)
    {
        Bounds b; Vector3 dir;
        if (!Measure(s, out b, out dir)) return false;
        Vector3 across = Vector3.Cross(Vector3.up, dir).normalized;
        float width = Mathf.Abs(Vector3.Dot(b.size, across));
        Vector3 p0 = FootPoint(b, dir);
        Vector3 p1 = new Vector3(b.center.x, b.max.y, b.center.z) + dir * (Along(b.size, dir) * 0.5f);
        Vector3 slope = (p1 - p0).normalized;
        Vector3 n = Vector3.Cross(slope, across);
        if (n.y < 0f) n = -n;
        // A little past the foot, into the ground; it ends exactly at the top so there is no lip at the doorway
        const float Under = 0.2f;
        float len = (p1 - p0).magnitude + Under;

        Transform g = Group(s);
        var go = new GameObject(s.name + " ramp");
        Undo.RegisterCreatedObjectUndo(go, UndoName);
        Transform ramp = go.transform;
        ramp.SetParent(g, false);
        go.layer = s.gameObject.layer;
        ramp.SetPositionAndRotation((p0 + p1) * 0.5f - slope * (Under * 0.5f) - n * (RampThickness * 0.5f - 0.02f), Quaternion.LookRotation(slope, n));
        ramp.localScale = new Vector3(width, RampThickness, len) / g.lossyScale.x;
        go.AddComponent<BoxCollider>();
        return true;
    }

    // "Stairs and passages" under the rocket port the staircase (or cabin) belongs to, made on first use
    static Transform Group(Transform s) { return GroupFor(s); }

    static Transform GroupFor(Transform t)
    {
        // The rocket port: the nearest ancestor named like a rocket system, else the planet's direct child
        Transform port = t;
        for (Transform p = t; p != null; p = p.parent) if (p.name.Contains("RocketSystem")) { port = p; break; }
        if (!port.name.Contains("RocketSystem")) while (port.parent != null && port.parent.parent != null) port = port.parent;
        Transform holder = UniformParent(port);
        if (holder == null) holder = port.parent;
        Transform g;
        if (groups.TryGetValue(port, out g)) return g;
        var go = new GameObject(GroupName);
        Undo.RegisterCreatedObjectUndo(go, UndoName);
        g = go.transform;
        g.SetParent(holder, false);
        g.position = Vector3.zero;
        g.rotation = Quaternion.identity;
        Vector3 ls = holder != null ? holder.lossyScale : Vector3.one;
        g.localScale = new Vector3(1f / ls.x, 1f / ls.y, 1f / ls.z); // work in metres
        groups[port] = g;
        return g;
    }

    static void Box(Transform parent, string name, Material mat, Vector3 pos, Quaternion rot, Vector3 size, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = new Vector3(size.x / parent.lossyScale.x, size.y / parent.lossyScale.y, size.z / parent.lossyScale.z);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
    }

    // The nearest parent scaled the same on every axis (or null for the scene root)
    static Transform UniformParent(Transform t)
    {
        for (; t != null; t = t.parent)
        {
            Vector3 l = t.lossyScale;
            float mx = Mathf.Max(Mathf.Abs(l.x), Mathf.Abs(l.y), Mathf.Abs(l.z)), mn = Mathf.Min(Mathf.Abs(l.x), Mathf.Abs(l.y), Mathf.Abs(l.z));
            if (mn > 0.0001f && mx / mn < 1.01f && l.x > 0f && l.y > 0f && l.z > 0f) return t;
        }
        return null;
    }

    // ================================================================ measuring

    static int StepCount(Transform t)
    {
        int n = 0;
        foreach (Transform c in t) if (c.name.StartsWith("CubeStairs") && c.gameObject.activeSelf && c.GetComponent<Renderer>() != null) n++;
        return n;
    }

    // Bounds of the steps, and the way they climb (bottom step to top step) along a world axis
    static bool Measure(Transform t, out Bounds b, out Vector3 dir)
    {
        b = new Bounds(t.position, Vector3.zero);
        dir = Vector3.forward;
        Renderer lo = null, hi = null;
        bool any = false;
        foreach (Transform c in t)
        {
            Renderer r = c.GetComponent<Renderer>();
            if (!c.name.StartsWith("CubeStairs") || !c.gameObject.activeSelf || r == null) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            if (lo == null || r.bounds.max.y < lo.bounds.max.y) lo = r;
            if (hi == null || r.bounds.max.y > hi.bounds.max.y) hi = r;
        }
        if (!any || lo == hi) return false;
        Vector3 v = Flat(hi.bounds.center - lo.bounds.center);
        if (v.sqrMagnitude < 0.0001f) return false;
        v.Normalize();
        dir = Mathf.Abs(v.x) > Mathf.Abs(v.z) ? new Vector3(Mathf.Sign(v.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(v.z));
        return Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.z)) > 0.9f;
    }

    // Middle of the top step's far edge, at the top
    static Vector3 TopEdge(Transform t)
    {
        Bounds b; Vector3 dir;
        Measure(t, out b, out dir);
        return new Vector3(b.center.x, b.max.y, b.center.z) + dir * (Along(b.size, dir) * 0.5f);
    }

    // Middle of the bottom step's front edge, on the ground
    static Vector3 FootPoint(Bounds b, Vector3 dir)
    {
        return new Vector3(b.center.x, b.min.y, b.center.z) - dir * (Along(b.size, dir) * 0.5f);
    }

    static float Along(Vector3 size, Vector3 dir)
    {
        return Mathf.Abs(Vector3.Dot(size, dir));
    }

    // First solid surface straight down, skipping the staircase itself, ramps, railings, jet bridge walls,
    // other staircases and (optionally) a rocket cabin
    static bool Down(Vector3 from, float dist, Transform skip, Transform skipRoom, out RaycastHit best)
    {
        best = default(RaycastHit);
        bool found = false;
        foreach (RaycastHit h in Physics.RaycastAll(from, Vector3.down, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform ht = h.collider.transform;
            if (skip != null && ht.IsChildOf(skip)) continue;
            if (skipRoom != null && ht.IsChildOf(skipRoom)) continue;
            if (ht.name.EndsWith(" ramp") || ht.name.StartsWith("Jet bridge") || ht.name == "Glass" || InStaircase(ht)) continue;
            if (!found || h.distance < best.distance) { best = h; found = true; }
        }
        return found;
    }

    static bool InStaircase(Transform t)
    {
        for (; t != null; t = t.parent) if (t.name.StartsWith("Staircase")) return true;
        return false;
    }

    static bool RenderBounds(Transform root, Transform skip, out Bounds b)
    {
        b = new Bounds(root.position, Vector3.zero);
        bool any = false;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(false))
        {
            if (r.transform.IsChildOf(skip) || r is ParticleSystemRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any;
    }

    static Transform FindChild(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    static string PathOf(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
#endif
