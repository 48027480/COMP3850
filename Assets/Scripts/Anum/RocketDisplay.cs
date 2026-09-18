using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class RocketDisplay : UdonSharpBehaviour
{
    public RocketSystem[] rockets;      
    public string[] rocketLabels;       
    public TMP_Text statusText;
    public float refreshInterval = 1f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            RefreshDisplay();
        }
    }

    void RefreshDisplay()
    {
        if (statusText == null)
        {
            Debug.LogWarning(gameObject.name + ": RocketDisplay is missing its Status Text reference.");
            return;
        }

        string result = "";
        for (int i = 0; i < rockets.Length; i++)
        {
            if (rockets[i] == null) continue;

            string label = (i < rocketLabels.Length) ? rocketLabels[i] : ("Rocket " + (i + 1));
            string status = rockets[i].isLaunching
                ? "In flight"
                : rockets[i].roomOccupancy + " / " + rockets[i].seatCapacity + " boarded";

            result += label + ": " + status + "\n";
        }
        statusText.text = result;
    }
}