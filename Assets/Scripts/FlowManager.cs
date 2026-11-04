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

    //====================================================
    // DIALOGUE ANIMATIONS
    //====================================================

    [Header("Dialogue Animations")]

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Optional. Leave empty if not used.")]
    public DialogueTriangleAnimation triangleAnimation;

    //====================================================
    // POPUP PANEL
    //====================================================

    [Header("Popup Panel")]

    public GameObject popupPanel;

    public Button continueButton;

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;

    //====================================================
    // SCENE MANAGEMENT
    //====================================================

    [Header("Scene Management")]

    public GameObject fadeManager;

    public string nextSceneName = "";

    //====================================================
    // VARIABLES
    //====================================================

    private int currentDialogue = 0;

    private bool waitingForButton = false;

    private bool panelShown = false;

    private bool isTyping = false;

    private Coroutine typingCoroutine;

    //====================================================
    // START
    //====================================================

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
            Debug.LogWarning(
                "Continue Button is not assigned in the Inspector!"
            );
        }

        ShowCurrentDialogue();
    }

    //====================================================
    // UPDATE
    //====================================================

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

    //====================================================
    // SHOW CURRENT DIALOGUE
    //====================================================

    void ShowCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("No dialogues assigned.");
            return;
        }

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }

        // Replace {babyName} before typing
        string processedDialogue = ReplaceBabyName(
            dialogues[currentDialogue]
        );

        StartTyping(processedDialogue);
    }

    //====================================================
    // BABY NAME
    //====================================================

    string ReplaceBabyName(string text)
    {
        // If the dialogue doesn't contain {babyName},
        // there is nothing to replace.
        if (string.IsNullOrEmpty(text))
            return text;

        if (!text.Contains("{babyName}"))
            return text;

        string babyName = null;

        // Safely try to get the saved baby name.
        // If SaveManager doesn't exist, no error will occur.
        if (SaveManager.Instance != null)
        {
            babyName = SaveManager.Instance.GetBabyName();
        }

        // If there is no saved baby name,
        // keep the original {babyName} text.
        if (string.IsNullOrEmpty(babyName))
        {
            return text;
        }

        return text.Replace("{babyName}", babyName);
    }

    //====================================================
    // START TYPING
    //====================================================

    void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        //================================================
        // DIALOGUE BOX AUTO SIZE
        //================================================

        if (dialogueAutoSize != null)
        {
            dialogueAutoSize.ResizeForText(text);
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

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    //====================================================
    // TYPE TEXT
    //====================================================

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

        typingCoroutine = null;
    }

    //====================================================
    // FINISH TYPING
    //====================================================

    void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Process baby name again in case the name
        // has changed before finishing the line.
        string processedDialogue = ReplaceBabyName(
            dialogues[currentDialogue]
        );

        dialogueText.text = processedDialogue;

        isTyping = false;

        typingCoroutine = null;
    }

    //====================================================
    // NEXT DIALOGUE
    //====================================================

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

    //====================================================
    // SHOW POPUP
    //====================================================

    void ShowPopupPanel()
    {
        panelShown = true;

        waitingForButton = true;

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    //====================================================
    // CONTINUE FROM POPUP
    //====================================================

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

    //====================================================
    // HIDE POPUP
    //====================================================

    void HidePopupPanel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    //====================================================
    // GO TO DIALOGUE
    //====================================================

    public void GoToDialogue(int dialogueIndex)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("No dialogues assigned.");
            return;
        }

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

    //====================================================
    // RESET DIALOGUE
    //====================================================

    public void ResetDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialogue = 0;

        waitingForButton = false;

        panelShown = false;

        isTyping = false;

        HidePopupPanel();

        ShowCurrentDialogue();
    }

    //====================================================
    // LOAD NEXT SCENE
    //====================================================

    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next scene name is not set!");
            return;
        }

        if (fadeManager != null)
        {
            fadeManager.SendMessage(
                "FadeToScene",
                nextSceneName,
                SendMessageOptions.DontRequireReceiver
            );
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                nextSceneName
            );
        }
    }
}