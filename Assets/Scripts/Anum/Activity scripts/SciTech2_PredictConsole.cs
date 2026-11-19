using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SciTech2_PredictConsole : UdonSharpBehaviour
{
    public SciTech2_PredictBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public TMP_Text titleText;
    public TMP_Text questionText;
    public TMP_Text[] optionTexts;
    public TMP_Text[] padTexts;
    public TMP_Text statusText;

    [UdonSynced] int caseIndex = 0;
    [UdonSynced] int[] claimIds = new int[3];
    [UdonSynced] int[] votes = new int[] { -1, -1, -1 };
    [UdonSynced] int lastAgreed = -1;

    int loadedCase = -1;
    string[] letters = new string[] { "A", "B", "C", "D" };

    void Start()
    {
        LoadCase();
    }

    public void Claim0() { Claim(0); }
    public void Claim1() { Claim(1); }
    public void Claim2() { Claim(2); }

    public void VoteA() { Vote(0); }
    public void VoteB() { Vote(1); }
    public void VoteC() { Vote(2); }
    public void VoteD() { Vote(3); }

    public void NextCase()
    {
        if (tracker != null && !tracker.IsDone()) return;
        TakeOwner();
        caseIndex = (caseIndex + 1) % bank.Count();
        for (int i = 0; i < 3; i++)
        {
            claimIds[i] = 0;
            votes[i] = -1;
        }
        lastAgreed = -1;
        Save();
        if (tracker != null) tracker.ResetTracker();
        LoadCase();
    }

    void Claim(int i)
    {
        if (!CanPlay()) return;
        if (claimIds[i] != 0) return;
        if (MyPad() >= 0)
        {
            statusText.text = "You already have a pad. Let someone else take this one.";
            return;
        }
        TakeOwner();
        claimIds[i] = Networking.LocalPlayer.playerId;
        votes[i] = -1;
        Save();
    }

    void Vote(int o)
    {
        if (!CanPlay()) return;
        int pad = MyPad();
        if (pad < 0)
        {
            statusText.text = "Claim a pad first to read your part of the setup.";
            return;
        }
        if (!AllClaimed())
        {
            statusText.text = "All three pads need someone on them before anyone votes.";
            return;
        }
        TakeOwner();
        votes[pad] = o;

        if (votes[0] >= 0 && votes[0] == votes[1] && votes[1] == votes[2])
        {
            lastAgreed = o;
            for (int i = 0; i < 3; i++) votes[i] = -1;
            Save();
            if (o == bank.GetCorrect(caseIndex)) tracker.Correct();
            else tracker.Wrong();
            RefreshView();
            return;
        }
        Save();
    }

    int MyPad()
    {
        int me = Networking.LocalPlayer.playerId;
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == me) return i;
        }
        return -1;
    }

    bool AllClaimed()
    {
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == 0) return false;
        }
        return true;
    }

    bool AllVoted()
    {
        for (int i = 0; i < 3; i++)
        {
            if (votes[i] < 0) return false;
        }
        return true;
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

    void LoadCase()
    {
        if (bank == null) return;
        loadedCase = caseIndex;
        if (titleText != null) titleText.text = bank.GetTitle(caseIndex);
        if (questionText != null) questionText.text = bank.GetQuestion(caseIndex);
        if (tracker != null) tracker.SetTexts(bank.GetHint1(caseIndex), bank.GetHint2(caseIndex), bank.GetCorrectText(caseIndex), bank.GetRevealText(caseIndex));
        RefreshView();
    }

    public void RefreshView()
    {
        if (bank == null) return;
        bool done = tracker != null && tracker.IsDone();
        int me = Networking.LocalPlayer != null ? Networking.LocalPlayer.playerId : -1;

        for (int i = 0; i < optionTexts.Length && i < 4; i++)
        {
            if (optionTexts[i] == null) continue;
            string t = letters[i] + ". " + bank.GetOption(caseIndex, i);
            if (done && i == bank.GetCorrect(caseIndex)) t = "<b><color=#33CCFF>" + t + "</color></b>";
            optionTexts[i].text = t;
        }

        for (int i = 0; i < padTexts.Length && i < 3; i++)
        {
            if (padTexts[i] == null) continue;
            if (claimIds[i] == 0)
            {
                padTexts[i].text = "Pad " + (i + 1) + "\nFree. Claim it to read one part of the setup.";
            }
            else if (claimIds[i] == me || done)
            {
                string v = votes[i] >= 0 ? letters[votes[i]] : "none yet";
                padTexts[i].text = "Pad " + (i + 1) + "\n" + bank.GetDetail(caseIndex, i) + "\nYour vote: " + v;
            }
            else
            {
                string v = votes[i] >= 0 ? "vote locked in" : "no vote yet";
                padTexts[i].text = "Pad " + (i + 1) + "\nHeld by " + NameOf(claimIds[i]) + ", " + v + ".\nAsk them what their setup says.";
            }
        }

        if (statusText == null) return;
        if (zone != null && !zone.HasGroup()) statusText.text = zone.WaitText();
        else if (done) statusText.text = "Experiment done. Press Next Experiment to load another.";
        else if (!AllClaimed()) statusText.text = "Each of you claim a pad, read your part aloud, then predict together.";
        else if (AllVoted()) statusText.text = "Not agreed yet. All three votes have to match. Talk it through and change your votes.";
        else if (lastAgreed >= 0) statusText.text = "Your group agreed on " + letters[lastAgreed] + ". Not quite. Vote again once you agree.";
        else statusText.text = "Agree on a prediction. It only counts when all three pads vote the same.";
    }

    public override void OnDeserialization()
    {
        if (caseIndex != loadedCase) LoadCase();
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
                votes[i] = -1;
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
