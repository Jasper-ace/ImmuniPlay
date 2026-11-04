using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TwoDialogueAdvanced : MonoBehaviour
{
    // =========================================================
    // ENUMS
    // =========================================================

    public enum Speaker
    {
        Speaker1,
        Speaker2
    }


    // =========================================================
    // DIALOGUE LINE
    // =========================================================

    [System.Serializable]
    public class DialogueLine
    {
        [Header("DIALOGUE")]

        [Tooltip("Choose which character is speaking.")]
        public Speaker speaker;

        [TextArea(2, 5)]
        public string text;


        [Header("AFTER THIS DIALOGUE")]

        [Tooltip("GameObjects to show after this dialogue.")]
        public List<GameObject> objectsToShow = new List<GameObject>();

        [Tooltip("GameObjects to hide after this dialogue.")]
        public List<GameObject> objectsToHide = new List<GameObject>();
    }


    // =========================================================
    // FINAL ACTION
    // =========================================================

    [System.Serializable]
    public class FinalAction
    {
        [Header("SHOW GAMEOBJECTS")]

        [Tooltip("GameObjects to show after ALL dialogues are complete.")]
        public List<GameObject> objectsToShow = new List<GameObject>();


        [Header("HIDE GAMEOBJECTS")]

        [Tooltip("GameObjects to hide after ALL dialogues are complete.")]
        public List<GameObject> objectsToHide = new List<GameObject>();


        [Header("SCENE TRANSITION")]

        [Tooltip("Enable this to load another scene after all dialogues.")]
        public bool loadNextScene = false;

        [Tooltip("Name of the Unity scene to load.")]
        public string nextSceneName;

        [Tooltip("Delay before loading the next scene.")]
        public float sceneLoadDelay = 0.5f;
    }


    // =========================================================
    // DIALOGUE SETTINGS
    // =========================================================

    [Header("DIALOGUE SETTINGS")]

    [Tooltip("List of all dialogue lines.")]
    [SerializeField] private DialogueLine[] dialogues;

    [Tooltip("Time between each letter.")]
    [SerializeField] private float typingSpeed = 0.03f;


    // =========================================================
    // SPEAKER 1 UI
    // =========================================================

    [Header("SPEAKER 1 UI")]

    [Tooltip("Name of Speaker 1. Example: Mother")]
    [SerializeField] private string speaker1Name = "Speaker 1";

    [Tooltip("Speaker 1's dialogue bubble.")]
    [SerializeField] private GameObject speaker1Bubble;

    [Tooltip("Speaker 1's dialogue text.")]
    [SerializeField] private TMP_Text speaker1Text;

    [Tooltip("TextMeshPro object that displays Speaker 1's name.")]
    [SerializeField] private TMP_Text speaker1NameText;

    [Tooltip("Optional Animator for Speaker 1.")]
    [SerializeField] private Animator speaker1Animator;


    // =========================================================
    // SPEAKER 2 UI
    // =========================================================

    [Header("SPEAKER 2 UI")]

    [Tooltip("Name of Speaker 2. Example: Nurse Ana")]
    [SerializeField] private string speaker2Name = "Speaker 2";

    [Tooltip("Speaker 2's dialogue bubble.")]
    [SerializeField] private GameObject speaker2Bubble;

    [Tooltip("Speaker 2's dialogue text.")]
    [SerializeField] private TMP_Text speaker2Text;

    [Tooltip("TextMeshPro object that displays Speaker 2's name.")]
    [SerializeField] private TMP_Text speaker2NameText;

    [Tooltip("Optional Animator for Speaker 2.")]
    [SerializeField] private Animator speaker2Animator;


    // =========================================================
    // DIALOGUE PANEL
    // =========================================================

    [Header("DIALOGUE PANEL")]

    [Tooltip("Optional. The entire dialogue panel.")]
    [SerializeField] private GameObject dialoguePanel;


    // =========================================================
    // FINAL ACTION
    // =========================================================

    [Header("FINAL ACTION")]

    [SerializeField] private FinalAction finalAction;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private bool dialogueFinished = false;

    private Coroutine typingCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Hide both dialogue bubbles at the beginning.

        if (speaker1Bubble != null)
        {
            speaker1Bubble.SetActive(false);
        }

        if (speaker2Bubble != null)
        {
            speaker2Bubble.SetActive(false);
        }


        // Set the character names.

        UpdateCharacterLabels();


        // Start dialogue.

        ShowCurrentDialogue();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Detect mouse click.

        if (Input.GetMouseButtonDown(0))
        {
            NextDialogue();
        }
    }


    // =========================================================
    // UPDATE CHARACTER LABELS
    // =========================================================

    private void UpdateCharacterLabels()
    {
        if (speaker1NameText != null)
        {
            speaker1NameText.text = speaker1Name;
        }

        if (speaker2NameText != null)
        {
            speaker2NameText.text = speaker2Name;
        }
    }


    // =========================================================
    // SHOW CURRENT DIALOGUE
    // =========================================================

    private void ShowCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "TwoDialogueAdvanced: No dialogues have been assigned."
            );

            return;
        }


        // Check if all dialogues are complete.

        if (currentDialogueIndex >= dialogues.Length)
        {
            FinishDialogue();
            return;
        }


        DialogueLine currentLine =
            dialogues[currentDialogueIndex];


        // Hide both bubbles first.

        if (speaker1Bubble != null)
        {
            speaker1Bubble.SetActive(false);
        }

        if (speaker2Bubble != null)
        {
            speaker2Bubble.SetActive(false);
        }


        // =====================================================
        // SPEAKER 1
        // =====================================================

        if (currentLine.speaker == Speaker.Speaker1)
        {
            if (speaker1Bubble != null)
            {
                speaker1Bubble.SetActive(true);
            }

            if (speaker1NameText != null)
            {
                speaker1NameText.text = speaker1Name;
            }

            // Optional animation.

            if (speaker1Animator != null)
            {
                speaker1Animator.SetTrigger("Talk");
            }

            StartTypingSpeaker1(currentLine.text);
        }


        // =====================================================
        // SPEAKER 2
        // =====================================================

        else if (currentLine.speaker == Speaker.Speaker2)
        {
            if (speaker2Bubble != null)
            {
                speaker2Bubble.SetActive(true);
            }

            if (speaker2NameText != null)
            {
                speaker2NameText.text = speaker2Name;
            }

            // Optional animation.

            if (speaker2Animator != null)
            {
                speaker2Animator.SetTrigger("Talk");
            }

            StartTypingSpeaker2(currentLine.text);
        }
    }


    // =========================================================
    // SPEAKER 1 TYPEWRITER
    // =========================================================

    private void StartTypingSpeaker1(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeSpeaker1(text));
    }


    private IEnumerator TypeSpeaker1(string text)
    {
        isTyping = true;

        if (speaker1Text != null)
        {
            speaker1Text.text = "";
        }


        foreach (char letter in text)
        {
            if (speaker1Text != null)
            {
                speaker1Text.text += letter;
            }

            yield return new WaitForSeconds(typingSpeed);
        }


        isTyping = false;
        typingCoroutine = null;
    }


    // =========================================================
    // SPEAKER 2 TYPEWRITER
    // =========================================================

    private void StartTypingSpeaker2(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeSpeaker2(text));
    }


    private IEnumerator TypeSpeaker2(string text)
    {
        isTyping = true;

        if (speaker2Text != null)
        {
            speaker2Text.text = "";
        }


        foreach (char letter in text)
        {
            if (speaker2Text != null)
            {
                speaker2Text.text += letter;
            }

            yield return new WaitForSeconds(typingSpeed);
        }


        isTyping = false;
        typingCoroutine = null;
    }


    // =========================================================
    // NEXT DIALOGUE
    // =========================================================

    private void NextDialogue()
    {
        // Do nothing if the dialogue has already finished.

        if (dialogueFinished)
        {
            return;
        }


        // -----------------------------------------------------
        // IF TEXT IS STILL TYPING
        // -----------------------------------------------------

        if (isTyping)
        {
            FinishTyping();

            return;
        }


        // -----------------------------------------------------
        // EXECUTE SHOW / HIDE FOR CURRENT DIALOGUE
        // -----------------------------------------------------

        ExecuteDialogueObjects();


        // -----------------------------------------------------
        // CHECK IF THIS IS THE LAST DIALOGUE
        // -----------------------------------------------------

        if (currentDialogueIndex >= dialogues.Length - 1)
        {
            FinishDialogue();

            return;
        }


        // -----------------------------------------------------
        // MOVE TO NEXT DIALOGUE
        // -----------------------------------------------------

        currentDialogueIndex++;

        ShowCurrentDialogue();
    }


    // =========================================================
    // EXECUTE DIALOGUE OBJECT ACTIONS
    // =========================================================

    private void ExecuteDialogueObjects()
    {
        if (dialogues == null)
        {
            return;
        }


        if (currentDialogueIndex >= dialogues.Length)
        {
            return;
        }


        DialogueLine currentLine =
            dialogues[currentDialogueIndex];


        // =====================================================
        // SHOW OBJECTS
        // =====================================================

        if (currentLine.objectsToShow != null)
        {
            foreach (GameObject obj in currentLine.objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }


        // =====================================================
        // HIDE OBJECTS
        // =====================================================

        if (currentLine.objectsToHide != null)
        {
            foreach (GameObject obj in currentLine.objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
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


        if (dialogues == null)
        {
            return;
        }


        if (currentDialogueIndex >= dialogues.Length)
        {
            return;
        }


        DialogueLine currentLine =
            dialogues[currentDialogueIndex];


        // Put the complete text into the correct text box.

        if (currentLine.speaker == Speaker.Speaker1)
        {
            if (speaker1Text != null)
            {
                speaker1Text.text = currentLine.text;
            }
        }
        else if (currentLine.speaker == Speaker.Speaker2)
        {
            if (speaker2Text != null)
            {
                speaker2Text.text = currentLine.text;
            }
        }


        isTyping = false;
    }


    // =========================================================
    // FINISH ALL DIALOGUE
    // =========================================================

    private void FinishDialogue()
    {
        if (dialogueFinished)
        {
            return;
        }


        dialogueFinished = true;


        Debug.Log(
            "TwoDialogueAdvanced: All dialogues completed."
        );


        // Stop typing.

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }

        isTyping = false;


        // Hide dialogue bubbles.

        if (speaker1Bubble != null)
        {
            speaker1Bubble.SetActive(false);
        }

        if (speaker2Bubble != null)
        {
            speaker2Bubble.SetActive(false);
        }


        // Execute final actions.

        ExecuteFinalAction();
    }


    // =========================================================
    // FINAL ACTION
    // =========================================================

    private void ExecuteFinalAction()
    {
        if (finalAction == null)
        {
            Debug.Log(
                "TwoDialogueAdvanced: No final action assigned."
            );

            return;
        }


        // =====================================================
        // SHOW FINAL OBJECTS
        // =====================================================

        if (finalAction.objectsToShow != null)
        {
            foreach (GameObject obj in finalAction.objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }


        // =====================================================
        // HIDE FINAL OBJECTS
        // =====================================================

        if (finalAction.objectsToHide != null)
        {
            foreach (GameObject obj in finalAction.objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }


        // =====================================================
        // LOAD NEXT SCENE
        // =====================================================

        if (finalAction.loadNextScene)
        {
            if (string.IsNullOrEmpty(
                finalAction.nextSceneName))
            {
                Debug.LogWarning(
                    "TwoDialogueAdvanced: Next Scene Name is empty."
                );

                return;
            }


            StartCoroutine(
                LoadFinalScene(
                    finalAction.nextSceneName,
                    finalAction.sceneLoadDelay
                )
            );
        }
    }


    // =========================================================
    // LOAD FINAL SCENE
    // =========================================================

    private IEnumerator LoadFinalScene(
        string sceneName,
        float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }


        SceneManager.LoadScene(sceneName);
    }


    // =========================================================
    // PUBLIC START DIALOGUE
    // =========================================================

    public void StartDialogue()
    {
        currentDialogueIndex = 0;

        dialogueFinished = false;

        isTyping = false;


        // Show dialogue panel if assigned.

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }


        // Hide bubbles before starting.

        if (speaker1Bubble != null)
        {
            speaker1Bubble.SetActive(false);
        }

        if (speaker2Bubble != null)
        {
            speaker2Bubble.SetActive(false);
        }


        // Update names.

        UpdateCharacterLabels();


        // Start dialogue.

        ShowCurrentDialogue();
    }


    // =========================================================
    // PUBLIC CLOSE DIALOGUE
    // =========================================================

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        isTyping = false;

        dialogueFinished = true;


        // Hide bubbles.

        if (speaker1Bubble != null)
        {
            speaker1Bubble.SetActive(false);
        }

        if (speaker2Bubble != null)
        {
            speaker2Bubble.SetActive(false);
        }


        // Hide entire panel.

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}