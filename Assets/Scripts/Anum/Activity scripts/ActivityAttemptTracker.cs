using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ActivityAttemptTracker : UdonSharpBehaviour
{
    public int maxAttempts = 3;
    public TextMeshProUGUI guideText;

    [TextArea] public string introText = "Read out your clues to each other and choose an answer as a group.";
    [TextArea] public string hint1 = "Not quite. Have another chat about it.";
    [TextArea] public string hint2 = "Still not right. Have another chat about it.";
    [TextArea] public string correctText = "Correct!";
    [TextArea] public string revealText = "Here is the answer.";

    public UdonBehaviour gate;
    public string gateEvent = "OnActivityCompleted";

    [UdonSynced] int wrong;
    [UdonSynced] bool done;
    [UdonSynced] bool gotItRight;

    bool sentEvent;

    void Start()
    {
        UpdateText();
    }

    public bool IsDone()
    {
        return done;
    }

    public void SetTexts(string h1, string h2, string right, string reveal)
    {
        hint1 = h1;
        hint2 = h2;
        correctText = right;
        revealText = reveal;
        UpdateText();
    }

    public void Correct()
    {
        if (done) return;
        MakeOwner();
        done = true;
        gotItRight = true;
        Send();
    }

    public void Wrong()
    {
        if (done) return;
        MakeOwner();
        wrong++;
        if (wrong >= maxAttempts)
        {
            done = true;
            gotItRight = false;
        }
        Send();
    }

    public void ResetTracker()
    {
        MakeOwner();
        wrong = 0;
        done = false;
        gotItRight = false;
        Send();
    }

    public override void OnDeserialization()
    {
        Refresh();
    }

    void MakeOwner()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    void Send()
    {
        RequestSerialization();
        Refresh();
    }

    void Refresh()
    {
        if (!done) sentEvent = false;

        UpdateText();

        if (done && !sentEvent)
        {
            sentEvent = true;
            if (gate != null) gate.SendCustomEvent(gateEvent);
        }
    }

    void UpdateText()
    {
        if (guideText == null) return;

        if (done)
        {
            if (gotItRight) guideText.text = correctText;
            else guideText.text = revealText;
        }
        else if (wrong == 0) guideText.text = introText;
        else if (wrong == 1) guideText.text = hint1;
        else guideText.text = hint2;
    }
}