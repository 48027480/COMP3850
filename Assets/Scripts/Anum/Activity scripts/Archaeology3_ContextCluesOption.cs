using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Archaeology3_ContextCluesOption : UdonSharpBehaviour
{
    public Archaeology3_ContextCluesConsole console;
    public int standIndex;

    public override void Interact()
    {
        console.PickStand(standIndex);
    }
}
