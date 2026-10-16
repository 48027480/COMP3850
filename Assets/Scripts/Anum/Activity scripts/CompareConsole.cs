using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CompareConsole : UdonSharpBehaviour
{
    public ComparePuzzleBank puzzleBank;
    public ActivityAttemptTracker tracker;
    public CompareObject objectA;
    public CompareObject objectB;
    public CompareBriefStation briefStation;
    public Transform answerSlot;
    public float slotRange = 0.3f;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] reasonTexts;
    public TextMeshProUGUI statusText;
    public bool needBrief = true;

    [UdonSynced] int puzzle = -1;
    [UdonSynced] int reason = -1;

    int loaded = -2;

    void Start()
    {
        if (puzzleBank == null) return;

        if (Networking.IsOwner(gameObject) && puzzle < 0)
        {
            puzzle = RandomPuzzle(-1);
            RequestSerialization();
        }

        LoadPuzzle();
        UpdateStatus("");
    }

    public void SelectReason0() { PickReason(0); }
    public void SelectReason1() { PickReason(1); }
    public void SelectReason2() { PickReason(2); }

    public void Submit()
    {
        if (puzzleBank == null || tracker == null || puzzle < 0) return;
        if (tracker.IsDone()) return;

        if (needBrief && briefStation != null && !briefStation.IsTaken())
        {
            UpdateStatus("Someone needs to take the briefing card first.");
            return;
        }

        int placed = PlacedObject();
        if (placed == -1)
        {
            UpdateStatus("Put one object on the answer pedestal first.");
            return;
        }
        if (placed == 2)
        {
            UpdateStatus("Only one object can go on the answer pedestal.");
            return;
        }
        if (reason < 0)
        {
            UpdateStatus("Pick a reason as well.");
            return;
        }

        bool right = placed == puzzleBank.correctObject[puzzle] && reason == puzzleBank.correctReason[puzzle];

        SetReason(-1);

        if (right)
        {
            tracker.Correct();
            UpdateStatus("You got it together.");
        }
        else
        {
            tracker.Wrong();
            if (tracker.IsDone()) UpdateStatus("The guide has revealed the answer. Press Next group for a new topic.");
            else UpdateStatus("Not quite. Check the guide's hint and talk it through.");
        }
    }

    public void ResetActivity()
    {
        if (puzzleBank == null) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        puzzle = RandomPuzzle(puzzle);
        reason = -1;
        RequestSerialization();

        if (objectA != null) objectA.ResetObject();
        if (objectB != null) objectB.ResetObject();
        if (briefStation != null) briefStation.ReleaseBrief();

        LoadPuzzle();
        if (tracker != null) tracker.ResetTracker();
        UpdateStatus("New topic loaded. Pick up an object to read it.");
    }

    public override void OnDeserialization()
    {
        LoadPuzzle();
        UpdateStatus("");
    }

    int RandomPuzzle(int last)
    {
        int count = puzzleBank.GetCount();
        if (count <= 1) return 0;

        int next = Random.Range(0, count);
        if (next == last) next = (next + 1 + Random.Range(0, count - 1)) % count;
        return next;
    }

    void LoadPuzzle()
    {
        if (puzzleBank == null || puzzle < 0 || puzzle == loaded) return;
        loaded = puzzle;

        if (questionText != null) questionText.text = puzzleBank.questions[puzzle];

        if (objectA != null) objectA.SetCard(puzzleBank.labelA[puzzle], puzzleBank.infoA[puzzle]);
        if (objectB != null) objectB.SetCard(puzzleBank.labelB[puzzle], puzzleBank.infoB[puzzle]);
        if (briefStation != null) briefStation.SetBrief(puzzleBank.briefs[puzzle]);

        if (tracker != null)
        {
            tracker.SetTexts(puzzleBank.hint1[puzzle], puzzleBank.hint2[puzzle], puzzleBank.correctText[puzzle], puzzleBank.revealText[puzzle]);
        }
    }

    void PickReason(int r)
    {
        if (tracker != null && tracker.IsDone()) return;
        SetReason(r);
        UpdateStatus("");
    }

    void SetReason(int r)
    {
        if (r > 2) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        reason = r;
        RequestSerialization();
    }

    int PlacedObject()
    {
        if (answerSlot == null) return -1;

        bool a = objectA != null && Vector3.Distance(objectA.transform.position, answerSlot.position) <= slotRange;
        bool b = objectB != null && Vector3.Distance(objectB.transform.position, answerSlot.position) <= slotRange;

        if (a && b) return 2;
        if (a) return 0;
        if (b) return 1;
        return -1;
    }

    void UpdateStatus(string msg)
    {
        if (puzzleBank != null && puzzle >= 0 && reasonTexts != null)
        {
            for (int i = 0; i < reasonTexts.Length && i < 3; i++)
            {
                if (reasonTexts[i] == null) continue;
                string r = puzzleBank.GetReason(puzzle, i);
                reasonTexts[i].text = i == reason ? "<b><color=#4FC3F7>" + r + "</color></b>" : r;
            }
        }

        if (statusText == null) return;

        string current = reason >= 0 ? "Reason picked. Place an object and press Submit." : "Place one object on the pedestal and pick a reason.";
        statusText.text = msg == "" ? current : current + "\n" + msg;
    }
}