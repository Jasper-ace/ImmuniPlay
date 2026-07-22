using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueFlowManager : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Popup Panel")]
    public GameObject popupPanel;

    [Tooltip("Show the panel after this dialogue index.\nExample: 2 = after Dialogue 3.")]
    public int panelAfterDialogue = 2;

    private int currentDialogue = 0;
    private bool waitingForButton = false;
    private bool panelShown = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

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
            panelShown = true;
            waitingForButton = true;

            if (popupPanel != null)
                popupPanel.SetActive(true);

            return;
        }

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }

        ShowCurrentDialogue();
    }

    // Assign this method to your UI Button's OnClick()
    public void ContinueFromPanel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        waitingForButton = false;

        currentDialogue++;

        if (currentDialogue >= dialogues.Length)
        {
            Debug.Log("Story Finished.");
            return;
        }

        ShowCurrentDialogue();
    }

    // Optional: Skip directly to any dialogue index
    public void GoToDialogue(int dialogueIndex)
    {
        if (dialogueIndex < 0 || dialogueIndex >= dialogues.Length)
        {
            Debug.LogWarning("Dialogue index is out of range.");
            return;
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);

        waitingForButton = false;
        currentDialogue = dialogueIndex;

        ShowCurrentDialogue();
    }
}