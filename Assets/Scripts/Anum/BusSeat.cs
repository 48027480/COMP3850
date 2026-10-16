using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BusSeat : UdonSharpBehaviour
{
    public ShuttleBus bus;
    public VRCStation station;

    public override void Interact()
    {
        if (bus == null || station == null || !bus.AtStop()) return;
        station.UseStation(Networking.LocalPlayer);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal && bus != null) bus.SetRider(station);
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal && bus != null) bus.ClearRider(station);
    }
}
