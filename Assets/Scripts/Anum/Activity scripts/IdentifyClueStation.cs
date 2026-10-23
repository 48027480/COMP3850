using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class IdentifyClueStation : UdonSharpBehaviour
{
    public TextMeshProUGUI clueText;

    [UdonSynced] int ownerId = -1;

    string label = "Clue";
    string clue = "";

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

    public void SetClue(string newLabel, string newClue)
    {
        label = newLabel;
        clue = newClue;
        UpdateText();
    }

    public void ReleaseClue()
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

    public int GetHolder()
    {
        return ownerId;
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
        if (ownerId == -1)
        {
            DisableInteractive = false;
        }
        else
        {
            DisableInteractive = true;
        }
        if (clueText == null) return;

        int myId = -2;
        if (Networking.LocalPlayer != null)
        {
            myId = Networking.LocalPlayer.playerId;
        }

        if (ownerId == -1)
        {
            clueText.text = label + "\n\nInteract to take this clue.";
        }
        else if (ownerId == myId)
        {
            clueText.text = label + "\n\n" + clue + "\n\nOnly you can see this. Tell your group what it says.";
        }
        else
        {
            clueText.text = label + "\n\nSomeone else has this clue. Ask them what it says.";
        }
    }
}
