using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SciTech4_FixConsole : UdonSharpBehaviour
{
    public SciTech4_FixBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;
    public SciTech4_Kit[] kits;

    public TMP_Text titleText;
    public TMP_Text briefText;
    public TMP_Text storyText;
    public TMP_Text statusText;

    [UdonSynced] int roundIndex = 0;
    [UdonSynced] int[] claimIds = new int[3];
    [UdonSynced] int lastRight = -1;

    int loadedRound = -1;

    void Start()
    {
        LoadRound();
    }

    public void Claim0() { Claim(0); }
    public void Claim1() { Claim(1); }
    public void Claim2() { Claim(2); }

    public void NextRound()
    {
        if (tracker != null && !tracker.IsDone()) return;
        ResetParts(roundIndex);
        TakeOwner();
        roundIndex = (roundIndex + 1) % bank.Count();
        for (int i = 0; i < 3; i++) claimIds[i] = 0;
        lastRight = -1;
        Save();
        if (tracker != null) tracker.ResetTracker();
        LoadRound();
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
                statusText.text = "You already opened a locker. Let someone else open this one.";
                return;
            }
        }
        TakeOwner();
        claimIds[i] = me;
        Save();
    }

    public void Check()
    {
        if (!CanPlay()) return;
        if (!AllOpen())
        {
            statusText.text = "Open all three lockers first. Each one needs a different person.";
            return;
        }
        int n = bank.GetSlotCount(roundIndex);
        int right = Score();
        TakeOwner();
        lastRight = right;
        Save();
        if (right == n) tracker.Correct();
        else tracker.Wrong();
        RefreshView();
    }

    int Score()
    {
        SciTech4_Kit kit = kits[roundIndex];
        int n = bank.GetSlotCount(roundIndex);
        int right = 0;

        if (!bank.IsAnyOrder(roundIndex))
        {
            for (int s = 0; s < n; s++)
            {
                if (kit.slots[s].GetPartType() == bank.GetSlotAnswer(roundIndex, s)) right++;
            }
            return right;
        }

        bool[] used = new bool[n];
        for (int s = 0; s < n; s++)
        {
            int t = kit.slots[s].GetPartType();
            for (int a = 0; a < n; a++)
            {
                if (!used[a] && bank.GetSlotAnswer(roundIndex, a) == t)
                {
                    used[a] = true;
                    right++;
                    break;
                }
            }
        }
        return right;
    }

    void ResetParts(int r)
    {
        if (kits == null || r >= kits.Length || kits[r] == null) return;
        SciTech4_Kit kit = kits[r];
        for (int i = 0; i < kit.parts.Length; i++)
        {
            if (kit.parts[i] == null) continue;
            Networking.SetOwner(Networking.LocalPlayer, kit.parts[i].gameObject);
            kit.parts[i].Respawn();
        }
        for (int s = 0; s < kit.slots.Length; s++)
        {
            if (kit.slots[s] != null) kit.slots[s].ClearSlot();
        }
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

    void LoadRound()
    {
        if (bank == null) return;
        loadedRound = roundIndex;
        if (titleText != null) titleText.text = bank.GetTitle(roundIndex);
        if (briefText != null) briefText.text = bank.GetBrief(roundIndex);
        if (tracker != null) tracker.SetTexts(bank.GetHint1(roundIndex), bank.GetHint2(roundIndex), bank.GetCorrectText(roundIndex), bank.GetRevealText(roundIndex));

        for (int k = 0; k < kits.Length; k++)
        {
            if (kits[k] == null) continue;
            for (int s = 0; s < kits[k].slots.Length; s++)
            {
                if (kits[k].slots[s] != null) kits[k].slots[s].ClearSlot();
            }
            kits[k].gameObject.SetActive(k == roundIndex);
        }

        SciTech4_Kit kit = kits[roundIndex];
        for (int s = 0; s < kit.slotLabels.Length; s++)
        {
            if (kit.slotLabels[s] != null) kit.slotLabels[s].text = bank.GetSlotLabel(roundIndex, s);
        }
        RefreshView();
    }

    public void RefreshView()
    {
        if (bank == null || kits == null || roundIndex >= kits.Length) return;
        bool done = tracker != null && tracker.IsDone();
        SciTech4_Kit kit = kits[roundIndex];

        for (int i = 0; i < kit.lockerDoors.Length && i < 3; i++)
        {
            if (kit.lockerDoors[i] != null) kit.lockerDoors[i].SetActive(claimIds[i] == 0);
        }

        if (storyText != null) storyText.text = done ? bank.GetStory(roundIndex) : "";

        if (statusText == null) return;
        if (zone != null && !zone.HasGroup()) statusText.text = zone.WaitText();
        else if (done) statusText.text = "Fix complete. Press Next Fix to load another.";
        else if (!AllOpen()) statusText.text = "Lockers open: " + OpenCount() + "/3. Each person opens one.";
        else if (lastRight >= 0) statusText.text = lastRight + " of " + bank.GetSlotCount(roundIndex) + " parts are right. Rethink it and check again.";
        else statusText.text = "Not every part belongs. Build the fix together, then press Check.";
    }

    public override void OnDeserialization()
    {
        if (roundIndex != loadedRound) LoadRound();
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
