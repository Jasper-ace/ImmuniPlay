using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FlowManager : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Popup Panel")]
    public GameObject popupPanel;
    public Button continueButton; // Add button reference

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;

    [Header("Scene Management")]
    public GameObject fadeManager; // FadeManager placeholder
    public string nextSceneName = ""; // Name of the next scene to load

    private int currentDialogue = 0;
    private bool waitingForButton = false;
    private bool panelShown = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // Setup button listener
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueFromPanel);
        }
        else
        {
            Debug.LogWarning("Continue Button is not assigned in the Inspector!");
        }

        ShowCurrentDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Don't continue while popup is open
            if (waitingForButton)
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
            return;
        }

        StartTyping(dialogues[currentDialogue]);
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

        dialogueText.text = dialogues[currentDialogue];
        isTyping = false;
    }

    void NextDialogue()
    {
        // Show popup after the selected dialogue
        if (!panelShown && currentDialogue == panelAfterDialogue)
        {
            ShowPopupPanel();
            return;
        }

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished. Loading next scene...");
            LoadNextScene();
            return;
        }

        ShowCurrentDialogue();
    }

    void ShowPopupPanel()
    {
        panelShown = true;
        waitingForButton = true;

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    // Called when the continue button is clicked
    public void ContinueFromPanel()
    {
        HidePopupPanel();
        waitingForButton = false;

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished. Loading next scene...");
            LoadNextScene();
            return;
        }

        ShowCurrentDialogue();
    }

    void HidePopupPanel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    // Optional: Skip directly to any dialogue index
    public void GoToDialogue(int dialogueIndex)
    {
        if (dialogueIndex < 0 || dialogueIndex >= dialogues.Length)
        {
            Debug.LogWarning("Dialogue index is out of range.");
            return;
        }

        HidePopupPanel();
        waitingForButton = false;
        panelShown = false;
        currentDialogue = dialogueIndex;

        ShowCurrentDialogue();
    }

    // Reset the dialogue system
    public void ResetDialogue()
    {
        currentDialogue = 0;
        waitingForButton = false;
        panelShown = false;
        isTyping = false;

        HidePopupPanel();
        ShowCurrentDialogue();
    }

    // Load next scene
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next scene name is not set!");
            return;
        }

        if (fadeManager != null)
        {
            fadeManager.SendMessage("FadeToScene", nextSceneName, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}