using UnityEngine;
using TMPro;
using System.Collections;

public class PopupPanelManager : MonoBehaviour
{
    //====================================================
    // REFERENCES
    //====================================================

    [SerializeField] private BabyCareManager babyCareManager;


    //====================================================
    // DIALOGUE
    //====================================================

    [Header("Dialogue")]

    [SerializeField] private GameObject chatBubble;

    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private string[] dialogues;

    [SerializeField] private float typingSpeed = 0.05f;


    //====================================================
    // TYPEWRITER SOUND
    //====================================================

    [Header("Typewriter Sound")]

    [Tooltip("Sound played while the dialogue is typing.")]
    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float typewriterVolume = 0.6f;


    //====================================================
    // DIALOGUE ANIMATIONS
    //====================================================

    [Header("Dialogue Animations")]

    [Tooltip("Optional. Automatically resizes the dialogue box.")]
    [SerializeField] private DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Optional. Plays the pop animation.")]
    [SerializeField] private DialoguePopAnimation popAnimation;

    [Tooltip("Optional. Plays the triangle animation.")]
    [SerializeField] private DialogueTriangleAnimation triangleAnimation;


    //====================================================
    // STATE
    //====================================================

    private int currentDialogueIndex = 0;

    private Coroutine typingRoutine;

    private bool isTyping = false;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        if (chatBubble != null)
            chatBubble.SetActive(true);


        if (dialogues != null && dialogues.Length > 0)
        {
            currentDialogueIndex = 0;

            string processedDialogue =
                ProcessDialogue(dialogues[currentDialogueIndex]);

            PrepareDialogueVisuals(processedDialogue);

            typingRoutine = StartCoroutine(
                TypeDialogue(processedDialogue)
            );
        }
        else
        {
            Debug.LogWarning(
                "[PopupPanelManager] No dialogues assigned."
            );
        }
    }


    //====================================================
    // DIALOGUE INPUT
    //====================================================

    public void AdvanceDialogue()
    {
        // First tap: finish typing
        if (isTyping)
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            if (
                dialogues != null &&
                currentDialogueIndex >= 0 &&
                currentDialogueIndex < dialogues.Length
            )
            {
                dialogueText.text = ProcessDialogue(
                    dialogues[currentDialogueIndex]
                );
            }

            isTyping = false;

            return;
        }


        // Second tap: next dialogue
        NextDialogue();
    }


    //====================================================
    // BABY NAME PROCESSING
    //====================================================

    private string ProcessDialogue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;


        // No baby name placeholder
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


            // Use the saved baby name
            if (!string.IsNullOrEmpty(savedName))
            {
                babyName = savedName;
            }
        }
        else
        {
            Debug.LogWarning(
                "[PopupPanelManager] SaveManager.Instance is NULL. " +
                "Keeping {babyName}."
            );
        }


        // Replace every occurrence of {babyName}
        return text.Replace(
            "{babyName}",
            babyName
        );
    }


    //====================================================
    // TYPE DIALOGUE
    //====================================================

    IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;


        if (dialogueText == null)
        {
            Debug.LogWarning(
                "[PopupPanelManager] Dialogue Text is not assigned."
            );

            isTyping = false;
            typingRoutine = null;

            yield break;
        }


        dialogueText.text = "";

        int characterCount = 0;


        foreach (char c in dialogue)
        {
            // Add character
            dialogueText.text += c;

            characterCount++;


            // Play typewriter sound through AudioManager
            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                c != ' ' &&
                characterCount %
                Mathf.Max(soundEveryCharacters, 1) == 0
            )
            {
                AudioManager.Instance.PlayDialogue(
                    typewriterSound,
                    typewriterVolume
                );
            }


            // Wait before next character
            yield return new WaitForSeconds(
                typingSpeed
            );
        }


        isTyping = false;
        typingRoutine = null;
    }


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        currentDialogueIndex++;


        if (
            dialogues == null ||
            currentDialogueIndex >= dialogues.Length
        )
        {
            CompleteThermometer();

            return;
        }


        // Process {babyName} BEFORE typewriter
        string processedDialogue =
            ProcessDialogue(
                dialogues[currentDialogueIndex]
            );


        PrepareDialogueVisuals(processedDialogue);


        typingRoutine = StartCoroutine(
            TypeDialogue(processedDialogue)
        );
    }


    //====================================================
    // DIALOGUE VISUALS
    //====================================================

    private void PrepareDialogueVisuals(string processedDialogue)
    {
        if (dialogueAutoSize != null)
        {
            dialogueAutoSize.ResizeForText(
                processedDialogue
            );
        }


        if (popAnimation != null)
        {
            popAnimation.PlayPop();
        }


        if (triangleAnimation != null)
        {
            triangleAnimation.PlayAnimation();
        }
    }


    //====================================================
    // COMPLETE THERMOMETER
    //====================================================

    void CompleteThermometer()
    {
        if (chatBubble != null)
            chatBubble.SetActive(false);


        gameObject.SetActive(false);


        // Redirect to BabyCareManager
        if (babyCareManager != null)
        {
            babyCareManager.ThermometerCompleted();


            Debug.Log(
                "Thermometer dialogue complete!"
            );
        }
        else
        {
            Debug.LogError(
                "[PopupPanelManager] " +
                "BabyCareManager not assigned!"
            );
        }
    }
}