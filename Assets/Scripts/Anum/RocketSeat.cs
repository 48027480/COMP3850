using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RocketSeat : UdonSharpBehaviour
{
    public RocketSystem rocketSystem;
    public VRCStation station;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        if (rocketSystem.isLaunching) return;
        if (rocketSystem.localPlayerSeated) return;

        station.UseStation(player);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            rocketSystem.LocalSatDown(station);
        }
        rocketSystem.SomeoneSatDown();
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            rocketSystem.LocalStoodUp();
        }
        rocketSystem.SomeoneStoodUp();
    }
}