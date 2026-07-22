using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BabyCareManager : MonoBehaviour
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
    [Header("Thermometer")]
    public int thermometerDialogueIndex = 2;
    public GameObject thermometerParent;
    public GameObject thermometerImage;

    [Header("Milk")]
    public int milkDialogueIndex = 5;
    public GameObject milkParent;
    public GameObject milkImage;

    [Header("Towel")]
    public int towelDialogueIndex = 8;
    public GameObject towelParent;
    public GameObject towelImage;
    public GameObject mainWetBaby;
    public GameObject mainDriedBaby;



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
        if (thermometerParent != null)
            thermometerParent.SetActive(false);

        if (milkParent != null)
            milkParent.SetActive(false);

        if (towelParent != null)
            towelParent.SetActive(false);

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
        // THERMOMETER
        //------------------------------------------------

        if (currentDialogue == thermometerDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            thermometerParent.SetActive(true);

            return;
        }

        //------------------------------------------------
        // MILK
        //------------------------------------------------

        if (currentDialogue == milkDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            milkParent.SetActive(true);

            return;
        }

        //------------------------------------------------
        // TOWEL
        //------------------------------------------------

        if (currentDialogue == towelDialogueIndex)
        {
            pauseDialogue = true;

            dialoguePanel.SetActive(false);
            towelParent.SetActive(true);

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
    // THERMOMETER
    //====================================================
    public void ThermometerCompleted()
    {
        thermometerParent.SetActive(false);

        if (thermometerImage != null)
            thermometerImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // MILK
    //====================================================
    public void MilkCompleted()
    {
        milkParent.SetActive(false);

        if (milkImage != null)
            milkImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // TOWEL
    //====================================================
    public void TowelCompleted()
    {
        towelParent.SetActive(false);

        if (towelImage != null)
            towelImage.SetActive(false);

        // Swap the main baby sprite (the one visible during dialogue)
        // to the dried version, since the mini-game's own baby objects
        // live inside towelParent and just got hidden above.
        if (mainWetBaby != null)
            mainWetBaby.SetActive(false);

        if (mainDriedBaby != null)
            mainDriedBaby.SetActive(true);

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