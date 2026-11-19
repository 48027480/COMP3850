using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class Archaeology3_ContextCluesStand : UdonSharpBehaviour
{
    public Archaeology3_ContextCluesConsole console;
    public int standIndex;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI clueText;
    public TextMeshProUGUI labText;
    public TextMeshProUGUI testingText;
    public GameObject clueDisplay;
    public GameObject labDisplay;

    bool shown = false;
    bool testing = false;
    bool analysed = false;
    int dots = 0;

    void Start()
    {
        ShowCards();
    }

    public void SetClue(string placeName, string surface, string lab)
    {
        nameText.text = placeName;
        clueText.text = surface;
        labText.text = lab;
    }

    public void ResetStand()
    {
        shown = false;
        testing = false;
        analysed = false;
        ShowCards();
    }

    public void SetAnalysed(bool value)
    {
        analysed = value;
        if (analysed) shown = true;
        ShowCards();
    }

    public override void Interact()
    {
        if (console != null && console.NoGroup()) return;

        if (!shown)
        {
            shown = true;
            ShowCards();
        }
        else if (!analysed && !testing)
        {
            RequestAnalysis();
        }
    }

    public void RequestAnalysis()
    {
        testing = true;
        dots = 0;
        ShowCards();
        SendCustomEventDelayedSeconds("TestStep", 0.5f);
    }

    public void TestStep()
    {
        if (!testing) return;

        dots = dots + 1;
        if (dots < 4)
        {
            ShowCards();
            SendCustomEventDelayedSeconds("TestStep", 0.5f);
        }
        else
        {
            testing = false;
            console.MarkAnalysed(standIndex);
        }
    }

    void ShowCards()
    {
        if (clueDisplay != null) clueDisplay.SetActive(shown);
        if (labDisplay != null) labDisplay.SetActive(analysed);

        if (testingText == null) return;

        if (testing)
        {
            string text = "Running tests";
            for (int i = 0; i < dots; i++)
            {
                text = text + ".";
            }
            testingText.text = text;
        }
        else if (analysed)
        {
            testingText.text = "";
        }
        else if (shown)
        {
            testingText.text = "Use again to send it to the lab.";
        }
        else
        {
            testingText.text = "Use to read the label.";
        }
    }
}
