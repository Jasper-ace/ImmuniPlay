using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogueEntry
{
    [TextArea(2, 5)]
    [Tooltip("The dialogue text for this entry.")]
    public string dialogue;

    [Header("GAMEOBJECT")]
    [Tooltip("This GameObject is enabled when this dialogue starts and disabled when this dialogue finishes.")]
    public GameObject gameObjectToEnable;
}

public class Scene3GameManager : MonoBehaviour
{
    // =========================================================
    // DIALOGUE
    // =========================================================

    [Header("DIALOGUE")]
    public GameObject dialoguePanel;

    public TextMeshProUGUI dialogueText;

    [Tooltip("Add dialogue lines in the order they should appear.")]
    public DialogueEntry[] dialogues;

    [Tooltip("Seconds between each typed character. Lower = faster.")]
    public float typingSpeed = 0.05f;


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
    // DIALOGUE ANIMATIONS
    // =========================================================

    [Header("DIALOGUE ANIMATIONS")]

    [Tooltip("Automatically resizes the dialogue bubble.")]
    public DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Pop animation for the dialogue bubble.")]
    public DialoguePopAnimation dialoguePopAnimation;

    [Tooltip("Triangle animation for the dialogue bubble.")]
    public DialogueTriangleAnimation dialogueTriangleAnimation;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentDialogue = -1;

    private Coroutine typingRoutine;

    private bool pauseDialogue = false;


    // =========================================================
    // MINI-GAMES
    // =========================================================

    [Header("ALCOHOL")]
    public int alcoholDialogueIndex = 2;

    public GameObject alcoholParent;

    public GameObject alcoholImage;


    [Header("SYRINGE")]
    public int syringeDialogueIndex = 5;

    public GameObject syringeParent;

    public GameObject syringeImage;


    [Header("BANDAGE")]
    public int bandageDialogueIndex = 8;

    public GameObject bandageParent;

    public GameObject bandageImage;


    // =========================================================
    // FINISH
    // =========================================================

    [Header("FINISH")]
    public GameObject popupPanel;


    // =========================================================
    // SCENE TRANSITION
    // =========================================================

    [Header("SCENE TRANSITION")]
    public string nextSceneName;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Hide mini-game objects
        if (alcoholParent != null)
            alcoholParent.SetActive(false);

        if (syringeParent != null)
            syringeParent.SetActive(false);

        if (bandageParent != null)
            bandageParent.SetActive(false);


        // Hide finish popup
        if (popupPanel != null)
            popupPanel.SetActive(false);


        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);


        // Start dialogue
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[Scene3GameManager] No dialogues assigned!",
                this
            );

            return;
        }


        currentDialogue = 0;

        StartTyping();
    }


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    // Called by the global Dialogue Tap Input Button.
    public void AdvanceDialogue()
    {
        // Don't allow dialogue input during mini-games
        if (pauseDialogue)
            return;

        // Tap while typing = finish the current dialogue immediately.
        if (typingRoutine != null)
        {
            FinishTyping();
        }
        else
        {
            // Tap after typing = finish the current dialogue and move on.
            NextDialogue();
        }
    }


    // =========================================================
    // PROCESS DIALOGUE
    // =========================================================

    private string ProcessDialogue(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";


        // =====================================================
        // BABY NAME
        // =====================================================

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[Scene3GameManager] SaveManager.Instance is NULL. " +
                "Keeping {babyName}.",
                this
            );

            // Keep {babyName} if SaveManager doesn't exist
            return text;
        }


        string babyName =
            SaveManager.Instance.GetBabyName();


        Debug.Log(
            "[Scene3GameManager] Baby Name: [" +
            babyName +
            "]"
        );


        // =====================================================
        // IF BABY NAME EXISTS
        // =====================================================

        if (!string.IsNullOrEmpty(babyName))
        {
            text = text.Replace(
                "{babyName}",
                babyName
            );
        }

        // If babyName is null/empty,
        // {babyName} remains unchanged.

        return text;
    }


    // =========================================================
    // START TYPING
    // =========================================================

    private void StartTyping()
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
                $"[Scene3GameManager] Invalid dialogue index: " +
                $"{currentDialogue}. Array length: {dialogues.Length}",
                this
            );

            return;
        }

        if (dialogues[currentDialogue] == null)
        {
            Debug.LogError(
                $"[Scene3GameManager] Dialogue Element {currentDialogue} is empty!",
                this
            );

            return;
        }


        // =====================================================
        // ENABLE CURRENT DIALOGUE GAMEOBJECT
        // =====================================================

        EnableCurrentDialogueObject();


        // =====================================================
        // GET CURRENT SENTENCE
        // =====================================================

        string currentSentence =
            dialogues[currentDialogue].dialogue;


        // =====================================================
        // PROCESS BABY NAME
        // =====================================================

        string processedDialogue =
            ProcessDialogue(currentSentence);


        // =====================================================
        // AUTO RESIZE
        // =====================================================

        if (dialogueAutoSize != null)
        {
            if (
                dialogueAutoSize.enabled &&
                dialogueAutoSize.gameObject.activeInHierarchy
            )
            {
                dialogueAutoSize.ResizeForText(
                    processedDialogue
                );
            }
        }


        // =====================================================
        // POP ANIMATION
        // =====================================================

        if (dialoguePopAnimation != null)
        {
            if (
                dialoguePopAnimation.enabled &&
                dialoguePopAnimation.gameObject.activeInHierarchy
            )
            {
                dialoguePopAnimation.PlayPop();
            }
        }


        // =====================================================
        // TRIANGLE ANIMATION
        // =====================================================

        if (dialogueTriangleAnimation != null)
        {
            if (
                dialogueTriangleAnimation.enabled &&
                dialogueTriangleAnimation.gameObject.activeInHierarchy
            )
            {
                dialogueTriangleAnimation.PlayAnimation();
            }
        }


        // =====================================================
        // START TYPEWRITER
        // =====================================================

        typingRoutine =
            StartCoroutine(
                TypeDialogue(processedDialogue)
            );
    }


    // =========================================================
    // TYPE DIALOGUE
    // =========================================================

    private IEnumerator TypeDialogue(string dialogue)
    {
        if (dialogueText != null)
            dialogueText.text = "";


        string safeText = dialogue ?? "";


        float safeTypingSpeed =
            Mathf.Max(
                typingSpeed,
                0.001f
            );


        int characterCount = 0;


        foreach (char character in safeText)
        {
            // Add character
            if (dialogueText != null)
                dialogueText.text += character;


            // =================================================
            // TYPEWRITER SOUND
            // =================================================

            characterCount++;


            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                character != ' ' &&
                characterCount %
                Mathf.Max(
                    soundEveryCharacters,
                    1
                ) == 0
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


        typingRoutine = null;
    }


    // =========================================================
    // FINISH TYPING
    // =========================================================

    private void FinishTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);

            typingRoutine = null;
        }


        if (
            dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length
        )
        {
            return;
        }


        if (dialogues[currentDialogue] == null)
            return;

        string fullDialogue =
            ProcessDialogue(
                dialogues[currentDialogue].dialogue
            );


        if (dialogueText != null)
            dialogueText.text = fullDialogue;
    }


    // =========================================================
    // DIALOGUE GAMEOBJECTS
    // =========================================================

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


    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    private void NextDialogue()
    {
        // The current dialogue is finished, so disable its assigned GameObject.
        DisableCurrentDialogueObject();

        currentDialogue++;


        // =====================================================
        // FINISHED ALL DIALOGUE
        // =====================================================

        if (currentDialogue >= dialogues.Length)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (popupPanel != null)
                popupPanel.SetActive(true);


            return;
        }


        // =====================================================
        // ALCOHOL
        // =====================================================

        if (currentDialogue == alcoholDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (alcoholParent != null)
                alcoholParent.SetActive(true);


            return;
        }


        // =====================================================
        // SYRINGE
        // =====================================================

        if (currentDialogue == syringeDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (syringeParent != null)
                syringeParent.SetActive(true);


            return;
        }


        // =====================================================
        // BANDAGE
        // =====================================================

        if (currentDialogue == bandageDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (bandageParent != null)
                bandageParent.SetActive(true);


            return;
        }


        // =====================================================
        // START NEXT DIALOGUE
        // =====================================================

        StartTyping();
    }


    // =========================================================
    // ALCOHOL COMPLETED
    // =========================================================

    public void AlcoholCompleted()
    {
        if (alcoholParent != null)
            alcoholParent.SetActive(false);


        if (alcoholImage != null)
            alcoholImage.SetActive(false);


        ResumeDialogue();
    }


    // =========================================================
    // SYRINGE COMPLETED
    // =========================================================

    public void SyringeCompleted()
    {
        if (syringeParent != null)
            syringeParent.SetActive(false);


        if (syringeImage != null)
            syringeImage.SetActive(false);


        ResumeDialogue();
    }


    // =========================================================
    // BANDAGE COMPLETED
    // =========================================================

    public void BandageCompleted()
    {
        if (bandageParent != null)
            bandageParent.SetActive(false);


        if (bandageImage != null)
            bandageImage.SetActive(false);


        ResumeDialogue();
    }


    // =========================================================
    // RESUME DIALOGUE
    // =========================================================

    private void ResumeDialogue()
    {
        pauseDialogue = false;


        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);


        StartTyping();
    }


    // =========================================================
    // CONTINUE DIALOGUE
    // =========================================================

    public void ContinueDialogue()
    {
        ResumeDialogue();
    }


    // =========================================================
    // FINISH SCENE
    // =========================================================

    public void FinishScene()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);


        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "[Scene3GameManager] Next Scene Name is empty!",
                this
            );

            return;
        }


        UnityEngine.SceneManagement.SceneManager.LoadScene(
            nextSceneName
        );
    }
}