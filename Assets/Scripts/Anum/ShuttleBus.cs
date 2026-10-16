using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ShuttleBus : UdonSharpBehaviour
{
    public Vector3[] route;
    public float stopA;
    public float stopB;
    public float speed = 7f;
    public float dwellA = 30f;
    public float dwellB = 20f;
    public int offsetMs = 0;
    public string nameA = "Plaza";
    public string nameB = "Departures";
    public TextMeshPro destinationSign;
    public TextMeshPro insideSign;
    public TextMeshPro[] signsA;
    public TextMeshPro[] signsB;
    public BusSeat[] seats;
    public Transform[] doors;
    public Vector3[] doorClosed;
    public Vector3[] doorOpen;
    public float doorTime = 1.2f;

    float[] cum;
    float total;
    float period;
    int lastAt = -2;
    float signTimer;
    float doorT;
    VRCStation localSeat;

    void Start()
    {
        if (route == null || route.Length < 2)
        {
            Debug.LogWarning("[ShuttleBus] " + gameObject.name + ": no route set, the shuttle will not move. Run Tools > Add Earth Shuttle Buses again.");
            return;
        }
        cum = new float[route.Length + 1];
        cum[0] = 0f;
        for (int i = 0; i < route.Length; i++)
            cum[i + 1] = cum[i] + Vector3.Distance(route[i], route[(i + 1) % route.Length]);
        total = cum[route.Length];
        period = dwellA + dwellB + total / speed;
        Debug.Log("[ShuttleBus] " + transform.parent.name + " running: " + Mathf.RoundToInt(total) + " m loop, a round trip every " + Clock(period) + ".");
    }

    float Now()
    {
        int periodMs = Mathf.Max(1, Mathf.RoundToInt(period * 1000f));
        int ms = Networking.GetServerTimeInMilliseconds() + offsetMs;
        int t = ms % periodMs;
        if (t < 0) t += periodMs;
        return t / 1000f;
    }

    void Update()
    {
        if (cum == null || total <= 0f) return;
        float t = Now();
        float legAB = (stopB - stopA) / speed;
        float tArriveB = dwellA + legAB;

        float s;
        int at;
        float leaveIn = 0f;
        if (t < dwellA) { s = stopA; at = 0; leaveIn = dwellA - t; }
        else if (t < tArriveB) { s = stopA + (t - dwellA) * speed; at = -1; }
        else if (t < tArriveB + dwellB) { s = stopB; at = 1; leaveIn = tArriveB + dwellB - t; }
        else { s = stopB + (t - tArriveB - dwellB) * speed; at = -1; }

        Vector3 here = Sample(s);
        Vector3 ahead = Sample(s + 2.5f) - Sample(s - 2.5f);
        ahead.y = 0f;
        transform.position = here;
        if (ahead.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(ahead.normalized);

        if (at != lastAt)
        {
            if (at >= 0 && lastAt == -1) LetOffLocalRider();
            bool canBoard = at >= 0;
            if (seats != null)
                for (int i = 0; i < seats.Length; i++)
                    if (seats[i] != null) seats[i].DisableInteractive = !canBoard;
            lastAt = at;
        }

        float want = at >= 0 && leaveIn > 3f ? 1f : 0f;
        if (doors != null && doorT != want)
        {
            doorT = Mathf.MoveTowards(doorT, want, Time.deltaTime / doorTime);
            for (int i = 0; i < doors.Length; i++)
                if (doors[i] != null && i < doorClosed.Length && i < doorOpen.Length)
                    doors[i].localPosition = Vector3.Lerp(doorClosed[i], doorOpen[i], doorT);
        }

        signTimer -= Time.deltaTime;
        if (signTimer <= 0f)
        {
            signTimer = 0.5f;
            bool toB = at == 0 || (at == -1 && t < tArriveB);
            string next = toB ? nameB : nameA;
            if (destinationSign != null) destinationSign.text = "TO " + next.ToUpper();
            if (insideSign != null)
                insideSign.text = at >= 0 ? "Departing in " + Clock(leaveIn) + "\nNext stop: " + next : "Next stop: " + next;

            string every = "Every " + Clock(period) + " min";
            string a = at == 0 ? "Boarding now  ·  departs in " + Clock(leaveIn) : "Next shuttle in " + Clock(period - t);
            float waitB = tArriveB - t;
            if (waitB < 0f) waitB += period;
            string b = at == 1 ? "Boarding now  ·  departs in " + Clock(leaveIn) : "Next shuttle in " + Clock(waitB);
            SetAll(signsA, "<b>SHUTTLE TO " + nameB.ToUpper() + "</b>\n" + every + "\n<b>" + a + "</b>");
            SetAll(signsB, "<b>SHUTTLE TO " + nameA.ToUpper() + "</b>\n" + every + "\n<b>" + b + "</b>");
        }
    }

    void SetAll(TextMeshPro[] list, string text)
    {
        if (list == null) return;
        for (int i = 0; i < list.Length; i++) if (list[i] != null) list[i].text = text;
    }

    Vector3 Sample(float s)
    {
        s = s % total;
        if (s < 0f) s += total;
        int lo = 0, hi = route.Length - 1;
        while (lo < hi)
        {
            int mid = (lo + hi + 1) / 2;
            if (cum[mid] <= s) lo = mid; else hi = mid - 1;
        }
        float seg = cum[lo + 1] - cum[lo];
        float k = seg > 0.0001f ? (s - cum[lo]) / seg : 0f;
        return Vector3.Lerp(route[lo], route[(lo + 1) % route.Length], k);
    }

    public bool AtStop()
    {
        return lastAt >= 0;
    }

    public void SetRider(VRCStation station)
    {
        localSeat = station;
    }

    public void ClearRider(VRCStation station)
    {
        if (localSeat == station) localSeat = null;
    }

    void LetOffLocalRider()
    {
        if (localSeat == null) return;
        VRCPlayerApi me = Networking.LocalPlayer;
        if (me == null) return;
        VRCStation seat = localSeat;
        localSeat = null;
        seat.ExitStation(me);
        Vector3 kerb = transform.position - transform.right * 3.2f + Vector3.up * 0.1f;
        me.TeleportTo(kerb, transform.rotation);
    }

    string Clock(float seconds)
    {
        int sec = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int m = sec / 60;
        sec = sec % 60;
        return m + ":" + (sec < 10 ? "0" : "") + sec;
    }
}
