using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class DopplerDragSnap : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Setup")]
    public RectTransform targetArea;
    public TargetSpawner targetSpawner;       // Reference to TargetSpawner script to reveal target
    public GameObject sceneParentToEnable;    // Scene parent to ENABLE when probe snaps
    public GameObject dragParentToDisable;    // Drag parent to DISABLE when probe snaps
    public float snapDistance = 100f;
    public bool lockAfterSnap = true;
    public bool returnToStartOnMiss = true;
    public float snapSpeed = 15f;
    public UnityEngine.Events.UnityEvent onSnapped;

    [Header("Dialogue System")]
    public GameObject dialoguePanelObject;      // Chat bubble panel
    public TextMeshProUGUI dialogueText;        // TMP text component
    public float typewriterSpeed = 0.05f;       // Delay between characters
    
    [Header("Dialogues")]
    public string[] dialogues = new string[]
    {
        "Hello! I need you to find the target using the ultrasound probe.",
        "The meter on the right will help guide you. It shows how close you are.",
        "Drag the probe across the screen to locate the hidden target.",
        "Good luck! Start when you're ready."
    };

    private RectTransform rect;
    private Canvas canvas;
    private Vector2 startPos;
    private bool snapped = false;
    private bool animating = false;
    private Vector2 animTarget;
    
    // Dialogue variables
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueComplete = false;
    private bool allDialoguesComplete = false;
    private bool dialogueLocked = true;
    private float inputCooldown = 0f;
    private const float INPUT_COOLDOWN_TIME = 0.1f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPos = rect.anchoredPosition;
        
        // Start with dialogue
        if (dialoguePanelObject != null)
            dialoguePanelObject.SetActive(true);
        
        StartCoroutine(ShowDialogue(dialogues[0]));
    }

    void Update()
    {
        if (animating)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, animTarget, Time.deltaTime * snapSpeed);
            if (Vector2.Distance(rect.anchoredPosition, animTarget) < 2f)
            {
                rect.anchoredPosition = animTarget;
                animating = false;
            }
        }
        
        // Keep snapped probe locked to target position
        if (snapped && lockAfterSnap)
        {
            rect.anchoredPosition = targetArea.anchoredPosition;
        }
        
        // Handle input cooldown
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
        }
        
        // Listen for dialogue input (with cooldown)
        if (!allDialoguesComplete && inputCooldown <= 0 && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            HandleDialogueInput();
            inputCooldown = INPUT_COOLDOWN_TIME;
        }
    }

    void HandleDialogueInput()
    {
        if (isTyping)
        {
            // If still typing, complete the current dialogue instantly
            StopAllCoroutines();
            dialogueText.text = dialogues[currentDialogueIndex];
            isTyping = false;
            dialogueComplete = true;
            Debug.Log("Text completed instantly");
        }
        else if (dialogueComplete)
        {
            // Move to next dialogue
            currentDialogueIndex++;
            
            if (currentDialogueIndex < dialogues.Length)
            {
                dialogueComplete = false;
                StartCoroutine(ShowDialogue(dialogues[currentDialogueIndex]));
                Debug.Log("Moving to dialogue: " + currentDialogueIndex);
            }
            else
            {
                // All dialogues complete
                FinishDialogue();
            }
        }
    }

    IEnumerator ShowDialogue(string text)
    {
        isTyping = true;
        if (dialogueText != null)
            dialogueText.text = "";

        // Typewriter effect
        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        dialogueComplete = true;
    }

    void FinishDialogue()
    {
        allDialoguesComplete = true;
        dialogueLocked = false;
        
        // Hide dialogue panel
        if (dialoguePanelObject != null)
        {
            dialoguePanelObject.SetActive(false);
        }
        
        Debug.Log("✅ Dialogue complete! Probe is now draggable.");
    }

    public void OnBeginDrag(PointerEventData data)
    {
        // Block drag if dialogue is still playing
        if (dialogueLocked) return;
        
        Debug.Log("OnBeginDrag called");
        if (snapped && lockAfterSnap) 
        {
            Debug.Log("Already snapped - cannot drag!");
            return;
        }
        animating = false;
    }

    public void OnDrag(PointerEventData data)
    {
        // Block drag if dialogue is still playing
        if (dialogueLocked) return;
        
        if (snapped && lockAfterSnap) 
        {
            return; // Don't move if snapped and locked
        }
        
        rect.anchoredPosition += data.delta / canvas.scaleFactor;
        Debug.Log("Dragging: " + rect.anchoredPosition);
    }

    public void OnEndDrag(PointerEventData data)
    {
        // Block drag if dialogue is still playing
        if (dialogueLocked) return;
        
        Debug.Log("OnEndDrag called");
        if (snapped && lockAfterSnap) return;

        // Use world position for distance check
        float dist = Vector3.Distance(rect.position, targetArea.position);
        Debug.Log("Distance: " + dist + " vs snapDistance: " + snapDistance);

        if (dist <= snapDistance)
        {
            Snap();
        }
        else if (returnToStartOnMiss)
        {
            animTarget = startPos;
            animating = true;
        }
    }

    void Snap()
    {
        snapped = true;
        
        // Get target world position
        Vector3 targetWorldPos = targetArea.position;
        
        // Get probe's canvas
        Canvas probeCanvas = rect.GetComponentInParent<Canvas>();
        RectTransform probeParent = rect.parent as RectTransform;
        
        // Convert world position to screen position using target's canvas camera
        Canvas targetCanvas = targetArea.GetComponentInParent<Canvas>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            targetCanvas.worldCamera, 
            targetWorldPos
        );
        
        // Convert screen position to probe parent's local position
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            probeParent,
            screenPoint,
            probeCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : probeCanvas.worldCamera,
            out localPoint
        );
        
        rect.anchoredPosition = localPoint;
        animating = false;
        
        // Toggle scene parents visibility
        if (dragParentToDisable != null)
        {
            dragParentToDisable.SetActive(false);
            Debug.Log("❌ Dragging UI Hidden");
        }
        
        if (sceneParentToEnable != null)
        {
            sceneParentToEnable.SetActive(true);
            Debug.Log("✅ Scene Parent Enabled");
        }
        
        // Reveal the target!
        if (targetSpawner != null)
        {
            targetSpawner.RevealTarget();
        }
        
        Debug.Log("✅ TARGET FOUND AND SCENE ACTIVATED!");
        onSnapped?.Invoke();
    }
}