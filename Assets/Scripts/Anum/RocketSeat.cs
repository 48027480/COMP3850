using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RocketSeat : UdonSharpBehaviour
{
    public RocketSystem rocketSystem;
    public VRCStation station;

    private bool localPlayerHere = false;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal && !localPlayerHere)
        {
            localPlayerHere = true;
            station.UseStation(player);
        }
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            rocketSystem.SeatOccupied(station);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            localPlayerHere = false;
            rocketSystem.SeatVacated();
        }
    }
}