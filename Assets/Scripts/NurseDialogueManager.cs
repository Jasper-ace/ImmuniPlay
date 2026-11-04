using UnityEngine;
using TMPro;

public class NurseDialogueManager : MonoBehaviour
{
    //====================================================
    // NURSE BUBBLE
    //====================================================

    [Header("Nurse Bubble (top, near nurse)")]

    public GameObject nurseBubble;

    public TMP_Text nurseText;

    [TextArea(2, 5)]
    public string[] nurseDialogues;


    //====================================================
    // NURSE BUBBLE ANIMATIONS
    //====================================================

    [Header("Optional Nurse Bubble Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize nurseAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation nursePopAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation nurseTriangleAnimation;


    //====================================================
    // CHAT BUBBLE
    //====================================================

    [Header("Chat Bubble (bottom of screen)")]

    public GameObject chatBubble;

    public TMP_Text chatText;

    [TextArea(2, 5)]
    public string[] chatDialogues;


    //====================================================
    // CHAT BUBBLE ANIMATIONS
    //====================================================

    [Header("Optional Chat Bubble Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize chatAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation chatPopAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation chatTriangleAnimation;


    //====================================================
    // TYPEWRITER
    //====================================================

    [Header("Typewriter")]

    public float typingSpeed = 0.03f;


    //====================================================
    // AFTER CHAT BUBBLE FINISHES
    //====================================================

    [Header("After Chat Bubble Finishes")]

    public GameObject previousParent;

    public GameObject nextParent;


    //====================================================
    // INTERNAL STATE
    //====================================================

    private enum Stage
    {
        NurseTyping,
        NurseWaitingForClick,
        ChatTyping,
        ChatWaitingForClick,
        Done
    }

    private Stage currentStage;

    private int nurseIndex = 0;

    private int chatIndex = 0;

    private TMP_Text activeTarget;

    private string activeFullLine;

    private int visibleChars = 0;

    private float charTimer = 0f;

    private bool isTyping = false;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(false);

        if (chatBubble != null)
            chatBubble.SetActive(false);

        StartNurseDialogue();
    }


    //====================================================
    // UPDATE
    //====================================================

    void Update()
    {
        //================================================
        // TYPEWRITER
        //================================================

        if (isTyping && activeTarget != null)
        {
            // Prevent a zero/negative typing speed
            float safeTypingSpeed = Mathf.Max(typingSpeed, 0.001f);

            charTimer += Time.unscaledDeltaTime;

            while (
                charTimer >= safeTypingSpeed &&
                visibleChars < activeFullLine.Length
            )
            {
                charTimer -= safeTypingSpeed;

                visibleChars++;

                activeTarget.text =
                    activeFullLine.Substring(0, visibleChars);
            }

            if (visibleChars >= activeFullLine.Length)
            {
                isTyping = false;
            }
        }


        //================================================
        // CLICK / TAP
        //================================================

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }


    //====================================================
    // HANDLE CLICK
    //====================================================

    private void HandleClick()
    {
        switch (currentStage)
        {
            //================================================
            // NURSE IS TYPING
            //================================================

            case Stage.NurseTyping:

                CompleteTyping();

                currentStage =
                    Stage.NurseWaitingForClick;

                break;


            //================================================
            // NURSE WAITING
            //================================================

            case Stage.NurseWaitingForClick:

                HideNurseBubble();

                StartChatDialogue();

                break;


            //================================================
            // CHAT IS TYPING
            //================================================

            case Stage.ChatTyping:

                CompleteTyping();

                currentStage =
                    Stage.ChatWaitingForClick;

                break;


            //================================================
            // CHAT WAITING
            //================================================

            case Stage.ChatWaitingForClick:

                chatIndex++;

                if (
                    chatDialogues != null &&
                    chatIndex < chatDialogues.Length
                )
                {
                    BeginTyping(
                        chatText,
                        chatDialogues[chatIndex],

                        // Chat animations
                        chatAutoSize,
                        chatPopAnimation,
                        chatTriangleAnimation
                    );

                    currentStage =
                        Stage.ChatTyping;
                }
                else
                {
                    currentStage =
                        Stage.Done;

                    SwitchParents();
                }

                break;


            //================================================
            // DONE
            //================================================

            case Stage.Done:

                break;
        }
    }


    //====================================================
    // NURSE BUBBLE
    //====================================================

    private void StartNurseDialogue()
    {
        nurseIndex = 0;

        // No nurse dialogue?
        // Go directly to chat.
        if (
            nurseDialogues == null ||
            nurseDialogues.Length == 0
        )
        {
            StartChatDialogue();

            return;
        }


        // Show nurse bubble
        if (nurseBubble != null)
            nurseBubble.SetActive(true);


        // Start first nurse line
        BeginTyping(
            nurseText,
            nurseDialogues[nurseIndex],

            // Nurse animations
            nurseAutoSize,
            nursePopAnimation,
            nurseTriangleAnimation
        );


        currentStage =
            Stage.NurseTyping;
    }


    //====================================================
    // HIDE NURSE BUBBLE
    //====================================================

    private void HideNurseBubble()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(false);

        if (nurseText != null)
            nurseText.text = "";
    }


    //====================================================
    // CHAT BUBBLE
    //====================================================

    private void StartChatDialogue()
    {
        chatIndex = 0;

        // No chat dialogue?
        if (
            chatDialogues == null ||
            chatDialogues.Length == 0
        )
        {
            currentStage =
                Stage.Done;

            SwitchParents();

            return;
        }


        // Show chat bubble
        if (chatBubble != null)
            chatBubble.SetActive(true);


        // Start first chat line
        BeginTyping(
            chatText,
            chatDialogues[chatIndex],

            // Chat animations
            chatAutoSize,
            chatPopAnimation,
            chatTriangleAnimation
        );


        currentStage =
            Stage.ChatTyping;
    }


    //====================================================
    // BEGIN TYPING
    //====================================================

    private void BeginTyping(
        TMP_Text target,
        string line,
        DialogueBoxAutoSize autoSize,
        DialoguePopAnimation popAnimation,
        DialogueTriangleAnimation triangleAnimation
    )
    {
        activeTarget = target;

        activeFullLine = line ?? "";

        visibleChars = 0;

        charTimer = 0f;

        isTyping = true;


        //================================================
        // OPTIONAL AUTO RESIZE
        //================================================

        if (
            autoSize != null &&
            autoSize.enabled &&
            autoSize.gameObject.activeInHierarchy
        )
        {
            autoSize.ResizeForText(activeFullLine);
        }


        //================================================
        // OPTIONAL POP ANIMATION
        //================================================

        if (
            popAnimation != null &&
            popAnimation.enabled &&
            popAnimation.gameObject.activeInHierarchy
        )
        {
            popAnimation.PlayPop();
        }


        //================================================
        // OPTIONAL TRIANGLE ANIMATION
        //================================================

        if (
            triangleAnimation != null &&
            triangleAnimation.enabled &&
            triangleAnimation.gameObject.activeInHierarchy
        )
        {
            triangleAnimation.PlayAnimation();
        }


        //================================================
        // CLEAR TEXT
        //================================================

        if (activeTarget != null)
            activeTarget.text = "";
    }


    //====================================================
    // COMPLETE TYPING
    //====================================================

    private void CompleteTyping()
    {
        if (activeTarget != null)
            activeTarget.text = activeFullLine;

        visibleChars = activeFullLine.Length;

        isTyping = false;
    }


    //====================================================
    // SWITCH PARENTS
    //====================================================

    private void SwitchParents()
    {
        if (previousParent != null)
            previousParent.SetActive(false);

        if (nextParent != null)
            nextParent.SetActive(true);
    }
}