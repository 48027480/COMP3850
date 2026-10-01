using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JigsawAnswerConsole : UdonSharpBehaviour
{
    public JigsawPuzzleBank puzzleBank;
    public ActivityAttemptTracker tracker;
    public JigsawPieceStation[] pieceStations;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] optionTexts;
    public TextMeshProUGUI statusText;
    public bool needAllClues = true;

    [UdonSynced] int puzzle = -1;
    [UdonSynced] int picked = -1;

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

    public void SelectOption0() { Pick(0); }
    public void SelectOption1() { Pick(1); }
    public void SelectOption2() { Pick(2); }
    public void SelectOption3() { Pick(3); }

    public void Submit()
    {
        if (puzzleBank == null || tracker == null || puzzle < 0) return;
        if (tracker.IsDone()) return;

        if (picked < 0)
        {
            UpdateStatus("You need to choose an answer first.");
            return;
        }

        if (needAllClues && !AllCluesTaken())
        {
            UpdateStatus("Wait until everyone has picked up a clue.");
            return;
        }

        if (picked == puzzleBank.correctOptionIndex[puzzle]) tracker.Correct();
        else tracker.Wrong();

        Pick(-1);
    }

    public void ResetActivity()
    {
        if (puzzleBank == null) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        puzzle = RandomPuzzle(puzzle);
        picked = -1;
        RequestSerialization();

        for (int i = 0; i < pieceStations.Length; i++)
        {
            if (pieceStations[i] != null) pieceStations[i].ReleasePiece();
        }

        LoadPuzzle();
        if (tracker != null) tracker.ResetTracker();
        UpdateStatus("");
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

        for (int i = 0; i < optionTexts.Length && i < 4; i++)
        {
            if (optionTexts[i] != null) optionTexts[i].text = puzzleBank.GetOption(puzzle, i);
        }

        if (pieceStations.Length >= 3)
        {
            pieceStations[0].SetPiece("Clue A", puzzleBank.pieceA[puzzle]);
            pieceStations[1].SetPiece("Clue B", puzzleBank.pieceB[puzzle]);
            pieceStations[2].SetPiece("Clue C", puzzleBank.pieceC[puzzle]);
        }

        if (tracker != null)
        {
            tracker.SetTexts(puzzleBank.hint1[puzzle], puzzleBank.hint2[puzzle], puzzleBank.correctText[puzzle], puzzleBank.revealText[puzzle]);
        }
    }

    void Pick(int option)
    {
        if (tracker != null && tracker.IsDone() && option != -1) return;
        if (option > 3) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        picked = option;
        RequestSerialization();
        UpdateStatus("");
    }

    bool AllCluesTaken()
    {
        for (int i = 0; i < pieceStations.Length; i++)
        {
            if (pieceStations[i] != null && !pieceStations[i].IsTaken()) return false;
        }
        return true;
    }

    void UpdateStatus(string msg)
    {
        string current = "Nothing chosen yet.";
        if (picked >= 0 && puzzleBank != null && puzzle >= 0)
            current = "Your group picked: " + puzzleBank.GetOption(puzzle, picked);

        if (statusText == null) { ShowPicked(); return; }

        if (msg == "") statusText.text = current;
        else statusText.text = current + "\n" + msg;

        ShowPicked();
    }

    void ShowPicked()
    {
        if (puzzleBank == null || puzzle < 0) return;

        for (int i = 0; i < optionTexts.Length && i < 4; i++)
        {
            if (optionTexts[i] == null) continue;

            string label = puzzleBank.GetOption(puzzle, i);
            Image bg = optionTexts[i].transform.parent.GetComponent<Image>();

            if (i == picked)
            {
                optionTexts[i].text = "<b>" + label + "</b>";
                if (bg != null) bg.color = new Color(0.55f, 0.75f, 1f);
            }
            else
            {
                optionTexts[i].text = label;
                if (bg != null) bg.color = Color.white;
            }
        }
    }
}