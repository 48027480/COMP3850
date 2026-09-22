using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class GuideDialogue : UdonSharpBehaviour
{
    [TextArea(2, 5)]
    public string[] dialogueLines;

    public TMP_Text dialogueText;
    public GameObject dialoguePanel;
    public GameObject nextButton; 

    private int currentLine = 0;
    private bool isTalking = false;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    
    public override void Interact()
    {
        if (!isTalking)
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (dialogueLines.Length == 0) return;

        isTalking = true;
        currentLine = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        ShowLine();
    }

    
    public void NextLine()
    {
        if (!isTalking) return;

        currentLine++;
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    void ShowLine()
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogueLines[currentLine];
        }

        
        if (nextButton != null)
        {
            nextButton.SetActive(currentLine < dialogueLines.Length - 1);
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        currentLine = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}