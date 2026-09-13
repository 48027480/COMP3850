using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class RocketSystem : UdonSharpBehaviour
{
    public int seatCapacity = 3;
    public float flightDurationSeconds = 180f;
    public bool useFixedSchedule = false;
    public float fixedScheduleSeconds = 300f;

    public TMP_Text capacityDisplay;
    public Transform destinationPoint;
    public Animator rocketAnimator;
    public AudioSource engineAudioSource;
    public AudioClip launchSound;
    public AudioClip flightLoopSound;
    public AudioClip landingSound;

    [UdonSynced] public int boardedCount = 0;
    [UdonSynced] public bool isLaunching = false;

    public bool localPlayerBoarded = false;
    public float scheduleTimer = 0f;

    void Start()
    {
        capacityDisplay.text = boardedCount + " / " + seatCapacity + " players boarded";
    }

    void Update()
    {
        if (useFixedSchedule && !isLaunching)
        {
            scheduleTimer += Time.deltaTime;

            if (scheduleTimer >= fixedScheduleSeconds)
            {
                scheduleTimer = 0f;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                LaunchRocket();
            }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            boardedCount = boardedCount + 1;
            localPlayerBoarded = true;
            RequestSerialization();
            capacityDisplay.text = boardedCount + " / " + seatCapacity + " players boarded";

            if (!useFixedSchedule && boardedCount >= seatCapacity)
            {
                LaunchRocket();
            }
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            boardedCount = boardedCount - 1;
            localPlayerBoarded = false;
            RequestSerialization();
            capacityDisplay.text = boardedCount + " / " + seatCapacity + " players boarded";
        }
    }

    public void LaunchRocket()
    {
        isLaunching = true;
        RequestSerialization();
        capacityDisplay.text = "In flight...";

        rocketAnimator.SetTrigger("Launch");
        engineAudioSource.PlayOneShot(launchSound);

        engineAudioSource.clip = flightLoopSound;
        engineAudioSource.loop = true;
        engineAudioSource.PlayScheduled(AudioSettings.dspTime + 1.5);

        if (localPlayerBoarded)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.5f, 0.8f, 0.5f);
        }

        SendCustomEventDelayedSeconds(nameof(TeleportIfBoarded), 3f);
        SendCustomEventDelayedSeconds(nameof(PlayLandingEffects), flightDurationSeconds - 3f);
        SendCustomEventDelayedSeconds(nameof(ResetRoom), flightDurationSeconds);
    }

    public void TeleportIfBoarded()
    {
        if (localPlayerBoarded)
        {
            Networking.LocalPlayer.TeleportTo(destinationPoint.position, destinationPoint.rotation);
        }
    }

    public void PlayLandingEffects()
    {
        rocketAnimator.SetTrigger("Land");
        engineAudioSource.loop = false;
        engineAudioSource.PlayOneShot(landingSound);

        if (localPlayerBoarded)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.5f, 1.0f, 0.5f);
        }
    }

    public void ResetRoom()
    {
        boardedCount = 0;
        isLaunching = false;
        localPlayerBoarded = false;
        engineAudioSource.Stop();
        RequestSerialization();
        capacityDisplay.text = boardedCount + " / " + seatCapacity + " players boarded";
    }

    public override void OnDeserialization()
    {
        capacityDisplay.text = boardedCount + " / " + seatCapacity + " players boarded";
    }
}