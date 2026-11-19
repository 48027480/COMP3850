using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Archaeology2_DecodeOption : UdonSharpBehaviour
{
    public Archaeology2_DecodeConsole console;
    public int optionIndex;
    public bool isScriptCard = false;

    public override void Interact()
    {
        if (isScriptCard)
        {
            console.PickScript(optionIndex);
        }
        else
        {
            console.PickOption(optionIndex);
        }
    }
}
