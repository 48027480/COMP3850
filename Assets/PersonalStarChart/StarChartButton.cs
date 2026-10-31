
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class StarChartButton : UdonSharpBehaviour
{
    public StarChartManager starChartManager;

    
        public override void Interact()
    {
        Debug.Log("[StarChart] Button clicked!");

        if (starChartManager == null)
        {
            Debug.LogError("[StarChart] Manager is missing!");
            return;
        }

        Debug.Log("[StarChart] Calling ToggleChart");
        starChartManager.ToggleChart();
    }

}
