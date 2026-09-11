using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class WelcomingText : UdonSharpBehaviour
{
    public GameObject welcomingUI;
    public TMP_Text welcomeMessage;
    public AudioSource welcomeAudioSource;
    public bool showWelcomingByDefault = true;

    public string placeholderMessage = "PLACEHOLDER: Welcome to Centaurus!";

    void Start()
    {
        welcomingUI.SetActive(showWelcomingByDefault);
        welcomeMessage.text = placeholderMessage;

        if (showWelcomingByDefault)
            welcomeAudioSource.Play();
    }

    public void SkipWelcoming()
    {
        welcomingUI.SetActive(false);
        welcomeAudioSource.Stop();
    }
}