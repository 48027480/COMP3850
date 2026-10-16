using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LiftPad : UdonSharpBehaviour
{
    public Transform target;

    public override void Interact()
    {
        if (target == null || Networking.LocalPlayer == null) return;
        Networking.LocalPlayer.TeleportTo(target.position, target.rotation);
    }
}
