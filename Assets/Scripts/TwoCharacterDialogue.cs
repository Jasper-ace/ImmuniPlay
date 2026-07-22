using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TwoCharacterDialogue : MonoBehaviour
{
    [System.Serializable]
    public class Character
    {
        public GameObject chatBubble;
        public TMP_Text textMeshPro;
    }

    [System.Serializable]
    public class DialogueLine
    {
        public enum Speaker { Character1, Character2 }
        public Speaker speaker;
        
        [TextArea(2, 4)]
        public string dialogue;
    }

    [Header("Character 1")]
    [SerializeField] private Character character1;

    [Header("Character 2")]
    [SerializeField] private Character character2;

    [Header("UI")]

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] dialogues;

    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [Tooltip("Show popup after this dialogue index. Example: 2 = after Dialogue 3.")]
    [SerializeField] private int popupAfterDialogue = 2;

    [Header("Typewriter")]
    [SerializeField] private float typingSpeed = 0.03f;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool waitingForButton = false;
    private bool popupShown = false;
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
        if (currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log("Dialogue Finished.");
            return;
        }

        DialogueLine currentLine = dialogues[currentDialogueIndex];

        // Show correct character's chat bubble and text
        Character activeCharacter = currentLine.speaker == DialogueLine.Speaker.Character1 ? character1 : character2;
        Character inactiveCharacter = currentLine.speaker == DialogueLine.Speaker.Character1 ? character2 : character1;

        if (activeCharacter.chatBubble != null)
            activeCharacter.chatBubble.SetActive(true);

        if (inactiveCharacter.chatBubble != null)
            inactiveCharacter.chatBubble.SetActive(false);

        // Start typing effect on active character's text
        if (activeCharacter.textMeshPro != null)
        {
            StartTyping(currentLine.dialogue, activeCharacter.textMeshPro);
        }
    }

    void StartTyping(string text, TMP_Text targetText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text, targetText));
    }

    IEnumerator TypeText(string text, TMP_Text targetText)
    {
        isTyping = true;
        targetText.text = "";

        foreach (char letter in text)
        {
            targetText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        DialogueLine currentLine = dialogues[currentDialogueIndex];
        Character activeCharacter = currentLine.speaker == DialogueLine.Speaker.Character1 ? character1 : character2;

        if (activeCharacter.textMeshPro != null)
        {
            activeCharacter.textMeshPro.text = currentLine.dialogue;
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        // Show popup after selected dialogue
        if (!popupShown && currentDialogueIndex == popupAfterDialogue)
        {
            ShowPopup();
            return;
        }

        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log("Dialogue Finished.");
            return;
        }

        ShowCurrentDialogue();
    }

    void ShowPopup()
    {
        popupShown = true;
        waitingForButton = true;

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        // Disable parent
        gameObject.SetActive(false);
    }

    public void ContinueFromPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // Re-enable parent
        gameObject.SetActive(true);

        waitingForButton = false;
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log("Dialogue Finished.");
            return;
        }

        ShowCurrentDialogue();
    }

    public void ResetDialogue()
    {
        currentDialogueIndex = 0;
        isTyping = false;
        waitingForButton = false;
        popupShown = false;

        if (popupPanel != null)
            popupPanel.SetActive(false);

        gameObject.SetActive(true);

        ShowCurrentDialogue();
    }
}