#if UNITY_EDITOR
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDKBase;

// Editor helper: Tools > Match Arrival Rockets
// When a rocket lands, its passengers are teleported into a twin cabin on the other planet. This makes that
// cabin look exactly like the one they took off in:
//  - the launch cabin's interior ("Rocket Interior": console, racks, the loose kit lying where it fell on
//    landing) is copied into the arrival cabin, in the same place in the room; the arrival cabin's own
//    interior is switched off (not deleted)
//  - the chair passenger i lands on is put exactly where seat i is in the launch cabin, so players land in the
//    same spot facing the same way; arrival lists with gaps or the same chair twice get other chairs of the
//    arrival cabin
//  - chairs nobody sits or lands on are switched off in both cabins, so both show the same chairs
// Both cabins are copies of Andy's Prefab_RocketRoom, so the same place in the room is the same view.
// Also run at the end of Tools > Decorate Rocket Interiors. Ctrl+Z undoes it. Save the scene.
public static class RocketArrivalMatcher
{
    const string UndoName = "Match Arrival Rockets";
    const string HolderName = "Rocket Interior";

    [MenuItem("Tools/Match Arrival Rockets")]
    public static void MatchMenu()
    {
        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();
        string log = Match();
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[RocketArrivalMatcher] Done." + log + "\nSave the scene to keep the changes. Ctrl+Z undoes this.");
    }

    // A rocket someone can take off in: switched on, with at least one seat switched on
    public static bool CanLaunch(RocketSystem rs)
    {
        if (rs == null || !rs.gameObject.activeInHierarchy || rs.seats == null) return false;
        foreach (VRCStation st in rs.seats)
            if (st != null && st.gameObject.activeInHierarchy) return true;
        return false;
    }

    // Launchable rockets, Earth's and Astronomy's before Tech's (Tech's rockets fly into the same cabins)
    static List<RocketSystem> Launchers()
    {
        var list = new List<RocketSystem>();
        foreach (RocketSystem rs in Object.FindObjectsOfType<RocketSystem>(true))
            if (CanLaunch(rs)) list.Add(rs);
        list.Sort((a, b) =>
        {
            int ta = a.transform.root.name.StartsWith("Tech") ? 1 : 0, tb = b.transform.root.name.StartsWith("Tech") ? 1 : 0;
            return ta != tb ? ta - tb : string.CompareOrdinal(PathOf(a.transform), PathOf(b.transform));
        });
        return list;
    }

    // The cabin a launchable rocket's passengers land in, for each such rocket (first rocket wins a shared cabin).
    // Tools > Decorate Rocket Interiors leaves these cabins to this tool
    public static HashSet<Transform> ArrivalRooms()
    {
        var set = new HashSet<Transform>();
        foreach (RocketSystem rs in Launchers())
        {
            Transform a = ArrivalRoom(rs);
            if (a != null && a != LaunchRoom(rs)) set.Add(a);
        }
        return set;
    }

    public static string Match()
    {
        string log = "";
        List<RocketSystem> launchers = Launchers();

        // Chairs that must stay: seats people sit in, and chairs people land on (after repairs below)
        var liveSeats = new HashSet<GameObject>();
        foreach (RocketSystem rs in launchers)
            foreach (VRCStation st in rs.seats)
                if (st != null) liveSeats.Add(st.gameObject);

        var claimed = new Dictionary<Transform, string>();
        var landing = new HashSet<GameObject>();
        var pairs = new List<KeyValuePair<RocketSystem, Transform>>();
        foreach (RocketSystem rs in launchers)
        {
            string name = PathOf(rs.transform);
            Transform d = LaunchRoom(rs), a = ArrivalRoom(rs);
            if (d == null || a == null || a == d) { log += "\n - " + name + ": no launch or arrival cabin found, skipped."; continue; }
            if (claimed.ContainsKey(a)) { log += "\n - " + name + ": lands in the same cabin as " + claimed[a] + ", which already matches that rocket; skipped."; continue; }
            if (HasLiveSeat(a, liveSeats)) { log += "\n - " + name + ": its arrival cabin is also a launch cabin; skipped."; continue; }
            if ((d.lossyScale - a.lossyScale).magnitude > 0.02f * d.lossyScale.magnitude)
            {
                // A stretched launch rocket: the rocket you land in is stretched the same way, door kept in place
                string resized = MatchSize(d, a);
                if (resized == null) { log += "\n - " + name + ": arrival cabin is a different size from the launch cabin and could not be resized; skipped."; continue; }
                log += "\n - " + name + ": " + resized;
            }
            claimed[a] = name;
            pairs.Add(new KeyValuePair<RocketSystem, Transform>(rs, a));
            Transform[] dests = RepairLandingList(rs, a, ref log);
            foreach (Transform t in dests) if (t != null) landing.Add(t.gameObject);
        }

        foreach (var p in pairs)
        {
            RocketSystem rs = p.Key;
            Transform d = LaunchRoom(rs), a = p.Value;
            int chairs = PlaceChairs(rs, d, a);
            int offA = ChairsOff(a, liveSeats, landing);
            int offD = ChairsOff(d, liveSeats, landing);
            string interior = CopyInterior(d, a);
            log += "\n - " + PathOf(rs.transform) + " -> " + PathOf(a) + ": " + chairs + " landing chairs placed like the seats"
                + (offA + offD > 0 ? ", " + (offA + offD) + " spare chairs switched off" : "") + ", " + interior;
        }
        return log;
    }

    // Scales the rocket the arrival cabin belongs to so the cabin ends up the same size as the launch cabin
    // on every axis (both are Andy's Prefab_RocketRoom, so their local axes match), then moves the rocket
    // so its doorway is exactly where it was and its stairs still meet it. Returns null if it cannot
    static string MatchSize(Transform d, Transform a)
    {
        Transform rocket = a.parent;
        if (rocket == null || rocket.GetComponent<RocketSystem>() == null) return null;
        Vector3 doorBefore = DoorCentre(a);
        Vector3 dl = d.lossyScale, al = a.lossyScale;
        Undo.RecordObject(rocket, UndoName);
        Vector3 ls = rocket.localScale;
        for (int j = 0; j < 3; j++)
        {
            // which of the cabin's axes runs along the rocket's axis j
            int best = 0;
            float bestDot = -1f;
            for (int i = 0; i < 3; i++)
            {
                float dot = Mathf.Abs(Vector3.Dot(a.localRotation * Axis(i), Axis(j)));
                if (dot > bestDot) { bestDot = dot; best = i; }
            }
            if (Mathf.Abs(al[best]) > 0.0001f) ls[j] *= dl[best] / al[best];
        }
        rocket.localScale = ls;
        rocket.position += doorBefore - DoorCentre(a);
        PrefabUtility.RecordPrefabInstancePropertyModifications(rocket);
        return "arrival rocket " + rocket.name + " resized to match the launch rocket (" + al.ToString("0.00") + " -> " + a.lossyScale.ToString("0.00")
            + "), doorway kept in place (run Tools > Fix Stairs again afterwards).";
    }

    static Vector3 Axis(int i) { return i == 0 ? Vector3.right : i == 1 ? Vector3.up : Vector3.forward; }

    static Vector3 DoorCentre(Transform room)
    {
        foreach (Transform t in room.GetComponentsInChildren<Transform>(true))
            if (t.name == "Gate")
            {
                Renderer r = t.GetComponent<Renderer>();
                return r != null ? r.bounds.center : t.position;
            }
        return room.position;
    }

    // ================================================================ chairs

    // One distinct chair in the arrival cabin per seat, in seat order
    static Transform[] RepairLandingList(RocketSystem rs, Transform a, ref string log)
    {
        int n = rs.seats.Length;
        var dests = new Transform[n];
        var used = new HashSet<Transform>();
        bool changed = rs.destinationPoints == null || rs.destinationPoints.Length != n;
        for (int i = 0; i < n; i++)
        {
            Transform t = rs.destinationPoints != null && i < rs.destinationPoints.Length ? rs.destinationPoints[i] : null;
            if (t != null && !used.Contains(t) && RoomOf(t) == a) { dests[i] = t; used.Add(t); }
            else changed = true;
        }
        if (!changed) return dests;

        List<Transform> spare = Chairs(a);
        spare.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
        int filled = 0;
        for (int i = 0; i < n; i++)
        {
            if (dests[i] != null) continue;
            foreach (Transform c in spare)
                if (!used.Contains(c)) { dests[i] = c; used.Add(c); filled++; break; }
        }
        Undo.RecordObject(rs, UndoName);
        rs.destinationPoints = dests;
        UdonSharpEditorUtility.CopyProxyToUdon(rs);
        EditorUtility.SetDirty(rs);
        log += "\n - " + PathOf(rs.transform) + ": landing list repaired, " + filled + " passengers now land on their own chair instead of a shared or missing one.";
        return dests;
    }

    // Each landing chair goes where its seat is in the launch cabin, shown but inert (nobody can sit in it)
    static int PlaceChairs(RocketSystem rs, Transform d, Transform a)
    {
        int n = 0;
        for (int i = 0; i < rs.seats.Length && i < rs.destinationPoints.Length; i++)
        {
            VRCStation seat = rs.seats[i];
            Transform c = rs.destinationPoints[i];
            if (seat == null || c == null) continue;
            Undo.RecordObject(c, UndoName);
            c.position = a.TransformPoint(d.InverseTransformPoint(seat.transform.position));
            c.rotation = a.rotation * (Quaternion.Inverse(d.rotation) * seat.transform.rotation);
            if (!c.gameObject.activeSelf) { Undo.RecordObject(c.gameObject, UndoName); c.gameObject.SetActive(true); }
            foreach (Collider col in c.GetComponentsInChildren<Collider>(true))
                if (col.enabled) { Undo.RecordObject(col, UndoName); col.enabled = false; }
            foreach (Behaviour b in c.GetComponentsInChildren<Behaviour>(true))
                if (b.enabled) { Undo.RecordObject(b, UndoName); b.enabled = false; }
            Record(c.gameObject);
            n++;
        }
        return n;
    }

    static int ChairsOff(Transform room, HashSet<GameObject> liveSeats, HashSet<GameObject> landing)
    {
        int n = 0;
        foreach (Transform c in Chairs(room))
        {
            if (!c.gameObject.activeSelf || liveSeats.Contains(c.gameObject) || landing.Contains(c.gameObject)) continue;
            Undo.RecordObject(c.gameObject, UndoName);
            c.gameObject.SetActive(false);
            Record(c.gameObject);
            n++;
        }
        return n;
    }

    // Whole chairs (not chair parts) in the cabin, and beside it under the same rocket
    static List<Transform> Chairs(Transform room)
    {
        var list = new List<Transform>();
        foreach (Transform t in room.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("Chair") && (t.parent == null || !t.parent.name.StartsWith("Chair"))) list.Add(t);
        if (room.parent != null)
            foreach (Transform t in room.parent)
                if (t.name.StartsWith("Chair")) list.Add(t);
        return list;
    }

    static bool HasLiveSeat(Transform room, HashSet<GameObject> liveSeats)
    {
        foreach (Transform c in Chairs(room))
            if (liveSeats.Contains(c.gameObject) && c.gameObject.activeInHierarchy) return true;
        return false;
    }

    // ================================================================ interior

    static string CopyInterior(Transform d, Transform a)
    {
        Transform src = d.Find(HolderName);
        if (src == null) return "launch cabin has no interior yet (run Tools > Decorate Rocket Interiors).";

        Transform old = a.Find(HolderName);
        if (old != null)
        {
            Undo.RecordObject(old.gameObject, UndoName);
            old.gameObject.SetActive(false);
            old.name = HolderName + " (replaced)";
        }

        GameObject copy = Object.Instantiate(src.gameObject, a);
        Undo.RegisterCreatedObjectUndo(copy, UndoName);
        copy.name = HolderName;
        copy.SetActive(true);
        copy.transform.localPosition = src.localPosition;
        copy.transform.localRotation = src.localRotation;
        copy.transform.localScale = src.localScale;

        // Nobody takes off from here: the kit lies where it fell when the launch cabin landed
        RocketSystem here = a.GetComponentInParent<RocketSystem>(true);
        int kit = 0;
        foreach (ZeroGFloat zg in copy.GetComponentsInChildren<ZeroGFloat>(true))
        {
            zg.rocket = here;
            zg.arrival = true;
            if (zg.items != null && zg.landPos != null && zg.landRot != null)
                for (int i = 0; i < zg.items.Length && i < zg.landPos.Length && i < zg.landRot.Length; i++)
                {
                    if (zg.items[i] == null) continue;
                    zg.items[i].localPosition = zg.landPos[i];
                    zg.items[i].localRotation = zg.landRot[i];
                    kit++;
                }
            UdonSharpEditorUtility.CopyProxyToUdon(zg);
            EditorUtility.SetDirty(zg);
        }
        return "interior copied from the launch cabin (" + kit + " loose items lying where they fell)"
            + (old != null ? ", its own interior switched off." : ".");
    }

    // ================================================================ finding cabins

    // A rocket's own cabin: its Prefab_RocketRoom child
    static Transform LaunchRoom(RocketSystem rs)
    {
        foreach (Transform c in rs.transform)
            if (c.name.StartsWith("Prefab_RocketRoom")) return c;
        return null;
    }

    static Transform ArrivalRoom(RocketSystem rs)
    {
        if (rs.destinationPoints == null) return null;
        foreach (Transform t in rs.destinationPoints)
        {
            Transform r = t != null ? RoomOf(t) : null;
            if (r != null) return r;
        }
        return null;
    }

    // The cabin a chair is in, or stands beside under the same rocket
    static Transform RoomOf(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.name.StartsWith("Prefab_RocketRoom")) return p;
            foreach (Transform c in p)
                if (c.name.StartsWith("Prefab_RocketRoom")) return c;
            if (p.GetComponent<RocketSystem>() != null) return null;
        }
        return null;
    }

    static void Record(GameObject go)
    {
        if (!PrefabUtility.IsPartOfPrefabInstance(go)) return;
        foreach (Component c in go.GetComponentsInChildren<Component>(true))
            if (c != null) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
    }

    static string PathOf(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
#endif
