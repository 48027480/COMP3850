using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class RocketSystem : UdonSharpBehaviour
{
    public int seatCapacity = 3;
    public float flightDurationSeconds = 180f;

    public TMP_Text capacityDisplay;
    public Transform[] destinationPoints; // same length/order as seats[]
    public Animator rocketAnimator;
    public AudioSource engineAudioSource;
    public AudioClip launchSound;
    public AudioClip flightLoopSound;
    public AudioClip landingSound;
    public AudioClip launchAnnouncementSound;
    public AudioClip landingAnnouncementSound;

    public VRCStation[] seats; // assign every seat in this room, length == seatCapacity

    [UdonSynced] public int roomOccupancy = 0;   // for the sign outside — anyone in the room
    [UdonSynced] public int seatedCount = 0;      // for launch logic — players actually seated
    [UdonSynced] public bool isLaunching = false;

    public bool localPlayerInRoom = false;
    public bool localPlayerSeated = false;
    private VRCStation localSeat;

    void Start()
    {
        UpdateDisplay();
    }

    // ---------- Room occupancy (display only) ----------

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal && !isLaunching)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            roomOccupancy = roomOccupancy + 1;
            localPlayerInRoom = true;
            RequestSerialization();
            UpdateDisplay();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal && !isLaunching)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            roomOccupancy = roomOccupancy - 1;
            localPlayerInRoom = false;
            RequestSerialization();
            UpdateDisplay();
        }
    }

    // ---------- Seat occupancy (launch logic) ----------

    public void SeatOccupied(VRCStation seat)
    {
        if (isLaunching) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        seatedCount = seatedCount + 1;
        localPlayerSeated = true;
        localSeat = seat;
        RequestSerialization();

        if (seatedCount >= seatCapacity)
        {
            LaunchRocket();
        }
    }

    public void SeatVacated()
    {
        if (isLaunching) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        seatedCount = seatedCount - 1;
        localPlayerSeated = false;
        localSeat = null;
        RequestSerialization();
    }

    // ---------- Launch sequence ----------

    public void LaunchRocket()
    {
        isLaunching = true;
        RequestSerialization();
        capacityDisplay.text = "In flight...";

        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].disableStationExit = true;
        }

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

        SendCustomEventDelayedSeconds(nameof(TeleportIfBoarded), flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds(nameof(PlayLandingEffects), flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds(nameof(ResetRoom), flightDurationSeconds);
    }

    public void TeleportIfBoarded()
    {
        if (localPlayerSeated)
        {
            int seatIndex = -1;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i] == localSeat)
                {
                    seatIndex = i;
                    break;
                }
            }

            Transform target = (seatIndex >= 0 && seatIndex < destinationPoints.Length)
                ? destinationPoints[seatIndex]
                : destinationPoints[0]; // fallback if something goes wrong

            Networking.LocalPlayer.TeleportTo(target.position, target.rotation);

            if (localSeat != null)
            {
                localSeat.ExitStation(Networking.LocalPlayer);
            }
        }
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
        roomOccupancy = 0;
        seatedCount = 0;
        isLaunching = false;
        localPlayerInRoom = false;
        localPlayerSeated = false;
        localSeat = null;
        engineAudioSource.Stop();

        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].disableStationExit = false;
        }

        RequestSerialization();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        capacityDisplay.text = roomOccupancy + " / " + seatCapacity + " players boarded";
    }

    public override void OnDeserialization()
    {
        UpdateDisplay();
    }
}