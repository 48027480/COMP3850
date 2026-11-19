using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Archaeology2_DecodeConsole : UdonSharpBehaviour
{
    public Archaeology2_DecodeBank bank;
    public ActivityAttemptTracker tracker1;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public TextMeshProUGUI keyText;
    public TextMeshProUGUI encodedText;
    public TextMeshProUGUI encodedText2;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI partText;
    public TextMeshProUGUI[] optionTexts;
    public TextMeshProUGUI[] cardTexts;

    public Color readyColor = new Color(0.17f, 0.11f, 0.06f);
    public Color waitingColor = new Color(0.17f, 0.11f, 0.06f, 0.3f);

    [UdonSynced] int puzzle = -1;
    [UdonSynced] bool scriptFound = false;

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

        for (int i = 0; i < cardTexts.Length; i++)
        {
            cardTexts[i].text = bank.cardText[puzzle * 4 + i];
        }

        if (tracker1 != null)
        {
            tracker1.SetTexts(bank.seg1Hint1[puzzle], bank.seg1Hint2[puzzle], bank.seg1Meaning[puzzle], bank.seg1Meaning[puzzle]);
        }
        if (tracker != null)
        {
            tracker.SetTexts(bank.seg2Hint1[puzzle], bank.seg2Hint2[puzzle], bank.seg2Meaning[puzzle], bank.seg2Meaning[puzzle]);
        }
    }

    int CurrentPart()
    {
        if (tracker1 != null && tracker1.IsDone()) return 2;
        return 1;
    }

    void UpdateView()
    {
        if (puzzle < 0) return;

        if (!scriptFound)
        {
            keyText.text = "Which script is this?\n\nCompare the cards on the lectern with the signs on the message, then pick the matching card.";
            encodedText.text = bank.seg1Text[puzzle];
            encodedText2.text = bank.seg2Text[puzzle];
            encodedText2.color = waitingColor;
            partText.text = "Name the script first.";
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i].text = "?";
            }
            if (tracker == null || !tracker.IsDone()) resultText.text = "Start at the lectern by the door.";
            if (!HasGroup() && (tracker == null || !tracker.IsDone())) resultText.text = zone.WaitText();
            return;
        }

        keyText.text = bank.scriptNames[puzzle] + "\n\n" + bank.keyText[puzzle];
        encodedText.text = bank.seg1Text[puzzle];
        encodedText2.text = bank.seg2Text[puzzle];

        if (CurrentPart() == 1)
        {
            encodedText2.color = waitingColor;
            partText.text = "Part one: translate the short line above the seam.";
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i].text = bank.seg1Options[puzzle * 4 + i];
            }
        }
        else
        {
            encodedText2.color = readyColor;
            partText.text = "Part two: now translate the longer line below the seam.";
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i].text = bank.seg2Options[puzzle * 4 + i];
            }
        }

        if (tracker != null && tracker.IsDone())
        {
            partText.text = "Both parts read.";
            resultText.text = bank.scriptNames[puzzle] + ". " + bank.facts[puzzle];
        }
        else
        {
            resultText.text = "Use the key to work out the line, then touch the tablet with your answer.";
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

    bool NoGroup()
    {
        if (HasGroup()) return false;
        resultText.text = zone.WaitText();
        return true;
    }

    public void PickScript(int index)
    {
        if (puzzle < 0) return;
        if (scriptFound) return;
        if (NoGroup()) return;

        if (index == bank.cardCorrect[puzzle])
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            scriptFound = true;
            RequestSerialization();
            UpdateView();
        }
        else
        {
            resultText.text = "That's not quite this alphabet. Compare the shapes again.";
        }
    }

    public void PickOption(int index)
    {
        if (puzzle < 0) return;
        if (tracker == null || tracker1 == null) return;
        if (tracker.IsDone()) return;
        if (NoGroup()) return;

        if (!scriptFound)
        {
            resultText.text = "Name the script at the lectern first.";
            return;
        }

        if (CurrentPart() == 1)
        {
            if (index == bank.seg1Correct[puzzle]) tracker1.Correct();
            else tracker1.Wrong();
        }
        else
        {
            if (index == bank.seg2Correct[puzzle]) tracker.Correct();
            else tracker.Wrong();
        }
        UpdateView();
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
        scriptFound = false;
        RequestSerialization();

        LoadPuzzle();
        if (tracker1 != null)
        {
            tracker1.ResetTracker();
        }
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
