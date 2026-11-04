using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryFlowManager : MonoBehaviour
{
    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("Dialogue")]

    public TMP_Text dialogueText;

    [Tooltip("Dialogue bubble used by the speaker.")]
    public GameObject dialogueBubble;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation dialoguePopAnimation;

    public DialogueBoxAutoSize dialogueBoxAutoSize;

    public DialogueTriangleAnimation dialogueTriangleAnimation;

    [TextArea(2, 5)]
    public string[] dialogues;


    // =========================================================
    // TYPEWRITER
    // =========================================================

    [Header("Typewriter")]

    public float typingSpeed = 0.03f;


    // =========================================================
    // TYPEWRITER SOUND
    // =========================================================

    [Header("TYPEWRITER SOUND")]

    [Tooltip("Short sound played while the text is typing.")]
    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the typing sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float typewriterVolume = 0.6f;


    // =========================================================
    // BABY IDENTIFICATION
    // =========================================================

    [Header("Baby Identification")]

    public GameObject babyIdentificationPanel;

    public Button confirmButton;

    public TMP_InputField babyNameInput;

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;


    // =========================================================
    // AFTER DIALOGUE - SHOW / HIDE OBJECTS
    // =========================================================

    [Header("After Dialogue - Show / Hide Objects")]

    [Tooltip("The dialogue index after which the objects will be changed.")]
    public int objectChangeAfterDialogue = 5;

    [Tooltip("GameObjects to enable after the specified dialogue.")]
    public GameObject[] objectsToShowAfterDialogue;

    [Tooltip("GameObjects to disable after the specified dialogue.")]
    public GameObject[] objectsToHideAfterDialogue;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentDialogue = 0;

    private bool waitingForConfirm = false;

    private bool panelShown = false;

    private bool objectsChanged = false;

    private bool isTyping = false;

    private bool dialogueFinished = false;

    private Coroutine typingCoroutine;

    private string currentFullText = "";

    private string babyName = "Baby";


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // -----------------------------------------------------
        // Load saved baby name
        // -----------------------------------------------------

        if (SaveManager.Instance != null)
        {
            string savedBabyName = SaveManager.Instance.GetBabyName();

            if (!string.IsNullOrEmpty(savedBabyName))
                babyName = savedBabyName;
            else
                babyName = PlayerPrefs.GetString("BabyName", "Baby");
        }
        else
        {
            // Fallback for scenes where SaveManager is not present.
            babyName = PlayerPrefs.GetString("BabyName", "Baby");
        }


        // -----------------------------------------------------
        // Hide dialogue bubble initially
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        // -----------------------------------------------------
        // Hide baby identification panel initially
        // -----------------------------------------------------

        if (babyIdentificationPanel != null)
            babyIdentificationPanel.SetActive(false);


        // -----------------------------------------------------
        // Setup confirm button
        // -----------------------------------------------------

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmBabyIdentification);
        }


        // -----------------------------------------------------
        // Setup baby name input
        // -----------------------------------------------------

        if (babyNameInput != null)
        {
            babyNameInput.onValueChanged.AddListener(OnBabyNameChanged);
        }


        // -----------------------------------------------------
        // Start dialogue
        // -----------------------------------------------------

        ShowCurrentDialogue();
    }


    // =========================================================
    // ADVANCE DIALOGUE
    // =========================================================

    // Called by the global Dialogue Tap Input UI Button.
    //
    // First tap while typing:
    // Finish the current dialogue.
    //
    // Second tap:
    // Advance to the next dialogue.

    public void AdvanceDialogue()
    {
        // Do nothing if the dialogue has already finished.
        if (dialogueFinished)
            return;


        // Do not advance while the baby identification panel
        // is waiting for confirmation.
        if (waitingForConfirm)
            return;


        // If text is still typing,
        // finish the current dialogue first.
        if (isTyping)
        {
            FinishTyping();
            return;
        }


        // Otherwise go to the next dialogue.
        NextDialogue();
    }


    // =========================================================
    // SHOW CURRENT DIALOGUE
    // =========================================================

    void ShowCurrentDialogue()
    {
        if (
            dialogues == null ||
            dialogues.Length == 0 ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length
        )
        {
            return;
        }


        // -----------------------------------------------------
        // Get dialogue
        // -----------------------------------------------------

        string dialogue = dialogues[currentDialogue];


        // -----------------------------------------------------
        // Replace {baby}
        // -----------------------------------------------------

        dialogue = dialogue.Replace("{baby}", babyName);


        // -----------------------------------------------------
        // Store full text
        // -----------------------------------------------------

        currentFullText = dialogue;


        // -----------------------------------------------------
        // Show dialogue bubble
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);


        // -----------------------------------------------------
        // Clear old text
        // -----------------------------------------------------

        if (dialogueText != null)
            dialogueText.text = "";


        // -----------------------------------------------------
        // Dialogue animations
        // -----------------------------------------------------

        if (dialoguePopAnimation != null)
            dialoguePopAnimation.PlayPop();


        if (dialogueBoxAutoSize != null)
            dialogueBoxAutoSize.ResizeForText(dialogue);


        if (dialogueTriangleAnimation != null)
            dialogueTriangleAnimation.PlayAnimation();


        // -----------------------------------------------------
        // Start typing
        // -----------------------------------------------------

        StartTyping(dialogue);
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


        typingCoroutine = StartCoroutine(TypeText(text));
    }


    // =========================================================
    // BABY NAME INPUT
    // =========================================================

    private void OnBabyNameChanged(string text)
    {
        string uppercaseText = text.ToUpper();


        if (babyNameInput.text != uppercaseText)
        {
            int caretPosition = babyNameInput.caretPosition;


            babyNameInput.SetTextWithoutNotify(uppercaseText);


            babyNameInput.caretPosition = caretPosition;
        }
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    IEnumerator TypeText(string text)
    {
        isTyping = true;


        if (dialogueText != null)
            dialogueText.text = "";


        string safeText = text ?? "";


        float safeTypingSpeed =
            Mathf.Max(typingSpeed, 0.001f);


        int characterCount = 0;


        foreach (char letter in safeText)
        {
            // -------------------------------------------------
            // Add character
            // -------------------------------------------------

            if (dialogueText != null)
                dialogueText.text += letter;


            characterCount++;


            // -------------------------------------------------
            // Typewriter sound
            // -------------------------------------------------

            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                letter != ' ' &&
                characterCount %
                Mathf.Max(soundEveryCharacters, 1) == 0
            )
            {
                AudioManager.Instance.PlayDialogue(
                    typewriterSound,
                    typewriterVolume
                );
            }


            // -------------------------------------------------
            // Wait before next character
            // -------------------------------------------------

            yield return new WaitForSeconds(
                safeTypingSpeed
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


        if (
            dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length
        )
        {
            return;
        }


        // Show the complete sentence immediately.
        if (dialogueText != null)
            dialogueText.text = currentFullText;


        isTyping = false;
    }


    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    void NextDialogue()
    {
        // -----------------------------------------------------
        // Baby Identification Panel
        // -----------------------------------------------------

        if (
            !panelShown &&
            currentDialogue == panelAfterDialogue
        )
        {
            panelShown = true;

            waitingForConfirm = true;


            if (babyIdentificationPanel != null)
            {
                babyIdentificationPanel.SetActive(true);


                if (babyNameInput != null)
                {
                    babyNameInput.text = "";

                    babyNameInput.ActivateInputField();
                }
            }


            return;
        }


        // -----------------------------------------------------
        // Show / Hide GameObjects
        // -----------------------------------------------------

        if (
            !objectsChanged &&
            currentDialogue == objectChangeAfterDialogue
        )
        {
            objectsChanged = true;


            // -------------------------------------------------
            // SHOW OBJECTS
            // -------------------------------------------------

            if (objectsToShowAfterDialogue != null)
            {
                foreach (
                    GameObject obj
                    in objectsToShowAfterDialogue
                )
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }


            // -------------------------------------------------
            // HIDE OBJECTS
            // -------------------------------------------------

            if (objectsToHideAfterDialogue != null)
            {
                foreach (
                    GameObject obj
                    in objectsToHideAfterDialogue
                )
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }


        // -----------------------------------------------------
        // Move to next dialogue
        // -----------------------------------------------------

        currentDialogue++;


        // -----------------------------------------------------
        // More dialogue remaining
        // -----------------------------------------------------

        if (currentDialogue < dialogues.Length)
        {
            ShowCurrentDialogue();

            return;
        }


        // -----------------------------------------------------
        // All dialogue finished
        // -----------------------------------------------------

        FinishDialogue();
    }


    // =========================================================
    // FINISH DIALOGUE
    // =========================================================

    private void FinishDialogue()
    {
        // Prevent this from running more than once.
        if (dialogueFinished)
            return;


        dialogueFinished = true;


        // -----------------------------------------------------
        // Stop typing
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        isTyping = false;


        // -----------------------------------------------------
        // HIDE DIALOGUE BUBBLE
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        // -----------------------------------------------------
        // Show / Hide GameObjects
        // -----------------------------------------------------

        if (!objectsChanged)
        {
            objectsChanged = true;


            // -------------------------------------------------
            // SHOW OBJECTS
            // -------------------------------------------------

            if (objectsToShowAfterDialogue != null)
            {
                foreach (
                    GameObject obj
                    in objectsToShowAfterDialogue
                )
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }


            // -------------------------------------------------
            // HIDE OBJECTS
            // -------------------------------------------------

            if (objectsToHideAfterDialogue != null)
            {
                foreach (
                    GameObject obj
                    in objectsToHideAfterDialogue
                )
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }


        Debug.Log("Story Finished.");
    }


    // =========================================================
    // CONFIRM BABY IDENTIFICATION
    // =========================================================

    public void ConfirmBabyIdentification()
    {
        if (babyNameInput == null)
        {
            Debug.LogError(
                "Baby Name Input is not assigned."
            );

            return;
        }


        string enteredName =
            babyNameInput.text.Trim();


        // -----------------------------------------------------
        // Make sure a name was entered
        // -----------------------------------------------------

        if (string.IsNullOrEmpty(enteredName))
        {
            Debug.Log(
                "Please enter a baby name."
            );


            babyNameInput.ActivateInputField();

            return;
        }


        // -----------------------------------------------------
        // Save baby's name
        // -----------------------------------------------------

        babyName = enteredName;

        // -----------------------------------------------------
        // Save baby's name to save.json through SaveManager
        // -----------------------------------------------------

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetBabyName(babyName);
        }
        else
        {
            Debug.LogWarning(
                "[StoryFlowManager] SaveManager.Instance is null. " +
                "Baby name was not saved to save.json."
            );
        }

        // Keep PlayerPrefs for compatibility with older systems.
        PlayerPrefs.SetString(
            "BabyName",
            babyName
        );

        PlayerPrefs.Save();


        // -----------------------------------------------------
        // Hide identification panel
        // -----------------------------------------------------

        if (babyIdentificationPanel != null)
            babyIdentificationPanel.SetActive(false);


        waitingForConfirm = false;


        // -----------------------------------------------------
        // Move to next dialogue
        // -----------------------------------------------------

        currentDialogue++;


        if (currentDialogue < dialogues.Length)
        {
            ShowCurrentDialogue();

            return;
        }


        // -----------------------------------------------------
        // Story finished
        // -----------------------------------------------------

        FinishDialogue();
    }
}