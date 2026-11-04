using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SingleDialogueAdvance : MonoBehaviour
{
    // =========================================================
    // DIALOGUE DATA
    // =========================================================

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;


        // =====================================================
        // BETWEEN-DIALOGUE ACTIONS
        // =====================================================

        [Header("AFTER THIS DIALOGUE")]

        [Tooltip("GameObjects to show after this dialogue.")]
        public GameObject[] objectsToShow;

        [Tooltip("GameObjects to hide after this dialogue.")]
        public GameObject[] objectsToHide;
    }


    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("DIALOGUE")]

    [Tooltip("List of dialogue lines.")]
    public Dialogue[] dialogues;


    // =========================================================
    // DIALOGUE BUBBLE
    // =========================================================

    [Header("DIALOGUE BUBBLE")]

    [Tooltip("The dialogue bubble GameObject.")]
    public GameObject dialogueBubble;

    [Tooltip("Text inside the dialogue bubble.")]
    public TextMeshProUGUI dialogueTMP;


    // =========================================================
    // OPTIONAL ANIMATIONS
    // =========================================================

    [Header("DIALOGUE ANIMATIONS")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize autoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation triangleAnimation;


    // =========================================================
    // TYPEWRITER
    // =========================================================

    [Header("TYPEWRITER")]

    [Tooltip("Time between each character.")]
    public float typingSpeed = 0.03f;


    // =========================================================
    // AFTER ALL DIALOGUE
    // =========================================================

    public enum TransitionType
    {
        None,
        UnityScene,
        DisableAndEnable,
        Quiz
    }


    [Header("AFTER ALL DIALOGUE")]

    [Tooltip("Choose what happens after the FINAL dialogue.")]
    public TransitionType transitionType =
        TransitionType.DisableAndEnable;


    // =========================================================
    // UNITY SCENE
    // =========================================================

    [Header("UNITY SCENE")]

    [Tooltip("Used only when Transition Type is UnityScene.")]
    public string nextSceneName = "";


    // =========================================================
    // GAMEOBJECT TRANSITION
    // =========================================================

    [Header("GAMEOBJECT TRANSITION")]

    [Tooltip("The current GameObject to disable after ALL dialogue. Leave empty to disable this GameObject.")]
    public GameObject currentObject;

    [Tooltip("The next GameObject to enable after ALL dialogue.")]
    public GameObject nextObject;


    // =========================================================
    // QUIZ TRANSITION
    // =========================================================

    [Header("QUIZ TRANSITION")]

    [Tooltip("The quiz JSON filename without .json. Example: Chapter1")]
    public string quizChapterFileName = "Chapter1";

    [Tooltip("The reusable quiz Unity scene.")]
    public string quizSceneName = "ChapterQuiz";


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentIndex = 0;

    private bool isTyping = false;

    private bool isTransitioning = false;

    private Coroutine typingCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Hide bubble at the beginning.

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }


        // Automatically start dialogue.

        if (dialogues != null && dialogues.Length > 0)
        {
            currentIndex = 0;

            ShowDialogue();
        }
        else
        {
            Debug.LogWarning(
                "SingleDialogueAdvance: No dialogues assigned!",
                this
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isTransitioning)
            return;


        // Tap / click anywhere.

        if (!Input.GetMouseButtonDown(0))
            return;


        // =====================================================
        // FINISH CURRENT LINE
        // =====================================================

        if (isTyping)
        {
            FinishTyping();
        }


        // =====================================================
        // NEXT DIALOGUE
        // =====================================================

        else
        {
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
            currentIndex < 0 ||
            currentIndex >= dialogues.Length
        )
        {
            return;
        }


        // =====================================================
        // SHOW BUBBLE
        // =====================================================

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }


        // =====================================================
        // GET CURRENT DIALOGUE
        // =====================================================

        Dialogue dialogue =
            dialogues[currentIndex];

        string text =
            dialogue.text ?? "";


        // =====================================================
        // CLEAR TEXT
        // =====================================================

        if (dialogueTMP != null)
        {
            dialogueTMP.text = "";
        }


        // =====================================================
        // OPTIONAL FEATURES
        // =====================================================

        PrepareDialogue(text);


        // =====================================================
        // TYPEWRITER
        // =====================================================

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }


        typingCoroutine =
            StartCoroutine(
                TypeText(text)
            );
    }


    // =========================================================
    // PREPARE DIALOGUE
    // =========================================================

    private void PrepareDialogue(string text)
    {
        // =====================================================
        // AUTO SIZE
        // =====================================================

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


        // =====================================================
        // POP ANIMATION
        // =====================================================

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


        // =====================================================
        // TRIANGLE ANIMATION
        // =====================================================

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
    // TYPE TEXT
    // =========================================================

    private IEnumerator TypeText(string text)
    {
        isTyping = true;


        if (dialogueTMP != null)
        {
            dialogueTMP.text = "";
        }


        string safeText =
            text ?? "";


        float safeTypingSpeed =
            Mathf.Max(
                typingSpeed,
                0.001f
            );


        foreach (char c in safeText)
        {
            if (dialogueTMP != null)
            {
                dialogueTMP.text += c;
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
            currentIndex < 0 ||
            currentIndex >= dialogues.Length
        )
        {
            isTyping = false;

            return;
        }


        if (dialogueTMP != null)
        {
            dialogueTMP.text =
                dialogues[currentIndex].text;
        }


        isTyping = false;
    }


    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    private void NextDialogue()
    {
        // =====================================================
        // EXECUTE BETWEEN-DIALOGUE ACTIONS
        // =====================================================

        ExecuteBetweenDialogueActions();


        // =====================================================
        // CHECK IF THIS WAS THE FINAL DIALOGUE
        // =====================================================

        if (
            dialogues == null ||
            currentIndex >= dialogues.Length - 1
        )
        {
            FinishDialogue();

            return;
        }


        // =====================================================
        // MOVE TO NEXT DIALOGUE
        // =====================================================

        currentIndex++;


        // =====================================================
        // SHOW NEXT LINE
        // =====================================================

        ShowDialogue();
    }


    // =========================================================
    // BETWEEN-DIALOGUE ACTIONS
    // =========================================================

    private void ExecuteBetweenDialogueActions()
    {
        if (
            dialogues == null ||
            currentIndex < 0 ||
            currentIndex >= dialogues.Length
        )
        {
            return;
        }


        Dialogue dialogue =
            dialogues[currentIndex];


        // =====================================================
        // SHOW GAMEOBJECTS
        // =====================================================

        if (dialogue.objectsToShow != null)
        {
            foreach (GameObject obj in dialogue.objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);


                    Debug.Log(
                        "SingleDialogueAdvance: Showing " +
                        obj.name,
                        obj
                    );
                }
            }
        }


        // =====================================================
        // HIDE GAMEOBJECTS
        // =====================================================

        if (dialogue.objectsToHide != null)
        {
            foreach (GameObject obj in dialogue.objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);


                    Debug.Log(
                        "SingleDialogueAdvance: Hiding " +
                        obj.name,
                        obj
                    );
                }
            }
        }
    }


    // =========================================================
    // FINISH DIALOGUE
    // =========================================================

    private void FinishDialogue()
    {
        if (isTransitioning)
            return;


        isTransitioning = true;


        // =====================================================
        // STOP TYPEWRITER
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
        {
            dialogueBubble.SetActive(false);
        }


        // =====================================================
        // QUIZ
        // =====================================================

        if (transitionType == TransitionType.Quiz)
        {
            if (string.IsNullOrEmpty(quizChapterFileName))
            {
                Debug.LogError(
                    "SingleDialogueAdvance: " +
                    "Quiz Chapter File Name is empty!",
                    this
                );


                isTransitioning = false;

                return;
            }


            if (string.IsNullOrEmpty(quizSceneName))
            {
                Debug.LogError(
                    "SingleDialogueAdvance: " +
                    "Quiz Scene Name is empty!",
                    this
                );


                isTransitioning = false;

                return;
            }


            QuizSceneData.chapterFileName =
                quizChapterFileName;


            Debug.Log(
                "SingleDialogueAdvance: Starting quiz " +
                quizChapterFileName
            );


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
                    "SingleDialogueAdvance: " +
                    "Next Scene Name is empty!",
                    this
                );


                isTransitioning = false;

                return;
            }


            Debug.Log(
                "SingleDialogueAdvance: " +
                "Loading scene: " +
                nextSceneName
            );


            SceneManager.LoadScene(
                nextSceneName
            );


            return;
        }


        // =====================================================
        // DISABLE CURRENT / ENABLE NEXT
        // =====================================================

        if (
            transitionType ==
            TransitionType.DisableAndEnable
        )
        {
            // -------------------------------------------------
            // Disable Current Object
            // -------------------------------------------------

            if (currentObject != null)
            {
                currentObject.SetActive(false);


                Debug.Log(
                    "SingleDialogueAdvance: " +
                    "Disabled: " +
                    currentObject.name,
                    currentObject
                );
            }
            else
            {
                // If Current Object is empty,
                // disable the GameObject containing this script.

                gameObject.SetActive(false);


                Debug.Log(
                    "SingleDialogueAdvance: " +
                    "Disabled current GameObject: " +
                    gameObject.name,
                    gameObject
                );
            }


            // -------------------------------------------------
            // Enable Next Object
            // -------------------------------------------------

            if (nextObject != null)
            {
                nextObject.SetActive(true);


                Debug.Log(
                    "SingleDialogueAdvance: " +
                    "Enabled: " +
                    nextObject.name,
                    nextObject
                );
            }
            else
            {
                Debug.LogWarning(
                    "SingleDialogueAdvance: " +
                    "Next Object is not assigned.",
                    this
                );
            }


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
                "SingleDialogueAdvance: " +
                "Dialogue finished with no transition.",
                this
            );
        }
    }


    // =========================================================
    // PUBLIC START METHOD
    // =========================================================

    public void StartDialogue()
    {
        if (isTransitioning)
            return;


        if (
            dialogues == null ||
            dialogues.Length == 0
        )
        {
            Debug.LogWarning(
                "SingleDialogueAdvance: " +
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


        currentIndex = 0;

        isTyping = false;

        isTransitioning = false;


        ShowDialogue();
    }


    // =========================================================
    // PUBLIC CLOSE METHOD
    // =========================================================

    public void CloseDialogue()
    {
        FinishDialogue();
    }
}