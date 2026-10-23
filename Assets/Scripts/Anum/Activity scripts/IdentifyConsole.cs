using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class IdentifyConsole : UdonSharpBehaviour
{
    public IdentifyPuzzleBank puzzleBank;
    public ActivityAttemptTracker tracker;
    public IdentifyPad[] pads;
    public IdentifyClueStation[] stations;

    public float padRadius = 0.75f;
    public float lockSeconds = 3f;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI statusText;
    public bool needAllClues = true;

    [UdonSynced] int puzzle = -1;
    [UdonSynced] int resultPad = -1;
    [UdonSynced] int resultState = 0;

    int loadedPuzzle = -2;
    int lockPad = -1;
    float holdTime = 0f;
    float checkTimer = 0f;
    bool mustStepOff = false;
    string lastStatus = "";

    int[] playerPads = new int[0];

    void Start()
    {
        if (puzzleBank == null) return;

        if (Networking.IsOwner(gameObject) && puzzle < 0)
        {
            puzzle = PickNewPuzzle(-1);
            RequestSerialization();
        }

        LoadPuzzle();
        SetStatus("Take a clue, scan the holograms, then stand together on your answer.");
    }

    void Update()
    {
        if (puzzleBank == null || pads == null || puzzle < 0) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < 0.2f) return;
        float timePassed = checkTimer;
        checkTimer = 0f;

        FindWhereEveryoneIs();
        CheckIfGroupSteppedOff();

        int groupPad = FindGroupPad();
        UpdateLockIn(groupPad, timePassed);
        UpdateStatus(groupPad);
        UpdatePadLooks();

        if (lockPad >= 0 && holdTime >= lockSeconds)
        {
            int chosen = lockPad;
            lockPad = -1;
            holdTime = 0f;
            mustStepOff = true;

            if (Networking.IsOwner(gameObject))
            {
                SubmitPad(chosen);
            }
        }
    }

    void FindWhereEveryoneIs()
    {
        int[] ids = GetClueHolders();
        playerPads = new int[ids.Length];

        for (int i = 0; i < ids.Length; i++)
        {
            playerPads[i] = GetPadUnderPlayer(ids[i]);
        }
    }

    bool AnyoneOnAPad()
    {
        for (int i = 0; i < playerPads.Length; i++)
        {
            if (playerPads[i] >= 0)
            {
                return true;
            }
        }
        return false;
    }

    void CheckIfGroupSteppedOff()
    {
        if (!mustStepOff) return;
        if (AnyoneOnAPad()) return;

        mustStepOff = false;

        if (Networking.IsOwner(gameObject) && resultState == 2)
        {
            resultState = 0;
            resultPad = -1;
            RequestSerialization();
        }
    }

    int FindGroupPad()
    {
        if (IsFinished()) return -1;
        if (mustStepOff) return -1;
        if (playerPads.Length == 0) return -1;

        int firstPad = playerPads[0];
        if (firstPad < 0) return -1;

        for (int i = 1; i < playerPads.Length; i++)
        {
            if (playerPads[i] != firstPad)
            {
                return -1;
            }
        }
        return firstPad;
    }

    void UpdateLockIn(int groupPad, float timePassed)
    {
        bool cluesReady = true;
        if (needAllClues)
        {
            cluesReady = AllCluesTaken();
        }

        if (groupPad >= 0 && cluesReady)
        {
            if (groupPad == lockPad)
            {
                holdTime += timePassed;
            }
            else
            {
                lockPad = groupPad;
                holdTime = 0f;
            }
        }
        else
        {
            lockPad = -1;
            holdTime = 0f;
        }
    }

    void UpdateStatus(int groupPad)
    {
        if (groupPad >= 0 && needAllClues && !AllCluesTaken())
        {
            SetStatus("Everyone needs to take a clue first.");
        }
        else if (lockPad >= 0)
        {
            int secondsLeft = Mathf.CeilToInt(lockSeconds - holdTime);
            if (secondsLeft < 1) secondsLeft = 1;
            SetStatus("Locking in " + puzzleBank.GetName(puzzle, lockPad) + "... " + secondsLeft);
        }
        else if (mustStepOff)
        {
            SetStatus("Step off the pads to try again.");
        }
        else if (!IsFinished())
        {
            SetStatus("Take a clue, scan the holograms, then stand together on your answer.");
        }
        else
        {
            SetStatus("");
        }
    }

    void UpdatePadLooks()
    {
        for (int i = 0; i < pads.Length; i++)
        {
            if (pads[i] == null) continue;

            bool someoneHere = false;
            for (int j = 0; j < playerPads.Length; j++)
            {
                if (playerPads[j] == i)
                {
                    someoneHere = true;
                }
            }

            if (resultPad == i && resultState == 1)
            {
                pads[i].SetState(2);
            }
            else if (resultPad == i && resultState == 2)
            {
                pads[i].SetState(3);
            }
            else if (someoneHere)
            {
                pads[i].SetState(1);
            }
            else
            {
                pads[i].SetState(0);
            }

            if (i == lockPad)
            {
                pads[i].SetBeam(holdTime / lockSeconds);
            }
            else
            {
                pads[i].SetBeam(0f);
            }
        }
    }

    void SubmitPad(int padNumber)
    {
        if (tracker == null) return;
        if (tracker.IsDone()) return;

        resultPad = padNumber;

        if (padNumber == puzzleBank.correctPad[puzzle])
        {
            resultState = 1;
            tracker.Correct();
        }
        else
        {
            resultState = 2;
            tracker.Wrong();

            if (tracker.IsDone())
            {
                resultPad = puzzleBank.correctPad[puzzle];
                resultState = 1;
            }
        }

        RequestSerialization();
    }

    public void ResetActivity()
    {
        if (puzzleBank == null) return;

        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        puzzle = PickNewPuzzle(puzzle);
        resultPad = -1;
        resultState = 0;
        RequestSerialization();

        if (stations != null)
        {
            for (int i = 0; i < stations.Length; i++)
            {
                if (stations[i] != null)
                {
                    stations[i].ReleaseClue();
                }
            }
        }

        mustStepOff = false;
        lockPad = -1;
        holdTime = 0f;

        LoadPuzzle();
        if (tracker != null)
        {
            tracker.ResetTracker();
        }
        SetStatus("New topic loaded. Take a clue to begin.");
    }

    public override void OnDeserialization()
    {
        LoadPuzzle();

        if (resultState == 2)
        {
            mustStepOff = true;
        }
        if (resultState == 0 && resultPad == -1)
        {
            mustStepOff = false;
        }
    }

    int PickNewPuzzle(int lastPuzzle)
    {
        int count = puzzleBank.GetCount();
        if (count <= 1) return 0;

        int next = Random.Range(0, count);
        while (next == lastPuzzle)
        {
            next = Random.Range(0, count);
        }
        return next;
    }

    void LoadPuzzle()
    {
        if (puzzleBank == null) return;
        if (puzzle < 0) return;
        if (puzzle == loadedPuzzle) return;
        loadedPuzzle = puzzle;

        if (questionText != null)
        {
            questionText.text = puzzleBank.questions[puzzle];
        }

        for (int i = 0; i < pads.Length && i < 4; i++)
        {
            if (pads[i] != null)
            {
                pads[i].SetPuzzle(puzzle, puzzleBank.GetName(puzzle, i), puzzleBank.GetCase(puzzle, i));
            }
        }

        if (stations != null && stations.Length >= 3)
        {
            if (stations[0] != null) stations[0].SetClue("Clue A", puzzleBank.clueA[puzzle]);
            if (stations[1] != null) stations[1].SetClue("Clue B", puzzleBank.clueB[puzzle]);
            if (stations[2] != null) stations[2].SetClue("Clue C", puzzleBank.clueC[puzzle]);
        }

        if (tracker != null)
        {
            tracker.SetTexts(puzzleBank.hint1[puzzle], puzzleBank.hint2[puzzle], puzzleBank.correctText[puzzle], puzzleBank.revealText[puzzle]);
        }
    }

    int[] GetClueHolders()
    {
        int count = 0;
        if (stations != null)
        {
            for (int i = 0; i < stations.Length; i++)
            {
                if (stations[i] != null && stations[i].IsTaken())
                {
                    count++;
                }
            }
        }

        if (count == 0 && !needAllClues && Networking.LocalPlayer != null)
        {
            int[] justMe = new int[1];
            justMe[0] = Networking.LocalPlayer.playerId;
            return justMe;
        }

        int[] ids = new int[count];
        int n = 0;
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null && stations[i].IsTaken())
            {
                ids[n] = stations[i].GetHolder();
                n++;
            }
        }
        return ids;
    }

    bool AllCluesTaken()
    {
        if (stations == null) return true;

        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null && !stations[i].IsTaken())
            {
                return false;
            }
        }
        return true;
    }

    bool IsFinished()
    {
        if (tracker == null) return false;
        return tracker.IsDone();
    }

    int GetPadUnderPlayer(int playerId)
    {
        VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);
        if (!Utilities.IsValid(player)) return -1;

        Vector3 playerPos = player.GetPosition();

        for (int i = 0; i < pads.Length; i++)
        {
            if (pads[i] == null) continue;

            Vector3 padPos = pads[i].transform.position;
            playerPos.y = padPos.y;
            float distance = Vector3.Distance(playerPos, padPos);

            if (distance <= padRadius)
            {
                return i;
            }
        }
        return -1;
    }

    void SetStatus(string message)
    {
        if (statusText == null) return;
        if (message == lastStatus) return;

        lastStatus = message;
        statusText.text = message;
    }
}
