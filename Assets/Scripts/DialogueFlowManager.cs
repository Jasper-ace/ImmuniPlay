using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueFlowManager : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text dialogueText;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation dialoguePopAnimation;

    public DialogueBoxAutoSize dialogueBoxAutoSize;

    public DialogueTriangleAnimation dialogueTriangleAnimation;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Popup Panel")]
    public GameObject popupPanel;

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;

    private int currentDialogue = 0;

    private bool waitingForButton = false;
    private bool panelShown = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[DialogueFlowManager] No dialogues assigned."
            );
            return;
        }

        ShowCurrentDialogue();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Don't continue while popup is open
            if (waitingForButton)
                return;


            // Finish typing instantly
            if (isTyping)
            {
                FinishTyping();
                return;
            }


            // Go to next dialogue
            NextDialogue();
        }
    }


    // =========================================================
    // SHOW CURRENT DIALOGUE
    // =========================================================

    void ShowCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
            return;

        if (currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }


        // -----------------------------------------------------
        // Process baby name FIRST
        // -----------------------------------------------------

        string processedDialogue =
            ProcessDialogue(dialogues[currentDialogue]);


        // -----------------------------------------------------
        // Auto Size
        // -----------------------------------------------------

        if (dialogueBoxAutoSize != null)
        {
            dialogueBoxAutoSize.ResizeForText(
                processedDialogue
            );
        }


        // -----------------------------------------------------
        // Pop Animation
        // -----------------------------------------------------

        if (dialoguePopAnimation != null)
        {
            dialoguePopAnimation.PlayPop();
        }


        // -----------------------------------------------------
        // Triangle Animation
        // -----------------------------------------------------

        if (dialogueTriangleAnimation != null)
        {
            dialogueTriangleAnimation.PlayAnimation();
        }


        // -----------------------------------------------------
        // Typewriter
        // -----------------------------------------------------

        StartTyping(processedDialogue);
    }


    // =========================================================
    // BABY NAME PROCESSING
    // =========================================================

    string ProcessDialogue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;


        // If there is no placeholder,
        // don't do anything.
        if (!text.Contains("{babyName}"))
            return text;


        // Default:
        // Keep {babyName} if no name is available.
        string babyName = "{babyName}";


        // Check if SaveManager exists
        if (SaveManager.Instance != null)
        {
            string savedName =
                SaveManager.Instance.GetBabyName();


            // Use saved name if one exists
            if (!string.IsNullOrEmpty(savedName))
            {
                babyName = savedName;
            }
        }
        else
        {
            Debug.LogWarning(
                "[DialogueFlowManager] SaveManager.Instance is NULL. " +
                "Keeping {babyName}."
            );
        }


        // Replace every {babyName} in the dialogue
        return text.Replace(
            "{babyName}",
            babyName
        );
    }


    // =========================================================
    // START TYPING
    // =========================================================

    void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        typingCoroutine =
            StartCoroutine(TypeText(text));
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    IEnumerator TypeText(string text)
    {
        isTyping = true;


        if (dialogueText == null)
        {
            Debug.LogWarning(
                "[DialogueFlowManager] Dialogue Text is not assigned."
            );

            isTyping = false;
            typingCoroutine = null;

            yield break;
        }


        dialogueText.text = "";


        foreach (char letter in text)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }


        isTyping = false;
        typingCoroutine = null;
    }


    // =========================================================
    // FINISH TYPING
    // =========================================================

    void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        if (dialogueText != null &&
            dialogues != null &&
            currentDialogue >= 0 &&
            currentDialogue < dialogues.Length)
        {
            // IMPORTANT:
            // Process the dialogue again so {babyName}
            // is replaced even when skipping the typewriter.

            string processedDialogue =
                ProcessDialogue(
                    dialogues[currentDialogue]
                );

            dialogueText.text = processedDialogue;
        }


        isTyping = false;
    }


    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    void NextDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
            return;


        // -----------------------------------------------------
        // Show popup after selected dialogue
        // -----------------------------------------------------

        if (!panelShown &&
            currentDialogue == panelAfterDialogue)
        {
            panelShown = true;
            waitingForButton = true;


            if (popupPanel != null)
                popupPanel.SetActive(true);


            return;
        }


        // -----------------------------------------------------
        // Move to next dialogue
        // -----------------------------------------------------

        currentDialogue++;


        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }


        ShowCurrentDialogue();
    }


    // =========================================================
    // CONTINUE FROM POPUP
    // =========================================================

    // Assign this method to your UI Button's OnClick()
    public void ContinueFromPanel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);


        waitingForButton = false;


        currentDialogue++;


        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }


        ShowCurrentDialogue();
    }


    // =========================================================
    // GO TO SPECIFIC DIALOGUE
    // =========================================================

    // Optional: Skip directly to any dialogue index
    public void GoToDialogue(int dialogueIndex)
    {
        if (dialogues == null ||
            dialogueIndex < 0 ||
            dialogueIndex >= dialogues.Length)
        {
            Debug.LogWarning(
                "[DialogueFlowManager] " +
                "Dialogue index is out of range."
            );

            return;
        }


        if (popupPanel != null)
            popupPanel.SetActive(false);


        waitingForButton = false;

        currentDialogue = dialogueIndex;

        ShowCurrentDialogue();
    }
}