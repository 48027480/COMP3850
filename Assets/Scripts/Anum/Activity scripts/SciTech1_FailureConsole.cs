using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SciTech1_FailureConsole : UdonSharpBehaviour
{
    public SciTech1_FailureBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public TMP_Text titleText;
    public TMP_Text summaryText;
    public TMP_Text[] evidenceTexts;
    public TMP_Text[] optionTexts;
    public TMP_Text statusText;

    [UdonSynced] int caseIndex = 0;
    [UdonSynced] int[] claimIds = new int[3];
    [UdonSynced] int picked = -1;

    int loadedCase = -1;

    void Start()
    {
        LoadCase();
    }

    public void Claim0() { Claim(0); }
    public void Claim1() { Claim(1); }
    public void Claim2() { Claim(2); }

    public void Pick0() { Pick(0); }
    public void Pick1() { Pick(1); }
    public void Pick2() { Pick(2); }
    public void Pick3() { Pick(3); }

    public void NextCase()
    {
        if (tracker != null && !tracker.IsDone()) return;
        TakeOwner();
        caseIndex = (caseIndex + 1) % bank.Count();
        for (int i = 0; i < 3; i++) claimIds[i] = 0;
        picked = -1;
        Save();
        if (tracker != null) tracker.ResetTracker();
        LoadCase();
    }

    void Claim(int i)
    {
        if (!CanPlay()) return;
        if (claimIds[i] != 0) return;
        int me = Networking.LocalPlayer.playerId;
        for (int k = 0; k < 3; k++)
        {
            if (claimIds[k] == me)
            {
                statusText.text = "You already opened a file. Let someone else open this one.";
                return;
            }
        }
        TakeOwner();
        claimIds[i] = me;
        Save();
    }

    void Pick(int i)
    {
        if (!CanPlay()) return;
        if (!AllOpen())
        {
            statusText.text = "Open all three evidence files first. Each one needs a different person.";
            return;
        }
        TakeOwner();
        picked = i;
        Save();
        if (i == bank.GetCorrect(caseIndex)) tracker.Correct();
        else tracker.Wrong();
        RefreshView();
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

    bool AllOpen()
    {
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] == 0) return false;
        }
        return true;
    }

    int OpenCount()
    {
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            if (claimIds[i] != 0) n++;
        }
        return n;
    }

    void LoadCase()
    {
        if (bank == null) return;
        loadedCase = caseIndex;
        if (titleText != null) titleText.text = bank.GetTitle(caseIndex);
        if (summaryText != null) summaryText.text = bank.GetSummary(caseIndex);
        if (tracker != null) tracker.SetTexts(bank.GetHint1(caseIndex), bank.GetHint2(caseIndex), bank.GetCorrectText(caseIndex), bank.GetRevealText(caseIndex));
        RefreshView();
    }

    public void RefreshView()
    {
        if (bank == null) return;
        bool done = tracker != null && tracker.IsDone();

        for (int i = 0; i < evidenceTexts.Length && i < 3; i++)
        {
            if (evidenceTexts[i] == null) continue;
            if (claimIds[i] != 0) evidenceTexts[i].text = "Evidence " + (i + 1) + "\n" + bank.GetEvidence(caseIndex, i);
            else evidenceTexts[i].text = "Evidence " + (i + 1) + "\nSealed. One person opens this file.";
        }

        for (int i = 0; i < optionTexts.Length && i < 4; i++)
        {
            if (optionTexts[i] == null) continue;
            string t = bank.GetOption(caseIndex, i);
            if (done && i == bank.GetCorrect(caseIndex)) t = "<b><color=#33CCFF>" + t + "</color></b>";
            else if (i == picked) t = "<b>" + t + "</b>";
            optionTexts[i].text = t;
        }

        if (statusText == null) return;
        if (zone != null && !zone.HasGroup()) statusText.text = zone.WaitText();
        else if (done) statusText.text = "Case closed. Press New Case to load another.";
        else if (!AllOpen()) statusText.text = "Evidence open: " + OpenCount() + "/3";
        else statusText.text = "All evidence is open. Talk it through, then choose the cause together.";
    }

    public override void OnDeserialization()
    {
        if (caseIndex != loadedCase) LoadCase();
        else RefreshView();
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
