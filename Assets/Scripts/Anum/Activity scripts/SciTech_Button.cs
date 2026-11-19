using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech_Button : UdonSharpBehaviour
{
    public UdonSharpBehaviour target;
    public string eventName;

    public override void Interact()
    {
        if (target != null) target.SendCustomEvent(eventName);
    }
}
