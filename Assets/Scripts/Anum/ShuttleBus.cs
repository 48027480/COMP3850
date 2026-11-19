using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ShuttleBus : UdonSharpBehaviour
{
    public Vector3[] route;
    public float[] stops;
    public float[] dwells;
    public string[] names;
    public float speed = 7f;
    public int offsetMs = 0;
    public TextMeshPro destinationSign;
    public TextMeshPro insideSign;
    public TextMeshPro[] signs;
    public int[] signStop;
    public BusSeat[] seats;
    public Transform[] doors;
    public Vector3[] doorClosed;
    public Vector3[] doorOpen;
    public float doorTime = 1.2f;

    float[] cum;
    float total;
    float period;
    float[] arrive;
    int lastAt = -2;
    float signTimer;
    float doorT;
    VRCStation localSeat;

    void Start()
    {
        if (route == null || route.Length < 2 || stops == null || stops.Length < 2 || dwells == null || dwells.Length != stops.Length)
        {
            Debug.LogWarning("[ShuttleBus] " + gameObject.name + ": no route or stops set, the shuttle will not move. Run Tools > Add Earth Shuttle Buses again.");
            return;
        }
        cum = new float[route.Length + 1];
        cum[0] = 0f;
        for (int i = 0; i < route.Length; i++)
            cum[i + 1] = cum[i] + Vector3.Distance(route[i], route[(i + 1) % route.Length]);
        total = cum[route.Length];

        arrive = new float[stops.Length];
        float t = 0f;
        for (int i = 0; i < stops.Length; i++)
        {
            arrive[i] = t;
            t += dwells[i] + Gap(i) / speed;
        }
        period = t;
        Debug.Log("[ShuttleBus] " + transform.parent.name + " running: " + Mathf.RoundToInt(total) + " m loop, " + stops.Length + " stops, a round trip every " + Clock(period) + ".");
    }

    float Gap(int i)
    {
        if (i + 1 < stops.Length) return stops[i + 1] - stops[i];
        return total - stops[i] + stops[0];
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
        if (cum == null || total <= 0f || arrive == null) return;
        float t = Now();

        int leg = stops.Length - 1;
        for (int i = 0; i < stops.Length; i++)
        {
            if (t >= arrive[i]) leg = i;
        }
        float into = t - arrive[leg];
        float s;
        int at;
        float leaveIn = 0f;
        if (into < dwells[leg])
        {
            s = stops[leg];
            at = leg;
            leaveIn = dwells[leg] - into;
        }
        else
        {
            s = stops[leg] + (into - dwells[leg]) * speed;
            at = -1;
        }
        int next = (leg + 1) % stops.Length;

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
            if (destinationSign != null) destinationSign.text = "TO " + names[next].ToUpper();
            if (insideSign != null)
                insideSign.text = at >= 0 ? "Departing in " + Clock(leaveIn) + "\nNext stop: " + names[next] : "Next stop: " + names[next];

            if (signs != null && signStop != null)
            {
                string every = "Every " + Clock(period) + " min";
                for (int i = 0; i < signs.Length; i++)
                {
                    if (signs[i] == null || i >= signStop.Length) continue;
                    int k = signStop[i];
                    string after = names[(k + 1) % names.Length];
                    string line;
                    if (at == k) line = "Boarding now  ·  departs in " + Clock(leaveIn);
                    else
                    {
                        float wait = arrive[k] - t;
                        if (wait < 0f) wait += period;
                        line = "Next shuttle in " + Clock(wait);
                    }
                    signs[i].text = "<b>SHUTTLE · NEXT STOP " + after.ToUpper() + "</b>\n" + every + "\n<b>" + line + "</b>";
                }
            }
        }
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
