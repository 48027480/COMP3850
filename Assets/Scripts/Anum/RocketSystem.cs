using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RocketSystem : UdonSharpBehaviour
{
    public int seatCapacity = 3;
    public float flightDurationSeconds = 180f;

    public TMP_Text capacityDisplay;
    public Transform[] destinationPoints;
    public Animator rocketAnimator;
    public AudioSource engineAudioSource;
    public AudioClip launchSound;
    public AudioClip flightLoopSound;
    public AudioClip landingSound;
    public AudioClip launchAnnouncementSound;
    public AudioClip landingAnnouncementSound;

    public VRCStation[] seats;

    [UdonSynced] public int roomOccupancy = 0;

    public bool localPlayerInRoom = false;
    public bool localPlayerSeated = false;
    public bool flying = false;
    // How many seats are filled, for RocketDisplay and RocketGate to read; the launch decision uses seatFilled
    public int seatedCount = 0;

    private bool[] seatFilled;
    private int localSeatIndex = -1;
    private Transform arrivalTarget;
    private int arrivalChecks = 0;
    private bool arrivalStarted = false;
    private float launchTime = -1000f;

    void Start()
    {
        seatFilled = new bool[seats.Length];
        UpdateDisplay();
    }

    // The landing starts this long before the passengers step out, so they feel and hear it first
    const float LandingLead = 6f;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        localPlayerInRoom = true;

        if (flying) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        roomOccupancy = roomOccupancy + 1;
        RequestSerialization();
        UpdateDisplay();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        localPlayerInRoom = false;

        if (flying) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        roomOccupancy = roomOccupancy - 1;
        if (roomOccupancy < 0) roomOccupancy = 0;
        RequestSerialization();
        UpdateDisplay();
    }

    public void LocalSatDown(int seatIndex)
    {
        localPlayerSeated = true;
        localSeatIndex = seatIndex;
        Log("you sat in seat " + seatIndex + (flying ? " (rocket still marked in flight)" : ""));
    }

    public void LocalStoodUp()
    {
        if (localPlayerSeated) Log("you left seat " + localSeatIndex);
        localPlayerSeated = false;
        localSeatIndex = -1;
    }

    // Safety net: a rocket still marked in flight well after its flight ended (its reset never ran) is
    // reset before anyone boards, so it can never be left stuck. RocketSeat asks this before seating someone
    public bool ReadyToBoard()
    {
        if (flying && Time.time > launchTime + flightDurationSeconds + 2f)
        {
            Log("was still marked in flight " + (Time.time - launchTime).ToString("0") + " s after launch; resetting it now");
            ResetRoom();
        }
        return !flying;
    }

    public void SeatFilled(int seatIndex)
    {
        ReadyToBoard();
        Log("seat " + seatIndex + " filled" + (flying ? ", ignored: in flight" : "") + (seatIndex >= 0 && seatIndex < seatFilled.Length && seatFilled[seatIndex] ? ", ignored: already counted" : ""));
        if (flying) return;
        if (seatIndex < 0 || seatIndex >= seatFilled.Length) return;
        if (seatFilled[seatIndex]) return;

        seatFilled[seatIndex] = true;
        seatedCount = CountFilled();
        UpdateDisplay();
        CheckFull();
    }

    public void SeatEmptied(int seatIndex)
    {
        if (flying) return;
        if (seatIndex < 0 || seatIndex >= seatFilled.Length) return;
        if (!seatFilled[seatIndex]) return;

        seatFilled[seatIndex] = false;
        seatedCount = CountFilled();
        UpdateDisplay();
    }

    int CountFilled()
    {
        int n = 0;
        for (int i = 0; i < seatFilled.Length; i++)
        {
            if (seatFilled[i]) n++;
        }
        return n;
    }

    void CheckFull()
    {
        if (flying) return;
        if (CountFilled() < seatCapacity) return;

        flying = true;
        Log("all " + seatCapacity + " seats filled: launching");
        LaunchRocket();
    }

    public void LaunchRocket()
    {
        launchTime = Time.time;
        capacityDisplay.text = "In flight...";

        // The seats' disableStationExit is left alone (always on, see RocketSeat): changing it while someone is
        // sitting makes VRChat throw them out, which is what emptied every rocket from its second trip on

        rocketAnimator.SetTrigger("Launch");
        engineAudioSource.PlayOneShot(launchSound);
        engineAudioSource.PlayOneShot(launchAnnouncementSound);

        engineAudioSource.clip = flightLoopSound;
        engineAudioSource.loop = true;
        engineAudioSource.PlayScheduled(AudioSettings.dspTime + 1.5);

        if (localPlayerSeated)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.5f, 0.8f, 0.5f);
        }

        // The landing shake and sounds start before the passengers step out, so they feel the landing
        SendCustomEventDelayedSeconds("PlayLandingEffects", Mathf.Max(1f, flightDurationSeconds - LandingLead));
        SendCustomEventDelayedSeconds("ArriveHere", flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds("ResetRoom", flightDurationSeconds);
    }

    public void ArriveHere()
    {
        if (!localPlayerSeated) { Log("arriving: you are not seated in this rocket, so you stay here"); return; }
        if (localSeatIndex < 0 || localSeatIndex >= seats.Length) { Log("arriving: your seat number " + localSeatIndex + " is not in the seat list"); return; }

        int index = localSeatIndex;
        if (index >= destinationPoints.Length) index = 0;
        Transform target = destinationPoints[index];

        VRCStation mySeat = seats[localSeatIndex];
        LocalStoodUp();

        // Teleport only once VRChat has really let go of the seat (RocketSeat calls SeatLetGo), and a moment
        // after that: teleporting while it is still letting go leaves the seat holding on to the player, and
        // the next time they sit anywhere it pulls them back out. ArrivalTimeout covers a let-go that never reports
        Log("arriving: leaving seat " + index + "; will teleport to " + target.name + " once the seat lets go");
        arrivalTarget = target;
        arrivalChecks = 4;
        arrivalStarted = false;
        SendCustomEventDelayedSeconds("ArrivalTimeout", 1f);

        mySeat.ExitStation(Networking.LocalPlayer); // works even with disableStationExit on
    }

    // Called by the seat when VRChat reports the local player has left it
    public void SeatLetGo()
    {
        if (arrivalTarget == null || arrivalStarted) return;
        arrivalStarted = true;
        Log("arriving: seat let go; teleporting to " + arrivalTarget.name);
        SendCustomEventDelayedSeconds("CheckArrived", 0.25f);
        SendCustomEventDelayedSeconds("CheckArrived", 0.6f);
        SendCustomEventDelayedSeconds("CheckArrived", 1f);
        SendCustomEventDelayedSeconds("CheckArrived", 1.5f);
    }

    public void ArrivalTimeout()
    {
        if (arrivalTarget == null || arrivalStarted) return;
        Log("arriving: the seat never reported letting go; teleporting anyway");
        SeatLetGo();
    }

    public void CheckArrived()
    {
        if (arrivalTarget == null) return;
        VRCPlayerApi me = Networking.LocalPlayer;
        if (Vector3.Distance(me.GetPosition(), arrivalTarget.position) > 1.5f)
        {
            me.TeleportTo(arrivalTarget.position, arrivalTarget.rotation);
        }
        arrivalChecks--;
        if (arrivalChecks <= 0) arrivalTarget = null;
    }

    public void PlayLandingEffects()
    {
        rocketAnimator.SetTrigger("Land");
        engineAudioSource.loop = false;
        engineAudioSource.PlayOneShot(landingSound);
        engineAudioSource.PlayOneShot(landingAnnouncementSound);

        if (localPlayerSeated)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.5f, 1.0f, 0.5f);
        }
    }

    public void ResetRoom()
    {
        Log("reset, ready to board");
        flying = false;
        localPlayerInRoom = false;
        engineAudioSource.Stop();

        for (int i = 0; i < seats.Length; i++)
        {
            seatFilled[i] = false;
        }
        seatedCount = 0;

        if (Networking.IsOwner(gameObject))
        {
            roomOccupancy = 0;
            RequestSerialization();
        }

        UpdateDisplay();
    }

    void Log(string message)
    {
        string where = transform.parent != null ? transform.parent.name + "/" + name : name;
        Debug.Log("[Rocket] " + where + ": " + message);
    }

    private void UpdateDisplay()
    {
        if (flying)
        {
            capacityDisplay.text = "In flight...";
            return;
        }
        capacityDisplay.text = CountFilled() + " / " + seatCapacity + " players boarded";
    }

    public override void OnDeserialization()
    {
        UpdateDisplay();
    }
}