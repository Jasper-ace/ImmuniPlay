using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DopplerMiniGameManager : MonoBehaviour
{
    public static DopplerMiniGameManager Instance;


    // =========================================================
    // DOPPLER
    // =========================================================

    [Header("Doppler")]
    public RectTransform doppler;
    public GameObject dopplerNormal;
    public GameObject dopplerZoomed;


    // =========================================================
    // POP ANIMATION
    // =========================================================

    [Header("Pop Animation")]
    public float popDuration = 0.35f;
    public float popOvershoot = 1.1f;

    [Header("Doppler SFX")]
    [Tooltip("Sound played when the player taps the normal Doppler.")]
    public AudioClip dopplerTapSound;

    [Tooltip("Volume of the Doppler tap sound.")]
    [Range(0f, 1f)]
    public float dopplerTapSoundVolume = 1f;

    [Tooltip("Sound played when the zoomed Doppler pops into view.")]
    public AudioClip dopplerPopSound;

    [Tooltip("Volume of the Doppler pop sound.")]
    [Range(0f, 1f)]
    public float dopplerPopSoundVolume = 1f;


    // =========================================================
    // LUBRICANT
    // =========================================================

    [Header("Lubricant")]
    public GameObject lubricant;


    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("Dialogue")]
    public GameObject dialogueBubble;
    public TextMeshProUGUI dialogueText;


    // =========================================================
    // DIALOGUE ANIMATIONS
    // =========================================================

    [Header("Dialogue Animations")]
    public DialogueBoxAutoSize autoSize;
    public DialoguePopAnimation popAnimation;
    public DialogueTriangleAnimation triangleAnimation;


    // =========================================================
    // TYPEWRITER
    // =========================================================

    [Header("Typewriter")]

    [Tooltip("Time between each character.")]
    [Range(0.001f, 0.2f)]
    public float typingSpeed = 0.03f;

    [Tooltip("Sound played while the dialogue is typing.")]
    public AudioClip typewriterSound;

    [Tooltip("Play the typewriter sound every X characters.")]
    [Min(1)]
    public int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    public float typewriterVolume = 1f;


    // =========================================================
    // INSTRUCTION 1
    // =========================================================

    [Header("Instruction 1")]
    [TextArea(2, 5)]
    public List<string> instruction1 = new List<string>()
    {
        "Okay, let's check the baby's heartbeat.",
        "First, we'll use the Doppler.",
        "Tap the Doppler when you're ready."
    };


    [Header("Instruction 1 - Objects To Show On Dialogue Start")]
    [Tooltip("GameObjects to show when Instruction 1 dialogue starts.")]
    public GameObject[] instruction1ObjectsToShow;


    [Header("Instruction 1 - Objects To Disable On Dialogue Start")]
    [Tooltip("GameObjects to disable when Instruction 1 dialogue starts.")]
    public GameObject[] instruction1ObjectsToDisableOnStart;


    [Header("Instruction 1 - Objects To Hide")]
    [Tooltip("GameObjects to hide when Instruction 1 is completed.")]
    public GameObject[] instruction1ObjectsToHide;


    // =========================================================
    // INSTRUCTION 2
    // =========================================================

    [Header("Instruction 2")]
    [TextArea(2, 5)]
    public List<string> instruction2 = new List<string>()
    {
        "Great!",
        "The Doppler is ready.",
        "We'll need some lubricant first.",
        "Apply it to the probe.",
        "Drag the lubricant onto the Doppler."
    };


    [Header("Instruction 2 - Objects To Show On Dialogue Start")]
    [Tooltip("GameObjects to show when Instruction 2 dialogue starts.")]
    public GameObject[] instruction2ObjectsToShow;


    [Header("Instruction 2 - Objects To Disable On Dialogue Start")]
    [Tooltip("GameObjects to disable when Instruction 2 dialogue starts.")]
    public GameObject[] instruction2ObjectsToDisableOnStart;


    [Header("Instruction 2 - Objects To Hide")]
    [Tooltip("GameObjects to hide when Instruction 2 is completed.")]
    public GameObject[] instruction2ObjectsToHide;


    // =========================================================
    // SCENE TRANSITION
    // =========================================================

    [Header("Scene Transition")]
    public GameObject currentGameObject;
    public GameObject nextGameObject;


    // =========================================================
    // INTERNAL
    // =========================================================

    private bool dialogueActive = false;

    // Which instruction group are we currently in?
    private int currentInstruction = 0;

    // Which dialogue line inside the instruction are we on?
    private int currentDialogueIndex = 0;

    private bool dopplerTapped = false;
    private bool gameCompleted = false;

    // Typewriter
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Show normal Doppler
        if (dopplerNormal != null)
            dopplerNormal.SetActive(true);

        // Hide zoomed Doppler
        if (dopplerZoomed != null)
            dopplerZoomed.SetActive(false);

        // Hide lubricant
        if (lubricant != null)
            lubricant.SetActive(false);

        // Hide next part
        if (nextGameObject != null)
            nextGameObject.SetActive(false);

        // Start Instruction 1
        StartInstruction(1);
    }


    // =========================================================
    // START INSTRUCTION
    // =========================================================

    private void StartInstruction(int instructionNumber)
    {
        currentInstruction = instructionNumber;
        currentDialogueIndex = 0;

        // Show assigned GameObjects when this instruction starts.
        ShowObjectsForCurrentInstruction();

        // Disable assigned GameObjects when this instruction dialogue starts.
        DisableObjectsForCurrentInstructionOnStart();

        List<string> selectedInstruction = GetCurrentInstruction();

        if (selectedInstruction == null ||
            selectedInstruction.Count == 0)
        {
            FinishInstruction();
            return;
        }

        ShowCurrentDialogue();
    }


    // =========================================================
    // GET CURRENT INSTRUCTION
    // =========================================================

    private List<string> GetCurrentInstruction()
    {
        switch (currentInstruction)
        {
            case 1:
                return instruction1;

            case 2:
                return instruction2;

            default:
                return null;
        }
    }


    // =========================================================
    // SHOW CURRENT LINE
    // =========================================================

    private void ShowCurrentDialogue()
    {
        List<string> selectedInstruction = GetCurrentInstruction();

        if (selectedInstruction == null ||
            selectedInstruction.Count == 0)
        {
            FinishInstruction();
            return;
        }


        // If there are no more lines
        if (currentDialogueIndex >= selectedInstruction.Count)
        {
            FinishInstruction();
            return;
        }


        dialogueActive = true;


        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);


        string text = selectedInstruction[currentDialogueIndex];

        currentFullText = text;


        // -----------------------------------------------------
        // PREPARE DIALOGUE
        // -----------------------------------------------------

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }


        // -----------------------------------------------------
        // PLAY DIALOGUE ANIMATIONS
        // -----------------------------------------------------

        PrepareDialogue(text);


        // -----------------------------------------------------
        // START TYPEWRITER
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(
                TypewriterEffect(text)
            );


        Debug.Log(
            "Instruction " +
            currentInstruction +
            " - Dialogue " +
            (currentDialogueIndex + 1) +
            ": " +
            text
        );
    }


    // =========================================================
    // TYPEWRITER EFFECT
    // =========================================================

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        int characterCounter = 0;


        for (int i = 0; i < text.Length; i++)
        {
            // Add one character
            if (dialogueText != null)
            {
                dialogueText.text += text[i];
            }


            // -------------------------------------------------
            // TYPEWRITER SOUND
            // -------------------------------------------------

            if (!char.IsWhiteSpace(text[i]))
            {
                characterCounter++;

                if (characterCounter >= soundEveryCharacters)
                {
                    if (
                        typewriterSound != null &&
                        AudioManager.Instance != null
                    )
                    {
                        AudioManager.Instance.PlayDialogue(
                            typewriterSound,
                            typewriterVolume
                        );
                    }

                    characterCounter = 0;
                }
            }


            // -------------------------------------------------
            // WAIT
            // -------------------------------------------------

            yield return new WaitForSeconds(typingSpeed);
        }


        // Make sure the complete text is displayed
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }

        isTyping = false;
        typingCoroutine = null;
    }


    // =========================================================
    // PREPARE DIALOGUE ANIMATIONS
    // =========================================================

    private void PrepareDialogue(string text)
    {
        // Auto Size
        if (autoSize != null)
        {
            if (autoSize.enabled &&
                autoSize.gameObject.activeInHierarchy)
            {
                autoSize.ResizeForText(text);
            }
        }


        // Pop Animation
        if (popAnimation != null)
        {
            if (popAnimation.enabled &&
                popAnimation.gameObject.activeInHierarchy)
            {
                popAnimation.PlayPop();
            }
        }


        // Triangle Animation
        if (triangleAnimation != null)
        {
            if (triangleAnimation.enabled &&
                triangleAnimation.gameObject.activeInHierarchy)
            {
                triangleAnimation.PlayAnimation();
            }
        }
    }


    // =========================================================
    // ADVANCE DIALOGUE
    // =========================================================

    public void AdvanceDialogue()
    {
        if (!dialogueActive)
            return;


        // -----------------------------------------------------
        // IF STILL TYPING
        // Finish the current line first
        // -----------------------------------------------------

        if (isTyping)
        {
            FinishTyping();
            return;
        }


        List<string> selectedInstruction =
            GetCurrentInstruction();

        if (selectedInstruction == null)
            return;


        currentDialogueIndex++;


        // -----------------------------------------------------
        // MORE DIALOGUE LINES REMAIN
        // -----------------------------------------------------

        if (currentDialogueIndex < selectedInstruction.Count)
        {
            ShowCurrentDialogue();
            return;
        }


        // -----------------------------------------------------
        // CURRENT INSTRUCTION FINISHED
        // -----------------------------------------------------

        FinishInstruction();
    }


    // =========================================================
    // FINISH TYPING
    // =========================================================

    private void FinishTyping()
    {
        if (!isTyping)
            return;


        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
        }


        isTyping = false;
    }


    // =========================================================
    // INSTRUCTION GAMEOBJECTS
    // =========================================================

    private GameObject[] GetObjectsToShowForCurrentInstruction()
    {
        switch (currentInstruction)
        {
            case 1:
                return instruction1ObjectsToShow;

            case 2:
                return instruction2ObjectsToShow;

            default:
                return null;
        }
    }


    private GameObject[] GetObjectsToDisableOnStartForCurrentInstruction()
    {
        switch (currentInstruction)
        {
            case 1:
                return instruction1ObjectsToDisableOnStart;

            case 2:
                return instruction2ObjectsToDisableOnStart;

            default:
                return null;
        }
    }


    private GameObject[] GetObjectsToHideForCurrentInstruction()
    {
        switch (currentInstruction)
        {
            case 1:
                return instruction1ObjectsToHide;

            case 2:
                return instruction2ObjectsToHide;

            default:
                return null;
        }
    }


    private void ShowObjectsForCurrentInstruction()
    {
        GameObject[] objectsToShow =
            GetObjectsToShowForCurrentInstruction();

        if (objectsToShow == null)
            return;

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }


    private void DisableObjectsForCurrentInstructionOnStart()
    {
        GameObject[] objectsToDisable =
            GetObjectsToDisableOnStartForCurrentInstruction();

        if (objectsToDisable == null)
            return;

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }


    private void HideObjectsForCurrentInstruction()
    {
        GameObject[] objectsToHide =
            GetObjectsToHideForCurrentInstruction();

        if (objectsToHide == null)
            return;

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }


    // =========================================================
    // FINISH INSTRUCTION
    // =========================================================

    private void FinishInstruction()
    {
        // Stop typewriter
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;


        dialogueActive = false;


        // Hide assigned GameObjects when this instruction finishes.
        HideObjectsForCurrentInstruction();


        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        Debug.Log(
            "Instruction " +
            currentInstruction +
            " completed."
        );


        // -----------------------------------------------------
        // INSTRUCTION 1 FINISHED
        // Player can now tap Doppler
        // -----------------------------------------------------

        if (currentInstruction == 1)
        {
            Debug.Log(
                "Instruction 1 finished. Doppler enabled."
            );
        }


        // -----------------------------------------------------
        // INSTRUCTION 2 FINISHED
        // Enable lubricant
        // -----------------------------------------------------

        else if (currentInstruction == 2)
        {
            EnableLubricant();
        }
    }


    // =========================================================
    // DOPPLER TAP
    // =========================================================

    public void TapDoppler()
    {
        // Don't allow tapping while dialogue is active
        if (dialogueActive)
            return;

        if (dopplerTapped)
            return;

        if (gameCompleted)
            return;


        dopplerTapped = true;

        // Play a soft tap sound when the player taps the Doppler.
        if (dopplerTapSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                dopplerTapSound,
                dopplerTapSoundVolume
            );
        }

        StartCoroutine(
            DopplerSequence()
        );
    }


    // =========================================================
    // DOPPLER SEQUENCE
    // =========================================================

    private IEnumerator DopplerSequence()
    {
        Debug.Log("Doppler tapped.");


        // -----------------------------------------------------
        // Hide normal Doppler
        // -----------------------------------------------------

        if (dopplerNormal != null)
            dopplerNormal.SetActive(false);


        // -----------------------------------------------------
        // Show zoomed Doppler
        // -----------------------------------------------------

        if (dopplerZoomed != null)
        {
            dopplerZoomed.SetActive(true);

            // Play the pop/show sound when the zoomed Doppler appears.
            if (dopplerPopSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    dopplerPopSound,
                    dopplerPopSoundVolume
                );
            }

            RectTransform zoomedRect =
                dopplerZoomed.GetComponent<RectTransform>();


            if (zoomedRect != null)
            {
                yield return StartCoroutine(
                    PopIn(zoomedRect)
                );
            }
        }


        // -----------------------------------------------------
        // Start Instruction 2
        // -----------------------------------------------------

        StartInstruction(2);


        Debug.Log(
            "Zoomed Doppler ready."
        );
    }


    // =========================================================
    // ENABLE LUBRICANT
    // =========================================================

    private void EnableLubricant()
    {
        if (lubricant != null)
            lubricant.SetActive(true);


        Debug.Log(
            "Lubricant interaction enabled."
        );
    }


    // =========================================================
    // POP ANIMATION
    // =========================================================

    private IEnumerator PopIn(RectTransform target)
    {
        Vector3 finalScale =
            target.localScale;

        target.localScale =
            Vector3.zero;

        float elapsed = 0f;


        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed / popDuration
                );


            float scale;


            if (t < 0.7f)
            {
                float firstPhase =
                    t / 0.7f;


                scale =
                    Mathf.Lerp(
                        0f,
                        popOvershoot,
                        firstPhase
                    );
            }
            else
            {
                float secondPhase =
                    (t - 0.7f) / 0.3f;


                scale =
                    Mathf.Lerp(
                        popOvershoot,
                        1f,
                        secondPhase
                    );
            }


            target.localScale =
                finalScale * scale;


            yield return null;
        }


        target.localScale =
            finalScale;
    }


    // =========================================================
    // COMPLETE MINI-GAME
    // =========================================================

    [Header("Completion Transition")]
    [Tooltip("How long to wait after the lubricant disappears before showing the next game object.")]
    public float completionDelay = 1.5f;


    public void CompleteMiniGame()
    {
        if (gameCompleted)
            return;

        gameCompleted = true;

        Debug.Log("Doppler mini-game completed!");

        StartCoroutine(CompleteMiniGameSequence());
    }


    // =========================================================
    // COMPLETE MINI-GAME SEQUENCE
    // =========================================================

    private IEnumerator CompleteMiniGameSequence()
    {
        // -----------------------------------------------------
        // Hide dialogue
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        // -----------------------------------------------------
        // Hide lubricant immediately
        // -----------------------------------------------------

        if (lubricant != null)
            lubricant.SetActive(false);


        Debug.Log(
            "Lubricant finished. Waiting " +
            completionDelay +
            " seconds before next part."
        );


        // -----------------------------------------------------
        // Wait before changing GameObjects
        // -----------------------------------------------------

        yield return new WaitForSeconds(completionDelay);


        // -----------------------------------------------------
        // Hide current part
        // -----------------------------------------------------

        if (currentGameObject != null)
            currentGameObject.SetActive(false);


        // -----------------------------------------------------
        // Show next part
        // -----------------------------------------------------

        if (nextGameObject != null)
            nextGameObject.SetActive(true);


        Debug.Log("Next game object shown.");
    }
}