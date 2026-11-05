
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using TMPro;

public class StarChartManager : UdonSharpBehaviour
{
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI planetProgressText;
    public TextMeshProUGUI activityProgressText;
    public GameObject starChartCanvas;
    public GameObject starChartButton;

    private const string EarthKey = "centaurus_earth";
    private const string AstronomyKey = "centaurus_astronomy";
    private const string TechnologyKey = "centaurus_technology";
    private const string ArchaeologyKey = "centaurus_archaeology";

    private const string JigsawKey = "centaurus_jigsaw";
    private const string SequencingKey = "centaurus_sequencing";
    private const string CompareKey = "centaurus_compare";
    private const string IdentifyKey = "centaurus_identify";
    private const string DebateKey = "centaurus_debate";
    private const string PairedKey = "centaurus_paired";

    [HideInInspector] public bool dataReady;

    [HideInInspector] public bool visitedEarth;
    [HideInInspector] public bool visitedAstronomy;
    [HideInInspector] public bool visitedTechnology;
    [HideInInspector] public bool visitedArchaeology;

    [HideInInspector] public bool completedJigsaw;
    [HideInInspector] public bool completedSequencing;
    [HideInInspector] public bool completedCompare;
    [HideInInspector] public bool completedIdentify;
    [HideInInspector] public bool completedDebate;
    [HideInInspector] public bool completedPaired;

    private void PositionChartInFrontOfPlayer()
    {
         VRCPlayerApi player = Networking.LocalPlayer;

        if (!Utilities.IsValid(player) || starChartCanvas == null)
            return;

        VRCPlayerApi.TrackingData head =
            player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        Vector3 forward = head.rotation * Vector3.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 targetPosition =
            head.position + forward * 1.5f + Vector3.down * 0.05f;

        starChartCanvas.transform.position = targetPosition;

        // The chart's front faces the player.
        starChartCanvas.transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);
    }

    

    public void ToggleChart()
{
    if (starChartCanvas == null)
        return;

    bool shouldOpen = !starChartCanvas.activeSelf;

    if (shouldOpen)
        PositionChartInFrontOfPlayer();

    starChartCanvas.SetActive(shouldOpen);

    if (starChartButton != null)
        starChartButton.SetActive(!shouldOpen);
}

public void CloseChart()
{
    if (starChartCanvas != null)
        starChartCanvas.SetActive(false);

    if (starChartButton != null)
        starChartButton.SetActive(true);
}

    private string FormatAchievement(bool completed, string name)
    {
        if (completed)
        {
            return "<color=#71F5C4>●  " + name + "</color>\n";
        }

        return "<color=#8299B3>○  " + name + "</color>\n";
    }

    private void RefreshChart()
    {
        if (!dataReady)
        {
            if (progressText != null)
                progressText.text = "Loading your personal Star Chart...";

            if (planetProgressText != null)
                planetProgressText.text = "Loading planet discoveries...";

            if (activityProgressText != null)
                activityProgressText.text = "Loading activity achievements...";

            return;
        }

        string planets =
            "<color=#72D8FF><b>PLANETS EXPLORED</b></color>\n\n" +
            FormatAchievement(visitedEarth, "Earth") +
            FormatAchievement(visitedAstronomy, "Astronomy Planet") +
            FormatAchievement(visitedTechnology, "Technology Planet") +
            FormatAchievement(visitedArchaeology, "Archaeology Planet");

        string activities =
            "<color=#72D8FF><b>ACTIVITIES COMPLETED</b></color>\n\n" +
            FormatAchievement(completedJigsaw, "Jigsaw") +
            FormatAchievement(completedSequencing, "Sequencing") +
            FormatAchievement(completedCompare, "Compare") +
            FormatAchievement(completedIdentify, "Identify") +
            FormatAchievement(completedDebate, "Debate") +
            FormatAchievement(completedPaired, "Paired Discussion");

        if (planetProgressText != null)
            planetProgressText.text = planets;

        if (activityProgressText != null)
            activityProgressText.text = activities;

        // Retain the original combined display as a fallback.
        if (progressText != null)
            progressText.text = planets + "\n\n" + activities;
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || !player.isLocal)
            return;
        

        visitedEarth = PlayerData.GetBool(player, EarthKey);
        visitedAstronomy = PlayerData.GetBool(player, AstronomyKey);
        visitedTechnology = PlayerData.GetBool(player, TechnologyKey);
        visitedArchaeology = PlayerData.GetBool(player, ArchaeologyKey);

        completedJigsaw = PlayerData.GetBool(player, JigsawKey);
        completedSequencing = PlayerData.GetBool(player, SequencingKey);
        completedCompare = PlayerData.GetBool(player, CompareKey);
        completedIdentify = PlayerData.GetBool(player, IdentifyKey);
        completedDebate = PlayerData.GetBool(player, DebateKey);
        completedPaired = PlayerData.GetBool(player, PairedKey);

        dataReady = true;

        RecordEarthVisit();
        RefreshChart();

        Debug.Log("[StarChart] Personal progress restored.");
    }

    public void RecordEarthVisit()
    {
        if (!dataReady || visitedEarth) return;

        visitedEarth = true;
        PlayerData.SetBool(EarthKey, true);
        RefreshChart();
    }

    public void RecordAstronomyVisit()
    {
        if (!dataReady || visitedAstronomy) return;

        visitedAstronomy = true;
        PlayerData.SetBool(AstronomyKey, true);
        RefreshChart();
    }

    public void RecordTechnologyVisit()
    {
        if (!dataReady || visitedTechnology) return;

        visitedTechnology = true;
        PlayerData.SetBool(TechnologyKey, true);
        RefreshChart();
    }

    public void RecordArchaeologyVisit()
    {
        if (!dataReady || visitedArchaeology) return;

        visitedArchaeology = true;
        PlayerData.SetBool(ArchaeologyKey, true);
        RefreshChart();
    }

    public void CompleteJigsaw()
    {
        if (!dataReady || completedJigsaw) return;

        completedJigsaw = true;
        PlayerData.SetBool(JigsawKey, true);
        RefreshChart();
    }

    public void CompleteSequencing()
    {
        if (!dataReady || completedSequencing) return;

        completedSequencing = true;
        PlayerData.SetBool(SequencingKey, true);
        RefreshChart();
    }

    public void CompleteCompare()
    {
        if (!dataReady || completedCompare) return;

        completedCompare = true;
        PlayerData.SetBool(CompareKey, true);
        RefreshChart();
    }

    public void CompleteIdentify()
    {
        if (!dataReady || completedIdentify) return;

        completedIdentify = true;
        PlayerData.SetBool(IdentifyKey, true);
        RefreshChart();
    }

    public void CompleteDebate()
    {
        if (!dataReady || completedDebate) return;

        completedDebate = true;
        PlayerData.SetBool(DebateKey, true);
        RefreshChart();
    }

    public void CompletePairedDiscussion()
    {
        if (!dataReady || completedPaired) return;

        completedPaired = true;
        PlayerData.SetBool(PairedKey, true);
        RefreshChart();
    }



}
