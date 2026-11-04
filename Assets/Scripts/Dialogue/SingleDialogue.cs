using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SingleCharDialogue : MonoBehaviour
{
    // =========================================================
    // DIALOGUE DATA
    // =========================================================

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)]
        public string text;
    }


    // =========================================================
    // GAMEOBJECTS ON DIALOGUE START
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE START")]
    [Tooltip("GameObjects to show when the dialogue starts.")]
    public GameObject[] gameObjectsToShowOnStart;

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE START")]
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
    // DIALOGUE BUBBLE
    // =========================================================

    [Header("DIALOGUE BUBBLE")]

    [Tooltip("Dialogue bubble used by the speaker.")]
    public GameObject dialogueBubble;

    [Tooltip("Dialogue text used by the speaker.")]
    public TextMeshProUGUI dialogueText;


    // =========================================================
    // DIALOGUE ANIMATIONS
    // =========================================================

    [Header("DIALOGUE ANIMATIONS")]

    [Tooltip("Automatically resizes the dialogue bubble.")]
    public DialogueBoxAutoSize autoSize;

    [Tooltip("Pop animation for the dialogue bubble.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Triangle animation for the dialogue bubble.")]
    public DialogueTriangleAnimation triangleAnimation;


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
    // QUIZ SETTINGS
    // =========================================================

    [Header("QUIZ SETTINGS")]

    [Tooltip("Quiz JSON filename without .json. Example: Chapter1")]
    public string quizChapterFileName = "";

    [Tooltip("Scene containing the shared ChapterQuiz.")]
    public string quizSceneName = "ChapterQuiz";

    [Tooltip("Scene to load after the quiz is completed.")]
    public string nextChapterScene = "";


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    [Header("DIALOGUE INPUT")]
    [Tooltip("Invisible UI Button used to advance the dialogue.")]
    public GameObject dialogueInput;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private bool dialogueFinished = false;

    private bool isTransitioning = false;

    private Coroutine typingCoroutine;

    private string currentFullText = "";


    // =========================================================
    // GAMEOBJECTS ON DIALOGUE START
    // =========================================================

    private void ApplyStartGameObjects()
    {
        // -----------------------------------------------------
        // HIDE
        // -----------------------------------------------------

        if (gameObjectsToHideOnStart != null)
        {
            foreach (GameObject obj in gameObjectsToHideOnStart)
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

        if (gameObjectsToShowOnStart != null)
        {
            foreach (GameObject obj in gameObjectsToShowOnStart)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        // Dialogue input is only active while dialogue is running.
        if (dialogueInput != null)
            dialogueInput.SetActive(false);


        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "SingleCharDialogue: No dialogues assigned!",
                this
            );

            return;
        }


        currentDialogueIndex = 0;

        if (dialogueInput != null)
            dialogueInput.SetActive(true);

        ApplyStartGameObjects();

        ShowDialogue();
    }


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    public void AdvanceDialogue()
    {
        // Do nothing if dialogue is finished or a transition is happening.
        if (dialogueFinished || isTransitioning)
            return;

        // Tap while text is typing -> finish current sentence.
        if (isTyping)
        {
            FinishTyping();
        }
        else
        {
            // Tap after text is finished -> next dialogue.
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
        
        if (SaveManager.Instance != null)
        {
            string babyName = SaveManager.Instance.GetBabyName();

            if (!string.IsNullOrEmpty(babyName))
            {
                text = text.Replace("{babyName}", babyName);
            }
        }


        // -----------------------------------------------------
        // Show dialogue bubble
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);


        // -----------------------------------------------------
        // Clear text
        // -----------------------------------------------------

        if (dialogueText != null)
            dialogueText.text = "";


        // -----------------------------------------------------
        // Prepare dialogue animations
        // -----------------------------------------------------

        PrepareDialogue(text);


        // -----------------------------------------------------
        // Store current full text
        // -----------------------------------------------------

        currentFullText = text;


        // -----------------------------------------------------
        // Stop previous typing coroutine
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // -----------------------------------------------------
        // Start typewriter
        // -----------------------------------------------------

        typingCoroutine =
            StartCoroutine(
                TypeText(
                    dialogueText,
                    text
                )
            );
    }


    // =========================================================
    // DIALOGUE ANIMATIONS
    // =========================================================

    private void PrepareDialogue(string text)
    {
        // -----------------------------------------------------
        // AUTO SIZE
        // -----------------------------------------------------

        if (autoSize != null)
        {
            if (
                autoSize.enabled &&
                autoSize.gameObject.activeInHierarchy
            )
            {
                autoSize.ResizeForText(text);
            }
        }


        // -----------------------------------------------------
        // POP
        // -----------------------------------------------------

        if (popAnimation != null)
        {
            if (
                popAnimation.enabled &&
                popAnimation.gameObject.activeInHierarchy
            )
            {
                popAnimation.PlayPop();
            }
        }


        // -----------------------------------------------------
        // TRIANGLE
        // -----------------------------------------------------

        if (triangleAnimation != null)
        {
            if (
                triangleAnimation.enabled &&
                triangleAnimation.gameObject.activeInHierarchy
            )
            {
                triangleAnimation.PlayAnimation();
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
            // -------------------------------------------------
            // Add character
            // -------------------------------------------------

            if (textComponent != null)
                textComponent.text += character;


            characterCount++;


            // -------------------------------------------------
            // TYPEWRITER SOUND
            //
            // Uses AudioManager instead of a local AudioSource.
            // -------------------------------------------------

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


        if (dialogueText != null)
        {
            dialogueText.text =
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


        // -----------------------------------------------------
        // More dialogue remaining
        // -----------------------------------------------------

        if (
            currentDialogueIndex <
            dialogues.Length
        )
        {
            ShowDialogue();

            return;
        }


        // -----------------------------------------------------
        // ALL DIALOGUE FINISHED
        // -----------------------------------------------------

        FinishDialogue();
    }


    // =========================================================
    // FINISH DIALOGUE
    // =========================================================

    private void FinishDialogue()
    {
        if (dialogueFinished || isTransitioning)
            return;

        dialogueFinished = true;

        // Disable dialogue input once the dialogue ends.
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
        // HIDE DIALOGUE BUBBLE
        // =====================================================

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        // =====================================================
        // QUIZ
        // =====================================================

        if (transitionType == TransitionType.Quiz)
        {
            if (string.IsNullOrEmpty(quizChapterFileName))
            {
                Debug.LogError(
                    "SingleDialogue: Quiz Chapter File Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            if (string.IsNullOrEmpty(quizSceneName))
            {
                Debug.LogError(
                    "SingleDialogue: Quiz Scene Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            if (string.IsNullOrEmpty(nextChapterScene))
            {
                Debug.LogError(
                    "SingleDialogue: Next Chapter Scene is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            QuizSceneData.chapterFileName =
                quizChapterFileName;

            QuizSceneData.nextChapterScene =
                nextChapterScene;

            Debug.Log(
                "SingleDialogue: Starting Quiz\n" +
                "Chapter File Name: " +
                quizChapterFileName +
                "\nQuiz Scene Name: " +
                quizSceneName +
                "\nNext Chapter Scene: " +
                nextChapterScene,
                this
            );

            SceneManager.LoadScene(
                quizSceneName
            );

            return;
        }


        // =====================================================
        // UNITY SCENE
        // =====================================================

        if (transitionType == TransitionType.UnityScene)
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError(
                    "SingleDialogue: Next Scene Name is empty!",
                    this
                );

                isTransitioning = false;
                dialogueFinished = false;

                return;
            }

            Debug.Log(
                "SingleDialogue: Loading scene: " +
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

        if (transitionType == TransitionType.DisableAndEnable)
        {
            if (gameObjectsToHide != null)
            {
                foreach (GameObject obj in gameObjectsToHide)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);

                        Debug.Log(
                            "SingleDialogue: Hidden: " +
                            obj.name,
                            obj
                        );
                    }
                }
            }

            if (gameObjectsToShow != null)
            {
                foreach (GameObject obj in gameObjectsToShow)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);

                        Debug.Log(
                            "SingleDialogue: Shown: " +
                            obj.name,
                            obj
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

        if (transitionType == TransitionType.None)
        {
            isTransitioning = false;

            Debug.Log(
                "SingleDialogue: Dialogue finished with no transition.",
                this
            );
        }
    }


    // =========================================================
    // PUBLIC START DIALOGUE
    // =========================================================

    public void StartDialogue()
    {
        if (
            dialogues == null ||
            dialogues.Length == 0
        )
        {
            Debug.LogWarning(
                "SingleCharDialogue: " +
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


        dialogueFinished = false;

        isTransitioning = false;

        isTyping = false;

        currentDialogueIndex = 0;

        ApplyStartGameObjects();

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