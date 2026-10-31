
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class EarthVisitDetector : UdonSharpBehaviour
{
    public StarChartManager starChartManager;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || !player.isLocal)
            return;

        if (starChartManager == null)
            return;

        if (starChartManager.dataReady)
        {
            starChartManager.RecordEarthVisit();
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(TryRecordEarthVisit), 1f);
        }
    }
    public void TryRecordEarthVisit()
    {
        if (starChartManager == null) return;

        if (starChartManager.dataReady)
        {
            starChartManager.RecordEarthVisit();
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(TryRecordEarthVisit), 1f);
        }
    }


}
