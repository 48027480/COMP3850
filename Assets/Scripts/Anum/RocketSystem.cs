using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

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
    [UdonSynced] public int seatedCount = 0;
    [UdonSynced] public bool isLaunching = false;

    public bool localPlayerInRoom = false;
    public bool localPlayerSeated = false;
    private VRCStation localSeat;
    private int localSeatIndex = -1;
    private bool flying = false;
    private Transform teleportTarget;

    void Start()
    {
        UpdateDisplay();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            localPlayerInRoom = true;
        }

        if (!Networking.IsOwner(gameObject)) return;
        if (isLaunching) return;

        roomOccupancy = roomOccupancy + 1;
        RequestSerialization();
        UpdateDisplay();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            localPlayerInRoom = false;
        }

        if (!Networking.IsOwner(gameObject)) return;
        if (isLaunching) return;

        roomOccupancy = roomOccupancy - 1;
        if (roomOccupancy < 0) roomOccupancy = 0;
        RequestSerialization();
        UpdateDisplay();
    }

    public void LocalSatDown(VRCStation seat)
    {
        localPlayerSeated = true;
        localSeat = seat;
        localSeatIndex = 0;

        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == seat)
            {
                localSeatIndex = i;
            }
        }
    }

    public void LocalStoodUp()
    {
        localPlayerSeated = false;
        localSeat = null;
        localSeatIndex = -1;
    }

    public void SomeoneSatDown()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (isLaunching) return;

        seatedCount = seatedCount + 1;

        if (seatedCount >= seatCapacity)
        {
            isLaunching = true;
            RequestSerialization();
            SendCustomNetworkEvent(NetworkEventTarget.All, "LaunchRocket");
        }
        else
        {
            RequestSerialization();
        }
    }

    public void SomeoneStoodUp()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (isLaunching) return;

        seatedCount = seatedCount - 1;
        if (seatedCount < 0) seatedCount = 0;
        RequestSerialization();
    }

    public void LaunchRocket()
    {
        if (flying) return;
        flying = true;

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

        SendCustomEventDelayedSeconds("ArriveHere", flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds("PlayLandingEffects", flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds("ResetRoom", flightDurationSeconds);
    }

    public void ArriveHere()
    {
        if (!localPlayerSeated) return;
        if (localSeat == null) return;

        int index = localSeatIndex;
        if (index < 0 || index >= destinationPoints.Length)
        {
            index = 0;
        }
        teleportTarget = destinationPoints[index];

        VRCStation mySeat = localSeat;
        LocalStoodUp();

        mySeat.disableStationExit = false;
        mySeat.ExitStation(Networking.LocalPlayer);

        SendCustomEventDelayedSeconds("TeleportNow", 0.3f);
    }

    public void TeleportNow()
    {
        if (teleportTarget == null) return;

        Networking.LocalPlayer.TeleportTo(teleportTarget.position, teleportTarget.rotation);
        teleportTarget = null;
    }

    public void PlayLandingEffects()
    {
        rocketAnimator.SetTrigger("Land");
        engineAudioSource.loop = false;
        engineAudioSource.PlayOneShot(landingSound);
        engineAudioSource.PlayOneShot(landingAnnouncementSound);

        if (teleportTarget != null)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.5f, 1.0f, 0.5f);
        }
    }

    public void ResetRoom()
    {
        flying = false;
        localPlayerInRoom = false;
        engineAudioSource.Stop();

        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].disableStationExit = false;
        }

        if (Networking.IsOwner(gameObject))
        {
            roomOccupancy = 0;
            seatedCount = 0;
            isLaunching = false;
            RequestSerialization();
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (isLaunching)
        {
            capacityDisplay.text = "In flight...";
            return;
        }
        capacityDisplay.text = roomOccupancy + " / " + seatCapacity + " players boarded";
    }

    public override void OnDeserialization()
    {
        UpdateDisplay();
    }
}