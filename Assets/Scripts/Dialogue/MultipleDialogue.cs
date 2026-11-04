using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultipleDialogue : MonoBehaviour
{
    // =========================================================
    // FACING
    // =========================================================

    public enum FacingDirection
    {
        None,
        Left,
        Right
    }

    // =========================================================
    // DIALOGUE START MODE
    // =========================================================

    public enum DialogueStartMode
    {
        Automatic,
        Manual
    }

    [Header("DIALOGUE START")]
    [Tooltip("Automatic = dialogue starts when the object starts. Manual = dialogue starts only when StartDialogue() is called.")]
    [SerializeField] private DialogueStartMode startMode = DialogueStartMode.Automatic;

    // =========================================================
    // GAMEOBJECTS TO SHOW ON DIALOGUE START
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE START")]
    [Tooltip("GameObjects to show when the dialogue starts.")]
    [SerializeField] private GameObject[] objectsToShowOnDialogueStart;

    // =========================================================
    // GAMEOBJECTS TO HIDE ON DIALOGUE START
    // =========================================================

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE START")]
    [Tooltip("GameObjects to hide when the dialogue starts.")]
    [SerializeField] private GameObject[] objectsToHideOnDialogueStart;

    // =========================================================
    // CHARACTER
    // =========================================================

    [System.Serializable]
    public class Character
    {
        [Header("CHARACTER")]
        [Tooltip("Developer-only label. This does NOT appear in the game.")]
        public string speakerLabel = "Speaker";

        [Tooltip("Character UI GameObject.")]
        public GameObject characterObject;

        [Tooltip("Dialogue bubble used by this character.")]
        public GameObject dialogueBubble;

        [Tooltip("Dialogue text used by this character.")]
        public TextMeshProUGUI dialogueText;

        [Header("ANIMATIONS")]
        [Tooltip("Automatically resizes the dialogue bubble.")]
        public DialogueBoxAutoSize autoSize;

        [Tooltip("Pop animation for the dialogue bubble.")]
        public DialoguePopAnimation popAnimation;

        [Tooltip("Triangle animation for the dialogue bubble.")]
        public DialogueTriangleAnimation triangleAnimation;
    }

    // =========================================================
    // DIALOGUE DATA
    // =========================================================

    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Index of the character who is speaking.")]
        public int speakerIndex;

        [TextArea(2, 5)]
        public string text;

        [Tooltip("Choose which direction the character faces.")]
        public FacingDirection facing;
    }

    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("CHARACTERS")]
    [Tooltip("Add all characters that can speak.")]
    [SerializeField] private Character[] characters;

    [Header("DIALOGUE")]
    [Tooltip("Add dialogue lines in the order they should appear.")]
    [SerializeField] private DialogueLine[] dialogues;

    [Tooltip("Seconds between each typed character. Lower = faster.")]
    [SerializeField] private float typingSpeed = 0.03f;

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
    // AFTER DIALOGUE
    // =========================================================

    public enum TransitionType
    {
        None,
        DisableAndEnable,
        UnityScene
    }

    [Header("AFTER DIALOGUE")]
    [Tooltip("Choose what happens after the final dialogue.")]
    [SerializeField] private TransitionType transitionType =
        TransitionType.DisableAndEnable;

    // =========================================================
    // GAMEOBJECTS TO SHOW
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW")]
    [Tooltip("GameObjects to show after the dialogue finishes.")]
    [SerializeField] private GameObject[] objectsToShow;

    // =========================================================
    // GAMEOBJECTS TO HIDE
    // =========================================================

    [Header("GAMEOBJECTS TO HIDE")]
    [Tooltip("GameObjects to hide after the dialogue finishes.")]
    [SerializeField] private GameObject[] objectsToHide;

    // =========================================================
    // UNITY SCENE
    // =========================================================

    [Header("UNITY SCENE")]
    [Tooltip("Used only when Transition Type is UnityScene.")]
    [SerializeField] private string nextSceneName = "";

    // =========================================================
    // QUIZ
    // =========================================================

    [Header("QUIZ")]
    [Tooltip("Optional quiz object to open after dialogue.")]
    [SerializeField] private GameObject quizObject;

    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private bool dialogueFinished = false;

    private bool dialogueStarted = false;

    private bool isTransitioning = false;

    private Coroutine typingCoroutine;

    private string currentFullText = "";

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        HideAllBubbles();

        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "MultipleDialogue: No dialogues assigned!",
                this
            );

            return;
        }

        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning(
                "MultipleDialogue: No characters assigned!",
                this
            );

            return;
        }

        // Dialogue has not started yet.
        dialogueStarted = false;
        dialogueFinished = false;
        isTransitioning = false;

        // =====================================================
        // AUTOMATIC MODE
        // =====================================================

        if (startMode == DialogueStartMode.Automatic)
        {
            currentDialogueIndex = 0;

            dialogueStarted = true;
            dialogueFinished = false;

            ApplyDialogueStartObjects();
            ShowDialogue();
        }

        // =====================================================
        // MANUAL MODE
        // =====================================================

        else
        {
            // Do NOT start the dialogue.
            // Wait for StartDialogue() to be called by a Button.
            dialogueStarted = false;
            dialogueFinished = false;
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    // =========================================================
    // DIALOGUE INPUT
    // =========================================================
    // This method is called by the GLOBAL Dialogue Input Button.
    // Do not assign a Dialogue Input GameObject here.
    // =========================================================

    public void AdvanceDialogue()
    {
        if (!dialogueStarted)
            return;

        if (dialogueFinished)
            return;

        if (isTransitioning)
            return;

        // If text is still typing, finish it immediately.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // Otherwise, advance to the next dialogue.
        NextDialogue();
    }

    // =========================================================
    // SHOW DIALOGUE
    // =========================================================

    private void ShowDialogue()
    {
        if (dialogues == null ||
            dialogues.Length == 0 ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length)
        {
            return;
        }

        DialogueLine dialogue = dialogues[currentDialogueIndex];

        // =====================================================
        // VALIDATE SPEAKER
        // =====================================================

        if (dialogue.speakerIndex < 0 ||
            dialogue.speakerIndex >= characters.Length)
        {
            Debug.LogWarning(
                "MultipleDialogue: Invalid speaker index: " +
                dialogue.speakerIndex,
                this
            );

            return;
        }

        Character character = characters[dialogue.speakerIndex];

        if (character == null)
        {
            Debug.LogWarning(
                "MultipleDialogue: Character is null at index " +
                dialogue.speakerIndex,
                this
            );

            return;
        }

        // =====================================================
        // BABY NAME REPLACEMENT
        // =====================================================

        string text = dialogue.text ?? "";

        string babyName = "";

        if (SaveManager.Instance != null)
        {
            babyName = SaveManager.Instance.GetBabyName();
        }

        if (!string.IsNullOrEmpty(babyName))
        {
            text = text.Replace(
                "{babyName}",
                babyName
            );
        }

        // =====================================================
        // HIDE ALL BUBBLES
        // =====================================================

        HideAllBubbles();

        // =====================================================
        // SHOW CURRENT SPEAKER
        // =====================================================

        if (character.dialogueBubble != null)
        {
            character.dialogueBubble.SetActive(true);
        }

        if (character.dialogueText != null)
        {
            character.dialogueText.text = "";
        }

        // =====================================================
        // APPLY FACING
        // =====================================================

        ApplyFacing(
            character.characterObject,
            dialogue.facing
        );

        // =====================================================
        // PREPARE BUBBLE
        // =====================================================

        PrepareCharacter(
            character,
            text
        );

        // =====================================================
        // SAVE FULL TEXT
        // =====================================================

        currentFullText = text;

        // =====================================================
        // STOP OLD TYPEWRITER
        // =====================================================

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // =====================================================
        // START TYPEWRITER
        // =====================================================

        typingCoroutine = StartCoroutine(
            TypeText(
                character.dialogueText,
                text
            )
        );
    }

    // =========================================================
    // APPLY FACING
    // =========================================================

    private void ApplyFacing(
        GameObject character,
        FacingDirection facing
    )
    {
        if (character == null)
            return;

        if (facing == FacingDirection.None)
            return;

        RectTransform rectTransform =
            character.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogWarning(
                "MultipleDialogue: Character does not have a RectTransform.",
                character
            );

            return;
        }

        Vector3 scale = rectTransform.localScale;

        float absoluteX = Mathf.Abs(scale.x);

        if (absoluteX == 0f)
            absoluteX = 1f;

        switch (facing)
        {
            case FacingDirection.Left:

                scale.x = -absoluteX;

                break;

            case FacingDirection.Right:

                scale.x = absoluteX;

                break;
        }

        rectTransform.localScale = scale;
    }

    // =========================================================
    // PREPARE CHARACTER
    // =========================================================

    private void PrepareCharacter(
        Character character,
        string text
    )
    {
        if (character == null)
            return;

        // =====================================================
        // AUTO SIZE
        // =====================================================

        if (character.autoSize != null)
        {
            if (
                character.autoSize.enabled &&
                character.autoSize.gameObject.activeInHierarchy
            )
            {
                character.autoSize.ResizeForText(text);
            }
        }

        // =====================================================
        // POP
        // =====================================================

        if (character.popAnimation != null)
        {
            if (
                character.popAnimation.enabled &&
                character.popAnimation.gameObject.activeInHierarchy
            )
            {
                character.popAnimation.PlayPop();
            }
        }

        // =====================================================
        // TRIANGLE
        // =====================================================

        if (character.triangleAnimation != null)
        {
            if (
                character.triangleAnimation.enabled &&
                character.triangleAnimation.gameObject.activeInHierarchy
            )
            {
                character.triangleAnimation.PlayAnimation();
            }
        }
    }

    // =========================================================
    // TYPEWRITER
    // =========================================================

    private IEnumerator TypeText(
        TextMeshProUGUI textComponent,
        string text
    )
    {
        isTyping = true;

        if (textComponent != null)
        {
            textComponent.text = "";
        }

        string safeText = text ?? "";

        float safeTypingSpeed =
            Mathf.Max(
                typingSpeed,
                0.001f
            );

        int characterCount = 0;

        foreach (char character in safeText)
        {
            if (textComponent != null)
            {
                textComponent.text += character;
            }

            characterCount++;

            // =================================================
            // TYPEWRITER SOUND
            // =================================================

            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                character != ' ' &&
                characterCount %
                Mathf.Max(soundEveryCharacters, 1) == 0
            )
            {
                AudioManager.Instance.PlayDialogue(
                    typewriterSound,
                    typewriterVolume
                );
            }

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

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (
            dialogues == null ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length
        )
        {
            return;
        }

        DialogueLine dialogue =
            dialogues[currentDialogueIndex];

        if (
            dialogue.speakerIndex >= 0 &&
            dialogue.speakerIndex < characters.Length
        )
        {
            Character character =
                characters[dialogue.speakerIndex];

            if (character != null &&
                character.dialogueText != null)
            {
                character.dialogueText.text =
                    currentFullText;
            }
        }

        isTyping = false;
    }

    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    private void NextDialogue()
    {
        if (isTransitioning)
            return;

        currentDialogueIndex++;

        if (
            currentDialogueIndex >=
            dialogues.Length
        )
        {
            FinishDialogue();
            return;
        }

        ShowDialogue();
    }

    // =========================================================
    // FINISH DIALOGUE
    // =========================================================

    private void FinishDialogue()
    {
        if (dialogueFinished || isTransitioning)
            return;

        dialogueFinished = true;

        // IMPORTANT:
        // Screen taps stop working after dialogue ends.
        dialogueStarted = false;

        isTransitioning = true;

        // =====================================================
        // STOP TYPING
        // =====================================================

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        // =====================================================
        // HIDE DIALOGUE BUBBLES
        // =====================================================

        HideAllBubbles();

        // =====================================================
        // QUIZ
        // =====================================================

        if (quizObject != null)
        {
            quizObject.SetActive(true);
        }

        // =====================================================
        // UNITY SCENE
        // =====================================================

        if (
            transitionType ==
            TransitionType.UnityScene
        )
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError(
                    "MultipleDialogue: Next Scene Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            Debug.Log(
                "MultipleDialogue: Loading scene: " +
                nextSceneName,
                this
            );

            SceneManager.LoadScene(
                nextSceneName
            );

            return;
        }

        // =====================================================
        // SHOW / HIDE GAMEOBJECTS
        // =====================================================

        if (
            transitionType ==
            TransitionType.DisableAndEnable
        )
        {
            // -------------------------------------------------
            // HIDE
            // -------------------------------------------------

            if (objectsToHide != null)
            {
                foreach (GameObject obj in objectsToHide)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);

                        Debug.Log(
                            "MultipleDialogue: Hidden: " +
                            obj.name,
                            this
                        );
                    }
                }
            }

            // -------------------------------------------------
            // SHOW
            // -------------------------------------------------

            if (objectsToShow != null)
            {
                foreach (GameObject obj in objectsToShow)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);

                        Debug.Log(
                            "MultipleDialogue: Shown: " +
                            obj.name,
                            this
                        );
                    }
                }
            }

            isTransitioning = false;

            return;
        }

        // =====================================================
        // NONE
        // =====================================================

        if (
            transitionType ==
            TransitionType.None
        )
        {
            isTransitioning = false;

            Debug.Log(
                "MultipleDialogue: Dialogue finished with no transition.",
                this
            );
        }
    }

    // =========================================================
    // APPLY OBJECTS ON DIALOGUE START
    // =========================================================

    private void ApplyDialogueStartObjects()
    {
        // -----------------------------------------------------
        // SHOW
        // -----------------------------------------------------

        if (objectsToShowOnDialogueStart != null)
        {
            foreach (GameObject obj in objectsToShowOnDialogueStart)
            {
                if (obj != null)
                {
                    obj.SetActive(true);

                    Debug.Log(
                        "MultipleDialogue: Shown on dialogue start: " +
                        obj.name,
                        this
                    );
                }
            }
        }

        // -----------------------------------------------------
        // HIDE
        // -----------------------------------------------------

        if (objectsToHideOnDialogueStart != null)
        {
            foreach (GameObject obj in objectsToHideOnDialogueStart)
            {
                if (obj != null)
                {
                    obj.SetActive(false);

                    Debug.Log(
                        "MultipleDialogue: Hidden on dialogue start: " +
                        obj.name,
                        this
                    );
                }
            }
        }
    }

    // =========================================================
    // HIDE ALL BUBBLES
    // =========================================================

    private void HideAllBubbles()
    {
        if (characters == null)
            return;

        foreach (Character character in characters)
        {
            if (character == null)
                continue;

            if (character.dialogueBubble != null)
            {
                character.dialogueBubble.SetActive(false);
            }
        }
    }

    // =========================================================
    // PUBLIC START DIALOGUE
    // =========================================================
    //
    // IMPORTANT:
    // This is the method you connect to your BUTTON.
    //
    // Press button ONCE:
    //      ↓
    // Dialogue starts
    //      ↓
    // Screen taps control the remaining dialogue
    //
    // =========================================================

    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "MultipleDialogue: No dialogues assigned!",
                this
            );

            return;
        }

        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning(
                "MultipleDialogue: No characters assigned!",
                this
            );

            return;
        }

        // =====================================================
        // STOP OLD TYPEWRITER
        // =====================================================

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // =====================================================
        // RESET DIALOGUE
        // =====================================================

        currentDialogueIndex = 0;

        dialogueFinished = false;

        isTransitioning = false;

        isTyping = false;

        // =====================================================
        // THIS IS THE IMPORTANT PART
        //
        // Screen taps are now allowed because the dialogue
        // has officially started.
        // =====================================================

        dialogueStarted = true;

        ApplyDialogueStartObjects();

        // =====================================================
        // SHOW FIRST DIALOGUE
        // =====================================================

        ShowDialogue();
    }

    // =========================================================
    // PUBLIC CLOSE DIALOGUE
    // =========================================================

    public void CloseDialogue()
    {
        if (isTransitioning)
            return;

        FinishDialogue();
    }
}