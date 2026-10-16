using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Activity2Console : UdonSharpBehaviour
{
    public Activity2PuzzleBank bank;

    public Activity2Sphere[] spheres;

    public Transform[] slots;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI feedbackText;

    public GameObject submitButton;
    public GameObject resetButton;

    public GameObject hint1Button;
    public GameObject hint2Button;
    public GameObject revealButton;

    public float autoResetDelay = 15f;

    public ParticleSystem celebrationParticles;
    public AudioSource celebrationAudio;

    [UdonSynced] int currentPuzzle = -1;
    [UdonSynced] int[] sphereOrder = new int[] { 0, 1, 2, 3 };
    [UdonSynced] int[] slotOccupant = new int[] { -1, -1, -1, -1 };
    [UdonSynced] int[] playerIdList = new int[0];
    [UdonSynced] int[] playerPlaceCount = new int[0];
    [UdonSynced] bool puzzleDone = false;
    [UdonSynced] string sharedFeedback = "";
    [UdonSynced] string sharedHint = "";
    [UdonSynced] bool showSubmit = false;
    [UdonSynced] int[] slotPlacer = new int[] { -1, -1, -1, -1 };
    [UdonSynced] int wrongAttempts = 0;

    public int maxPerPlayer = 2;
    public int minPlayers = 3;

    public bool soloTestMode = false;

    bool celebratedLocally = false;

    void Start()
    {
        if (submitButton != null && submitButton.transform.parent != null)
        {
            Transform row = submitButton.transform.parent;
            if (hint1Button == null)
            {
                Transform t = row.Find("Hint1Button");
                if (t != null) hint1Button = t.gameObject;
            }
            if (hint2Button == null)
            {
                Transform t = row.Find("Hint2Button");
                if (t != null) hint2Button = t.gameObject;
            }
            if (revealButton == null)
            {
                Transform t = row.Find("RevealButton");
                if (t != null) revealButton = t.gameObject;
            }
            if (resetButton == null)
            {
                Transform t = row.Find("ResetButton");
                if (t != null) resetButton = t.gameObject;
            }
        }
        if (revealButton != null)
        {
            TextMeshProUGUI label = revealButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = "HINT";
        }

        UpdateUI();

        if (!Networking.IsOwner(gameObject)) return;
        StartNewPuzzle();
    }

    void StartNewPuzzle()
    {
        if (bank == null) return;

        int count = bank.GetCount();
        int next = Random.Range(0, count);

        currentPuzzle = next;
        puzzleDone = false;
        sharedFeedback = "";
        sharedHint = "";
        showSubmit = false;
        wrongAttempts = 0;

        sphereOrder = new int[] { 0, 1, 2, 3 };
        for (int i = sphereOrder.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = sphereOrder[i];
            sphereOrder[i] = sphereOrder[j];
            sphereOrder[j] = tmp;
        }

        slotOccupant = new int[] { -1, -1, -1, -1 };
        slotPlacer = new int[] { -1, -1, -1, -1 };
        celebratedLocally = false;
        playerIdList = new int[0];
        playerPlaceCount = new int[0];

        RequestSerialization();
        UpdateUI();
        LoadSphereContent();
    }

    void LoadSphereContent()
    {
        if (bank == null || currentPuzzle < 0) return;

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null) continue;
            int bankSlot = sphereOrder[i];
            string label = bank.GetSphereLabel(currentPuzzle, bankSlot);
            string info = bank.GetSphereInfo(currentPuzzle, bankSlot);
            spheres[i].SetContent(label, info);
        }
    }

    void UpdateUI()
    {
        if (bank != null && currentPuzzle >= 0)
        {
            if (questionText != null) questionText.text = bank.questions[currentPuzzle];
        }
        if (hintText != null) hintText.text = sharedHint;
        if (feedbackText != null)
            feedbackText.text = soloTestMode ? "[SOLO TEST MODE]\n" + sharedFeedback : sharedFeedback;
        if (submitButton != null) submitButton.SetActive(showSubmit);
        if (hint1Button != null) hint1Button.SetActive(false);
        if (hint2Button != null) hint2Button.SetActive(false);
        if (revealButton != null) revealButton.SetActive(!puzzleDone && wrongAttempts >= 1);
        if (resetButton != null) resetButton.SetActive(false);
    }

    public int FindNearestOpenSlot(Vector3 position, float maxDist)
    {
        int best = -1;
        float bestDist = maxDist;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (slotOccupant[i] >= 0) continue;

            float dist = Vector3.Distance(position, slots[i].position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }

        return best;
    }

    public bool CanPlayerPlace(int playerId)
    {
        for (int i = 0; i < playerIdList.Length; i++)
        {
            if (playerIdList[i] == playerId)
                return soloTestMode || playerPlaceCount[i] < maxPerPlayer;
        }
        return true;
    }

    public void ClearSlotForSphere(int sphereIndex)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        for (int i = 0; i < slotOccupant.Length; i++)
        {
            if (slotOccupant[i] == sphereIndex)
            {
                int placerId = FindPlacerForSlot(i);
                slotOccupant[i] = -1;
                slotPlacer[i] = -1;

                if (placerId >= 0)
                {
                    for (int j = 0; j < playerIdList.Length; j++)
                    {
                        if (playerIdList[j] == placerId && playerPlaceCount[j] > 0)
                        {
                            playerPlaceCount[j]--;
                            break;
                        }
                    }
                }
            }
        }

        showSubmit = false;
        RequestSerialization();
        UpdateUI();
    }

    int FindPlacerForSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotPlacer.Length) return -1;
        return slotPlacer[slotIndex];
    }

    public void RegisterPlacement(int sphereIndex, int slotIndex, int playerId)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        slotOccupant[slotIndex] = sphereIndex;
        slotPlacer[slotIndex] = playerId;

        bool found = false;
        for (int i = 0; i < playerIdList.Length; i++)
        {
            if (playerIdList[i] == playerId)
            {
                playerPlaceCount[i]++;
                found = true;
                break;
            }
        }

        if (!found)
        {
            int newLen = playerIdList.Length + 1;
            int[] newIds = new int[newLen];
            int[] newCounts = new int[newLen];
            for (int i = 0; i < playerIdList.Length; i++)
            {
                newIds[i] = playerIdList[i];
                newCounts[i] = playerPlaceCount[i];
            }
            newIds[newLen - 1] = playerId;
            newCounts[newLen - 1] = 1;
            playerIdList = newIds;
            playerPlaceCount = newCounts;
        }

        bool allFilled = true;
        for (int i = 0; i < slotOccupant.Length; i++)
        {
            if (slotOccupant[i] < 0) allFilled = false;
        }

        showSubmit = allFilled;
        RequestSerialization();
        UpdateUI();
    }

    public void OnSubmit()
    {
        if (puzzleDone) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        if (!soloTestMode && playerIdList.Length < minPlayers)
        {
            sharedFeedback = minPlayers == 3
                ? "At least three visitors must take part before you can submit."
                : "At least " + minPlayers + " visitors must take part before you can submit.";
            RequestSerialization();
            UpdateUI();
            return;
        }

        bool correct = true;
        for (int slot = 0; slot < slotOccupant.Length; slot++)
        {
            int sphereIdx = slotOccupant[slot];
            if (sphereIdx < 0) { correct = false; break; }
            int bankSlot = sphereOrder[sphereIdx];
            if (bankSlot != slot) { correct = false; break; }
        }

        if (correct)
        {
            puzzleDone = true;
            sharedFeedback = bank.correctText[currentPuzzle] + "\nA new puzzle starts in " + Mathf.RoundToInt(autoResetDelay) + " seconds.";
            showSubmit = false;
            RequestSerialization();
            UpdateUI();
            Celebrate();
            SendCustomEventDelayedSeconds(nameof(AutoReset), autoResetDelay);
        }
        else
        {
            wrongAttempts++;
            sharedFeedback = "Not quite. Pick up the spheres to rearrange them, then submit again.";
            sharedHint = wrongAttempts == 1 ? bank.hint1[currentPuzzle] : bank.hint2[currentPuzzle];
            RequestSerialization();
            UpdateUI();
        }
    }

    public void OnShowHint1()
    {
        if (bank == null || currentPuzzle < 0) return;
        if (wrongAttempts < 1) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        sharedHint = bank.hint1[currentPuzzle];
        RequestSerialization();
        UpdateUI();
    }

    public void OnShowHint2()
    {
        if (bank == null || currentPuzzle < 0) return;
        if (wrongAttempts < 2) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        sharedHint = bank.hint2[currentPuzzle];
        RequestSerialization();
        UpdateUI();
    }

    public void OnReveal()
    {
        if (bank == null || currentPuzzle < 0 || puzzleDone) return;
        if (wrongAttempts < 1) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        sharedHint = wrongAttempts >= 2 ? bank.hint2[currentPuzzle] : bank.hint1[currentPuzzle];
        RequestSerialization();
        UpdateUI();
    }

    public void AutoReset()
    {
        if (!puzzleDone || !Networking.IsOwner(gameObject)) return;
        ResetActivity();
    }

    public void ResetActivity()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] != null) spheres[i].ResetSphere();
        }

        StartNewPuzzle();
    }

    public Transform GetSlotTransform(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex];
    }

    void Celebrate()
    {
        if (celebratedLocally) return;
        celebratedLocally = true;
        if (celebrationParticles != null) celebrationParticles.Play();
        if (celebrationAudio != null) celebrationAudio.Play();
    }

    public override void OnDeserialization()
    {
        UpdateUI();
        LoadSphereContent();

        if (puzzleDone) Celebrate();
        else celebratedLocally = false;
    }
}