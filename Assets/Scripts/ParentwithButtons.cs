using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ParentwithButtons : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public string speaker;

        [TextArea(2, 5)]
        public string text;
    }

    [Header("Dialogue")]
    public Dialogue[] dialogues;

    [Header("Father")]
    public GameObject fatherBubble;
    public TMP_Text fatherTMP;

    [Header("Mother")]
    public GameObject motherBubble;
    public TMP_Text motherTMP;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("OPTIONAL - Popup After Dialogue")]
    public bool showPopupAfterDialogue = false;
    public GameObject popupParent;

    [Header("OPTIONAL - Change Parent")]
    public bool changeParentAfterDialogue = false;
    public GameObject parentToShow;

    [Header("OPTIONAL - Next Scene")]
    public bool changeSceneAfterDialogue = false;
    public string nextSceneName;

    private int currentDialogue = 0;

    private bool isTyping = false;
    private bool dialogueFinished = false;

    private Coroutine typingCoroutine;

    void Start()
    {
        // Hide dialogue objects
        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (motherBubble != null)
            motherBubble.SetActive(false);

        // Hide popup initially
        if (popupParent != null)
            popupParent.SetActive(false);

        // Hide target parent initially
        if (parentToShow != null)
            parentToShow.SetActive(false);

        // Start dialogue
        if (dialogues != null && dialogues.Length > 0)
        {
            ShowDialogue();
        }
        else
        {
            Debug.LogWarning("No dialogues added.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !dialogueFinished)
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        // If text is still typing
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // Text finished
        NextDialogue();
    }

    void ShowDialogue()
    {
        if (currentDialogue >= dialogues.Length)
        {
            FinishDialogue();
            return;
        }

        Dialogue dialogue = dialogues[currentDialogue];

        // Hide both bubbles
        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (motherBubble != null)
            motherBubble.SetActive(false);

        // Stop previous typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        string speaker = dialogue.speaker.ToLower();

        // FATHER
        if (speaker == "father")
        {
            if (fatherBubble != null)
                fatherBubble.SetActive(true);

            if (fatherTMP != null)
                fatherTMP.text = "";

            if (fatherTMP != null)
            {
                typingCoroutine = StartCoroutine(
                    TypeText(fatherTMP, dialogue.text)
                );
            }
        }

        // MOTHER
        else if (speaker == "mother")
        {
            if (motherBubble != null)
                motherBubble.SetActive(true);

            if (motherTMP != null)
                motherTMP.text = "";

            if (motherTMP != null)
            {
                typingCoroutine = StartCoroutine(
                    TypeText(motherTMP, dialogue.text)
                );
            }
        }

        else
        {
            Debug.LogWarning(
                "Unknown speaker: " + dialogue.speaker
            );
        }
    }

    IEnumerator TypeText(TMP_Text textObject, string text)
    {
        isTyping = true;

        textObject.text = "";

        foreach (char letter in text)
        {
            textObject.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        Dialogue dialogue = dialogues[currentDialogue];

        string speaker = dialogue.speaker.ToLower();

        if (speaker == "father" && fatherTMP != null)
        {
            fatherTMP.text = dialogue.text;
        }
        else if (speaker == "mother" && motherTMP != null)
        {
            motherTMP.text = dialogue.text;
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            FinishDialogue();
            return;
        }

        ShowDialogue();
    }

    void FinishDialogue()
    {
        dialogueFinished = true;

        // Hide dialogue bubbles
        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (motherBubble != null)
            motherBubble.SetActive(false);

        // =========================================
        // OPTIONAL: SHOW POPUP
        // =========================================

        if (showPopupAfterDialogue)
        {
            if (popupParent != null)
            {
                popupParent.SetActive(true);
            }
        }

        // =========================================
        // OPTIONAL: CHANGE PARENT
        // =========================================

        if (changeParentAfterDialogue)
        {
            if (parentToShow != null)
            {
                parentToShow.SetActive(true);
            }
        }

        // =========================================
        // OPTIONAL: CHANGE SCENE
        // =========================================

        if (changeSceneAfterDialogue)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning(
                    "Change Scene is enabled, but Next Scene Name is empty."
                );
            }
        }
    }
}