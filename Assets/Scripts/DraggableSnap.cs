using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class DraggableSnap : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("TARGET")]
    [SerializeField] private RectTransform target;

    [Header("SNAP DISTANCE")]
    [SerializeField] private float snapDistance = 80f;

    [Header("CANVAS")]
    [SerializeField] private Canvas canvas;

    [Header("CANVAS GROUP")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("SECOND OBJECT")]
    [Tooltip("An object (e.g. 'Game 2', which already has its own Pump graphic) to show once snapped. This script does NOT reparent the Sphygmomanometer onto it — it just toggles visibility.")]
    [SerializeField] private GameObject secondObject;

    [Header("OBJECT TO HIDE ON SNAP (optional)")]
    [Tooltip("e.g. 'Game' - hidden when snapped so it looks like it swapped for Second Object. The Sphygmomanometer's own parent is never changed either way.")]
    [SerializeField] private GameObject objectToHide;

    [Header("DIALOGUE BUBBLE")]
    [Tooltip("The chat bubble GameObject to show/hide (e.g. a panel with a TMP text inside).")]
    [SerializeField] private GameObject dialogueBubble;

    [SerializeField] private TMP_Text dialogueText;

    [TextArea]
    [SerializeField] private string instructionMessage = "Drag the Sphygmomanometer onto Maria's arm";

    [Tooltip("Seconds between each typed character. Lower = faster typing.")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Tooltip("Seconds to wait after the text finishes typing (or is skipped) before hiding the bubble. Set to 0 to hide immediately.")]
    [SerializeField] private float autoHideDelay = 1.5f;

    private Coroutine typingCoroutine;
    private string currentFullMessage;
    private bool isTyping;

    private RectTransform rectTransform;

    private Vector2 originalPosition;

    private bool isSnapped = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Find Canvas automatically
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        // Find CanvasGroup automatically
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Remember original position
        originalPosition = rectTransform.anchoredPosition;
    }


    // =========================================================
    // START - show the initial instruction bubble
    // =========================================================

    private void Start()
    {
        ShowDialogue(instructionMessage);
    }


    // =========================================================
    // UPDATE - click anywhere on screen to complete the typing
    // =========================================================

    private void Update()
    {
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            CompleteTyping();
        }
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSnapped)
            return;

        if (canvasGroup != null)
        {
            // Allows the Target to receive the pointer
            canvasGroup.blocksRaycasts = false;
        }
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (isSnapped)
            return;

        if (canvas == null)
        {
            Debug.LogError(
                "DraggableSnap: Canvas is not assigned!",
                this
            );

            return;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }


    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (isSnapped)
            return;

        if (target == null)
        {
            Debug.LogError(
                "DraggableSnap: Target is not assigned!",
                this
            );

            return;
        }

        // Calculate distance
        float distance = Vector2.Distance(
            rectTransform.position,
            target.position
        );

        Debug.Log(
            "Distance: " +
            distance +
            " | Snap Distance: " +
            snapDistance
        );

        if (distance <= snapDistance)
        {
            SnapToTarget();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }


    // =========================================================
    // SNAP TO TARGET (stays under its original parent)
    // =========================================================

    private void SnapToTarget()
    {
        Debug.Log("SPHYGMOMANOMETER HIT TARGET!");

        // Snap its own position onto the target, but keep the
        // same parent/hierarchy position - no SetParent call here.
        rectTransform.position = target.position;
        rectTransform.SetAsLastSibling(); // draw on top of siblings

        // Optionally reveal a second object (e.g. "Game 2" with its own
        // Pump graphic) instead of moving this one elsewhere.
        if (secondObject != null)
        {
            secondObject.SetActive(true);
        }

        // Optionally hide the old container (e.g. "Game") so it looks like
        // a swap happened - the Sphygmomanometer's parent is unaffected.
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }

        isSnapped = true;

        HideDialogue();

        Debug.Log("SPHYGMOMANOMETER SNAPPED IN PLACE (parent unchanged).");
    }


    // =========================================================
    // DIALOGUE BUBBLE HELPERS
    // =========================================================

    private void ShowDialogue(string message)
    {
        CancelInvoke(nameof(HideDialogue));

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);

            currentFullMessage = message;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(message));
        }
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        QueueAutoHide();
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullMessage;
        }

        isTyping = false;

        QueueAutoHide();
    }

    private void QueueAutoHide()
    {
        CancelInvoke(nameof(HideDialogue));

        if (autoHideDelay > 0f)
        {
            Invoke(nameof(HideDialogue), autoHideDelay);
        }
        else
        {
            HideDialogue();
        }
    }

    private void HideDialogue()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // RETURN TO ORIGINAL POSITION
    // =========================================================

    private void ReturnToOriginalPosition()
    {
        Debug.Log(
            "Missed Target - Returning to original position."
        );

        rectTransform.anchoredPosition =
            originalPosition;
    }
}