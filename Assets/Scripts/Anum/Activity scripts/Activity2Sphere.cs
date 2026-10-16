using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Activity2Sphere : UdonSharpBehaviour
{
    public int sphereIndex = 0;

    public GameObject infoPanel;
    public TextMeshProUGUI infoTitleText;
    public TextMeshProUGUI infoBodyText;

    public AudioSource audioSource;
    public AudioClip sphereClip;

    public Activity2Console console;
    public float snapDistance = 0.4f;

    // While held, the info panel floats in front of the holder's head so it is easy to read
    public float panelDistance = 1.2f;
    public float panelDrop = 0.15f;

    string storedLabel = "";
    string storedInfo = "";
    bool held = false;

    public void SetContent(string label, string info)
    {
        storedLabel = label;
        storedInfo = info;
    }

    public override void OnPickup()
    {
        if (infoTitleText != null) infoTitleText.text = storedLabel;
        if (infoBodyText != null) infoBodyText.text = storedInfo;
        held = true;
        PlacePanelInFrontOfPlayer();
        if (infoPanel != null) infoPanel.SetActive(true);

        if (audioSource != null && sphereClip != null)
        {
            audioSource.clip = sphereClip;
            audioSource.Play();
        }

        if (console != null)
        {
            console.ClearSlotForSphere(sphereIndex);
        }
    }

    public override void OnDrop()
    {
        held = false;
        if (infoPanel != null) infoPanel.SetActive(false);
        TrySnap();
    }

    void Update()
    {
        if (held) PlacePanelInFrontOfPlayer();
    }

    void PlacePanelInFrontOfPlayer()
    {
        if (infoPanel == null) return;
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        // Use the flat look direction so the panel stays upright when looking up or down
        Vector3 forward = head.rotation * Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 panelPos = head.position + forward * panelDistance + Vector3.down * panelDrop;
        Transform panel = infoPanel.transform;
        panel.position = panelPos;
        // A world-space canvas reads correctly when its forward points away from the viewer
        panel.rotation = Quaternion.LookRotation(panelPos - head.position);
    }

    void TrySnap()
    {
        if (console == null) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        int playerId = localPlayer.playerId;

        int slot = console.FindNearestOpenSlot(transform.position, snapDistance);
        if (slot < 0) return;

        if (!console.CanPlayerPlace(playerId)) return;

        MoveToSlot(slot);
        console.RegisterPlacement(sphereIndex, slot, playerId);
    }

    void MoveToSlot(int slot)
    {
        Transform slotTransform = console.GetSlotTransform(slot);
        if (slotTransform == null) return;
        transform.position = slotTransform.position;
        transform.rotation = slotTransform.rotation;

        VRC_Pickup pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        if (pickup != null) pickup.Drop();
    }

    public void ResetSphere()
    {
        held = false;
        if (infoPanel != null) infoPanel.SetActive(false);

        // Respawn only works for the owner, so take ownership first
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        VRCObjectSync objSync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
        if (objSync != null) objSync.Respawn();
    }
}