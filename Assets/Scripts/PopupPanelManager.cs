using UnityEngine;
using TMPro;
using System.Collections;

public class PopupPanelManager : MonoBehaviour
{
    //====================================================
    // REFERENCES
    //====================================================
    [SerializeField] private BabyCareManager babyCareManager;

    [Header("Dialogue")]
    [SerializeField] private GameObject chatBubble;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private string[] dialogues;
    [SerializeField] private float typingSpeed = 0.05f;

    //====================================================
    // STATE
    //====================================================
    private int currentDialogueIndex = 0;
    private Coroutine typingRoutine;
    private bool isTyping = false;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        if (chatBubble != null)
            chatBubble.SetActive(true);

        if (dialogues.Length > 0)
        {
            currentDialogueIndex = 0;
            typingRoutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex]));
        }
    }

    //====================================================
    // UPDATE
    //====================================================
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip typing
                if (typingRoutine != null)
                    StopCoroutine(typingRoutine);

                dialogueText.text = dialogues[currentDialogueIndex];
                isTyping = false;
            }
            else
            {
                // Next dialogue
                NextDialogue();
            }
        }
    }

    //====================================================
    // TYPE DIALOGUE
    //====================================================
    IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in dialogue)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    //====================================================
    // NEXT DIALOGUE
    //====================================================
    void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogues.Length)
        {
            CompleteThermometer();
            return;
        }

        typingRoutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex]));
    }

    //====================================================
    // COMPLETE THERMOMETER
    //====================================================
    void CompleteThermometer()
    {
        if (chatBubble != null)
            chatBubble.SetActive(false);

        gameObject.SetActive(false);

        // Redirect to BabyCareManager
        if (babyCareManager != null)
        {
            babyCareManager.ThermometerCompleted();
            Debug.Log("Thermometer dialogue complete!");
        }
        else
        {
            Debug.LogError("BabyCareManager not assigned!");
        }
    }
}