using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Scene3GameManager : MonoBehaviour
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
    // MINI-GAMES
    //====================================================
    [Header("Alcohol")]
    public int alcoholDialogueIndex = 2;
    public GameObject alcoholParent;
    public GameObject alcoholImage;

    [Header("Syringe")]
    public int syringeDialogueIndex = 5;
    public GameObject syringeParent;
    public GameObject syringeImage;

    [Header("Bandage")]
    public int bandageDialogueIndex = 8;
    public GameObject bandageParent;
    public GameObject bandageImage;



    //====================================================
    // FINISH
    //====================================================
    [Header("Finish")]
    public GameObject popupPanel;

    [Header("Scene Transition")]
    public string nextSceneName;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        if (alcoholParent != null)
            alcoholParent.SetActive(false);

        if (syringeParent != null)
            syringeParent.SetActive(false);

        if (bandageParent != null)
            bandageParent.SetActive(false);

        if (popupPanel != null)
            popupPanel.SetActive(false);

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

            if (popupPanel != null)
                popupPanel.SetActive(true);

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

        //------------------------------------------------
        // SYRINGE
        //------------------------------------------------

        if (currentDialogue == syringeDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            syringeParent.SetActive(true);

            return;
        }

        //------------------------------------------------
        // BANDAGE
        //------------------------------------------------

        if (currentDialogue == bandageDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            bandageParent.SetActive(true);

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
    // ALCOHOL
    //====================================================
    public void AlcoholCompleted()
    {
        alcoholParent.SetActive(false);

        if (alcoholImage != null)
            alcoholImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // SYRINGE
    //====================================================
    public void SyringeCompleted()
    {
        syringeParent.SetActive(false);

        if (syringeImage != null)
            syringeImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // BANDAGE
    //====================================================
    public void BandageCompleted()
    {
        bandageParent.SetActive(false);

        if (bandageImage != null)
            bandageImage.SetActive(false);

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

    //====================================================
    // FINISH
    //====================================================
    public void FinishScene()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}