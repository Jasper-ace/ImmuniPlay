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
    [Tooltip("Buttons shown one at a time, in order, when SetChoiceButtonsActive(true) is triggered.")]
    public GameObject[] choiceButtons;
    [Tooltip("The parent panel each choice button opens (same order/index as choiceButtons). Leave a slot as None if that button has no panel (e.g. a plain 'Continue' button).")]
    public GameObject[] choiceButtonParents;
    public int choiceStep = -1;

    private int currentOpenChoiceParentIndex = -1;

    [Header("End Sequence Buttons (after dialogue finishes)")]
    [Tooltip("3 buttons shown one at a time, in order.")]
    public GameObject[] endButtons;
    [Tooltip("The parent panel each button opens (same order/index as endButtons).")]
    public GameObject[] endButtonParents;

    private int currentOpenEndParentIndex = -1;

    [Header("First Button After Dialogue")]
    [Tooltip("Shown as soon as the dialogue finishes. Each button then reveals the next one via its own SequentialButtonUnlocker component.")]
    public GameObject firstButtonAfterDialogue;

    [Header("Info Panels")]
    public GameObject epiPanel;

    [Header("Mother Character (for flipping)")]
    [Tooltip("The mother's character GameObject/Transform. Flipped horizontally while she talks to the Nurse/Father.")]
    public Transform motherCharacter;

    private int step = -1;
    private bool isTyping = false;
    private string currentDialogue = "";
    private TextMeshProUGUI currentText;
    private bool pendingMotherFlip = false;
    [Header("EPI Dialogue")]
    public DialogueTyper epiDialogue;
    private void SetChoiceButtonsActive(bool active)
    {
        if (choiceButtons == null) return;

        if (!active)
        {
            // Hide everything and reset the sequence
            currentOpenChoiceParentIndex = -1;

            foreach (GameObject btn in choiceButtons)
            {
                if (btn != null)
                    btn.SetActive(false);
            }

            if (choiceButtonParents != null)
            {
                foreach (GameObject parent in choiceButtonParents)
                {
                    if (parent != null)
                        parent.SetActive(false);
                }
            }
            return;
        }

        // active == true: start the sequence - show only the first button
        currentOpenChoiceParentIndex = -1;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                choiceButtons[i].SetActive(i == 0);
        }

        if (choiceButtonParents != null)
        {
            foreach (GameObject parent in choiceButtonParents)
            {
                if (parent != null)
                    parent.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hook this up to each choice button's OnClick, passing its index (0, 1, 2...).
    /// Hides the button and opens its matching parent panel.
    /// </summary>
    public void OnChoiceButtonClicked(int index)
    {
        if (choiceButtons == null || choiceButtonParents == null) return;
        if (index < 0 || index >= choiceButtons.Length || index >= choiceButtonParents.Length) return;

        if (choiceButtons[index] != null)
            choiceButtons[index].SetActive(false);

        if (choiceButtonParents[index] != null)
        {
            currentOpenChoiceParentIndex = index;
            choiceButtonParents[index].SetActive(true);
        }
        else
        {
            // No panel for this button (e.g. a plain "Continue" button) - skip ahead
            AdvanceChoiceSequence(index);
        }
    }

    /// <summary>
    /// Hook this up to each panel's own close/exit button (no arguments needed).
    /// Reveals the next choice button in the sequence.
    /// </summary>
    public void CloseCurrentChoiceParent()
    {
        if (currentOpenChoiceParentIndex < 0 || choiceButtonParents == null) return;
        if (currentOpenChoiceParentIndex >= choiceButtonParents.Length) return;

        int closedIndex = currentOpenChoiceParentIndex;

        if (choiceButtonParents[closedIndex] != null)
            choiceButtonParents[closedIndex].SetActive(false);

        currentOpenChoiceParentIndex = -1;

        AdvanceChoiceSequence(closedIndex);
    }

    private void AdvanceChoiceSequence(int justFinishedIndex)
    {
        int nextIndex = justFinishedIndex + 1;

        if (choiceButtons != null && nextIndex < choiceButtons.Length && choiceButtons[nextIndex] != null)
        {
            choiceButtons[nextIndex].SetActive(true);
        }
        else
        {
            // No more buttons in this chain (e.g. EPI's panel just closed) -
            // resume the normal dialogue flow. Whatever lines come next will
            // play out as usual, and firstButtonAfterDialogue (e.g. Continue)
            // will show automatically once the true last line finishes.
            ContinueDialogue();
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
        // Re-enable Update() in case there's more dialogue after this point
        // that still needs normal tap-to-advance behavior.
        enabled = true;

        SetChoiceButtonsActive(false);
        NextDialogue();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dialogues != null && speakers != null && dialogues.Length != speakers.Length)
        {
            Debug.LogWarning("[ChatManagers] '" + gameObject.name + "': Dialogues (" + dialogues.Length +
                ") and Speakers (" + speakers.Length + ") array lengths don't match. " +
                "Every dialogue line needs a matching Speaker entry at the same index, or lines will be skipped at runtime.", this);
        }
    }
#endif

    void Start()
    {
        Debug.Log("[ChatManagers] Start() called on GameObject: " + gameObject.name + " | enabled=" + enabled + " | activeInHierarchy=" + gameObject.activeInHierarchy);

        HideAllBubbles();
        SetChoiceButtonsActive(false);

        // Initial state: talking to Nurse/Father, so flip immediately (no click needed).
        FlipMother(true);
    }

    /// <summary>
    /// Called by ConversationManager when an NPC conversation ends.
    /// Mother stays facing the NPC direction until the player clicks the screen
    /// to continue the main dialogue - that click is what triggers the flip back.
    /// </summary>
    public void QueueMotherFlipBack()
    {
        pendingMotherFlip = true;
    }

    private void FlipMother(bool flip)
    {
        if (motherCharacter == null) return;

        Vector3 scale = motherCharacter.localScale;
        scale.x = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        motherCharacter.localScale = scale;
    }

    /// <summary>
    /// Called once the last dialogue line has finished. Hides everything and
    /// reveals just the first end button. If no end buttons are configured,
    /// falls back to the old behavior of fading straight to the next scene.
    /// </summary>
   private void StartEndButtonSequence()
{
    Debug.Log("===== START END BUTTON SEQUENCE =====");

    currentOpenEndParentIndex = -1;

    if (endButtons == null)
    {
        Debug.LogError("endButtons is NULL!");
        return;
    }

    if (endButtons.Length == 0)
    {
        Debug.LogError("endButtons has 0 elements!");
        return;
    }

    Debug.Log("End Buttons count: " + endButtons.Length);

    // Hide all buttons first
    for (int i = 0; i < endButtons.Length; i++)
    {
        if (endButtons[i] != null)
        {
            Debug.Log(
                "Hiding button " + i + ": " +
                endButtons[i].name
            );

            endButtons[i].SetActive(false);
        }
        else
        {
            Debug.LogError(
                "End Button " + i + " is NULL!"
            );
        }
    }

    // Show first button
    if (endButtons[0] != null)
    {
        Debug.Log(
            "SHOWING FIRST BUTTON: " +
            endButtons[0].name
        );

        endButtons[0].SetActive(true);

        Debug.Log(
            "activeSelf = " +
            endButtons[0].activeSelf +
            " | activeInHierarchy = " +
            endButtons[0].activeInHierarchy
        );
    }
}

    /// <summary>
    /// Hook this up to each end button's OnClick, passing its index (0, 1, 2).
    /// Hides the button and opens its matching parent panel.
    /// </summary>
    public void OnEndButtonClicked(int index)
    {
        if (endButtons == null || endButtonParents == null) return;
        if (index < 0 || index >= endButtons.Length || index >= endButtonParents.Length) return;

        if (endButtons[index] != null)
            endButtons[index].SetActive(false);

        if (endButtonParents[index] != null)
        {
            // Normal case: this button opens a panel; wait for its close button.
            currentOpenEndParentIndex = index;
            endButtonParents[index].SetActive(true);
        }
        else
        {
            // No panel assigned for this button (e.g. a plain "Continue..." button) -
            // skip straight to revealing the next button / finishing the sequence.
            AdvanceEndSequence(index);
        }
    }

    /// <summary>
    /// Hook this up to each parent panel's own close/exit button (no arguments needed -
    /// it closes whichever panel is currently open). Reveals the next button in the
    /// sequence, or finishes up (fades to next scene) once all 3 have been done.
    /// </summary>
    public void CloseCurrentEndParent()
    {
        if (currentOpenEndParentIndex < 0 || endButtonParents == null) return;
        if (currentOpenEndParentIndex >= endButtonParents.Length) return;

        int closedIndex = currentOpenEndParentIndex;

        if (endButtonParents[closedIndex] != null)
            endButtonParents[closedIndex].SetActive(false);

        currentOpenEndParentIndex = -1;

        AdvanceEndSequence(closedIndex);
    }

    /// <summary>
    /// Reveals the button after justFinishedIndex, or - if that was the last one -
    /// fades to the next scene (if configured).
    /// </summary>
    private void AdvanceEndSequence(int justFinishedIndex)
    {
        int nextIndex = justFinishedIndex + 1;

        if (endButtons != null && nextIndex < endButtons.Length && endButtons[nextIndex] != null)
        {
            endButtons[nextIndex].SetActive(true);
        }
        else
        {
            // All buttons done - proceed to the next scene, if configured
            if (fadeManager != null && !string.IsNullOrEmpty(nextScene))
            {
                fadeManager.FadeToScene(nextScene);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[ChatManagers] Click detected. step=" + step);

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
                        Debug.Log("[ChatManagers] Click was on a Button: " + result.gameObject.name + " (blocking advance)");
                        break;
                    }
                }
                if (clickedButton) return;
            }

            // First valid screen click after returning from an NPC conversation:
            // flip the mother back to facing the Nurse/Father before handling the click.
            if (pendingMotherFlip)
            {
                FlipMother(true);
                pendingMotherFlip = false;
            }

            if (isTyping)
            {
                // First click: just finish typing this line's text - do NOT reveal
                // choice buttons yet. The player needs a separate click after this
                // to actually see the buttons, so they get a chance to read.
                StopAllCoroutines();

                if (currentText != null)
                    currentText.text = currentDialogue;

                isTyping = false;
            }
            else
            {
                // Not typing anymore. If we're on the choice step, THIS click is what
                // reveals the buttons (whether the line finished typing naturally or
                // was skipped by the previous click) - either way it now waits for
                // this explicit click before showing anything.
                if (choiceStep >= 0 && step == choiceStep)
                {
                    HideAllBubbles();
                    SetChoiceButtonsActive(true);

                    // Stop listening for plain screen-taps now that the choice
                    // buttons are showing - otherwise every tap elsewhere would
                    // keep re-triggering this and resetting back to the first button.
                    // Only the buttons' own OnClick calls should matter from here on.
                    enabled = false;
                    return;
                }

                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        step++;
        Debug.Log("[ChatManagers] NextDialogue called. new step=" + step);

        // Dialogue finished
        if (step >= dialogues.Length)
        {
            HideAllBubbles();

            if (objectsToHide != null)
            {
                foreach (GameObject obj in objectsToHide)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }

            // Start the End Button sequence
            // 0 = What is Vaccination
            // 1 = What is EPI
            // 2 = Continue
            StartEndButtonSequence();

            // Stop normal dialogue screen-click input
            enabled = false;

            return;
        }

        // Safety check: if Speakers is shorter than Dialogues, don't silently
        // skip the line - fall back to Nurse so the text still shows, and
        // warn loudly so the Inspector mismatch gets fixed.
        Speaker speakerForStep;
        if (step >= speakers.Length)
        {
            Debug.LogWarning("[ChatManagers] Speakers array is shorter than Dialogues array at step " + step +
                " (Dialogues has " + dialogues.Length + " entries, Speakers has " + speakers.Length +
                "). Falling back to Nurse for this line - add a matching Speaker entry in the Inspector.");
            speakerForStep = Speaker.Nurse;
        }
        else
        {
            speakerForStep = speakers[step];
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

        switch (speakerForStep)
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
        int actualChoiceStep = choiceStep;   // -1 now means "no choice step - never triggers"
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

        // Note: we deliberately do NOT auto-show choice buttons here anymore.
        // Even on the choice step, the bubble now just sits fully typed and
        // waits for the player's next click - see Update() for where the
        // buttons actually get revealed.
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