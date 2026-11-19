using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SciTech3_LuckyConsole : UdonSharpBehaviour
{
    public SciTech3_LuckyBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public TMP_Text titleText;
    public TMP_Text[] cardTexts;
    public TMP_Text[] choiceTexts;
    public TMP_Text optionsText;
    public TMP_Text statusText;

    [UdonSynced] int roundIndex = 0;
    [UdonSynced] int[] claimIds = new int[3];
    [UdonSynced] int[] picks = new int[] { -1, -1, -1 };
    [UdonSynced] int lastRight = -1;

    int loadedRound = -1;

    void Start()
    {
        LoadRound();
    }

    public void Claim0() { Claim(0); }
    public void Claim1() { Claim(1); }
    public void Claim2() { Claim(2); }

    public void Cycle0() { Cycle(0); }
    public void Cycle1() { Cycle(1); }
    public void Cycle2() { Cycle(2); }

    public void NextRound()
    {
        if (tracker != null && !tracker.IsDone()) return;
        TakeOwner();
        roundIndex = (roundIndex + 1) % bank.Count();
        for (int i = 0; i < 3; i++)
        {
            claimIds[i] = 0;
            picks[i] = -1;
        }
        lastRight = -1;
        Save();
        if (tracker != null) tracker.ResetTracker();
        LoadRound();
    }

    void Claim(int i)
    {
        if (!CanPlay()) return;
        if (claimIds[i] != 0) return;
        if (MyCard() >= 0)
        {
            statusText.text = "You already hold a card. Let someone else take this one.";
            return;
        }
        TakeOwner();
        claimIds[i] = Networking.LocalPlayer.playerId;
        Save();
    }

    void Cycle(int i)
    {
        if (!CanPlay()) return;
        if (claimIds[i] != Networking.LocalPlayer.playerId)
        {
            statusText.text = "Only the person holding this card can change it.";
            return;
        }
        TakeOwner();
        picks[i] = (picks[i] + 1) % 4;
        Save();
    }

    public void Check()
    {
        if (!CanPlay()) return;
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == 0 || picks[i] < 0)
            {
                statusText.text = "Every card needs a holder and a choice before you check.";
                return;
            }
        }
        int right = 0;
        for (int i = 0; i < 3; i++)
        {
            if (picks[i] == bank.GetAnswer(roundIndex, i)) right++;
        }
        TakeOwner();
        lastRight = right;
        Save();
        if (right == 3) tracker.Correct();
        else tracker.Wrong();
        RefreshView();
    }

    int MyCard()
    {
        int me = Networking.LocalPlayer.playerId;
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == me) return i;
        }
        return -1;
    }

    bool CanPlay()
    {
        if (bank == null || tracker == null) return false;
        if (zone != null && !zone.HasGroup())
        {
            RefreshView();
            return false;
        }
        if (tracker.IsDone()) return false;
        return true;
    }

    string NameOf(int id)
    {
        VRCPlayerApi p = VRCPlayerApi.GetPlayerById(id);
        if (p == null || !p.IsValid()) return "someone";
        return p.displayName;
    }

    void LoadRound()
    {
        if (bank == null) return;
        loadedRound = roundIndex;
        if (titleText != null) titleText.text = bank.GetTitle(roundIndex);
        if (optionsText != null)
        {
            optionsText.text = "Discoveries: " + bank.GetOption(roundIndex, 0) + ", " + bank.GetOption(roundIndex, 1) + ", " + bank.GetOption(roundIndex, 2) + ", " + bank.GetOption(roundIndex, 3) + "\nOne of these is a decoy.";
        }
        if (tracker != null) tracker.SetTexts(bank.GetHint1(roundIndex), bank.GetHint2(roundIndex), bank.GetCorrectText(roundIndex), bank.GetRevealText(roundIndex));
        RefreshView();
    }

    public void RefreshView()
    {
        if (bank == null) return;
        bool done = tracker != null && tracker.IsDone();

        for (int i = 0; i < cardTexts.Length && i < 3; i++)
        {
            if (cardTexts[i] == null) continue;
            string t = bank.GetAccident(roundIndex, i);
            if (done) t = t + "\n\n<b>" + bank.GetFact(roundIndex, i) + "</b>";
            cardTexts[i].text = t;
        }

        for (int i = 0; i < choiceTexts.Length && i < 3; i++)
        {
            if (choiceTexts[i] == null) continue;
            if (done)
            {
                choiceTexts[i].text = "<b><color=#33CCFF>" + bank.GetOption(roundIndex, bank.GetAnswer(roundIndex, i)) + "</color></b>";
                continue;
            }
            string holder = claimIds[i] == 0 ? "Free. Claim this card." : "Held by " + NameOf(claimIds[i]);
            string pick = picks[i] < 0 ? "No choice yet" : bank.GetOption(roundIndex, picks[i]);
            choiceTexts[i].text = holder + "\n<b>" + pick + "</b>";
        }

        if (statusText == null) return;
        if (zone != null && !zone.HasGroup()) statusText.text = zone.WaitText();
        else if (done) statusText.text = "Set finished. Press Next Set to load another.";
        else if (lastRight >= 0) statusText.text = lastRight + " of 3 matched. Swap ideas and check again.";
        else statusText.text = "Each of you hold one card, pick the discovery it led to, then press Check together.";
    }

    public override void OnDeserialization()
    {
        if (roundIndex != loadedRound) LoadRound();
        else RefreshView();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!Networking.IsOwner(gameObject)) return;
        bool changed = false;
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == player.playerId)
            {
                claimIds[i] = 0;
                changed = true;
            }
        }
        if (changed) Save();
    }

    void TakeOwner()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    void Save()
    {
        RequestSerialization();
        RefreshView();
    }
}
