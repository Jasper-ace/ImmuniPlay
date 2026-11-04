using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class BabyCareDialogueEntry
{
    [TextArea(2, 5)]
    [Tooltip("The dialogue text for this entry.")]
    public string dialogue;

    [Header("GAMEOBJECT")]
    [Tooltip("This GameObject is enabled when this dialogue starts and disabled when this dialogue finishes.")]
    public GameObject gameObjectToEnable;
}

public class BabyCareManager : MonoBehaviour
{
    //====================================================
    // DIALOGUE
    //====================================================

    [Header("Dialogue")]

    public GameObject dialoguePanel;

    public TextMeshProUGUI dialogueText;

    public BabyCareDialogueEntry[] dialogues;

    public float typingSpeed = 0.05f;

    private int currentDialogue = -1;

    private Coroutine typingRoutine;

    private bool pauseDialogue = false;


    //====================================================
    // DIALOGUE ANIMATIONS
    //====================================================

    [Header("Dialogue Animations")]

    [Tooltip("Optional. Automatically resizes the dialogue box.")]
    public DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Optional. Plays the pop animation.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Optional. Plays the triangle animation.")]
    public DialogueTriangleAnimation triangleAnimation;


    //====================================================
    // TYPEWRITER SOUND
    //====================================================

    [Header("Typewriter Sound")]

    [Tooltip("Short sound played while the dialogue is typing.")]
    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the typing sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float typewriterVolume = 0.6f;


    //====================================================
    // MINI-GAMES
    //====================================================

    [Header("Thermometer")]

    public int thermometerDialogueIndex = 2;

    public GameObject thermometerParent;

    public GameObject thermometerImage;


    [Header("Milk")]

    public int milkDialogueIndex = 5;

    public GameObject milkParent;

    public GameObject milkImage;


    [Header("Towel")]

    public int towelDialogueIndex = 8;

    public GameObject towelParent;

    public GameObject towelImage;

    public GameObject mainWetBaby;

    public GameObject mainDriedBaby;


    //====================================================
    // FINISH
    //====================================================

    [Header("Finish")]

    public GameObject popupPanel;


    //====================================================
    // SCENE TRANSITION
    //====================================================

    [Header("Scene Transition")]

    public string nextSceneName;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        // Hide mini-games
        if (thermometerParent != null)
            thermometerParent.SetActive(false);

        if (milkParent != null)
            milkParent.SetActive(false);

        if (towelParent != null)
            towelParent.SetActive(false);


        // Hide finish popup
        if (popupPanel != null)
            popupPanel.SetActive(false);


        // Show dialogue
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);


        // Start first dialogue
        if (dialogues != null && dialogues.Length > 0)
        {
            currentDialogue = 0;

            StartTyping();
        }
        else
        {
            Debug.LogWarning(
                "[BabyCareManager] No dialogues assigned."
            );
        }
    }


    //====================================================
    // DIALOGUE INPUT
    //====================================================

    public void AdvanceDialogue()
    {
        if (pauseDialogue)
            return;

        // First tap: finish typing
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;

            if (currentDialogue >= 0 &&
                currentDialogue < dialogues.Length)
            {
                dialogueText.text =
                    ProcessDialogue(
                        dialogues[currentDialogue].dialogue
                    );
            }

            return;
        }

        // Second tap: next dialogue
        NextDialogue();
    }


    //====================================================
    // PROCESS BABY NAME
    //====================================================

    private string ProcessDialogue(string text)
    {
        // Nothing to process
        if (string.IsNullOrEmpty(text))
            return text;


        // If this dialogue doesn't contain {babyName},
        // simply return the original text.
        if (!text.Contains("{babyName}"))
            return text;


        //================================================
        // CHECK SAVEMANAGER
        //================================================

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[BabyCareManager] SaveManager.Instance is NULL. " +
                "Keeping {babyName}."
            );

            return text;
        }


        //================================================
        // GET BABY NAME
        //================================================

        string babyName = null;

        babyName = SaveManager.Instance.GetBabyName();


        //================================================
        // CHECK BABY NAME
        //================================================

        if (string.IsNullOrEmpty(babyName))
        {
            Debug.LogWarning(
                "[BabyCareManager] No baby name found. " +
                "Keeping {babyName}."
            );

            return text;
        }


        //================================================
        // REPLACE PLACEHOLDER
        //================================================

        return text.Replace(
            "{babyName}",
            babyName
        );
    }


    //====================================================
    // TYPE DIALOGUE
    //====================================================

    IEnumerator TypeDialogue(string dialogue)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning(
                "[BabyCareManager] Dialogue Text is not assigned."
            );

            typingRoutine = null;

            yield break;
        }


        dialogueText.text = "";


        string safeDialogue = dialogue ?? "";


        float safeTypingSpeed = Mathf.Max(
            typingSpeed,
            0.001f
        );


        int characterCount = 0;


        foreach (char c in safeDialogue.ToCharArray())
        {
            dialogueText.text += c;

            characterCount++;


            //================================================
            // TYPEWRITER SOUND
            //================================================

            if (
                typewriterSound != null &&
                AudioManager.Instance != null &&
                c != ' ' &&
                characterCount %
                Mathf.Max(soundEveryCharacters, 1) == 0
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


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        DisableCurrentDialogueObject();

        currentDialogue++;


        //================================================
        // FINISHED ALL DIALOGUE
        //================================================

        if (dialogues == null ||
            currentDialogue >= dialogues.Length)
        {

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (popupPanel != null)
                popupPanel.SetActive(true);

            return;
        }


        //================================================
        // THERMOMETER
        //================================================

        if (currentDialogue == thermometerDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (thermometerParent != null)
                thermometerParent.SetActive(true);


            return;
        }


        //================================================
        // MILK
        //================================================

        if (currentDialogue == milkDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (milkParent != null)
                milkParent.SetActive(true);


            return;
        }


        //================================================
        // TOWEL
        //================================================

        if (currentDialogue == towelDialogueIndex)
        {
            pauseDialogue = true;


            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);


            if (towelParent != null)
                towelParent.SetActive(true);


            return;
        }


        //================================================
        // NORMAL DIALOGUE
        //================================================

        StartTyping();
    }


    //====================================================
    // START TYPING
    //====================================================

    void StartTyping()
    {
        EnableCurrentDialogueObject();

        // Stop previous coroutine
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);

            typingRoutine = null;
        }


        // Safety check
        if (dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
        {
            Debug.LogError(
                $"[BabyCareManager] Invalid dialogue index: " +
                $"{currentDialogue}. " +
                $"Array length: {dialogues?.Length ?? 0}"
            );

            return;
        }


        //================================================
        // PROCESS BABY NAME
        //================================================

        string currentSentence =
            ProcessDialogue(
                dialogues[currentDialogue].dialogue
            );


        //================================================
        // DIALOGUE BOX AUTO SIZE
        //================================================

        if (dialogueAutoSize != null)
        {
            dialogueAutoSize.ResizeForText(
                currentSentence
            );
        }


        //================================================
        // POP ANIMATION
        //================================================

        if (popAnimation != null)
        {
            popAnimation.PlayPop();
        }


        //================================================
        // TRIANGLE ANIMATION
        //================================================

        if (triangleAnimation != null)
        {
            triangleAnimation.PlayAnimation();
        }


        //================================================
        // TYPEWRITER
        //================================================

        typingRoutine =
            StartCoroutine(
                TypeDialogue(
                    currentSentence
                )
            );
    }


    //====================================================
    // DIALOGUE GAMEOBJECT
    //====================================================

    private void EnableCurrentDialogueObject()
    {
        if (dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
            return;

        GameObject obj = dialogues[currentDialogue].gameObjectToEnable;

        if (obj != null)
            obj.SetActive(true);
    }

    private void DisableCurrentDialogueObject()
    {
        if (dialogues == null ||
            currentDialogue < 0 ||
            currentDialogue >= dialogues.Length)
            return;

        GameObject obj = dialogues[currentDialogue].gameObjectToEnable;

        if (obj != null)
            obj.SetActive(false);
    }


    //====================================================
    // THERMOMETER
    //====================================================

    public void ThermometerCompleted()
    {
        if (thermometerParent != null)
            thermometerParent.SetActive(false);


        if (thermometerImage != null)
            thermometerImage.SetActive(false);


        ResumeDialogue();
    }


    //====================================================
    // MILK
    //====================================================

    public void MilkCompleted()
    {
        if (milkParent != null)
            milkParent.SetActive(false);


        if (milkImage != null)
            milkImage.SetActive(false);


        ResumeDialogue();
    }


    //====================================================
    // TOWEL
    //====================================================

    public void TowelCompleted()
    {
        if (towelParent != null)
            towelParent.SetActive(false);


        if (towelImage != null)
            towelImage.SetActive(false);


        //================================================
        // SWAP MAIN BABY
        //================================================

        if (mainWetBaby != null)
            mainWetBaby.SetActive(false);


        if (mainDriedBaby != null)
            mainDriedBaby.SetActive(true);


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


    //====================================================
    // FINISH
    //====================================================

    public void FinishScene()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);


        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "[BabyCareManager] Next scene name is not set!"
            );

            return;
        }


        UnityEngine.SceneManagement.SceneManager.LoadScene(
            nextSceneName
        );
    }
}