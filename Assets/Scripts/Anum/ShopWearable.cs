using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class ShopWearable : UdonSharpBehaviour
{
    public VRCObjectPool pool;
    public HumanBodyBones bone = HumanBodyBones.Head;
    public Vector3 offset = new Vector3(0f, 0.12f, 0f);
    public Vector3 turn = new Vector3(0f, 0f, 0f);
    public float returnAfter = 90f;
    public float trialSeconds = 60f; // a try-on copy goes back to the shelf after this long, worn or not

    // While you wear it, your own copy is drawn only in mirrors (VRChat's MirrorReflection layer), so a
    // helmet or glasses never cover your eyes. The layer is local: everyone else still sees it on you
    const int MirrorOnlyLayer = 18;

    VRC_Pickup pickup;
    VRCObjectSync sync;
    Transform[] parts;
    int normalLayer;
    bool worn;
    bool held;
    float timer;
    bool trial;
    float trialTimer;

    void Start()
    {
        pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        sync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
        parts = GetComponentsInChildren<Transform>(true);
        normalLayer = gameObject.layer;
    }

    // Only the visible parts (children) change layer: the root keeps its own, so its collider can still be
    // grabbed to take the item off
    void SetLayer(int layer)
    {
        if (parts == null) return;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null && parts[i] != transform) parts[i].gameObject.layer = layer;
        }
    }

    void OnEnable()
    {
        SetLayer(normalLayer);
        worn = false;
        held = false;
        timer = 0f;
        trial = false;
        trialTimer = 0f;
    }

    // Called by the shop when this copy is only being tried on
    public void StartTrial()
    {
        trial = true;
        trialTimer = 0f;
    }

    void ReturnToShelf()
    {
        worn = false;
        held = false;
        timer = 0f;
        trial = false;
        if (pickup != null) pickup.Drop();
        SetLayer(normalLayer);
        if (sync != null) sync.SetKinematic(false);
        Networking.SetOwner(Networking.LocalPlayer, pool.gameObject);
        pool.Return(gameObject);
    }

    public override void OnPickup()
    {
        SetLayer(normalLayer);
        held = true;
        worn = false;
        timer = 0f;
        if (sync != null) sync.SetKinematic(false);
    }

    public override void OnDrop()
    {
        held = false;
        timer = 0f;
    }

    public override void OnPickupUseDown()
    {
        if (pickup != null) pickup.Drop();
        worn = true;
        held = false;
        if (sync != null) sync.SetKinematic(true);
        SetLayer(MirrorOnlyLayer);
    }

    void Update()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (trial)
        {
            trialTimer += Time.deltaTime;
            if (trialTimer >= trialSeconds) { ReturnToShelf(); return; }
        }

        if (worn)
        {
            VRCPlayerApi me = Networking.LocalPlayer;
            Quaternion boneRot = me.GetBoneRotation(bone);
            transform.position = me.GetBonePosition(bone) + boneRot * offset;
            transform.rotation = boneRot * Quaternion.Euler(turn);
            return;
        }

        if (held) return;

        timer += Time.deltaTime;
        if (timer >= returnAfter) ReturnToShelf();
    }
}
