using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class IdentifyPad : UdonSharpBehaviour
{
    public TextMeshProUGUI nameText;
    public GameObject caseCard;
    public TextMeshProUGUI caseText;
    public GameObject[] holograms;

    public Renderer ring;
    public Material idleMat;
    public Material activeMat;
    public Material correctMat;
    public Material wrongMat;

    public Transform beam;
    public float scanTime = 8f;

    Vector3 beamScale;
    float hideAt;
    int state = -1;

    void Start()
    {
        if (beam != null)
        {
            beamScale = beam.localScale;
            beam.gameObject.SetActive(false);
        }
        if (caseCard != null) caseCard.SetActive(false);
        SetState(0);
    }

    public void SetPuzzle(int p, string newName, string newCase)
    {
        if (nameText != null) nameText.text = newName;
        if (caseText != null) caseText.text = newName + "\n\n" + newCase;

        if (holograms != null)
        {
            for (int i = 0; i < holograms.Length; i++)
            {
                if (holograms[i] != null) holograms[i].SetActive(i == p);
            }
        }

        if (caseCard != null) caseCard.SetActive(false);
    }

    public override void Interact()
    {
        if (caseCard == null) return;
        caseCard.SetActive(true);
        hideAt = Time.time + scanTime;
        SendCustomEventDelayedSeconds("HideCase", scanTime + 0.1f);
    }

    public void HideCase()
    {
        if (Time.time < hideAt) return;
        if (caseCard != null) caseCard.SetActive(false);
    }

    public void SetState(int s)
    {
        if (s == state) return;
        state = s;
        if (ring == null) return;

        if (s == 1 && activeMat != null) ring.sharedMaterial = activeMat;
        else if (s == 2 && correctMat != null) ring.sharedMaterial = correctMat;
        else if (s == 3 && wrongMat != null) ring.sharedMaterial = wrongMat;
        else if (idleMat != null) ring.sharedMaterial = idleMat;
    }

    public void SetBeam(float amount)
    {
        if (beam == null) return;

        if (amount <= 0f)
        {
            if (beam.gameObject.activeSelf) beam.gameObject.SetActive(false);
            return;
        }

        if (!beam.gameObject.activeSelf) beam.gameObject.SetActive(true);
        beam.localScale = new Vector3(beamScale.x, beamScale.y * Mathf.Clamp01(amount), beamScale.z);
    }
}
