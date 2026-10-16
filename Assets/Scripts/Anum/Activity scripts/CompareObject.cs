using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Components;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class CompareObject : UdonSharpBehaviour
{
    public TextMeshProUGUI labelText;
    public GameObject infoCard;
    public TextMeshProUGUI infoText;

    // The label and card float above the orb, upright and turned to face whoever is looking
    public Transform labelCanvas;
    public float labelHeight = 0.2f;
    public float cardHeight = 0.42f;

    // Dropped anywhere over the table, the orb settles onto the answer pedestal
    public Transform snapPoint;
    public float snapRange = 1.3f;
    public float snapHeight = 0.8f;

    string label = "";
    string info = "";

    void Start()
    {
        if (infoCard != null) infoCard.SetActive(false);
    }

    void LateUpdate()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null) return;
        Vector3 head = local.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        if (labelCanvas != null) Float(labelCanvas, labelHeight, head);
        if (infoCard != null && infoCard.activeSelf) Float(infoCard.transform, cardHeight, head);
    }

    void Float(Transform t, float height, Vector3 head)
    {
        t.position = transform.position + Vector3.up * height;
        Vector3 look = t.position - head;
        if (look.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(look);
    }

    public void SetCard(string newLabel, string newInfo)
    {
        label = newLabel;
        info = newInfo;

        if (labelText != null) labelText.text = label;
        if (infoText != null) infoText.text = label + "\n\n" + info + "\n\nOnly you can see this. Tell your group what it says.";
    }

    public override void OnPickup()
    {
        if (infoCard != null) infoCard.SetActive(true);
    }

    public override void OnDrop()
    {
        if (infoCard != null) infoCard.SetActive(false);
        SnapToPedestal();
    }

    void SnapToPedestal()
    {
        if (snapPoint == null) return;

        Vector3 d = transform.position - snapPoint.position;
        if (Mathf.Abs(d.y) > snapHeight) return;
        if (new Vector2(d.x, d.z).magnitude > snapRange) return;

        transform.SetPositionAndRotation(snapPoint.position, Quaternion.identity);
        VRCObjectSync sync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
        if (sync != null) sync.FlagDiscontinuity();
    }

    public void ResetObject()
    {
        SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(DoRespawn));
    }

    public void DoRespawn()
    {
        VRC_Pickup pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        if (pickup != null) pickup.Drop();

        VRCObjectSync sync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
        if (sync != null) sync.Respawn();

        if (infoCard != null) infoCard.SetActive(false);
    }
}
