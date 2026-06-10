using UnityEngine;
using TMPro;
using System.Collections;

public class SAMPLE : MonoBehaviour
{
    [System.Serializable]
    public class DialogueData
    {
        [TextArea(2, 5)]
        public string dialogue;

        public bool isMotherTalking;
    }

    [Header("Speech Bubbles")]
    public GameObject motherBubble;
    public GameObject fatherBubble;

    [Header("Text Components")]
    public TMP_Text motherText;
    public TMP_Text fatherText;

    [Header("Conversation")]
    public DialogueData[] dialogues;

    [Header("Next Button (Shows After Conversation)")]
    public GameObject nextButton;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private string currentSentence;
    private Coroutine typingCoroutine;

    void Start()
    {
        motherText.text = "";
        fatherText.text = "";

        if (nextButton != null)
            nextButton.SetActive(false);

        if (dialogues.Length > 0)
        {
            ShowDialogue();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If typing, complete instantly
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogues.Length)
        {
            EndConversation();
            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        DialogueData current = dialogues[currentDialogueIndex];

        currentSentence = current.dialogue;

        if (current.isMotherTalking)
        {
            motherBubble.SetActive(true);
            fatherBubble.SetActive(false);

            motherText.text = "";

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeSentence(motherText));
        }
        else
        {
            fatherBubble.SetActive(true);
            motherBubble.SetActive(false);

            fatherText.text = "";

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeSentence(fatherText));
        }
    }

    IEnumerator TypeSentence(TMP_Text targetText)
    {
        isTyping = true;

        targetText.text = "";

        foreach (char letter in currentSentence)
        {
            targetText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void CompleteTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        DialogueData current = dialogues[currentDialogueIndex];

        if (current.isMotherTalking)
            motherText.text = currentSentence;
        else
            fatherText.text = currentSentence;

        isTyping = false;
    }

    void EndConversation()
    {
        motherBubble.SetActive(false);
        fatherBubble.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(true);

        Debug.Log("Conversation Finished");
    }
}