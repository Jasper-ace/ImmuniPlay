using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NurseStory : MonoBehaviour
{
    //====================================================
    // UI
    //====================================================

    [Header("UI")]

    public GameObject nurseBubble;
    public GameObject bottomDialogue;

    public TextMeshProUGUI nurseText;
    public TextMeshProUGUI bottomText;


    //====================================================
    // DIALOGUE
    //====================================================

    [Header("Dialogue")]

    [TextArea(2, 5)]
    public string[] dialogues;

    public bool[] isNurseTalking;


    //====================================================
    // TYPEWRITER
    //====================================================

    [Header("Typewriter")]

    public float typingSpeed = 0.03f;


    //====================================================
    // NURSE BUBBLE ANIMATIONS
    //====================================================

    [Header("Nurse Bubble Animations")]

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueBoxAutoSize nurseAutoSize;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation nursePopAnimation;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueTriangleAnimation nurseTriangleAnimation;


    //====================================================
    // BOTTOM DIALOGUE ANIMATIONS
    //====================================================

    [Header("Bottom Dialogue Animations")]

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueBoxAutoSize bottomAutoSize;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation bottomPopAnimation;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueTriangleAnimation bottomTriangleAnimation;


    //====================================================
    // STARTUP
    //====================================================

    [Header("Startup")]

    public bool startImmediately = false;


    //====================================================
    // SCENE TRANSITION
    //====================================================

    [Header("Scene Transition")]

    public SceneFade fadeManager;

    public string nextScene = "Scene5";


    //====================================================
    // OBJECTS TO HIDE
    //====================================================

    [Header("Objects To Hide After Dialogue")]

    public GameObject[] objectsToHide;


    //====================================================
    // VARIABLES
    //====================================================

    private int step = -1;

    private bool isTyping = false;

    private string currentDialogue;

    private Coroutine typingCoroutine;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(false);

        if (bottomDialogue != null)
            bottomDialogue.SetActive(false);

        if (startImmediately)
        {
            NextDialogue();
        }
    }


    //====================================================
    // UPDATE
    //====================================================

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //================================================
            // IGNORE INPUT IF CLICKING A UI BUTTON
            //================================================

            if (EventSystem.current != null)
            {
                PointerEventData pointerData =
                    new PointerEventData(EventSystem.current);

                pointerData.position = Input.mousePosition;

                List<RaycastResult> results =
                    new List<RaycastResult>();

                EventSystem.current.RaycastAll(
                    pointerData,
                    results
                );

                bool clickedButton = false;

                foreach (var result in results)
                {
                    if (result.gameObject.GetComponentInParent<Button>() != null)
                    {
                        clickedButton = true;
                        break;
                    }
                }

                if (clickedButton)
                    return;
            }


            //================================================
            // FINISH TYPING
            //================================================

            if (isTyping)
            {
                CompleteTyping();
            }
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
        step++;


        //================================================
        // END OF DIALOGUE
        //================================================

        if (step >= dialogues.Length)
        {
            if (nurseBubble != null)
                nurseBubble.SetActive(false);

            if (bottomDialogue != null)
                bottomDialogue.SetActive(false);


            // Hide additional objects

            if (objectsToHide != null)
            {
                foreach (GameObject obj in objectsToHide)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }


            // Change scene

            if (fadeManager != null)
            {
                fadeManager.FadeToScene(nextScene);
            }

            enabled = false;

            return;
        }


        //================================================
        // SAFETY CHECK
        //================================================

        if (step >= isNurseTalking.Length)
        {
            Debug.LogError(
                "[NurseStory] isNurseTalking array is shorter than dialogues array!"
            );

            return;
        }


        //================================================
        // GET CURRENT DIALOGUE
        //================================================

        currentDialogue = dialogues[step];


        //================================================
        // NURSE TALKING
        //================================================

        if (isNurseTalking[step])
        {
            ShowNurseDialogue();
        }


        //================================================
        // BOTTOM DIALOGUE
        //================================================

        else
        {
            ShowBottomDialogue();
        }
    }


    //====================================================
    // SHOW NURSE DIALOGUE
    //====================================================

    void ShowNurseDialogue()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(true);

        if (bottomDialogue != null)
            bottomDialogue.SetActive(false);


        //================================================
        // AUTO RESIZE
        //================================================

        if (nurseAutoSize != null)
        {
            nurseAutoSize.ResizeForText(currentDialogue);
        }


        //================================================
        // POP ANIMATION
        //================================================

        if (nursePopAnimation != null)
        {
            nursePopAnimation.PlayPop();
        }


        //================================================
        // TRIANGLE ANIMATION
        //================================================

        if (nurseTriangleAnimation != null)
        {
            nurseTriangleAnimation.PlayAnimation();
        }


        //================================================
        // START TYPEWRITER
        //================================================

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine =
            StartCoroutine(
                TypeText(nurseText, currentDialogue)
            );
    }


    //====================================================
    // SHOW BOTTOM DIALOGUE
    //====================================================

    void ShowBottomDialogue()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(false);

        if (bottomDialogue != null)
            bottomDialogue.SetActive(true);


        //================================================
        // AUTO RESIZE
        //================================================

        if (bottomAutoSize != null)
        {
            bottomAutoSize.ResizeForText(currentDialogue);
        }


        //================================================
        // POP ANIMATION
        //================================================

        if (bottomPopAnimation != null)
        {
            bottomPopAnimation.PlayPop();
        }


        //================================================
        // TRIANGLE ANIMATION
        //================================================

        if (bottomTriangleAnimation != null)
        {
            bottomTriangleAnimation.PlayAnimation();
        }


        //================================================
        // START TYPEWRITER
        //================================================

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine =
            StartCoroutine(
                TypeText(bottomText, currentDialogue)
            );
    }


    //====================================================
    // TYPE TEXT
    //====================================================

    IEnumerator TypeText(
        TextMeshProUGUI textUI,
        string message
    )
    {
        if (textUI == null)
            yield break;

        isTyping = true;

        textUI.text = "";

        foreach (char letter in message)
        {
            textUI.text += letter;

            yield return new WaitForSeconds(typingSpeed);
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


        //================================================
        // SHOW COMPLETE TEXT
        //================================================

        if (step >= 0 && step < isNurseTalking.Length)
        {
            if (isNurseTalking[step])
            {
                if (nurseText != null)
                    nurseText.text = currentDialogue;
            }
            else
            {
                if (bottomText != null)
                    bottomText.text = currentDialogue;
            }
        }

        isTyping = false;
    }
}