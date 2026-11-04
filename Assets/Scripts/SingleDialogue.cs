using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SingleDialogue : MonoBehaviour
{
    //====================================================
    // CHAT BUBBLE
    //====================================================

    [Header("Chat Bubble")]

    public GameObject chatBubble;

    public TMP_Text dialogueText;


    //====================================================
    // OPTIONAL DIALOGUE ANIMATIONS
    //====================================================

    [Header("Optional Dialogue Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation triangleAnimation;


    //====================================================
    // DIALOGUES
    //====================================================

    [Header("Dialogues")]

    [TextArea(2, 5)]
    public string[] dialogues;


    //====================================================
    // TYPEWRITER
    //====================================================

    [Header("Typewriter")]

    public float typingSpeed = 0.03f;


    //====================================================
    // NEXT SCENE
    //====================================================

    [Header("Next Scene")]

    public string nextSceneName;


    //====================================================
    // CURRENT DIALOGUE
    //====================================================

    private int dialogueIndex = 0;


    //====================================================
    // TYPEWRITER VARIABLES
    //====================================================

    private int currentCharacter = 0;

    private float timer = 0f;

    private bool isTyping = false;

    private bool dialogueFinished = false;


    //====================================================
    // START
    //====================================================

    private void Start()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "SingleDialogue: No dialogues assigned!",
                this
            );

            return;
        }


        if (chatBubble != null)
        {
            chatBubble.SetActive(true);
        }


        ShowDialogue();
    }


    //====================================================
    // UPDATE
    //====================================================

    private void Update()
    {
        //================================================
        // TYPEWRITER
        //================================================

        if (isTyping)
        {
            UpdateTypewriter();
        }


        //================================================
        // MOUSE
        //================================================

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }


        //================================================
        // MOBILE TOUCH
        //================================================

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleClick();
            }
        }
    }


    //====================================================
    // SHOW CURRENT DIALOGUE
    //====================================================

    private void ShowDialogue()
    {
        if (dialogues == null)
            return;


        if (dialogueIndex >= dialogues.Length)
            return;


        //================================================
        // SHOW BUBBLE
        //================================================

        if (chatBubble != null)
        {
            chatBubble.SetActive(true);
        }


        //================================================
        // GET CURRENT DIALOGUE
        //================================================

        string currentDialogue =
            dialogues[dialogueIndex] ?? "";


        //================================================
        // OPTIONAL ANIMATIONS
        //================================================

        PrepareDialogue(currentDialogue);


        //================================================
        // RESET TYPEWRITER
        //================================================

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }


        currentCharacter = 0;

        timer = 0f;

        isTyping = true;

        dialogueFinished = false;
    }


    //====================================================
    // OPTIONAL DIALOGUE FEATURES
    //====================================================

    private void PrepareDialogue(string text)
    {
        //================================================
        // AUTO RESIZE
        //================================================

        if (dialogueAutoSize != null)
        {
            // Only use it when its component and
            // GameObject are active.

            if (
                dialogueAutoSize.enabled &&
                dialogueAutoSize.gameObject.activeInHierarchy
            )
            {
                dialogueAutoSize.ResizeForText(text);
            }
        }


        //================================================
        // POP ANIMATION
        //================================================

        if (popAnimation != null)
        {
            // Only use it when its component and
            // GameObject are active.

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
            // Only use it when its component and
            // GameObject are active.

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
    // TYPEWRITER
    //====================================================

    private void UpdateTypewriter()
    {
        if (dialogueText == null)
            return;


        if (
            dialogues == null ||
            dialogueIndex < 0 ||
            dialogueIndex >= dialogues.Length
        )
        {
            return;
        }


        string currentDialogue =
            dialogues[dialogueIndex] ?? "";


        // Prevent typingSpeed from being zero
        float safeTypingSpeed =
            Mathf.Max(typingSpeed, 0.001f);


        timer += Time.unscaledDeltaTime;


        while (
            timer >= safeTypingSpeed &&
            currentCharacter < currentDialogue.Length
        )
        {
            timer -= safeTypingSpeed;

            currentCharacter++;


            dialogueText.text =
                currentDialogue.Substring(
                    0,
                    currentCharacter
                );
        }


        //================================================
        // FINISHED TYPING
        //================================================

        if (currentCharacter >= currentDialogue.Length)
        {
            currentCharacter =
                currentDialogue.Length;

            isTyping = false;

            dialogueFinished = true;
        }
    }


    //====================================================
    // CLICK
    //====================================================

    private void HandleClick()
    {
        //================================================
        // CLICK WHILE TYPING
        // → COMPLETE CURRENT DIALOGUE
        //================================================

        if (isTyping)
        {
            CompleteDialogue();

            return;
        }


        //================================================
        // CLICK AFTER COMPLETE
        // → NEXT DIALOGUE
        //================================================

        if (dialogueFinished)
        {
            NextDialogue();
        }
    }


    //====================================================
    // COMPLETE CURRENT DIALOGUE
    //====================================================

    private void CompleteDialogue()
    {
        if (
            dialogues == null ||
            dialogueIndex < 0 ||
            dialogueIndex >= dialogues.Length
        )
        {
            return;
        }


        string currentDialogue =
            dialogues[dialogueIndex] ?? "";


        if (dialogueText != null)
        {
            dialogueText.text = currentDialogue;
        }


        currentCharacter =
            currentDialogue.Length;

        isTyping = false;

        dialogueFinished = true;
    }


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    private void NextDialogue()
    {
        dialogueIndex++;


        //================================================
        // MORE DIALOGUES
        //================================================

        if (
            dialogues != null &&
            dialogueIndex < dialogues.Length
        )
        {
            ShowDialogue();

            return;
        }


        //================================================
        // LAST DIALOGUE FINISHED
        // → HIDE BUBBLE
        // → CHANGE SCENE
        //================================================

        FinishDialogue();
    }


    //====================================================
    // FINISH
    //====================================================

    private void FinishDialogue()
    {
        if (chatBubble != null)
        {
            chatBubble.SetActive(false);
        }


        ChangeScene();
    }


    //====================================================
    // CHANGE SCENE
    //====================================================

    private void ChangeScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "SingleDialogue: Next Scene Name is empty!",
                this
            );

            return;
        }


        SceneManager.LoadScene(nextSceneName);
    }
}