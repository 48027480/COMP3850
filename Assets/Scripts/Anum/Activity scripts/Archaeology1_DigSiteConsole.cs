using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Archaeology1_DigSiteConsole : UdonSharpBehaviour
{
    public Archaeology1_DigSiteBank bank;
    public ActivityAttemptTracker tracker;
    public Archaeology_GroupZone zone;

    public Archaeology1_DigSiteTile[] tiles;
    public GameObject[] tileGlows;
    public GameObject digPitObject;
    public GameObject toolChoice;
    public GameObject[] soilLayers;
    public GameObject[] findModels;

    public VRC_Pickup surveyMeter;
    public TextMeshProUGUI meterText;

    public TextMeshProUGUI statusText;
    public TextMeshProUGUI findText;
    public GameObject[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    [UdonSynced] int puzzle = -1;
    [UdonSynced] bool hotspotFound = false;
    [UdonSynced] int sweeps = 0;
    [UdonSynced] bool fullyFound = false;
    [UdonSynced] bool toolsShown = false;
    [UdonSynced] bool brushChosen = false;
    [UdonSynced] int lastSurvey = -1;
    [UdonSynced] bool[] surveyed = new bool[4];

    int loadedPuzzle = -2;
    int[] optionSite = new int[4];

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

        BuildOptions();

        if (tracker != null)
        {
            tracker.SetTexts(bank.hint1[puzzle], bank.hint2[puzzle], "Correct! It is " + bank.siteNames[puzzle] + ".", "The answer is " + bank.siteNames[puzzle] + ".");
        }
    }

    void BuildOptions()
    {
        int count = bank.GetCount();
        int correctSlot = (puzzle * 3 + 1) % 4;
        int other = puzzle;

        for (int i = 0; i < 4; i++)
        {
            if (i == correctSlot)
            {
                optionSite[i] = puzzle;
            }
            else
            {
                other = (other + 1) % count;
                optionSite[i] = other;
            }
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = bank.siteNames[optionSite[i]];
        }
    }

    void UpdateView()
    {
        if (puzzle < 0) return;

        MovePitToSpot();
        if (digPitObject != null) digPitObject.SetActive(hotspotFound);
        if (toolChoice != null)
        {
            bool showTools = hotspotFound && toolsShown && !brushChosen;
            toolChoice.SetActive(showTools);
            if (showTools) TurnToolsToPlayer();
        }
        SetOptionsVisible(fullyFound);

        for (int i = 0; i < tileGlows.Length; i++)
        {
            if (tileGlows[i] != null) tileGlows[i].SetActive(hotspotFound && i == bank.hotspotTile[puzzle]);
        }

        for (int i = 0; i < soilLayers.Length; i++)
        {
            if (soilLayers[i] != null) soilLayers[i].SetActive(sweeps <= i);
        }

        for (int i = 0; i < findModels.Length; i++)
        {
            if (findModels[i] != null) findModels[i].SetActive(i == puzzle);
        }

        if (meterText != null)
        {
            if (lastSurvey < 0) meterText.text = "Stand over a numbered spot and press Use.";
            else meterText.text = "Spot " + (lastSurvey + 1) + "\n" + bank.surveyText[puzzle * 4 + lastSurvey];
        }

        if (tracker != null && tracker.IsDone())
        {
            statusText.text = bank.siteNames[puzzle] + ". " + bank.facts[puzzle];
            findText.text = bank.foundText[puzzle];
        }
        else if (!hotspotFound)
        {
            if (lastSurvey < 0) statusText.text = "Hold the meter over a numbered spot and press Use to survey it.";
            else statusText.text = "Meter, spot " + (lastSurvey + 1) + ": " + bank.surveyText[puzzle * 4 + lastSurvey] + " Then use the best spot to dig.";
            findText.text = "";
        }
        else if (!fullyFound)
        {
            findText.text = "";
            if (!toolsShown) statusText.text = "This spot looks promising. Use the glowing spot to start digging.";
            else if (!brushChosen) statusText.text = "Pick a tool to dig with.";
            else if (sweeps <= 0) statusText.text = "Good choice. Use the glowing spot to brush away the soil.";
            else if (sweeps == 1) statusText.text = bank.stage1Text[puzzle];
            else statusText.text = bank.stage2Text[puzzle];
        }
        else
        {
            statusText.text = "Fully uncovered. What have you found?";
            findText.text = bank.foundText[puzzle];
        }

        if (!HasGroup() && (tracker == null || !tracker.IsDone()))
        {
            statusText.text = zone.WaitText();
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
        statusText.text = zone.WaitText();
        if (meterText != null) meterText.text = zone.WaitText();
        return true;
    }

    void MovePitToSpot()
    {
        if (digPitObject == null || tiles == null) return;

        int spot = bank.hotspotTile[puzzle];
        if (spot >= tiles.Length || tiles[spot] == null) return;

        Vector3 pos = tiles[spot].transform.position;
        pos.y = digPitObject.transform.position.y;
        digPitObject.transform.position = pos;
    }

    void TurnToolsToPlayer()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (player == null) return;

        Vector3 away = toolChoice.transform.position - player.GetPosition();
        away.y = 0f;
        if (away.magnitude < 0.01f) return;
        toolChoice.transform.rotation = Quaternion.LookRotation(away);
    }

    void SetOptionsVisible(bool show)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null) optionButtons[i].SetActive(show);
        }
    }

    public bool IsHoldingMeter()
    {
        if (surveyMeter == null) return false;
        if (!surveyMeter.IsHeld) return false;
        VRCPlayerApi holder = surveyMeter.currentPlayer;
        if (holder == null) return false;
        return holder.isLocal;
    }

    public void MeterTooFar()
    {
        if (meterText != null) meterText.text = "Too far away.\nStand over a numbered spot and press Use.";
        if (statusText != null) statusText.text = "The meter is too far away. Stand over a numbered spot and press Use.";
    }

    public void SurveyTile(int tileIndex)
    {
        if (puzzle < 0) return;
        if (NoGroup()) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        lastSurvey = tileIndex;
        surveyed[tileIndex] = true;
        RequestSerialization();
        UpdateView();
    }

    public void ScanTile(int tileIndex)
    {
        if (puzzle < 0) return;
        if (NoGroup()) return;
        if (tracker != null && tracker.IsDone()) return;

        if (hotspotFound)
        {
            if (tileIndex == bank.hotspotTile[puzzle]) Excavate();
            else statusText.text = "Use the glowing spot to keep digging.";
            return;
        }

        if (!surveyed[tileIndex])
        {
            statusText.text = "Survey spot " + (tileIndex + 1) + " with the meter before you dig.";
            return;
        }

        if (tileIndex == bank.hotspotTile[puzzle])
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            hotspotFound = true;
            RequestSerialization();
            UpdateView();
        }
        else
        {
            statusText.text = "Nothing here, try another spot.";
        }
    }

    public void Excavate()
    {
        if (!hotspotFound) return;
        if (fullyFound) return;
        if (NoGroup()) return;

        if (!toolsShown)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            toolsShown = true;
            RequestSerialization();
            UpdateView();
            return;
        }

        if (!brushChosen)
        {
            statusText.text = "Pick a tool to dig with.";
            return;
        }

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        sweeps = sweeps + 1;
        if (sweeps >= 3) fullyFound = true;
        RequestSerialization();
        UpdateView();
    }

    public void PickBrush()
    {
        if (!hotspotFound) return;
        if (brushChosen) return;
        if (NoGroup()) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        brushChosen = true;
        RequestSerialization();
        UpdateView();
    }

    public void PickTrowel()
    {
        if (!hotspotFound) return;
        if (brushChosen) return;
        if (NoGroup()) return;

        statusText.text = bank.trowelText[puzzle];
    }

    public void PickOption0() { PickOption(0); }
    public void PickOption1() { PickOption(1); }
    public void PickOption2() { PickOption(2); }
    public void PickOption3() { PickOption(3); }

    void PickOption(int index)
    {
        if (tracker == null) return;
        if (tracker.IsDone()) return;
        if (!fullyFound) return;
        if (NoGroup()) return;

        if (optionSite[index] == puzzle)
        {
            tracker.Correct();
        }
        else
        {
            tracker.Wrong();
            if (!tracker.IsDone())
            {
                statusText.text = "Not quite. Look at the find again and try another answer.";
            }
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
        hotspotFound = false;
        sweeps = 0;
        fullyFound = false;
        toolsShown = false;
        brushChosen = false;
        lastSurvey = -1;
        for (int i = 0; i < surveyed.Length; i++)
        {
            surveyed[i] = false;
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
