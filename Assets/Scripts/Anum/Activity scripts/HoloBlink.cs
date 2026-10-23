using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class HoloBlink : UdonSharpBehaviour
{
    public GameObject target;
    public float onTime = 0.5f;
    public float offTime = 0.5f;

    float timer;
    bool showing = true;

    void Update()
    {
        if (target == null) return;

        timer += Time.deltaTime;
        if (showing && timer >= onTime)
        {
            showing = false;
            timer = 0f;
            target.SetActive(false);
        }
        else if (!showing && timer >= offTime)
        {
            showing = true;
            timer = 0f;
            target.SetActive(true);
        }
    }
}
