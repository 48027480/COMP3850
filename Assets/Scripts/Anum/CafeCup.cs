using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class CafeCup : UdonSharpBehaviour
{
    public VRCObjectPool pool;
    public float returnAfter = 90f;

    float timer;
    bool held;

    void OnEnable()
    {
        timer = 0f;
        held = false;
    }

    public override void OnPickup()
    {
        held = true;
        timer = 0f;
    }

    public override void OnDrop()
    {
        held = false;
        timer = 0f;
    }

    void Update()
    {
        if (held) return;
        if (!Networking.IsOwner(gameObject)) return;

        timer += Time.deltaTime;
        if (timer >= returnAfter)
        {
            timer = 0f;
            Networking.SetOwner(Networking.LocalPlayer, pool.gameObject);
            pool.Return(gameObject);
        }
    }
}
