using UnityEngine;
using TMPro;
using System.Collections;

public class ChatManagers : MonoBehaviour
{
    public enum Speaker
    {
        Nurse,
        Father,
        Mother
    }

    [Header("Chat Bubbles")]
    
    public GameObject nurseBubble;
    public GameObject fatherBubble;
    public GameObject motherBubble;

    [Header("Dialogue Text")]
    public TextMeshProUGUI nurseText;
    public TextMeshProUGUI fatherText;
    public TextMeshProUGUI motherText;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogues;
    

    public Speaker[] speakers;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    public SceneFade fadeManager;
    public string nextScene = "";

    [Header("Objects To Hide After Dialogue")]
    public GameObject[] objectsToHide;

    [Header("End Option Buttons")]
    public GameObject[] choiceButtons;
    public int choiceStep = -1;

    [Header("Info Panels")]
    public GameObject epiPanel;

    private int step = -1;
    private bool isTyping = false;
    private string currentDialogue = "";
    private TextMeshProUGUI currentText;
    [Header("EPI Dialogue")]
    public DialogueTyper epiDialogue;
    private void SetChoiceButtonsActive(bool active)
    {
        if (choiceButtons != null)
        {
            foreach (GameObject btn in choiceButtons)
            {
                if (btn != null)
                    btn.SetActive(active);
            }
        }
    }

    public void OpenEPIPanel()
    {
        if (epiPanel != null)
        {
            epiPanel.SetActive(true);
            SetChoiceButtonsActive(false);

            if (epiDialogue != null)
                epiDialogue.RestartDialogue();
        }
    }

    public void CloseEPIPanel()
{
    Debug.Log("CloseEPIPanel called");

    if (epiPanel != null)
    {
        epiPanel.SetActive(false);
        SetChoiceButtonsActive(true);
    }
}

    public void ContinueDialogue()
    {
        SetChoiceButtonsActive(false);
        NextDialogue();
    }

    void Start()
    {
        HideAllBubbles();
        SetChoiceButtonsActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore input if it's on a UI Button (like the Eye Button or Choice Buttons)
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
                if (clickedButton) return;
            }

            if (isTyping)
            {
                StopAllCoroutines();

                if (currentText != null)
                    currentText.text = currentDialogue;

                isTyping = false;

                // Show choices immediately when the user skips the typing animation on the choice step
                int actualChoiceStep = choiceStep == -1 ? dialogues.Length - 1 : choiceStep;
                if (step == actualChoiceStep)
                {
                    HideAllBubbles();
                    SetChoiceButtonsActive(true);
                }
            }
            else
            {
                // If we are on the choice step and it has finished typing, prevent screen-clicks from advancing
                int actualChoiceStep = choiceStep == -1 ? dialogues.Length - 1 : choiceStep;
                if (step == actualChoiceStep)
                {
                    return;
                }

                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        step++;

        // Dialogue finished
        if (step >= dialogues.Length)
        {
            HideAllBubbles();

            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            if (fadeManager != null && !string.IsNullOrEmpty(nextScene))
            {
                fadeManager.FadeToScene(nextScene);
            }

            enabled = false;
            return;
        }

        // Safety check
        if (step >= speakers.Length)
        {
            Debug.LogWarning("Speakers array length does not match Dialogues array length.");
            return;
        }

        currentDialogue = dialogues[step];

        // Hide bubbles if dialogue is empty, but do not skip (silent pause)
        if (string.IsNullOrWhiteSpace(currentDialogue))
        {
            HideAllBubbles();
            currentText = null;
            isTyping = false;

            // Show remaining NPC eye icons during the empty element
            ConversationManager convManager = FindAnyObjectByType<ConversationManager>();
            if (convManager != null)
            {
                convManager.SetEyesActive(true);
            }
            return;
        }

        // If we are showing a valid dialogue (continuing chatting), hide the NPC eye buttons
        ConversationManager activeConvManager = FindAnyObjectByType<ConversationManager>();
        if (activeConvManager != null)
        {
            activeConvManager.SetEyesActive(false);
        }

        HideAllBubbles();

        switch (speakers[step])
        {
            case Speaker.Nurse:
                nurseBubble.SetActive(true);
                currentText = nurseText;
                break;

            case Speaker.Father:
                fatherBubble.SetActive(true);
                currentText = fatherText;
                break;

            case Speaker.Mother:
                motherBubble.SetActive(true);
                currentText = motherText;
                break;
        }

        // Ensure choices are hidden during intermediate steps
        int actualChoiceStep = choiceStep == -1 ? dialogues.Length - 1 : choiceStep;
        if (step != actualChoiceStep)
        {
            SetChoiceButtonsActive(false);
        }

        StartCoroutine(TypeText(currentText, currentDialogue));
    }

    IEnumerator TypeText(TextMeshProUGUI textUI, string message)
    {
        isTyping = true;
        textUI.text = "";

        foreach (char letter in message)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Automatically show choices when typing completes on the choice step
        int actualChoiceStep = choiceStep == -1 ? dialogues.Length - 1 : choiceStep;
        if (step == actualChoiceStep)
        {
            HideAllBubbles();
            SetChoiceButtonsActive(true);
        }
    }

    void HideAllBubbles()
    {
        if (nurseBubble != null)
            nurseBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (motherBubble != null)
            motherBubble.SetActive(false);
    }
}