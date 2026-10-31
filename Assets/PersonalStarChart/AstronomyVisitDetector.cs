
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class AstronomyVisitDetector : UdonSharpBehaviour
{
    public StarChartManager starChartManager;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || !player.isLocal) return;
        TryRecordVisit();
    }

    public void TryRecordVisit()
    {
        if (starChartManager == null) return;

        if (starChartManager.dataReady)
        {
            starChartManager.RecordAstronomyVisit();
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(TryRecordVisit), 1f);
        }
    }
}
