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
    public string title = "DEPARTURES";

    private float timer;
    private float[] launchStart;

    void Start()
    {
        launchStart = new float[rockets.Length];
        for (int i = 0; i < launchStart.Length; i++) launchStart[i] = -1f;
    }

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
        if (launchStart == null || launchStart.Length != rockets.Length) Start();

        string result = "<b>" + title + "</b>\n";
        bool arrivals = title == "ARRIVALS";
        for (int i = 0; i < rockets.Length; i++)
        {
            RocketSystem r = rockets[i];
            if (r == null) continue;

            string label = (i < rocketLabels.Length) ? rocketLabels[i] : ("Rocket " + (i + 1));
            string status;
            if (r.flying)
            {
                if (launchStart[i] < 0f) launchStart[i] = Time.time;
                float left = r.flightDurationSeconds - (Time.time - launchStart[i]);
                status = left > 0f ? "<color=#FF8A3D>In flight</color>  " + (arrivals ? "lands in " : "back in ") + Clock(left) : "<color=#FF8A3D>Landing</color>";
            }
            else
            {
                launchStart[i] = -1f;
                status = r.seatedCount >= r.seatCapacity ? "<color=#FFB74D>Full</color>" : arrivals ? "<color=#7CD992>Boarding on Earth</color>" : "<color=#7CD992>Boarding</color>";
            }

            result += label + "    " + r.seatedCount + " / " + r.seatCapacity + " seats    " + status + "\n";
        }
        statusText.text = result;
    }

    string Clock(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        int m = s / 60;
        s = s % 60;
        return m + ":" + (s < 10 ? "0" : "") + s;
    }
}
