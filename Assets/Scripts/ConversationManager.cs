using TMPro;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    private int conversationStartFrame;
    private bool npc1EyeCompleted = false;
    private bool npc2EyeCompleted = false;
    private bool npc3EyeCompleted = false;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullSentence;

    [Header("Mother")]
    public GameObject motherBubble;
    public TMP_Text motherText;

    [Header("NPC Bubbles")]
    public GameObject npc1Bubble;
    public TMP_Text npc1Text;

    public GameObject npc2Bubble;
    public TMP_Text npc2Text;

    public GameObject npc3Bubble;
    public TMP_Text npc3Text;

    [Header("Eye Buttons")]
    public GameObject npc1Eye;
    public GameObject npc2Eye;
    public GameObject npc3Eye;

    [Header("Conversations")]
    public DialogueData[] conversations;

    private int currentConversation;
    private int currentLine;

    private bool conversationActive = false;

    private GameObject clickedEye;

    private void Start()
    {
        SetEyesActive(false);
    }

    public void SetEyesActive(bool active)
    {
        if (npc1Eye != null) npc1Eye.SetActive(active && !npc1EyeCompleted);
        if (npc2Eye != null) npc2Eye.SetActive(active && !npc2EyeCompleted);
        if (npc3Eye != null) npc3Eye.SetActive(active && !npc3EyeCompleted);
    }

    public void StartConversation(int id, GameObject eye)
    {
        // Don't start another conversation if one is already active
        if (conversationActive)
            return;

        // Check if the conversation exists
        if (id < 0 || id >= conversations.Length)
        {
            Debug.LogError("Conversation ID does not exist.");
            return;
        }

        // Disable intro dialogue managers to prevent double-talking/clicks
        ChatManagers chatManagers = FindAnyObjectByType<ChatManagers>();
        if (chatManagers != null)
        {
            chatManagers.enabled = false;
        }

        NurseStory nurseStory = FindAnyObjectByType<NurseStory>();
        if (nurseStory != null)
        {
            nurseStory.enabled = false;
        }

        // Save the clicked eye
        clickedEye = eye;

        // Save the current conversation
        currentConversation = id;
        currentLine = 0;

        // Start the conversation
        conversationActive = true;

        // Track the starting frame to avoid double-processing the trigger click
        conversationStartFrame = Time.frameCount;

        // Hide all eye buttons while talking
        npc1Eye.SetActive(false);
        npc2Eye.SetActive(false);
        npc3Eye.SetActive(false);

        // Show the first dialogue
        ShowCurrentLine();
    }

    private void Update()
    {
        if (!conversationActive)
            return;

        // Ignore input on the frame that the conversation started
        if (Time.frameCount == conversationStartFrame)
            return;

        bool shouldAdvance = false;

        if (Input.GetMouseButtonDown(0))
        {
            // Don't advance if clicking on a UI Button (e.g. HamburgerMenu, HomeButton)
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                bool clickedButton = false;
                foreach (var result in results)
                {
                    if (result.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                    {
                        clickedButton = true;
                        break;
                    }
                }
                if (!clickedButton)
                {
                    shouldAdvance = true;
                }
            }
            else
            {
                shouldAdvance = true;
            }
        }
        else
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Don't advance if touching a UI Button
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                    {
                        position = Input.GetTouch(0).position
                    };
                    var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                    bool clickedButton = false;
                    foreach (var result in results)
                    {
                        if (result.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                        {
                            clickedButton = true;
                            break;
                        }
                    }
                    if (!clickedButton)
                    {
                        shouldAdvance = true;
                    }
                }
                else
                {
                    shouldAdvance = true;
                }
            }
#endif
        }

        if (shouldAdvance)
        {
            NextDialogue();
        }
    }

public void NextDialogue()
{
    // If still typing, finish the sentence first
    if (isTyping)
    {
        StopCoroutine(typingCoroutine);
        isTyping = false;

        DialogueLine line = conversations[currentConversation].lines[currentLine];

        switch (line.speaker)
        {
            case Speaker.Mother:
                motherText.text = fullSentence;
                break;

            case Speaker.NPC1:
                npc1Text.text = fullSentence;
                break;

            case Speaker.NPC2:
                npc2Text.text = fullSentence;
                break;

            case Speaker.NPC3:
                npc3Text.text = fullSentence;
                break;
        }

        return;
    }

    currentLine++;

    if (currentLine >= conversations[currentConversation].lines.Length)
    {
        EndConversation();
        return;
    }

    ShowCurrentLine();
}

    private void ShowCurrentLine()
    {
        HideAllBubbles();

        DialogueLine line = conversations[currentConversation].lines[currentLine];

        switch (line.speaker)
        {
            case Speaker.Mother:
                motherBubble.SetActive(true);
                StartTyping(motherText, line.dialogue);
                break;

            case Speaker.NPC1:
                npc1Bubble.SetActive(true);
                StartTyping(npc1Text, line.dialogue);
                break;

            case Speaker.NPC2:
                npc2Bubble.SetActive(true);
                StartTyping(npc2Text, line.dialogue);
                break;

            case Speaker.NPC3:
                npc3Bubble.SetActive(true);
                StartTyping(npc3Text, line.dialogue);
                break;
        }
    }

    private void StartTyping(TMP_Text textComponent, string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        fullSentence = text;
        typingCoroutine = StartCoroutine(TypeSentence(textComponent, text));
    }

    private System.Collections.IEnumerator TypeSentence(TMP_Text textComponent, string text)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void HideAllBubbles()
    {
        motherBubble.SetActive(false);

        npc1Bubble.SetActive(false);
        npc2Bubble.SetActive(false);
        npc3Bubble.SetActive(false);
    }

    private void EndConversation()
    {
        conversationActive = false;

        HideAllBubbles();

        // Mark the clicked eye as permanently completed
        if (clickedEye == npc1Eye)
            npc1EyeCompleted = true;
        else if (clickedEye == npc2Eye)
            npc2EyeCompleted = true;
        else if (clickedEye == npc3Eye)
            npc3EyeCompleted = true;

        // Show remaining active eye buttons
        SetEyesActive(true);

        // Re-enable ChatManagers so the player can continue the main story sequence
        ChatManagers chatManagers = FindAnyObjectByType<ChatManagers>();
        if (chatManagers != null)
        {
            chatManagers.enabled = true;
        }

        Debug.Log("Conversation Finished");
    }
}