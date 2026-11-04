using UnityEngine;
using TMPro;

public class ScaleGameManager : MonoBehaviour
{
    [Header("Weight Sliders")]
    public WeightSlider largeWeight;
    public WeightSlider smallWeight;

    // =========================================================
    // GAMEOBJECTS ON DIALOGUE START
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE START")]
    [Tooltip("GameObjects to show whenever an instruction dialogue starts.")]
    public GameObject[] gameObjectsToShowOnDialogueStart;

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE START")]
    [Tooltip("GameObjects to hide whenever an instruction dialogue starts.")]
    public GameObject[] gameObjectsToHideOnDialogueStart;


    // =========================================================
    // INSTRUCTION CHAT BUBBLE
    // =========================================================

    [Header("Instruction Chat Bubble")]
    public GameObject instructionBubble;
    public TMP_Text instructionText;


    // =========================================================
    // GAMEOBJECTS ON DIALOGUE FINISH
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE FINISH")]
    [Tooltip("GameObjects to show when an instruction dialogue finishes.")]
    public GameObject[] gameObjectsToShowOnDialogueFinish;

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE FINISH")]
    [Tooltip("GameObjects to hide when an instruction dialogue finishes.")]
    public GameObject[] gameObjectsToHideOnDialogueFinish;

    [Header("DIALOGUE ANIMATIONS")]
    public DialogueBoxAutoSize autoSize;
    public DialoguePopAnimation popAnimation;
    public DialogueTriangleAnimation triangleAnimation;

    [TextArea(2, 3)]
    public string largeWeightInstruction =
        "MOVE THE LARGE WEIGHT TO THE GREEN LINE.";

    [TextArea(2, 3)]
    public string smallWeightInstruction =
        "THEN MOVE THE SMALL WEIGHT TO THE GREEN LINE.";

    [Header("Error Message")]
    public TMP_Text errorText;

    [TextArea(2, 3)]
    public string largeWeightError =
        "PLACE THE LARGE WEIGHT ON THE GREEN LINE.";

    [TextArea(2, 3)]
    public string smallWeightError =
        "PLACE THE SMALL WEIGHT ON THE GREEN LINE.";

    public float errorDisplayDuration = 2f;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Tooltip("One typewriter sound used for the instruction text.")]
    public AudioClip typewriterSound;

    [Tooltip("Play the typewriter sound every X characters.")]
    public int soundEveryCharacters = 2;

    [Range(0f, 1f)]
    public float typewriterVolume = 1f;

    [Header("Snap Sound")]
    [Tooltip("One popping sound shared by both slider snaps.")]
    public AudioClip snapPopSound;

    [Range(0f, 1f)]
    public float snapPopVolume = 1f;

    [Header("\"DRAG\" Hints")]
    public GameObject largeWeightDragHint;
    public GameObject smallWeightDragHint;

    [Header("On Game Complete")]
    public GameObject prevParent;
    public GameObject nextParent;


    // =========================================================
    // STEP
    // =========================================================

    private enum ScaleStep
    {
        LargeWeight,
        SmallWeight,
        Complete
    }

    private ScaleStep currentStep;


    // =========================================================
    // TYPEWRITER
    // =========================================================

    private string activeFullLine = "";
    private int visibleChars = 0;
    private float charTimer = 0f;
    private bool isTyping = false;
    private bool instructionWaitingForClick = false;

    // Counts characters for typewriter sound timing.
    private int typewriterCharacterCount = 0;


    // =========================================================
    // ERROR
    // =========================================================

    private float errorTimer = 0f;
    private bool showingError = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentStep = ScaleStep.LargeWeight;

        // -----------------------------------------------------
        // Lock both weights at the beginning.
        // -----------------------------------------------------

        if (largeWeight != null)
        {
            largeWeight.Lock();
        }

        if (smallWeight != null)
        {
            smallWeight.Lock();
        }


        // -----------------------------------------------------
        // Hide drag hints.
        // -----------------------------------------------------

        if (largeWeightDragHint != null)
        {
            largeWeightDragHint.SetActive(false);
        }

        if (smallWeightDragHint != null)
        {
            smallWeightDragHint.SetActive(false);
        }


        // -----------------------------------------------------
        // Hide instruction bubble.
        // -----------------------------------------------------

        if (instructionBubble != null)
        {
            instructionBubble.SetActive(false);
        }


        // -----------------------------------------------------
        // Hide error text.
        // -----------------------------------------------------

        HideError();


        // -----------------------------------------------------
        // Listen for successful snapping.
        // -----------------------------------------------------

        if (largeWeight != null)
        {
            largeWeight.onSnapped.AddListener(
                OnLargeWeightSnapped
            );

            largeWeight.onInvalidDrop.AddListener(
                OnLargeWeightInvalidDrop
            );
        }


        if (smallWeight != null)
        {
            smallWeight.onSnapped.AddListener(
                OnSmallWeightSnapped
            );

            smallWeight.onInvalidDrop.AddListener(
                OnSmallWeightInvalidDrop
            );
        }


        // -----------------------------------------------------
        // Start first step.
        // -----------------------------------------------------

        StartLargeWeightStep();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateTypewriter();
        UpdateErrorMessage();
    }


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    public void AdvanceDialogue()
    {
        if (
            instructionBubble == null ||
            !instructionBubble.activeSelf
        )
        {
            return;
        }


        // =====================================================
        // FIRST TAP
        // =====================================================

        // If text is still typing,
        // complete it immediately.

        if (isTyping)
        {
            CompleteTyping();
            return;
        }


        // =====================================================
        // SECOND TAP
        // =====================================================

        // Text is complete.
        // Finish the instruction dialogue.

        HideInstruction();
    }


    // =========================================================
    // LARGE WEIGHT STEP
    // =========================================================

    private void StartLargeWeightStep()
    {
        currentStep = ScaleStep.LargeWeight;


        // -----------------------------------------------------
        // Make absolutely sure the small weight is locked.
        // -----------------------------------------------------

        if (smallWeight != null)
        {
            smallWeight.Lock();
        }


        // -----------------------------------------------------
        // Large weight must also be locked while instruction
        // is displayed.
        // -----------------------------------------------------

        if (largeWeight != null)
        {
            largeWeight.Lock();
        }


        // -----------------------------------------------------
        // Show instruction.
        // -----------------------------------------------------

        ShowInstruction(
            largeWeightInstruction
        );


        // -----------------------------------------------------
        // Show large drag hint.
        // -----------------------------------------------------

        if (largeWeightDragHint != null)
        {
            largeWeightDragHint.SetActive(true);
        }


        // -----------------------------------------------------
        // Hide small hint.
        // -----------------------------------------------------

        if (smallWeightDragHint != null)
        {
            smallWeightDragHint.SetActive(false);
        }
    }


    // =========================================================
    // LARGE WEIGHT SUCCESS
    // =========================================================

    private void OnLargeWeightSnapped()
    {
        if (currentStep != ScaleStep.LargeWeight)
        {
            return;
        }


        Debug.Log(
            "Large weight successfully placed."
        );


        // -----------------------------------------------------
        // Play shared snap/pop sound.
        // Uses AudioManager -> SFXSource.
        // -----------------------------------------------------

        PlaySnapSound();


        // -----------------------------------------------------
        // Large weight is finished.
        // -----------------------------------------------------

        if (largeWeightDragHint != null)
        {
            largeWeightDragHint.SetActive(false);
        }


        if (largeWeight != null)
        {
            largeWeight.Lock();
        }


        // -----------------------------------------------------
        // Now move to the small weight.
        // -----------------------------------------------------

        StartSmallWeightStep();
    }


    // =========================================================
    // LARGE WEIGHT INVALID DROP
    // =========================================================

    private void OnLargeWeightInvalidDrop()
    {
        if (currentStep != ScaleStep.LargeWeight)
        {
            return;
        }


        Debug.Log(
            "Large weight placed incorrectly."
        );


        ShowError(
            largeWeightError
        );
    }


    // =========================================================
    // SMALL WEIGHT STEP
    // =========================================================

    private void StartSmallWeightStep()
    {
        currentStep = ScaleStep.SmallWeight;


        // -----------------------------------------------------
        // Large weight remains locked.
        // -----------------------------------------------------

        if (largeWeight != null)
        {
            largeWeight.Lock();
        }


        // -----------------------------------------------------
        // Small weight remains locked while instruction
        // is displayed.
        // -----------------------------------------------------

        if (smallWeight != null)
        {
            smallWeight.Lock();
        }


        // -----------------------------------------------------
        // Show instruction.
        // -----------------------------------------------------

        ShowInstruction(
            smallWeightInstruction
        );


        // -----------------------------------------------------
        // Show small drag hint.
        // -----------------------------------------------------

        if (smallWeightDragHint != null)
        {
            smallWeightDragHint.SetActive(true);
        }


        // -----------------------------------------------------
        // Hide large hint.
        // -----------------------------------------------------

        if (largeWeightDragHint != null)
        {
            largeWeightDragHint.SetActive(false);
        }
    }


    // =========================================================
    // SMALL WEIGHT SUCCESS
    // =========================================================

    private void OnSmallWeightSnapped()
    {
        if (currentStep != ScaleStep.SmallWeight)
        {
            return;
        }


        Debug.Log(
            "Small weight successfully placed."
        );


        // -----------------------------------------------------
        // Play same shared snap/pop sound.
        // Uses AudioManager -> SFXSource.
        // -----------------------------------------------------

        PlaySnapSound();


        currentStep = ScaleStep.Complete;


        // -----------------------------------------------------
        // Hide hint.
        // -----------------------------------------------------

        if (smallWeightDragHint != null)
        {
            smallWeightDragHint.SetActive(false);
        }


        // -----------------------------------------------------
        // Lock both.
        // -----------------------------------------------------

        if (largeWeight != null)
        {
            largeWeight.Lock();
        }

        if (smallWeight != null)
        {
            smallWeight.Lock();
        }


        // -----------------------------------------------------
        // Hide instruction.
        // -----------------------------------------------------

        HideInstructionWithoutUnlocking();


        // -----------------------------------------------------
        // Complete game.
        // -----------------------------------------------------

        CompleteGame();
    }


    // =========================================================
    // SMALL WEIGHT INVALID DROP
    // =========================================================

    private void OnSmallWeightInvalidDrop()
    {
        if (currentStep != ScaleStep.SmallWeight)
        {
            return;
        }


        Debug.Log(
            "Small weight placed incorrectly."
        );


        ShowError(
            smallWeightError
        );
    }


    // =========================================================
    // SNAP SOUND
    // =========================================================

    private void PlaySnapSound()
    {
        if (
            snapPopSound != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                snapPopSound,
                snapPopVolume
            );
        }
    }


    // =========================================================
    // DIALOGUE START GAMEOBJECTS
    // =========================================================

    private void ApplyDialogueStartGameObjects()
    {
        // -----------------------------------------------------
        // HIDE
        // -----------------------------------------------------

        if (gameObjectsToHideOnDialogueStart != null)
        {
            foreach (GameObject obj in gameObjectsToHideOnDialogueStart)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // -----------------------------------------------------
        // SHOW
        // -----------------------------------------------------

        if (gameObjectsToShowOnDialogueStart != null)
        {
            foreach (GameObject obj in gameObjectsToShowOnDialogueStart)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }


    // =========================================================
    // DIALOGUE FINISH GAMEOBJECTS
    // =========================================================

    private void ApplyDialogueFinishGameObjects()
    {
        // -----------------------------------------------------
        // HIDE
        // -----------------------------------------------------

        if (gameObjectsToHideOnDialogueFinish != null)
        {
            foreach (GameObject obj in gameObjectsToHideOnDialogueFinish)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // -----------------------------------------------------
        // SHOW
        // -----------------------------------------------------

        if (gameObjectsToShowOnDialogueFinish != null)
        {
            foreach (GameObject obj in gameObjectsToShowOnDialogueFinish)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }


    // =========================================================
    // SHOW INSTRUCTION
    // =========================================================

    private void ShowInstruction(string text)
    {
        // Apply the configured GameObjects for dialogue start
        // before showing the instruction bubble.
        ApplyDialogueStartGameObjects();

        if (instructionBubble != null)
        {
            instructionBubble.SetActive(true);
        }


        activeFullLine = text ?? "";

        visibleChars = 0;
        charTimer = 0f;

        isTyping = true;
        instructionWaitingForClick = true;

        // Reset sound counter for each new instruction.
        typewriterCharacterCount = 0;


        if (instructionText != null)
        {
            instructionText.text = "";
        }

        if (autoSize != null)
        {
            if (autoSize.enabled && autoSize.gameObject.activeInHierarchy)
            {
                autoSize.ResizeForText(text);
            }
        }

        if (popAnimation != null)
        {
            if (popAnimation.enabled && popAnimation.gameObject.activeInHierarchy)
            {
                popAnimation.PlayPop();
            }
        }

        if (triangleAnimation != null)
        {
            if (triangleAnimation.enabled && triangleAnimation.gameObject.activeInHierarchy)
            {
                triangleAnimation.PlayAnimation();
            }
        }


        // =====================================================
        // IMPORTANT
        // =====================================================

        // While instruction bubble is visible,
        // player cannot drag either weight.

        if (largeWeight != null)
        {
            largeWeight.Lock();
        }

        if (smallWeight != null)
        {
            smallWeight.Lock();
        }
    }


    // =========================================================
    // HIDE INSTRUCTION
    // =========================================================

    private void HideInstruction()
    {
        if (instructionBubble != null)
        {
            instructionBubble.SetActive(false);
        }

        // Apply the configured GameObjects for dialogue finish.
        ApplyDialogueFinishGameObjects();


        instructionWaitingForClick = false;


        // =====================================================
        // ONLY UNLOCK THE CURRENT STEP
        // =====================================================

        if (currentStep == ScaleStep.LargeWeight)
        {
            if (
                largeWeight != null &&
                !largeWeight.hasSnapped
            )
            {
                largeWeight.Unlock();
            }


            // Small weight MUST remain locked.

            if (smallWeight != null)
            {
                smallWeight.Lock();
            }
        }


        else if (currentStep == ScaleStep.SmallWeight)
        {
            // Large weight MUST remain locked.

            if (largeWeight != null)
            {
                largeWeight.Lock();
            }


            if (
                smallWeight != null &&
                !smallWeight.hasSnapped
            )
            {
                smallWeight.Unlock();
            }
        }
    }


    // =========================================================
    // HIDE INSTRUCTION WITHOUT UNLOCKING
    // =========================================================

    private void HideInstructionWithoutUnlocking()
    {
        if (instructionBubble != null)
        {
            instructionBubble.SetActive(false);
        }

        // Apply the configured GameObjects for dialogue finish.
        ApplyDialogueFinishGameObjects();


        instructionWaitingForClick = false;
        isTyping = false;
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    private void UpdateTypewriter()
    {
        if (!isTyping)
        {
            return;
        }


        if (instructionText == null)
        {
            return;
        }


        charTimer += Time.unscaledDeltaTime;


        // -----------------------------------------------------
        // Prevent zero or negative typing speed from
        // causing problems.
        // -----------------------------------------------------

        float actualTypingSpeed =
            Mathf.Max(typingSpeed, 0.001f);


        while (
            charTimer >= actualTypingSpeed &&
            visibleChars < activeFullLine.Length
        )
        {
            charTimer -= actualTypingSpeed;

            visibleChars++;


            instructionText.text =
                activeFullLine.Substring(
                    0,
                    visibleChars
                );


            // -------------------------------------------------
            // TYPEWRITER SOUND
            // -------------------------------------------------

            char character =
                activeFullLine[visibleChars - 1];

            typewriterCharacterCount++;


            // Don't play sound for spaces.
            // Uses AudioManager -> DialogueSource.

            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                character != ' ' &&
                typewriterCharacterCount %
                    Mathf.Max(soundEveryCharacters, 1) == 0
            )
            {
                AudioManager.Instance.PlayDialogue(
                    typewriterSound,
                    typewriterVolume
                );
            }
        }


        // -----------------------------------------------------
        // Finished automatically.
        // -----------------------------------------------------

        if (visibleChars >= activeFullLine.Length)
        {
            visibleChars =
                activeFullLine.Length;

            isTyping = false;
            instructionWaitingForClick = true;
        }
    }


    // =========================================================
    // COMPLETE TYPING
    // =========================================================

    private void CompleteTyping()
    {
        if (instructionText != null)
        {
            instructionText.text =
                activeFullLine;
        }


        visibleChars =
            activeFullLine.Length;

        isTyping = false;
        instructionWaitingForClick = true;
    }


    // =========================================================
    // SHOW ERROR
    // =========================================================

    private void ShowError(string message)
    {
        if (errorText == null)
        {
            return;
        }


        errorText.text = message;
        errorText.gameObject.SetActive(true);

        errorTimer = errorDisplayDuration;
        showingError = true;
    }


    // =========================================================
    // UPDATE ERROR
    // =========================================================

    private void UpdateErrorMessage()
    {
        if (!showingError)
        {
            return;
        }


        errorTimer -= Time.unscaledDeltaTime;


        if (errorTimer <= 0f)
        {
            HideError();
        }
    }


    // =========================================================
    // HIDE ERROR
    // =========================================================

    private void HideError()
    {
        showingError = false;
        errorTimer = 0f;


        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // COMPLETE GAME
    // =========================================================

    private void CompleteGame()
    {
        if (prevParent != null)
        {
            prevParent.SetActive(false);
        }


        if (nextParent != null)
        {
            nextParent.SetActive(true);
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (largeWeight != null)
        {
            largeWeight.onSnapped.RemoveListener(
                OnLargeWeightSnapped
            );

            largeWeight.onInvalidDrop.RemoveListener(
                OnLargeWeightInvalidDrop
            );
        }


        if (smallWeight != null)
        {
            smallWeight.onSnapped.RemoveListener(
                OnSmallWeightSnapped
            );

            smallWeight.onInvalidDrop.RemoveListener(
                OnSmallWeightInvalidDrop
            );
        }
    }
}
