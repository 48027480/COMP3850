using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// A seat anyone can sit on: click it and you sit in its VRCStation (on the same object or set by hand).
// Used by the cafe's stools and banquette (Tools > Anum > Coin System > 2 Build Cafe).
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CafeSeat : UdonSharpBehaviour
{
    public VRCStation station;

    public override void Interact()
    {
        if (station != null) station.UseStation(Networking.LocalPlayer);
    }
}
