using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class QuizBooth : UdonSharpBehaviour
{
    public EarthQuizBank bank;
    public CoinWallet wallet;
    public int boothIndex = 0;
    public int questionsPerBooth = 10;
    public int coinsPerAnswer = 5;
    public float nextDelay = 6f;
    public float cooldown = 30f;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI resultText;

    public AudioSource guideVoice;
    public AudioClip[] correctClips;
    public AudioClip[] wrongClips;

    public Transform lid;
    public Vector3 lidClosed = new Vector3(0f, 0f, 0f);
    public Vector3 lidOpen = new Vector3(-110f, 0f, 0f);
    public GameObject coinPile;
    public AudioSource coinSound;

    int current = 0;
    bool swapped;
    bool waiting;
    bool near;
    float lastPaid = -1000f;

    void Start()
    {
        if (titleText != null) titleText.text = bank.boothNames[boothIndex];
        current = Random.Range(0, questionsPerBooth);
        ShowQuestion();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        near = true;
        if (!waiting) AskQuestion();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        near = false;
        if (guideVoice != null) guideVoice.Stop();
    }

    int GetQuestion()
    {
        return boothIndex * questionsPerBooth + current;
    }

    void ShowQuestion()
    {
        int q = GetQuestion();
        swapped = Random.Range(0, 2) == 1;

        questionText.text = bank.questions[q];
        if (swapped)
        {
            leftText.text = bank.optionB[q];
            rightText.text = bank.optionA[q];
        }
        else
        {
            leftText.text = bank.optionA[q];
            rightText.text = bank.optionB[q];
        }

        resultText.text = "Pick an answer";
        CloseBox();
    }

    public void AskQuestion()
    {
        int q = GetQuestion();
        if (guideVoice == null) return;
        if (bank.questionClips == null || q >= bank.questionClips.Length) return;
        if (bank.questionClips[q] == null) return;

        guideVoice.Stop();
        guideVoice.clip = bank.questionClips[q];
        guideVoice.Play();
    }

    public void PickLeft()
    {
        if (swapped) CheckAnswer(1);
        else CheckAnswer(0);
    }

    public void PickRight()
    {
        if (swapped) CheckAnswer(0);
        else CheckAnswer(1);
    }

    void CheckAnswer(int picked)
    {
        if (waiting) return;
        waiting = true;

        int q = GetQuestion();

        if (picked == bank.correct[q])
        {
            float wait = cooldown - (Time.time - lastPaid);
            if (!wallet.IsReady())
            {
                resultText.text = "<b>Correct!</b> Your coins are still loading, so none this time.\n" + bank.facts[q];
            }
            else if (wait > 0f)
            {
                resultText.text = "<b>Correct!</b> This stall's coin box refills in " + Mathf.CeilToInt(wait) + " s.\n" + bank.facts[q];
            }
            else
            {
                wallet.AddCoins(coinsPerAnswer);
                lastPaid = Time.time;
                resultText.text = "<b>Correct! +" + coinsPerAnswer + " coins</b>\n" + bank.facts[q];
                OpenBox();
            }
            Say(correctClips);
        }
        else
        {
            string answer = bank.optionA[q];
            if (bank.correct[q] == 1) answer = bank.optionB[q];
            resultText.text = "<b>Not quite.</b> The answer was: " + answer + "\n" + bank.facts[q];
            Say(wrongClips);
        }

        SendCustomEventDelayedSeconds("NextQuestion", nextDelay);
    }

    void Say(AudioClip[] clips)
    {
        if (guideVoice == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        guideVoice.Stop();
        guideVoice.clip = clip;
        guideVoice.Play();
    }

    public void NextQuestion()
    {
        int next = Random.Range(0, questionsPerBooth - 1);
        if (next >= current) next++;
        current = next;

        waiting = false;
        ShowQuestion();
        if (near) AskQuestion();
    }

    void OpenBox()
    {
        if (lid != null) lid.localEulerAngles = lidOpen;
        if (coinPile != null) coinPile.SetActive(true);
        if (coinSound != null) coinSound.Play();
    }

    void CloseBox()
    {
        if (lid != null) lid.localEulerAngles = lidClosed;
        if (coinPile != null) coinPile.SetActive(false);
    }
}
