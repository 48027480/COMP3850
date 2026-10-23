using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

// One per seat. Sits the local player down when they walk into it, tells the RocketSystem when the seat
// fills or empties, and swivels: the seated player turns the chair (and themselves) with the turn input
// (right thumbstick in VR, mouse on desktop). The turn is synced from the seated player, so everyone sees
// the chair face the same way; it swings back to the front when they stand up.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RocketSeat : UdonSharpBehaviour
{
    public RocketSystem rocketSystem;
    public VRCStation station;
    public float turnSpeed = 120f; // degrees per second at full stick

    [UdonSynced] float yaw = 0f;

    int mySeatIndex = -1;
    bool localSeated = false;
    float lookInput = 0f;
    float shownYaw = 0f;
    float appliedYaw = 0f;
    float nextSend = 0f;
    bool dirty = false;
    float lastLeft = -100f;
    float satAt = -100f;
    int reseats = 0;

    Vector3 restPos;
    Quaternion restRot;
    Vector3 pivot; // middle of the chair, in its parent's space: the chair turns about this
    Bounds chairBounds; // the seat's trigger box, taken at start: its live bounds are unreliable while someone sits

    void Start()
    {
        // Exit by jumping is always blocked by VRChat; InputJump below lets the player out before launch.
        // Never switch it at runtime: changing it while someone is sitting throws them out of the seat
        if (station != null && !station.disableStationExit) station.disableStationExit = true;
        Collider c = GetComponent<Collider>();
        chairBounds = c != null ? c.bounds : new Bounds(transform.position, Vector3.one * 2f);
        FindMyIndex();
        restPos = transform.localPosition;
        restRot = transform.localRotation;
        pivot = restPos;
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            pivot = transform.parent != null ? transform.parent.InverseTransformPoint(b.center) : b.center;
            pivot.y = restPos.y;
        }
    }

    void FindMyIndex()
    {
        for (int i = 0; i < rocketSystem.seats.Length; i++)
        {
            if (rocketSystem.seats[i] == station)
            {
                mySeatIndex = i;
            }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        // After a teleport VRChat can still think the player is inside the seat they flew from, and fire this
        // again when they sit somewhere else (even on another planet), pulling them into the old seat. Only
        // seat someone who is really standing at this chair
        if (!IsAtChair(player, 1f)) return;
        // Standing up leaves the player at the chair, inside this trigger: don't sit them straight back down
        if (Time.time - lastLeft < 2f) return;
        if (!rocketSystem.ReadyToBoard()) return;
        if (rocketSystem.localPlayerSeated) return;

        station.UseStation(player);
    }

    bool IsAtChair(VRCPlayerApi player, float margin)
    {
        Bounds b = chairBounds;
        b.Expand(margin * 2f);
        return b.Contains(player.GetPosition());
    }

    // Jump stands the player up while the rocket is still boarding; in flight they stay seated
    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (!value || !localSeated || rocketSystem.flying) return;
        station.ExitStation(Networking.LocalPlayer);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        // Someone can sit before Start has run on this seat; without the index the seat is not counted
        // and its passenger is never flown, so look it up again
        if (mySeatIndex < 0) FindMyIndex();
        if (player.isLocal && !IsAtChair(player, 5f))
        {
            // A seat the player left on another trip trying to take them back: let go, count nothing
            Debug.Log("[Rocket] " + Where() + ": you were pulled into this seat from "
                + Vector3.Distance(player.GetPosition(), transform.position).ToString("0") + " m away; letting go");
            station.ExitStation(player);
            return;
        }
        if (player.isLocal)
        {
            if (!rocketSystem.flying) reseats = 0; // a new trip
            satAt = Time.time;
            lookInput = 0f; // a turn left over from walking around would spin the chair straight away
            localSeated = true;
            Networking.SetOwner(player, gameObject);
            SetYaw(0f);
            rocketSystem.LocalSatDown(mySeatIndex);
        }
        rocketSystem.SeatFilled(mySeatIndex);
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal && !localSeated) return; // the let-go above, never counted

        // Pushed out during the flight by anything but the landing (which marks the player stood up before
        // letting go): sit them straight back down, a few times at most, so they still fly
        if (player.isLocal && rocketSystem.flying && rocketSystem.localPlayerSeated && reseats < 3)
        {
            reseats++;
            Debug.Log("[Rocket] " + Where() + ": you were pushed out of your seat " + (Time.time - satAt).ToString("0.00")
                + " s after sitting, now " + Vector3.Distance(player.GetPosition(), transform.position).ToString("0.0")
                + " m from it; sitting you back down");
            lookInput = 0f;
            // Wait a moment: an old seat grabbing the player needs to finish letting go first, or it grabs again
            SendCustomEventDelayedSeconds(nameof(Reseat), 0.3f);
            return;
        }
        if (player.isLocal)
        {
            lastLeft = Time.time;
            localSeated = false;
            lookInput = 0f;
            SetYaw(0f);
            rocketSystem.LocalStoodUp();
            rocketSystem.SeatLetGo();
        }
        rocketSystem.SeatEmptied(mySeatIndex);
    }

    public void Reseat()
    {
        if (!rocketSystem.flying) return;
        station.UseStation(Networking.LocalPlayer);
    }

    string Where()
    {
        Transform r = rocketSystem.transform;
        return (r.parent != null ? r.parent.name + "/" : "") + r.name + "/" + name;
    }

    public override void InputLookHorizontal(float value, UdonInputEventArgs args)
    {
        lookInput = Mathf.Clamp(value, -1f, 1f);
    }

    void SetYaw(float y)
    {
        yaw = y;
        dirty = true;
    }

    void Update()
    {
        if (localSeated)
        {
            if (Mathf.Abs(lookInput) > 0.2f)
            {
                yaw = Mathf.Repeat(yaw + lookInput * turnSpeed * Time.deltaTime, 360f);
                dirty = true;
            }
            shownYaw = yaw;
        }
        else
        {
            shownYaw = Mathf.MoveTowardsAngle(shownYaw, yaw, 360f * Time.deltaTime); // smooth for everyone else
        }

        // The seated player sends their turn a few times a second (and once more when they stand up)
        if (dirty && Networking.IsOwner(gameObject) && Time.time >= nextSend)
        {
            nextSend = Time.time + 0.2f;
            dirty = false;
            RequestSerialization();
        }

        if (shownYaw != appliedYaw)
        {
            appliedYaw = shownYaw;
            Quaternion turn = Quaternion.Euler(0f, shownYaw, 0f);
            transform.localRotation = turn * restRot;
            transform.localPosition = pivot + turn * (restPos - pivot);
        }
    }
}
