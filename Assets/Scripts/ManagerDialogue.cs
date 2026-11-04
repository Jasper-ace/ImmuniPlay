using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class ManagerDialogueEntry
{
    [TextArea(2, 5)]
    [Tooltip("The dialogue text for this entry.")]
    public string dialogue;

    [Header("GAMEOBJECT")]
    [Tooltip("This GameObject is enabled when this dialogue starts and disabled when this dialogue finishes.")]
    public GameObject gameObjectToEnable;
}

public class ManagerDialogue : MonoBehaviour
{
    //====================================================
    // DIALOGUE
    //====================================================

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public ManagerDialogueEntry[] dialogues;
    public float typingSpeed = 0.05f;

    //====================================================
    // TYPEWRITER SOUND
    //====================================================

    [Header("TYPEWRITER SOUND")]

    [Tooltip("Short sound played while the dialogue is typing.")]
    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the typing sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float typewriterVolume = 0.6f;

    //====================================================
    // DIALOGUE ANIMATIONS
    //====================================================

    [Header("DIALOGUE ANIMATIONS")]

    [Tooltip("Automatically resizes the dialogue bubble.")]
    public DialogueBoxAutoSize autoSize;

    [Tooltip("Pop animation for the dialogue bubble.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Triangle animation for the dialogue bubble.")]
    public DialogueTriangleAnimation triangleAnimation;

    //====================================================
    // INTERNAL
    //====================================================

    private int currentDialogue = -1;
    private Coroutine typingRoutine;
    private bool pauseDialogue = false;

    //====================================================
    // ALCOHOL
    //====================================================

    [Header("Alcohol")]
    public int alcoholDialogueIndex = 2;
    public GameObject alcoholParent;
    public GameObject alcoholImage;

    //====================================================
    // START
    //====================================================

    void Start()
    {
        if (alcoholParent != null)
            alcoholParent.SetActive(false);

        if (alcoholImage != null)
            alcoholImage.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogues != null && dialogues.Length > 0)
        {
            currentDialogue = 0;
            StartTyping();
        }
    }

    //====================================================
    // DIALOGUE INPUT
    //====================================================

    // Called by the global Dialogue Tap Input Button.
    public void AdvanceDialogue()
    {
        if (pauseDialogue)
            return;

        //================================================
        // FINISH CURRENT TYPING
        //================================================

        if (typingRoutine != null)
        {
            FinishTyping();
        }
        else
        {
            //================================================
            // NEXT DIALOGUE
            //================================================

            NextDialogue();
        }
    }

    //====================================================
    // FINISH TYPING
    //====================================================

    private void FinishTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (dialogueText != null &&
            currentDialogue >= 0 &&
            currentDialogue < dialogues.Length &&
            dialogues[currentDialogue] != null)
        {
            dialogueText.text = dialogues[currentDialogue].dialogue ?? "";
        }
    }

    //====================================================
    // TYPE DIALOGUE
    //====================================================

    IEnumerator TypeDialogue(string dialogue)
    {
        if (dialogueText != null)
            dialogueText.text = "";

        string safeDialogue = dialogue ?? "";

        float safeTypingSpeed = Mathf.Max(typingSpeed, 0.001f);

        int characterCount = 0;

        foreach (char c in safeDialogue.ToCharArray())
        {
            //================================================
            // ADD CHARACTER
            //================================================

            if (dialogueText != null)
                dialogueText.text += c;

            characterCount++;

            //================================================
            // TYPEWRITER SOUND
            //================================================

            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                c != ' ' &&
                characterCount % Mathf.Max(soundEveryCharacters, 1) == 0
            )
            {
                AudioManager.Instance.PlayDialogue(
                    typewriterSound,
                    typewriterVolume
                );
            }

            yield return new WaitForSeconds(safeTypingSpeed);
        }

        typingRoutine = null;
    }

    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        // Disable the GameObject assigned to the dialogue
        // that just finished.
        DisableCurrentDialogueObject();

        currentDialogue++;

        if (dialogues == null ||
            currentDialogue >= dialogues.Length)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            return;
        }

        //================================================
        // ALCOHOL
        //================================================

        if (currentDialogue == alcoholDialogueIndex)
        {
            pauseDialogue = true;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (alcoholParent != null)
                alcoholParent.SetActive(true);

            return;
        }

        StartTyping();
    }

    //====================================================
    // START TYPING
    //====================================================

    void StartTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
        {
            Debug.LogError(
                $"Invalid dialogue index: {currentDialogue}. " +
                $"Array length: {dialogues.Length}"
            );

            return;
        }

        //================================================
        // ENABLE GAMEOBJECT FOR THIS DIALOGUE
        //================================================

        EnableCurrentDialogueObject();

        string currentText = dialogues[currentDialogue] != null
            ? dialogues[currentDialogue].dialogue ?? ""
            : "";

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
                autoSize.ResizeForText(currentText);
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

        //================================================
        // START TYPEWRITER
        //================================================

        typingRoutine = StartCoroutine(
            TypeDialogue(currentText)
        );
    }

    //====================================================
    // DIALOGUE GAMEOBJECTS
    //====================================================

    private void EnableCurrentDialogueObject()
    {
        if (dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
        {
            return;
        }

        if (dialogues[currentDialogue] == null)
            return;

        GameObject target =
            dialogues[currentDialogue].gameObjectToEnable;

        if (target != null)
            target.SetActive(true);
    }

    private void DisableCurrentDialogueObject()
    {
        if (dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
        {
            return;
        }

        if (dialogues[currentDialogue] == null)
            return;

        GameObject target =
            dialogues[currentDialogue].gameObjectToEnable;

        if (target != null)
            target.SetActive(false);
    }

    //====================================================
    // ALCOHOL COMPLETED
    //====================================================

    public void AlcoholCompleted()
    {
        if (alcoholParent != null)
            alcoholParent.SetActive(false);

        if (alcoholImage != null)
            alcoholImage.SetActive(false);

        ResumeDialogue();
    }

    //====================================================
    // RESUME DIALOGUE
    //====================================================

    void ResumeDialogue()
    {
        pauseDialogue = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartTyping();
    }

    //====================================================
    // CONTINUE DIALOGUE
    //====================================================

    public void ContinueDialogue()
    {
        ResumeDialogue();
    }
}