using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ParentDialogueManager : MonoBehaviour
{
    //====================================================
    // DIALOGUE DATA
    //====================================================

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;

        public bool isMother;
    }


    [Header("Dialogue")]
    public Dialogue[] dialogues;


    //====================================================
    // MOTHER
    //====================================================

    [Header("Mother")]

    public GameObject motherBubble;

    public TextMeshProUGUI motherTMP;


    //====================================================
    // MOTHER OPTIONAL ANIMATIONS
    //====================================================

    [Header("Optional Mother Bubble Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize motherAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation motherPopAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation motherTriangleAnimation;


    //====================================================
    // FATHER
    //====================================================

    [Header("Father")]

    public GameObject fatherBubble;

    public TextMeshProUGUI fatherTMP;


    //====================================================
    // FATHER OPTIONAL ANIMATIONS
    //====================================================

    [Header("Optional Father Bubble Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize fatherAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation fatherPopAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation fatherTriangleAnimation;


    //====================================================
    // TYPEWRITER
    //====================================================

    [Header("Typewriter")]

    public float typingSpeed = 0.03f;


    //====================================================
    // SCENE MANAGER
    //====================================================

    [Header("Scene Manager")]

    public GameObject sceneManager;


    //====================================================
    // NEXT SCENE
    //====================================================

    [Header("Next Scene (Optional)")]

    public string nextSceneName = "";


    //====================================================
    // NEXT PARENT
    //====================================================

    [Header("Next Parent (Fallback)")]

    public GameObject nextParent;


    //====================================================
    // INTERNAL VARIABLES
    //====================================================

    private int currentIndex = 0;

    private bool isTyping = false;

    private bool isTransitioning = false;

    private Coroutine typingCoroutine;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (dialogues != null &&
            dialogues.Length > 0)
        {
            ShowDialogue();
        }
        else
        {
            Debug.LogWarning(
                "[ParentDialogueManager] No dialogues assigned."
            );
        }
    }


    //====================================================
    // UPDATE
    //====================================================

    void Update()
    {
        if (isTransitioning)
            return;


        if (!Input.GetMouseButtonDown(0))
            return;


        //================================================
        // FINISH CURRENT LINE
        //================================================

        if (isTyping)
        {
            FinishTyping();
        }


        //================================================
        // NEXT DIALOGUE
        //================================================

        else
        {
            NextDialogue();
        }
    }


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        currentIndex++;


        //================================================
        // ALL DIALOGUES FINISHED
        //================================================

        if (dialogues == null ||
            currentIndex >= dialogues.Length)
        {
            isTransitioning = true;


            //================================================
            // NEXT SCENE
            //================================================

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                if (sceneManager != null)
                {
                    SceneFade fade =
                        sceneManager.GetComponent<SceneFade>();


                    if (fade != null)
                    {
                        // SceneFade handles the transition
                        fade.FadeToScene(nextSceneName);
                    }
                    else
                    {
                        Debug.LogError(
                            "SceneFade component not found on sceneManager!"
                        );

                        SceneManager.LoadScene(
                            nextSceneName
                        );
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "Scene Manager not assigned, loading scene directly"
                    );

                    SceneManager.LoadScene(
                        nextSceneName
                    );
                }

                return;
            }


            //================================================
            // NEXT PARENT
            //================================================

            if (nextParent != null)
            {
                gameObject.SetActive(false);

                nextParent.SetActive(true);

                return;
            }


            //================================================
            // NO NEXT PARENT OR SCENE
            //================================================

            gameObject.SetActive(false);

            return;
        }


        //================================================
        // SHOW NEXT DIALOGUE
        //================================================

        ShowDialogue();
    }


    //====================================================
    // SHOW DIALOGUE
    //====================================================

    void ShowDialogue()
    {
        if (dialogues == null ||
            currentIndex < 0 ||
            currentIndex >= dialogues.Length)
        {
            return;
        }


        //================================================
        // HIDE BOTH BUBBLES
        //================================================

        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);


        //================================================
        // GET CURRENT DIALOGUE
        //================================================

        Dialogue dialogue =
            dialogues[currentIndex];


        //================================================
        // PROCESS BABY NAME
        //================================================

        string processedDialogue =
            ProcessDialogue(dialogue.text);


        //================================================
        // MOTHER
        //================================================

        if (dialogue.isMother)
        {
            if (motherBubble != null)
                motherBubble.SetActive(true);

            if (motherTMP != null)
                motherTMP.text = "";


            // Optional animations
            PrepareDialogue(
                processedDialogue,
                motherAutoSize,
                motherPopAnimation,
                motherTriangleAnimation
            );


            typingCoroutine =
                StartCoroutine(
                    TypeText(
                        motherTMP,
                        processedDialogue
                    )
                );
        }


        //================================================
        // FATHER
        //================================================

        else
        {
            if (fatherBubble != null)
                fatherBubble.SetActive(true);

            if (fatherTMP != null)
                fatherTMP.text = "";


            // Optional animations
            PrepareDialogue(
                processedDialogue,
                fatherAutoSize,
                fatherPopAnimation,
                fatherTriangleAnimation
            );


            typingCoroutine =
                StartCoroutine(
                    TypeText(
                        fatherTMP,
                        processedDialogue
                    )
                );
        }
    }


    //====================================================
    // BABY NAME PROCESSING
    //====================================================

    private string ProcessDialogue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;


        // If there is no baby name placeholder,
        // return the original text.
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
                "[ParentDialogueManager] " +
                "SaveManager.Instance is NULL. " +
                "Keeping {babyName}."
            );
        }


        // Replace every {babyName}
        return text.Replace(
            "{babyName}",
            babyName
        );
    }


    //====================================================
    // OPTIONAL DIALOGUE FEATURES
    //====================================================

    void PrepareDialogue(
        string text,
        DialogueBoxAutoSize autoSize,
        DialoguePopAnimation popAnimation,
        DialogueTriangleAnimation triangleAnimation
    )
    {
        //================================================
        // AUTO RESIZE
        //================================================

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


        //================================================
        // POP ANIMATION
        //================================================

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


        //================================================
        // TRIANGLE ANIMATION
        //================================================

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


    //====================================================
    // TYPE TEXT
    //====================================================

    IEnumerator TypeText(
        TextMeshProUGUI target,
        string text
    )
    {
        isTyping = true;


        if (target != null)
            target.text = "";


        string safeText = text ?? "";


        // Prevent zero/negative typing speed
        float safeTypingSpeed =
            Mathf.Max(typingSpeed, 0.001f);


        foreach (char c in safeText)
        {
            if (target != null)
                target.text += c;


            yield return new WaitForSeconds(
                safeTypingSpeed
            );
        }


        isTyping = false;

        typingCoroutine = null;
    }


    //====================================================
    // FINISH TYPING
    //====================================================

    void FinishTyping()
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


        Dialogue dialogue =
            dialogues[currentIndex];


        //================================================
        // PROCESS BABY NAME AGAIN
        //================================================

        string processedDialogue =
            ProcessDialogue(dialogue.text);


        //================================================
        // MOTHER
        //================================================

        if (dialogue.isMother)
        {
            if (motherTMP != null)
                motherTMP.text = processedDialogue;
        }


        //================================================
        // FATHER
        //================================================

        else
        {
            if (fatherTMP != null)
                fatherTMP.text = processedDialogue;
        }


        isTyping = false;
    }
}