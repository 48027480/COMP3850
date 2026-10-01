using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JigsawPieceStation : UdonSharpBehaviour
{
    public TextMeshProUGUI clueText;
    public JigsawPieceStation[] others;

    [UdonSynced] int ownerId = -1;

    string label = "Clue";
    string clue = "";

    void Start()
    {
        ShowClue();
    }

    public bool IsTaken()
    {
        return ownerId != -1;
    }

    public int GetOwnerId()
    {
        return ownerId;
    }

    public void SetPiece(string newLabel, string newClue)
    {
        label = newLabel;
        clue = newClue;
        ShowClue();
    }

    public override void Interact()
    {
        VRCPlayerApi me = Networking.LocalPlayer;
        if (me == null) return;
        if (ownerId != -1) return;
        if (HasOtherClue(me.playerId)) return;

        Networking.SetOwner(me, gameObject);
        ownerId = me.playerId;
        RequestSerialization();
        ShowClue();
    }

    public void ReleasePiece()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ownerId = -1;
        RequestSerialization();
        ShowClue();
    }

    public override void OnDeserialization()
    {
        ShowClue();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null || player.playerId != ownerId) return;

        VRCPlayerApi me = Networking.LocalPlayer;
        if (me != null && me.isMaster) ReleasePiece();
    }

    bool HasOtherClue(int id)
    {
        if (others == null) return false;

        for (int i = 0; i < others.Length; i++)
        {
            if (others[i] != null && others[i].GetOwnerId() == id) return true;
        }
        return false;
    }

    void ShowClue()
    {
        if (clueText == null) return;

        VRCPlayerApi me = Networking.LocalPlayer;
        int myId = -2;
        if (me != null) myId = me.playerId;

        if (ownerId == -1)
            clueText.text = label + "\n\nClick here to get this clue.";
        else if (ownerId == myId)
            clueText.text = label + "\n\n" + clue + "\n\nThis clue is only on your screen, so share it with your group.";
        else
            clueText.text = label + "\n\nSomeone else has this clue. Ask them what it says.";
    }
}