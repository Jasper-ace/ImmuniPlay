using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Added for scene loading
using TMPro;

public class StoryFlowManager : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Baby Identification")]
    public GameObject babyIdentificationPanel;
    public Button confirmButton;
    public TMP_InputField babyNameInput;

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;

    [Header("Drag & Drop Placeholder")]
    public GameObject dragDropPanel;
    public Button dragDropConfirmButton;

    [Tooltip("Show the Drag & Drop panel after this dialogue index.")]
    public int dragDropAfterDialogue = 5;

    [Header("Scene Transition")]
    [Tooltip("Name of the scene to load when the story finishes.")]
    public string nextSceneName; // Developer types the scene name here in the Inspector

    [Tooltip("Optional delay in seconds before loading the next scene.")]
    public float sceneLoadDelay = 0.5f;

    private int currentDialogue = 0;

    private bool waitingForConfirm = false;
    private bool panelShown = false;

    private bool waitingForDragDrop = false;
    private bool dragDropShown = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private string babyName = "Baby";

    void Start()
    {
        // Load previously saved baby name
        babyName = PlayerPrefs.GetString("BabyName", "Baby");

        if (babyIdentificationPanel != null)
            babyIdentificationPanel.SetActive(false);

        if (dragDropPanel != null)
            dragDropPanel.SetActive(false);

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmBabyIdentification);
        }

        if (dragDropConfirmButton != null)
        {
            dragDropConfirmButton.onClick.RemoveAllListeners();
            dragDropConfirmButton.onClick.AddListener(ConfirmDragDrop);
        }

        ShowCurrentDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore taps while a popup is open
            if (waitingForConfirm || waitingForDragDrop)
                return;

            // Finish typing instantly
            if (isTyping)
            {
                FinishTyping();
                return;
            }

            NextDialogue();
        }
    }

    void ShowCurrentDialogue()
    {
        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            StartCoroutine(LoadNextScene());
            return;
        }

        string dialogue = dialogues[currentDialogue];

        // Replace {baby} with the saved baby name
        dialogue = dialogue.Replace("{baby}", babyName);

        StartTyping(dialogue);
    }

    void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        string dialogue = dialogues[currentDialogue];
        dialogue = dialogue.Replace("{baby}", babyName);

        dialogueText.text = dialogue;
        isTyping = false;
    }

    void NextDialogue()
    {
        // Show Baby Identification Panel
        if (!panelShown && currentDialogue == panelAfterDialogue)
        {
            panelShown = true;
            waitingForConfirm = true;

            if (babyIdentificationPanel != null)
            {
                babyIdentificationPanel.SetActive(true);

                if (babyNameInput != null)
                {
                    babyNameInput.text = "";
                    babyNameInput.ActivateInputField();
                }
            }

            return;
        }

        // Show Drag & Drop Placeholder Panel
        if (!dragDropShown && currentDialogue == dragDropAfterDialogue)
        {
            dragDropShown = true;
            waitingForDragDrop = true;

            if (dragDropPanel != null)
                dragDropPanel.SetActive(true);

            return;
        }

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            StartCoroutine(LoadNextScene());
            return;
        }

        ShowCurrentDialogue();
    }

    public void ConfirmBabyIdentification()
    {
        if (babyNameInput == null)
        {
            Debug.LogError("Baby Name Input is not assigned.");
            return;
        }

        string enteredName = babyNameInput.text.Trim();

        if (string.IsNullOrEmpty(enteredName))
        {
            Debug.Log("Please enter a baby name.");
            babyNameInput.ActivateInputField();
            return;
        }

        // Save baby's name
        babyName = enteredName;

        PlayerPrefs.SetString("BabyName", babyName);
        PlayerPrefs.Save();

        if (babyIdentificationPanel != null)
            babyIdentificationPanel.SetActive(false);

        waitingForConfirm = false;

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            StartCoroutine(LoadNextScene());
            return;
        }

        ShowCurrentDialogue();
    }

    public void ConfirmDragDrop()
    {
        if (dragDropPanel != null)
            dragDropPanel.SetActive(false);

        waitingForDragDrop = false;

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            StartCoroutine(LoadNextScene());
            return;
        }

        ShowCurrentDialogue();
    }

    // Handles transitioning to the next scene
    private IEnumerator LoadNextScene()
    {
        if (sceneLoadDelay > 0f)
            yield return new WaitForSeconds(sceneLoadDelay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Story finished, but 'Next Scene Name' is not set in the Inspector.");
        }
    }
}