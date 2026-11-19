using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech4_Kit : UdonSharpBehaviour
{
    public SciTech4_Slot[] slots;
    public TMP_Text[] slotLabels;
    public VRCObjectSync[] parts;
    public GameObject[] lockerDoors;
}
