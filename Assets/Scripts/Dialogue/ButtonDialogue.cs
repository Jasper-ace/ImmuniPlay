using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ButtonDialogue : MonoBehaviour
{
    // =========================================================
    // DIALOGUE DATA
    // =========================================================

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;
    }


    [Header("Dialogue")]

    public Dialogue[] dialogues;


    // =========================================================
    // REUSABLE DIALOGUE BUBBLE
    // =========================================================

    [Header("Reusable Dialogue Bubble")]

    [Tooltip("The single dialogue bubble that will be reused.")]
    public GameObject dialogueBubble;

    [Tooltip("Text inside the dialogue bubble.")]
    public TextMeshProUGUI dialogueTMP;


    // =========================================================
    // OPTIONAL DIALOGUE FEATURES
    // =========================================================

    [Header("Optional Dialogue Features")]

    [Tooltip("Automatically resizes the dialogue bubble based on the text.")]
    public DialogueBoxAutoSize autoSize;

    [Tooltip("Plays the pop animation whenever a new dialogue appears.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Plays the triangle animation whenever a new dialogue appears.")]
    public DialogueTriangleAnimation triangleAnimation;


    // =========================================================
    // GAMEOBJECTS HIDDEN DURING DIALOGUE
    // =========================================================

    [Header("Objects Hidden During Dialogue")]

    [Tooltip(
        "These GameObjects will be hidden when the dialogue starts " +
        "and shown again when the dialogue is completely finished."
    )]
    public List<GameObject> objectsToHide =
        new List<GameObject>();

    // =========================================================
    // AFTER DIALOGUE
    // =========================================================

    [Header("After Dialogue")]

    [Tooltip("These GameObjects will be shown after the dialogue finishes.")]
    public List<GameObject> objectsToShowAfterDialogue =
        new List<GameObject>();

    [Tooltip("These GameObjects will be hidden after the dialogue finishes.")]
    public List<GameObject> objectsToHideAfterDialogue =
        new List<GameObject>();


    // =========================================================
    // TYPEWRITER
    // =========================================================

    [Header("Typewriter")]

    [Tooltip("Time between each character.")]
    public float typingSpeed = 0.03f;


    // =========================================================
    // TYPEWRITER SOUND
    // =========================================================

    [Header("Typewriter Sound")]

    [Tooltip("Short sound played while characters are appearing.")]
    public AudioClip typewriterSound;

    [Tooltip("Play the sound every X characters.")]
    public int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    public float typewriterVolume = 0.6f;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentIndex = 0;

    private bool isTyping = false;

    private bool dialogueActive = false;

    private Coroutine typingCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Make sure dialogue bubble is hidden
        // when the scene starts.

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);
    }


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    public void AdvanceDialogue()
    {
        // Don't process input if dialogue isn't active.
        if (!dialogueActive)
            return;

        // If the current dialogue is still typing,
        // finish the current line.
        if (isTyping)
        {
            FinishTyping();
        }
        // Otherwise go to the next dialogue.
        else
        {
            NextDialogue();
        }
    }


    // =========================================================
    // SHOW DIALOGUE
    // =========================================================

    public void ShowDialogue()
    {
        // Prevent the dialogue from restarting
        // if the button is pressed while already active.

        if (dialogueActive)
            return;


        // Make sure there is dialogue.

        if (
            dialogues == null ||
            dialogues.Length == 0
        )
        {
            Debug.LogWarning(
                "ButtonDialogue: No dialogues assigned!"
            );

            return;
        }


        // Reset dialogue position.

        currentIndex = 0;

        dialogueActive = true;


        // -----------------------------------------------------
        // HIDE SELECTED GAMEOBJECTS
        // -----------------------------------------------------

        HideObjects();


        // -----------------------------------------------------
        // SHOW DIALOGUE BUBBLE
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);


        // -----------------------------------------------------
        // SHOW FIRST DIALOGUE
        // -----------------------------------------------------

        ShowCurrentDialogue();
    }


    // =========================================================
    // SHOW CURRENT DIALOGUE
    // =========================================================

    private void ShowCurrentDialogue()
    {
        // Safety check.

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


        string text =
            dialogue.text ?? "";


        // -----------------------------------------------------
        // CLEAR OLD TEXT
        // -----------------------------------------------------

        if (dialogueTMP != null)
            dialogueTMP.text = "";


        // -----------------------------------------------------
        // AUTO SIZE
        // -----------------------------------------------------

        if (
            autoSize != null &&
            autoSize.enabled &&
            autoSize.gameObject.activeInHierarchy
        )
        {
            autoSize.ResizeForText(text);
        }


        // -----------------------------------------------------
        // POP ANIMATION
        // -----------------------------------------------------

        if (
            popAnimation != null &&
            popAnimation.enabled &&
            popAnimation.gameObject.activeInHierarchy
        )
        {
            popAnimation.PlayPop();
        }


        // -----------------------------------------------------
        // TRIANGLE ANIMATION
        // -----------------------------------------------------

        if (
            triangleAnimation != null &&
            triangleAnimation.enabled &&
            triangleAnimation.gameObject.activeInHierarchy
        )
        {
            triangleAnimation.PlayAnimation();
        }


        // -----------------------------------------------------
        // STOP PREVIOUS TYPEWRITER
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // -----------------------------------------------------
        // START TYPEWRITER
        // -----------------------------------------------------

        typingCoroutine =
            StartCoroutine(
                TypeText(text)
            );
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    private IEnumerator TypeText(string text)
    {
        isTyping = true;


        if (dialogueTMP != null)
            dialogueTMP.text = "";


        string safeText =
            text ?? "";


        float safeTypingSpeed =
            Mathf.Max(
                typingSpeed,
                0.001f
            );


        int characterCount = 0;


        // -----------------------------------------------------
        // TYPE EACH CHARACTER
        // -----------------------------------------------------

        foreach (char c in safeText)
        {
            if (dialogueTMP != null)
                dialogueTMP.text += c;


            characterCount++;


            // -------------------------------------------------
            // TYPEWRITER SOUND
            // -------------------------------------------------

            // Don't play sound for spaces or line breaks.

            if (
                c != ' ' &&
                c != '\n' &&
                c != '\r'
            )
            {
                if (
                    typewriterSound != null &&
                    AudioManager.Instance != null &&
                    soundEveryCharacters > 0 &&
                    characterCount % soundEveryCharacters == 0
                )
                {
                    AudioManager.Instance.PlayDialogue(
                        typewriterSound,
                        typewriterVolume
                    );
                }
            }


            yield return new WaitForSeconds(
                safeTypingSpeed
            );
        }


        // -----------------------------------------------------
        // TYPING FINISHED
        // -----------------------------------------------------

        isTyping = false;

        typingCoroutine = null;
    }


    // =========================================================
    // FINISH TYPING
    // =========================================================

    private void FinishTyping()
    {
        // Stop typewriter coroutine.

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // Safety check.

        if (
            dialogues == null ||
            currentIndex < 0 ||
            currentIndex >= dialogues.Length
        )
        {
            isTyping = false;

            return;
        }


        // Immediately show the entire dialogue.

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
        currentIndex++;


        // -----------------------------------------------------
        // CHECK IF ALL DIALOGUES ARE FINISHED
        // -----------------------------------------------------

        if (currentIndex >= dialogues.Length)
        {
            EndDialogue();

            return;
        }


        // -----------------------------------------------------
        // SHOW NEXT DIALOGUE
        // -----------------------------------------------------

        ShowCurrentDialogue();
    }


    // =========================================================
    // HIDE OBJECTS
    // =========================================================

    private void HideObjects()
    {
        if (objectsToHide == null)
            return;


        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }


    // =========================================================
    // SHOW OBJECTS
    // =========================================================

    private void ShowObjects()
    {
        if (objectsToHide == null)
            return;


        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }


    // =========================================================
    // END DIALOGUE
    // =========================================================

    private void EndDialogue()
    {
        dialogueActive = false;

        isTyping = false;


        // -----------------------------------------------------
        // STOP TYPEWRITER
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // -----------------------------------------------------
        // HIDE DIALOGUE BUBBLE
        // -----------------------------------------------------

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);


        // -----------------------------------------------------
        // SHOW HIDDEN OBJECTS AGAIN
        // -----------------------------------------------------

        ShowObjects();

        // -----------------------------------------------------
        // AFTER DIALOGUE - SHOW OBJECTS
        // -----------------------------------------------------

        if (objectsToShowAfterDialogue != null)
        {
            foreach (GameObject obj in objectsToShowAfterDialogue)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // -----------------------------------------------------
        // AFTER DIALOGUE - HIDE OBJECTS
        // -----------------------------------------------------

        if (objectsToHideAfterDialogue != null)
        {
            foreach (GameObject obj in objectsToHideAfterDialogue)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }


        // -----------------------------------------------------
        // CLEAR TEXT
        // -----------------------------------------------------

        if (dialogueTMP != null)
            dialogueTMP.text = "";


        Debug.Log(
            "ButtonDialogue: Dialogue finished."
        );
    }


    // =========================================================
    // MANUAL CLOSE
    // =========================================================

    public void CloseDialogue()
    {
        EndDialogue();
    }
}