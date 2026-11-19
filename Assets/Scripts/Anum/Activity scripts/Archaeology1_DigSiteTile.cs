using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Archaeology1_DigSiteTile : UdonSharpBehaviour
{
    public Archaeology1_DigSiteConsole console;
    public int tileIndex;

    public override void Interact()
    {
        if (console.IsHoldingMeter())
        {
            console.SurveyTile(tileIndex);
        }
        else
        {
            console.ScanTile(tileIndex);
        }
    }
}
