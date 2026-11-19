using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Archaeology1_DigSiteExcavateSpot : UdonSharpBehaviour
{
    public Archaeology1_DigSiteConsole console;
    public int tool = 0;

    public override void Interact()
    {
        if (tool == 1)
        {
            console.PickBrush();
        }
        else if (tool == 2)
        {
            console.PickTrowel();
        }
        else
        {
            console.Excavate();
        }
    }
}
