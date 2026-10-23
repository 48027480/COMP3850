using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class HoloSpin : UdonSharpBehaviour
{
    public Vector3 spin = new Vector3(0f, 30f, 0f);
    public float bobHeight = 0.03f;
    public float bobSpeed = 1f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        transform.Rotate(spin * Time.deltaTime, Space.Self);
        if (bobHeight > 0f)
        {
            transform.localPosition = startPos + new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);
        }
    }
}
