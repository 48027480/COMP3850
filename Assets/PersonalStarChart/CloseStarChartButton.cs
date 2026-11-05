using UdonSharp;
using UnityEngine;

public class CloseStarChartButton : UdonSharpBehaviour
{
    public StarChartManager starChartManager;

    public override void Interact()
    {
        if (starChartManager != null)
        {
            starChartManager.CloseChart();
        }
    }
}