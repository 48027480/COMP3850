using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MirrorToggle : UdonSharpBehaviour
{
    public GameObject mirror;
    public float offDistance = 6f;

    void Start()
    {
        if (mirror != null) mirror.SetActive(false);
    }

    public override void Interact()
    {
        if (mirror != null) mirror.SetActive(!mirror.activeSelf);
    }

    void Update()
    {
        if (mirror == null || !mirror.activeSelf) return;

        VRCPlayerApi me = Networking.LocalPlayer;
        if (me == null) return;

        if (Vector3.Distance(me.GetPosition(), transform.position) > offDistance)
        {
            mirror.SetActive(false);
        }
    }
}
