using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CompareBriefStation : UdonSharpBehaviour
{
    public TextMeshProUGUI briefText;

    [UdonSynced] int ownerId = -1;

    string brief = "";

    void Start()
    {
        UpdateText();
    }

    public override void Interact()
    {
        if (ownerId != -1) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ownerId = Networking.LocalPlayer.playerId;
        RequestSerialization();
        UpdateText();
    }

    public void SetBrief(string newBrief)
    {
        brief = newBrief;
        UpdateText();
    }

    public void ReleaseBrief()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ownerId = -1;
        RequestSerialization();
        UpdateText();
    }

    public bool IsTaken()
    {
        return ownerId != -1;
    }

    public override void OnDeserialization()
    {
        UpdateText();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null) return;
        if (player.playerId != ownerId) return;
        if (!Networking.IsMaster) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ownerId = -1;
        RequestSerialization();
        UpdateText();
    }

    void UpdateText()
    {
        DisableInteractive = ownerId != -1;
        if (briefText == null) return;

        int myId = Networking.LocalPlayer != null ? Networking.LocalPlayer.playerId : -2;

        if (ownerId == -1)
            briefText.text = "Briefing card\n\nInteract to take the briefing.";
        else if (ownerId == myId)
            briefText.text = "Briefing card\n\n" + brief + "\n\nOnly you can see this. Read it to your group.";
        else
            briefText.text = "Briefing card\n\nSomeone else has the briefing. Ask them what it says.";
    }
}