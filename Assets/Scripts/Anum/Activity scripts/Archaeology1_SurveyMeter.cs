using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class Archaeology1_SurveyMeter : UdonSharpBehaviour
{
    public VRCObjectSync objectSync;
    public Archaeology1_DigSiteConsole console;
    public Transform[] spots;
    public float range = 1.4f;

    public override void OnPickupUseDown()
    {
        if (console == null || spots == null) return;

        int best = -1;
        float bestDistance = range;
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i] == null) continue;
            Vector3 d = spots[i].position - transform.position;
            d.y = 0f;
            if (d.magnitude < bestDistance)
            {
                bestDistance = d.magnitude;
                best = i;
            }
        }

        if (best < 0)
        {
            console.MeterTooFar();
            return;
        }
        console.SurveyTile(best);
    }

    public override void OnDrop()
    {
        if (objectSync != null)
        {
            objectSync.Respawn();
        }
    }
}
