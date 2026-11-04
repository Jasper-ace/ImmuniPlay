using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TwoDialogue : MonoBehaviour
{
    // =========================================================
    // SPEAKER
    // =========================================================

    public enum Speaker
    {
        Speaker1,
        Speaker2
    }

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
    // DIALOGUE DATA
    // =========================================================

    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Choose which speaker is speaking.")]
        public Speaker speaker;

        [TextArea(2, 5)]
        public string text;

        [Tooltip("Choose which direction the character faces.")]
        public FacingDirection facing;
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
    public DialogueStartMode startMode = DialogueStartMode.Automatic;

    // =========================================================
    // GAMEOBJECTS WHEN DIALOGUE STARTS
    // =========================================================

    [Header("WHEN DIALOGUE STARTS - SHOW")]
    [Tooltip("GameObjects to show when the dialogue starts.")]
    public GameObject[] gameObjectsToShowOnStart;

    [Header("WHEN DIALOGUE STARTS - HIDE")]
    [Tooltip("GameObjects to hide when the dialogue starts.")]
    public GameObject[] gameObjectsToHideOnStart;

    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("DIALOGUE")]
    [Tooltip("Add dialogue lines in the order they should appear.")]
    public DialogueLine[] dialogues;

    [Tooltip("Seconds between each typed character. Lower = faster.")]
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
    // SPEAKER 1
    // =========================================================

    [Header("SPEAKER 1")]
    [Tooltip("Developer-only label. This does NOT appear in the game.")]
    public string speaker1Label = "Speaker 1";

    [Tooltip("Dialogue bubble used by Speaker 1.")]
    public GameObject speaker1Bubble;

    [Tooltip("Dialogue text used by Speaker 1.")]
    public TextMeshProUGUI speaker1Text;

    [Tooltip("Character UI GameObject used by Speaker 1.")]
    public GameObject speaker1Character;

    // =========================================================
    // SPEAKER 1 ANIMATIONS
    // =========================================================

    [Header("SPEAKER 1 ANIMATIONS")]
    [Tooltip("Automatically resizes Speaker 1's dialogue bubble.")]
    public DialogueBoxAutoSize speaker1AutoSize;

    [Tooltip("Pop animation for Speaker 1's dialogue bubble.")]
    public DialoguePopAnimation speaker1PopAnimation;

    [Tooltip("Triangle animation for Speaker 1's dialogue bubble.")]
    public DialogueTriangleAnimation speaker1TriangleAnimation;

    // =========================================================
    // SPEAKER 2
    // =========================================================

    [Header("SPEAKER 2")]
    [Tooltip("Developer-only label. This does NOT appear in the game.")]
    public string speaker2Label = "Speaker 2";

    [Tooltip("Dialogue bubble used by Speaker 2.")]
    public GameObject speaker2Bubble;

    [Tooltip("Dialogue text used by Speaker 2.")]
    public TextMeshProUGUI speaker2Text;

    [Tooltip("Character UI GameObject used by Speaker 2.")]
    public GameObject speaker2Character;

    // =========================================================
    // SPEAKER 2 ANIMATIONS
    // =========================================================

    [Header("SPEAKER 2 ANIMATIONS")]
    [Tooltip("Automatically resizes Speaker 2's dialogue bubble.")]
    public DialogueBoxAutoSize speaker2AutoSize;

    [Tooltip("Pop animation for Speaker 2's dialogue bubble.")]
    public DialoguePopAnimation speaker2PopAnimation;

    [Tooltip("Triangle animation for Speaker 2's dialogue bubble.")]
    public DialogueTriangleAnimation speaker2TriangleAnimation;

    // =========================================================
    // AFTER DIALOGUE
    // =========================================================

    public enum TransitionType
    {
        None,
        UnityScene,
        DisableAndEnable,
        Quiz
    }

    [Header("AFTER DIALOGUE")]
    [Tooltip("Choose what happens after the final dialogue.")]
    public TransitionType transitionType =
        TransitionType.DisableAndEnable;

    // =========================================================
    // UNITY SCENE
    // =========================================================

    [Header("UNITY SCENE")]
    [Tooltip("Used only when Transition Type is UnityScene.")]
    public string nextSceneName = "";

    // =========================================================
    // GAMEOBJECTS TO SHOW
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW")]
    [Tooltip("GameObjects to show after the dialogue finishes.")]
    public GameObject[] gameObjectsToShow;

    // =========================================================
    // GAMEOBJECTS TO HIDE
    // =========================================================

    [Header("GAMEOBJECTS TO HIDE")]
    [Tooltip("GameObjects to hide after the dialogue finishes.")]
    public GameObject[] gameObjectsToHide;

    // =========================================================
    // QUIZ TRANSITION
    // =========================================================

    [Header("QUIZ TRANSITION")]
    [Tooltip("Quiz JSON filename without .json. Example: Chapter1")]
    public string quizChapterFileName = "Chapter1";

    [Tooltip("Scene containing the shared ChapterQuiz.")]
    public string quizSceneName = "ChapterQuiz";

    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private bool dialogueFinished = false;

    private bool isTransitioning = false;

    // NEW:
    // Prevents screen taps from doing anything
    // before a Manual dialogue is started.
    private bool dialogueStarted = false;

    private Coroutine typingCoroutine;

    private string currentFullText = "";

    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    [Header("DIALOGUE INPUT")]
    [Tooltip("Invisible UI input area used to advance the dialogue.")]
    public GameObject dialogueInput;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        HideAllBubbles();

        // Dialogue input should only be active while dialogue is running.
        if (dialogueInput != null)
            dialogueInput.SetActive(false);

        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "TwoDialogue: No dialogues assigned!",
                this
            );

            return;
        }

        // Reset dialogue state
        dialogueStarted = false;
        dialogueFinished = false;
        isTransitioning = false;
        isTyping = false;

        // =====================================================
        // AUTOMATIC MODE
        // =====================================================

        if (startMode == DialogueStartMode.Automatic)
        {
            currentDialogueIndex = 0;

            dialogueStarted = true;
            dialogueFinished = false;

            ApplyDialogueStartGameObjects();

            if (dialogueInput != null)
                dialogueInput.SetActive(true);

            ShowDialogue();
        }

        // =====================================================
        // MANUAL MODE
        // =====================================================
        // Do nothing here.
        //
        // The dialogue will wait until:
        //
        // Start Button
        //      ↓
        // StartDialogue()
        //
        // is called.
    }

    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    public void AdvanceDialogue()
    {
        // Do nothing if:
        // - Dialogue has not started yet
        // - Dialogue is already finished
        // - A transition is currently happening
        if (!dialogueStarted || dialogueFinished || isTransitioning)
            return;

        // =====================================================
        // TAP WHILE TEXT IS TYPING
        // =====================================================

        if (isTyping)
        {
            // Finish the current sentence immediately
            FinishTyping();
        }
        else
        {
            // =================================================
            // TAP AFTER TEXT FINISHED
            // =================================================

            // Go to the next dialogue
            NextDialogue();
        }
    }

    // =========================================================
    // SHOW DIALOGUE
    // =========================================================

    private void ShowDialogue()
    {
        if (
            dialogues == null ||
            dialogues.Length == 0 ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length
        )
        {
            return;
        }

        DialogueLine dialogue =
            dialogues[currentDialogueIndex];

        string text =
            dialogue.text ?? "";

        // =====================================================
        // BABY NAME REPLACEMENT
        // =====================================================

        string babyName = "";

        if (SaveManager.Instance != null)
        {
            babyName =
                SaveManager.Instance.GetBabyName();
        }

        if (!string.IsNullOrEmpty(babyName))
        {
            text = text.Replace(
                "{babyName}",
                babyName
            );
        }

        // =====================================================
        // HIDE BOTH BUBBLES FIRST
        // =====================================================

        HideAllBubbles();

        // =====================================================
        // SPEAKER 1
        // =====================================================

        if (dialogue.speaker == Speaker.Speaker1)
        {
            if (speaker1Bubble != null)
                speaker1Bubble.SetActive(true);

            if (speaker1Text != null)
                speaker1Text.text = "";

            ApplyFacing(
                speaker1Character,
                dialogue.facing
            );

            PrepareSpeaker1(text);

            currentFullText = text;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            typingCoroutine =
                StartCoroutine(
                    TypeText(
                        speaker1Text,
                        text
                    )
                );
        }

        // =====================================================
        // SPEAKER 2
        // =====================================================

        else if (dialogue.speaker == Speaker.Speaker2)
        {
            if (speaker2Bubble != null)
                speaker2Bubble.SetActive(true);

            if (speaker2Text != null)
                speaker2Text.text = "";

            ApplyFacing(
                speaker2Character,
                dialogue.facing
            );

            PrepareSpeaker2(text);

            currentFullText = text;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            typingCoroutine =
                StartCoroutine(
                    TypeText(
                        speaker2Text,
                        text
                    )
                );
        }
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
                "TwoDialogue: Character does not have a RectTransform.",
                character
            );

            return;
        }

        Vector3 scale =
            rectTransform.localScale;

        float absoluteX =
            Mathf.Abs(scale.x);

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
    // SPEAKER 1 ANIMATIONS
    // =========================================================

    private void PrepareSpeaker1(string text)
    {
        // =====================================================
        // AUTO SIZE
        // =====================================================

        if (speaker1AutoSize != null)
        {
            if (
                speaker1AutoSize.enabled &&
                speaker1AutoSize.gameObject.activeInHierarchy
            )
            {
                speaker1AutoSize.ResizeForText(text);
            }
        }

        // =====================================================
        // POP
        // =====================================================

        if (speaker1PopAnimation != null)
        {
            if (
                speaker1PopAnimation.enabled &&
                speaker1PopAnimation.gameObject.activeInHierarchy
            )
            {
                speaker1PopAnimation.PlayPop();
            }
        }

        // =====================================================
        // TRIANGLE
        // =====================================================

        if (speaker1TriangleAnimation != null)
        {
            if (
                speaker1TriangleAnimation.enabled &&
                speaker1TriangleAnimation.gameObject.activeInHierarchy
            )
            {
                speaker1TriangleAnimation.PlayAnimation();
            }
        }
    }

    // =========================================================
    // SPEAKER 2 ANIMATIONS
    // =========================================================

    private void PrepareSpeaker2(string text)
    {
        // =====================================================
        // AUTO SIZE
        // =====================================================

        if (speaker2AutoSize != null)
        {
            if (
                speaker2AutoSize.enabled &&
                speaker2AutoSize.gameObject.activeInHierarchy
            )
            {
                speaker2AutoSize.ResizeForText(text);
            }
        }

        // =====================================================
        // POP
        // =====================================================

        if (speaker2PopAnimation != null)
        {
            if (
                speaker2PopAnimation.enabled &&
                speaker2PopAnimation.gameObject.activeInHierarchy
            )
            {
                speaker2PopAnimation.PlayPop();
            }
        }

        // =====================================================
        // TRIANGLE
        // =====================================================

        if (speaker2TriangleAnimation != null)
        {
            if (
                speaker2TriangleAnimation.enabled &&
                speaker2TriangleAnimation.gameObject.activeInHierarchy
            )
            {
                speaker2TriangleAnimation.PlayAnimation();
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
            textComponent.text = "";

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
                textComponent.text += character;

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

        if (dialogue.speaker == Speaker.Speaker1)
        {
            if (speaker1Text != null)
                speaker1Text.text =
                    currentFullText;
        }
        else if (dialogue.speaker == Speaker.Speaker2)
        {
            if (speaker2Text != null)
                speaker2Text.text =
                    currentFullText;
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
        // Disable dialogue tap input after dialogue ends.
        dialogueStarted = false;

        if (dialogueInput != null)
            dialogueInput.SetActive(false);

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

        if (
            transitionType ==
            TransitionType.Quiz
        )
        {
            if (string.IsNullOrEmpty(quizChapterFileName))
            {
                Debug.LogError(
                    "TwoDialogue: Quiz Chapter File Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            if (string.IsNullOrEmpty(quizSceneName))
            {
                Debug.LogError(
                    "TwoDialogue: Quiz Scene Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            Debug.Log(
                "TwoDialogue: Starting Quiz - " +
                quizChapterFileName,
                this
            );

            QuizSceneData.chapterFileName =
                quizChapterFileName;

            SceneManager.LoadScene(
                quizSceneName
            );

            return;
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
                    "TwoDialogue: " +
                    "Next Scene Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            Debug.Log(
                "TwoDialogue: " +
                "Loading scene: " +
                nextSceneName
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
            // Hide assigned GameObjects
            // -------------------------------------------------

            if (gameObjectsToHide != null)
            {
                foreach (GameObject obj in gameObjectsToHide)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);

                        Debug.Log(
                            "TwoDialogue: " +
                            "Hidden: " +
                            obj.name
                        );
                    }
                }
            }

            // -------------------------------------------------
            // Show assigned GameObjects
            // -------------------------------------------------

            if (gameObjectsToShow != null)
            {
                foreach (GameObject obj in gameObjectsToShow)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);

                        Debug.Log(
                            "TwoDialogue: " +
                            "Shown: " +
                            obj.name
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
                "TwoDialogue: " +
                "Dialogue finished with no transition."
            );
        }
    }

    // =========================================================
    // HIDE ALL BUBBLES
    // =========================================================

    private void HideAllBubbles()
    {
        if (speaker1Bubble != null)
            speaker1Bubble.SetActive(false);

        if (speaker2Bubble != null)
            speaker2Bubble.SetActive(false);
    }

    // =========================================================
    // PUBLIC START DIALOGUE
    // =========================================================

    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "TwoDialogue: " +
                "No dialogues assigned!",
                this
            );

            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // =====================================================
        // START DIALOGUE
        // =====================================================

        dialogueStarted = true;

        if (dialogueInput != null)
            dialogueInput.SetActive(true);

        dialogueFinished = false;

        isTransitioning = false;

        isTyping = false;

        currentDialogueIndex = 0;
        
        ApplyDialogueStartGameObjects();

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

    // =========================================================
    // APPLY GAMEOBJECTS WHEN DIALOGUE STARTS
    // =========================================================

    private void ApplyDialogueStartGameObjects()
    {
        // -----------------------------------------------------
        // SHOW GAMEOBJECTS
        // -----------------------------------------------------

        if (gameObjectsToShowOnStart != null)
        {
            foreach (GameObject obj in gameObjectsToShowOnStart)
            {
                if (obj != null)
                {
                    obj.SetActive(true);

                    Debug.Log(
                        "TwoDialogue: Shown on dialogue start: " +
                        obj.name
                    );
                }
            }
        }

        // -----------------------------------------------------
        // HIDE GAMEOBJECTS
        // -----------------------------------------------------

        if (gameObjectsToHideOnStart != null)
        {
            foreach (GameObject obj in gameObjectsToHideOnStart)
            {
                if (obj != null)
                {
                    obj.SetActive(false);

                    Debug.Log(
                        "TwoDialogue: Hidden on dialogue start: " +
                        obj.name
                    );
                }
            }
        }
    }
}