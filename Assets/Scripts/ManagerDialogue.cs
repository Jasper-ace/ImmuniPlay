using UnityEngine;
using TMPro;
using System.Collections;

public class ManagerDialogue : MonoBehaviour
{
    //====================================================
    // DIALOGUE
    //====================================================
    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogues;
    public float typingSpeed = 0.05f;

    private int currentDialogue = -1;
    private Coroutine typingRoutine;
    private bool pauseDialogue = false;

    //====================================================
    // ALCOHOL
    //====================================================
    [Header("Alcohol")]
    public int alcoholDialogueIndex = 2;
    public GameObject alcoholParent;
    public GameObject alcoholImage;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        if (alcoholParent != null)
            alcoholParent.SetActive(false);

        if (alcoholImage != null)
            alcoholImage.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogues.Length > 0)
        {
            currentDialogue = 0;
            typingRoutine = StartCoroutine(TypeDialogue(dialogues[currentDialogue]));
        }
    }

    //====================================================
    // UPDATE
    //====================================================
    void Update()
    {
        if (pauseDialogue)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                dialogueText.text = dialogues[currentDialogue];
                typingRoutine = null;
            }
            else
            {
                NextDialogue();
            }
        }
    }

    //====================================================
    // TYPE DIALOGUE
    //====================================================
    IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";

        foreach (char c in dialogue.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingRoutine = null;
    }

    //====================================================
    // NEXT DIALOGUE
    //====================================================
    void NextDialogue()
    {
        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        //------------------------------------------------
        // ALCOHOL
        //------------------------------------------------

        if (currentDialogue == alcoholDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            alcoholParent.SetActive(true);

            return;
        }

        StartTyping();
    }

    //====================================================
    // START TYPING
    //====================================================
    void StartTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (currentDialogue < 0 || currentDialogue >= dialogues.Length)
        {
            Debug.LogError($"Invalid dialogue index: {currentDialogue}. Array length: {dialogues.Length}");
            return;
        }

        typingRoutine = StartCoroutine(TypeDialogue(dialogues[currentDialogue]));
    }

    //====================================================
    // ALCOHOL COMPLETED
    //====================================================
    public void AlcoholCompleted()
    {
        alcoholParent.SetActive(false);

        if (alcoholImage != null)
            alcoholImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // RESUME DIALOGUE
    //====================================================
    void ResumeDialogue()
    {
        pauseDialogue = false;

        dialoguePanel.SetActive(true);

        StartTyping();
    }

    //====================================================
    // CONTINUE DIALOGUE (Optional - for manual resume)
    //====================================================
    public void ContinueDialogue()
    {
        ResumeDialogue();
    }
}