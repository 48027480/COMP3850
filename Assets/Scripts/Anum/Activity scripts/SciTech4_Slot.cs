using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech4_Slot : UdonSharpBehaviour
{
    public GameObject glow;

    SciTech4_Part current;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        SciTech4_Part p = other.GetComponent<SciTech4_Part>();
        if (p == null) return;
        current = p;
        if (glow != null) glow.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        SciTech4_Part p = other.GetComponent<SciTech4_Part>();
        if (p == null) return;
        if (p == current)
        {
            current = null;
            if (glow != null) glow.SetActive(false);
        }
    }

    public int GetPartType()
    {
        if (current == null) return -1;
        if (!current.gameObject.activeInHierarchy) return -1;
        return current.partType;
    }

    public void ClearSlot()
    {
        current = null;
        if (glow != null) glow.SetActive(false);
    }
}
