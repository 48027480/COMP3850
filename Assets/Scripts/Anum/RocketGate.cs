using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RocketGate : UdonSharpBehaviour
{
    public Transform doorTransform;
    public Vector3 pivotPoint;
    public Vector3 rotationAxis = Vector3.up;
    public float openAngle = 100f;
    public float speed = 90f;
    public float closeDelay = 1f;

    private int playersInZone = 0;
    private bool isOpen = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Update()
    {
        if (Mathf.Abs(currentAngle - targetAngle) > 0.01f)
        {
            float step = speed * Time.deltaTime;
            float newAngle = Mathf.MoveTowards(currentAngle, targetAngle, step);
            float delta = newAngle - currentAngle;

            doorTransform.RotateAround(pivotPoint, rotationAxis, delta);
            currentAngle = newAngle;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        playersInZone = playersInZone + 1;
        if (!isOpen)
        {
            isOpen = true;
            targetAngle = openAngle;
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        playersInZone = playersInZone - 1;
        if (playersInZone <= 0)
        {
            playersInZone = 0;
            SendCustomEventDelayedSeconds(nameof(CloseGateIfEmpty), closeDelay);
        }
    }

    public void CloseGateIfEmpty()
    {
        if (playersInZone <= 0)
        {
            isOpen = false;
            targetAngle = 0f;
        }
    }
}