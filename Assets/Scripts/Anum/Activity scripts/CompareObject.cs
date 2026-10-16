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

    public Transform labelCanvas;
    public float labelHeight = 0.2f;
    public float cardHeight = 0.42f;

    public Transform snapPoint;
    public float snapRange = 1.3f;
    public float snapHeight = 0.8f;
    public CompareObject partner;

    string label = "";
    string info = "";
    Vector3 homePos;
    Quaternion homeRot;

    void Start()
    {
        homePos = transform.position;
        homeRot = transform.rotation;
        if (infoCard != null) infoCard.SetActive(false);
    }

    void LateUpdate()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null) return;
        Vector3 head = local.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        if (labelCanvas != null && labelCanvas.gameObject.activeSelf) Float(labelCanvas, labelHeight, head);
        if (infoCard != null && infoCard.activeSelf)
            InView(infoCard.transform, head, local.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
    }

    public float panelDistance = 1.2f;
    public float panelDrop = 0.15f;

    void InView(Transform t, Vector3 head, Quaternion headRot)
    {
        Vector3 fwd = headRot * Vector3.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 pos = head + fwd * panelDistance + Vector3.down * panelDrop;
        t.position = pos;
        t.rotation = Quaternion.LookRotation(pos - head);
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
        if (labelCanvas != null) labelCanvas.gameObject.SetActive(false);
    }

    public override void OnDrop()
    {
        if (infoCard != null) infoCard.SetActive(false);
        if (labelCanvas != null) labelCanvas.gameObject.SetActive(true);
        if (!SnapToPedestal()) MoveTo(homePos, homeRot);
    }

    bool SnapToPedestal()
    {
        if (snapPoint == null) return false;

        Vector3 d = transform.position - snapPoint.position;
        if (Mathf.Abs(d.y) > snapHeight) return false;
        if (new Vector2(d.x, d.z).magnitude > snapRange) return false;

        if (partner != null && Vector3.Distance(partner.transform.position, snapPoint.position) < 0.3f)
            partner.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(GoHome));

        MoveTo(snapPoint.position, Quaternion.identity);
        return true;
    }

    public void GoHome()
    {
        MoveTo(homePos, homeRot);
    }

    void MoveTo(Vector3 p, Quaternion r)
    {
        transform.SetPositionAndRotation(p, r);
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
        if (labelCanvas != null) labelCanvas.gameObject.SetActive(true);
    }
}
