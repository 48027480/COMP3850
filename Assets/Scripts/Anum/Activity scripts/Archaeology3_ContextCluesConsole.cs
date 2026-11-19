using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Archaeology3_ContextCluesConsole : UdonSharpBehaviour
{
    public Archaeology3_ContextCluesBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public Archaeology3_ContextCluesStand[] stands;
    public TextMeshProUGUI[] optionTexts;
    public GameObject[] findModels;
    public TextMeshProUGUI artifactText;
    public TextMeshProUGUI resultText;

    [UdonSynced] int puzzle = -1;
    [UdonSynced] bool[] analysed = new bool[4];

    int loadedPuzzle = -2;

    void Start()
    {
        if (bank == null) return;

        if (Networking.IsOwner(gameObject) && puzzle < 0)
        {
            puzzle = PickNewPuzzle(-1);
            RequestSerialization();
        }

        LoadPuzzle();
        UpdateView();
    }

    void LoadPuzzle()
    {
        if (puzzle < 0) return;
        if (puzzle == loadedPuzzle) return;
        loadedPuzzle = puzzle;

        for (int i = 0; i < stands.Length; i++)
        {
            stands[i].SetClue(bank.standNames[puzzle * 4 + i], bank.surfaceClues[puzzle * 4 + i], bank.labClues[puzzle * 4 + i]);
            stands[i].ResetStand();
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = bank.standNames[puzzle * 4 + i];
        }

        for (int i = 0; i < findModels.Length; i++)
        {
            if (findModels[i] != null) findModels[i].SetActive(i == puzzle);
        }

        if (tracker != null)
        {
            tracker.SetTexts(bank.hint1[puzzle], bank.hint2[puzzle], "Correct! It is " + bank.artifactNames[puzzle] + ".", "The answer is " + bank.standNames[puzzle * 4 + bank.correctStand[puzzle]] + ".");
        }
    }

    void UpdateView()
    {
        if (puzzle < 0) return;

        for (int i = 0; i < stands.Length; i++)
        {
            stands[i].SetAnalysed(analysed[i]);
        }

        if (tracker != null && tracker.IsDone())
        {
            artifactText.text = bank.artifactNames[puzzle] + "\n\n" + bank.findText[puzzle];
            resultText.text = bank.artifactNames[puzzle] + ". " + bank.facts[puzzle];
        }
        else
        {
            artifactText.text = "The find\n\n" + bank.findText[puzzle];
            if (CountAnalysed() < 2) resultText.text = "Read each stand, then send at least two to the lab before you choose.";
            else resultText.text = "Talk it over, then press the plaque for the place this find came from.";
        }

        if (!HasGroup() && (tracker == null || !tracker.IsDone()))
        {
            resultText.text = zone.WaitText();
        }
    }

    public bool HasGroup()
    {
        if (zone == null) return true;
        return zone.HasGroup();
    }

    public bool NoGroup()
    {
        if (HasGroup()) return false;
        resultText.text = zone.WaitText();
        return true;
    }

    int CountAnalysed()
    {
        int count = 0;
        for (int i = 0; i < analysed.Length; i++)
        {
            if (analysed[i]) count++;
        }
        return count;
    }

    public void MarkAnalysed(int index)
    {
        if (puzzle < 0) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        analysed[index] = true;
        RequestSerialization();
        UpdateView();
    }

    public void PickStand(int index)
    {
        if (tracker == null) return;
        if (tracker.IsDone()) return;
        if (NoGroup()) return;

        if (CountAnalysed() < 2)
        {
            resultText.text = "Send at least two stands to the lab before you choose.";
            return;
        }

        if (index == bank.correctStand[puzzle])
        {
            tracker.Correct();
        }
        else
        {
            tracker.Wrong();
        }
        UpdateView();
        if (!tracker.IsDone())
        {
            resultText.text = "Not quite. Look over the lab results again.";
        }
    }

    public void RefreshView()
    {
        UpdateView();
    }

    int PickNewPuzzle(int lastPuzzle)
    {
        int count = bank.GetCount();
        if (count <= 1) return 0;

        int next = Random.Range(0, count);
        while (next == lastPuzzle)
        {
            next = Random.Range(0, count);
        }
        return next;
    }

    public void ResetActivity()
    {
        if (bank == null) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        puzzle = PickNewPuzzle(puzzle);
        for (int i = 0; i < analysed.Length; i++)
        {
            analysed[i] = false;
        }
        RequestSerialization();

        LoadPuzzle();
        if (tracker != null)
        {
            tracker.ResetTracker();
        }
        UpdateView();
    }

    public override void OnDeserialization()
    {
        LoadPuzzle();
        UpdateView();
    }
}
