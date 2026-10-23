using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// Rocket door on a vertical hinge: closed across the doorway by default, swings outward while
// anyone is near it, and stays shut while its rocket is in flight (launch until the room resets).
// The swing is animated on the hinge rather than driven by physics, so it looks the same for every
// player. Each client works it out locally from shared state (player positions and the rocket's
// its flying flag), so no extra networking is needed.
// Set up by Tools > Set Up Rocket Doors.
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RocketGate : UdonSharpBehaviour
{
    public RocketSystem rocket;
    public Transform doorTransform;
    public Collider approachZone;

    // All in the door's parent space, so the door moves with the rocket
    public Vector3 hingeLocal;
    public Vector3 hingeAxisLocal = Vector3.up;
    public Vector3 closedLocalPos;
    public Quaternion closedLocalRot = Quaternion.identity;
    public float openAngle = 90f;

    // The pose the door was saved in (Andy's open position); the setup tool restores it on re-runs
    public bool hasSavedOpenPose = false;
    public Vector3 savedOpenPos;
    public Quaternion savedOpenRot = Quaternion.identity;

    public float openSeconds = 1.2f;
    public float closeDelay = 1f;
    public float checkInterval = 0.2f;

    float openAmount = 0f; // 0 closed, 1 open
    float target = 0f;
    float lastSeenNear = -1000f;
    float nextCheck = 0f;
    VRCPlayerApi[] players = new VRCPlayerApi[100];
    bool ready = false;

    void Start()
    {
        if (doorTransform == null) return;
        ApplyPose(0f);
        ready = true;
    }

    void Update()
    {
        if (!ready) return;

        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + checkInterval;
            if (SomeoneNear()) lastSeenNear = Time.time;

            bool inFlight = rocket != null && rocket.flying;
            bool wantOpen = !inFlight && Time.time - lastSeenNear < closeDelay;
            target = wantOpen ? 1f : 0f;
        }

        if (openAmount != target)
        {
            openAmount = Mathf.MoveTowards(openAmount, target, Time.deltaTime / Mathf.Max(openSeconds, 0.05f));
            ApplyPose(Mathf.SmoothStep(0f, 1f, openAmount));
        }
    }

    // Turn the door about its hinge: 0 = closed, 1 = fully open
    void ApplyPose(float t)
    {
        Quaternion turn = Quaternion.AngleAxis(openAngle * t, hingeAxisLocal);
        doorTransform.localRotation = turn * closedLocalRot;
        doorTransform.localPosition = hingeLocal + turn * (closedLocalPos - hingeLocal);
    }

    bool SomeoneNear()
    {
        if (approachZone == null) return false;
        Bounds zone = approachZone.bounds;
        int count = VRCPlayerApi.GetPlayerCount();
        if (count > players.Length) players = new VRCPlayerApi[count];
        VRCPlayerApi.GetPlayers(players);
        for (int i = 0; i < count; i++)
        {
            VRCPlayerApi p = players[i];
            if (!Utilities.IsValid(p)) continue;
            if (zone.Contains(p.GetPosition())) return true;
        }
        return false;
    }
}
