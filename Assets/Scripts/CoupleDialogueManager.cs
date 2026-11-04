using UnityEngine;
using TMPro;
using System.Collections;

public class SAMPLE : MonoBehaviour
{
    //====================================================
    // DIALOGUE DATA
    //====================================================

    [System.Serializable]
    public class DialogueData
    {
        [TextArea(2, 5)]
        public string dialogue;

        public bool isMotherTalking;
    }


    //====================================================
    // SPEECH BUBBLES
    //====================================================

    [Header("Speech Bubbles")]

    public GameObject motherBubble;
    public GameObject fatherBubble;


    //====================================================
    // TEXT COMPONENTS
    //====================================================

    [Header("Text Components")]

    public TMP_Text motherText;
    public TMP_Text fatherText;


    //====================================================
    // CONVERSATION
    //====================================================

    [Header("Conversation")]

    public DialogueData[] dialogues;


    //====================================================
    // NEXT BUTTON
    //====================================================

    [Header("Next Button (Shows After Conversation)")]

    public GameObject nextButton;


    //====================================================
    // TYPEWRITER SETTINGS
    //====================================================

    [Header("Typewriter Settings")]

    public float typingSpeed = 0.03f;


    //====================================================
    // DIALOGUE ANIMATIONS
    //====================================================

    [Header("Dialogue Animations")]

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueBoxAutoSize motherAutoSize;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueBoxAutoSize fatherAutoSize;


    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation motherPopAnimation;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation fatherPopAnimation;


    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueTriangleAnimation motherTriangleAnimation;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueTriangleAnimation fatherTriangleAnimation;


    //====================================================
    // VARIABLES
    //====================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private string currentSentence;

    private Coroutine typingCoroutine;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        // Clear text
        if (motherText != null)
            motherText.text = "";

        if (fatherText != null)
            fatherText.text = "";


        // Hide next button
        if (nextButton != null)
            nextButton.SetActive(false);


        // Hide both bubbles initially
        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);


        // Start conversation
        if (dialogues != null && dialogues.Length > 0)
        {
            ShowDialogue();
        }
    }


    //====================================================
    // UPDATE
    //====================================================

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //============================================
            // IF CURRENTLY TYPING
            //============================================

            if (isTyping)
            {
                CompleteTyping();
            }

            //============================================
            // IF TYPING IS FINISHED
            //============================================

            else
            {
                NextDialogue();
            }
        }
    }


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogues.Length)
        {
            EndConversation();
            return;
        }

        ShowDialogue();
    }


    //====================================================
    // SHOW DIALOGUE
    //====================================================

    void ShowDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
            return;


        if (currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length)
            return;


        DialogueData current =
            dialogues[currentDialogueIndex];

        currentSentence = current.dialogue;


        //================================================
        // MOTHER TALKING
        //================================================

        if (current.isMotherTalking)
        {
            // Show Mother bubble
            if (motherBubble != null)
                motherBubble.SetActive(true);

            // Hide Father bubble
            if (fatherBubble != null)
                fatherBubble.SetActive(false);


            //============================================
            // MOTHER AUTO SIZE
            //============================================

            if (motherAutoSize != null)
            {
                motherAutoSize.ResizeForText(
                    currentSentence
                );
            }


            //============================================
            // MOTHER POP ANIMATION
            //============================================

            if (motherPopAnimation != null)
            {
                motherPopAnimation.PlayPop();
            }


            //============================================
            // MOTHER TRIANGLE ANIMATION
            //============================================

            if (motherTriangleAnimation != null)
            {
                motherTriangleAnimation.PlayAnimation();
            }


            //============================================
            // CLEAR MOTHER TEXT
            //============================================

            if (motherText != null)
                motherText.text = "";


            //============================================
            // STOP PREVIOUS TYPING
            //============================================

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }


            //============================================
            // START MOTHER TYPEWRITER
            //============================================

            if (motherText != null)
            {
                typingCoroutine =
                    StartCoroutine(
                        TypeSentence(motherText)
                    );
            }
        }


        //================================================
        // FATHER TALKING
        //================================================

        else
        {
            // Show Father bubble
            if (fatherBubble != null)
                fatherBubble.SetActive(true);

            // Hide Mother bubble
            if (motherBubble != null)
                motherBubble.SetActive(false);


            //============================================
            // FATHER AUTO SIZE
            //============================================

            if (fatherAutoSize != null)
            {
                fatherAutoSize.ResizeForText(
                    currentSentence
                );
            }


            //============================================
            // FATHER POP ANIMATION
            //============================================

            if (fatherPopAnimation != null)
            {
                fatherPopAnimation.PlayPop();
            }


            //============================================
            // FATHER TRIANGLE ANIMATION
            //============================================

            if (fatherTriangleAnimation != null)
            {
                fatherTriangleAnimation.PlayAnimation();
            }


            //============================================
            // CLEAR FATHER TEXT
            //============================================

            if (fatherText != null)
                fatherText.text = "";


            //============================================
            // STOP PREVIOUS TYPING
            //============================================

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }


            //============================================
            // START FATHER TYPEWRITER
            //============================================

            if (fatherText != null)
            {
                typingCoroutine =
                    StartCoroutine(
                        TypeSentence(fatherText)
                    );
            }
        }
    }


    //====================================================
    // TYPE SENTENCE
    //====================================================

    IEnumerator TypeSentence(TMP_Text targetText)
    {
        isTyping = true;


        if (targetText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }


        targetText.text = "";


        foreach (char letter in currentSentence)
        {
            targetText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }


        isTyping = false;

        typingCoroutine = null;
    }


    //====================================================
    // COMPLETE TYPING
    //====================================================

    void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        if (dialogues == null ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length)
        {
            isTyping = false;
            return;
        }


        DialogueData current =
            dialogues[currentDialogueIndex];


        //============================================
        // COMPLETE MOTHER TEXT
        //============================================

        if (current.isMotherTalking)
        {
            if (motherText != null)
                motherText.text = currentSentence;
        }


        //============================================
        // COMPLETE FATHER TEXT
        //============================================

        else
        {
            if (fatherText != null)
                fatherText.text = currentSentence;
        }


        isTyping = false;
    }


    //====================================================
    // END CONVERSATION
    //====================================================

    void EndConversation()
    {
        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);


        if (nextButton != null)
            nextButton.SetActive(true);


        Debug.Log("Conversation Finished");
    }
}