using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TwoCharacterDialogue : MonoBehaviour
{
    //====================================================
    // CHARACTER
    //====================================================

    [System.Serializable]
    public class Character
    {
        [Header("Chat Bubble")]
        public GameObject chatBubble;

        [Header("Text")]
        public TMP_Text textMeshPro;

        //================================================
        // DIALOGUE ANIMATIONS
        //================================================

        [Header("Dialogue Animations")]

        [Tooltip("Optional. Automatically resizes the dialogue box.")]
        public DialogueBoxAutoSize dialogueAutoSize;

        [Tooltip("Optional. Plays the pop animation when dialogue appears.")]
        public DialoguePopAnimation popAnimation;

        [Tooltip("Optional. Plays the triangle animation when dialogue appears.")]
        public DialogueTriangleAnimation triangleAnimation;
    }

    //====================================================
    // DIALOGUE LINE
    //====================================================

    [System.Serializable]
    public class DialogueLine
    {
        public enum Speaker
        {
            Character1,
            Character2
        }

        [Header("Speaker")]
        public Speaker speaker;

        [TextArea(2, 4)]
        public string dialogue;
    }

    //====================================================
    // CHARACTERS
    //====================================================

    [Header("Character 1")]
    [SerializeField] private Character character1;

    [Header("Character 2")]
    [SerializeField] private Character character2;

    //====================================================
    // DIALOGUE
    //====================================================

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] dialogues;

    //====================================================
    // POPUP
    //====================================================

    [Header("Popup")]

    [SerializeField] private GameObject popupPanel;

    [Tooltip("Show popup after this dialogue index. Example: 2 = after Dialogue 3.")]
    [SerializeField] private int popupAfterDialogue = 2;

    //====================================================
    // TYPEWRITER
    //====================================================

    [Header("Typewriter")]

    [SerializeField] private float typingSpeed = 0.03f;

    //====================================================
    // VARIABLES
    //====================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;

    private bool waitingForButton = false;

    private bool popupShown = false;

    private Coroutine typingCoroutine;

    //====================================================
    // START
    //====================================================

    private void Start()
    {
        // Hide popup at the beginning
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Hide both bubbles first
        HideAllChatBubbles();

        // Start dialogue
        ShowCurrentDialogue();
    }

    //====================================================
    // UPDATE
    //====================================================

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // Don't advance while popup is open
        if (waitingForButton)
            return;

        // If text is currently typing,
        // finish the current line instantly.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // Otherwise go to the next dialogue
        NextDialogue();
    }

    //====================================================
    // SHOW CURRENT DIALOGUE
    //====================================================

    private void ShowCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[TwoCharacterDialogue] No dialogues assigned."
            );

            return;
        }

        if (currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log(
                "[TwoCharacterDialogue] Dialogue Finished."
            );

            return;
        }

        DialogueLine currentLine =
            dialogues[currentDialogueIndex];

        //================================================
        // DETERMINE ACTIVE CHARACTER
        //================================================

        Character activeCharacter;

        Character inactiveCharacter;

        if (currentLine.speaker ==
            DialogueLine.Speaker.Character1)
        {
            activeCharacter = character1;
            inactiveCharacter = character2;
        }
        else
        {
            activeCharacter = character2;
            inactiveCharacter = character1;
        }

        //================================================
        // CHECK ACTIVE CHARACTER
        //================================================

        if (activeCharacter == null)
        {
            Debug.LogWarning(
                "[TwoCharacterDialogue] Active character is not assigned."
            );

            return;
        }

        //================================================
        // SHOW ACTIVE BUBBLE
        //================================================

        if (activeCharacter.chatBubble != null)
        {
            activeCharacter.chatBubble.SetActive(true);
        }

        //================================================
        // HIDE INACTIVE BUBBLE
        //================================================

        if (inactiveCharacter != null &&
            inactiveCharacter.chatBubble != null)
        {
            inactiveCharacter.chatBubble.SetActive(false);
        }

        //================================================
        // PROCESS BABY NAME
        //================================================

        string processedDialogue =
            ProcessDialogue(currentLine.dialogue);

        //================================================
        // DIALOGUE BOX AUTO SIZE
        //================================================

        if (activeCharacter.dialogueAutoSize != null)
        {
            activeCharacter.dialogueAutoSize.ResizeForText(
                processedDialogue
            );
        }

        //================================================
        // POP ANIMATION
        //================================================

        if (activeCharacter.popAnimation != null)
        {
            activeCharacter.popAnimation.PlayPop();
        }

        //================================================
        // TRIANGLE ANIMATION
        //================================================

        if (activeCharacter.triangleAnimation != null)
        {
            activeCharacter.triangleAnimation.PlayAnimation();
        }

        //================================================
        // TYPEWRITER
        //================================================

        if (activeCharacter.textMeshPro != null)
        {
            StartTyping(
                processedDialogue,
                activeCharacter.textMeshPro
            );
        }
        else
        {
            Debug.LogWarning(
                "[TwoCharacterDialogue] Active character has no TMP Text assigned."
            );
        }
    }

    //====================================================
    // PROCESS BABY NAME
    //====================================================

    private string ProcessDialogue(string text)
    {
        // If the dialogue is empty,
        // simply return it.
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // If there is no {babyName} placeholder,
        // don't do anything.
        if (!text.Contains("{babyName}"))
        {
            return text;
        }

        //================================================
        // CHECK SAVEMANAGER
        //================================================

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[TwoCharacterDialogue] SaveManager.Instance is NULL. " +
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
                "[TwoCharacterDialogue] No baby name found. " +
                "Keeping {babyName}."
            );

            return text;
        }

        //================================================
        // REPLACE
        //================================================

        return text.Replace(
            "{babyName}",
            babyName
        );
    }

    //====================================================
    // START TYPING
    //====================================================

    private void StartTyping(
        string text,
        TMP_Text targetText
    )
    {
        // Stop previous typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start new typing coroutine
        typingCoroutine =
            StartCoroutine(
                TypeText(
                    text,
                    targetText
                )
            );
    }

    //====================================================
    // TYPE TEXT
    //====================================================

    private IEnumerator TypeText(
        string text,
        TMP_Text targetText
    )
    {
        isTyping = true;

        targetText.text = "";

        foreach (char letter in text)
        {
            targetText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        isTyping = false;

        typingCoroutine = null;
    }

    //====================================================
    // FINISH TYPING
    //====================================================

    private void FinishTyping()
    {
        // Stop typewriter
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Safety check
        if (dialogues == null ||
            dialogues.Length == 0 ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length)
        {
            isTyping = false;
            typingCoroutine = null;
            return;
        }

        DialogueLine currentLine =
            dialogues[currentDialogueIndex];

        Character activeCharacter;

        if (currentLine.speaker ==
            DialogueLine.Speaker.Character1)
        {
            activeCharacter = character1;
        }
        else
        {
            activeCharacter = character2;
        }

        // Put the complete processed dialogue immediately
        if (activeCharacter != null &&
            activeCharacter.textMeshPro != null)
        {
            activeCharacter.textMeshPro.text =
                ProcessDialogue(
                    currentLine.dialogue
                );
        }

        isTyping = false;

        typingCoroutine = null;
    }

    //====================================================
    // NEXT DIALOGUE
    //====================================================

    private void NextDialogue()
    {
        //================================================
        // SHOW POPUP
        //================================================

        if (!popupShown &&
            currentDialogueIndex == popupAfterDialogue)
        {
            ShowPopup();

            return;
        }

        //================================================
        // ADVANCE INDEX
        //================================================

        currentDialogueIndex++;

        //================================================
        // CHECK END
        //================================================

        if (currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log(
                "[TwoCharacterDialogue] Dialogue Finished."
            );

            return;
        }

        //================================================
        // SHOW NEXT LINE
        //================================================

        ShowCurrentDialogue();
    }

    //====================================================
    // SHOW POPUP
    //====================================================

    private void ShowPopup()
    {
        popupShown = true;

        waitingForButton = true;

        // Stop typing just in case
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }

        isTyping = false;

        // Hide chat bubbles
        HideAllChatBubbles();

        // Show popup
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        // Disable this manager
        gameObject.SetActive(false);
    }

    //====================================================
    // CONTINUE FROM POPUP
    //====================================================

    public void ContinueFromPopup()
    {
        // Hide popup
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Re-enable dialogue manager
        gameObject.SetActive(true);

        waitingForButton = false;

        // Go to next dialogue
        currentDialogueIndex++;

        // Check end
        if (dialogues == null ||
            currentDialogueIndex >= dialogues.Length)
        {
            Debug.Log(
                "[TwoCharacterDialogue] Dialogue Finished."
            );

            return;
        }

        // Show next dialogue
        ShowCurrentDialogue();
    }

    //====================================================
    // HIDE ALL CHAT BUBBLES
    //====================================================

    private void HideAllChatBubbles()
    {
        if (character1 != null &&
            character1.chatBubble != null)
        {
            character1.chatBubble.SetActive(false);
        }

        if (character2 != null &&
            character2.chatBubble != null)
        {
            character2.chatBubble.SetActive(false);
        }
    }

    //====================================================
    // RESET DIALOGUE
    //====================================================

    public void ResetDialogue()
    {
        // Stop current typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }

        // Reset variables
        currentDialogueIndex = 0;

        isTyping = false;

        waitingForButton = false;

        popupShown = false;

        // Hide popup
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Make sure manager is enabled
        gameObject.SetActive(true);

        // Hide both bubbles
        HideAllChatBubbles();

        // Start again
        ShowCurrentDialogue();
    }
}