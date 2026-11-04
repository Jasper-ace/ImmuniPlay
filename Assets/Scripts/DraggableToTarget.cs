using UnityEngine;
using UnityEngine.EventSystems;

// Attach this directly to the UI object you want the player to drag
// (e.g. Maria). Requires the object to have an Image/Graphic with
// "Raycast Target" checked so it can receive drag events.
[RequireComponent(typeof(RectTransform))]
public class DraggableToTarget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Draggable Object")]
    public RectTransform draggableObject; // drag the object itself here (must be this same GameObject)

    [Header("Drag Target")]
    public RectTransform target;      // where the object needs to be dropped (e.g. the weighing scale)
    public float snapDistance = 60f;  // how close (in pixels) counts as a successful drop

    [Header("On Successful Drop")]
    public GameObject prevParent;     // gets disabled once the object sticks to the target
    public GameObject nextParent;     // gets enabled once the object sticks to the target

    [Header("Behavior")]
    public bool returnToStartIfMissed = true; // snap back to original spot on a missed drop
    public bool lockAfterStick = true;        // stop further dragging once successfully placed

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private Vector2 originalAnchoredPosition;
    private bool hasStuck = false;

    void Awake()
    {
        rectTransform = draggableObject != null ? draggableObject : GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hasStuck && lockAfterStick) return;

        // Let drag events pass through so dropping over other UI still works correctly
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasStuck && lockAfterStick) return;

        float scale = (parentCanvas != null) ? parentCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (hasStuck && lockAfterStick) return;

        canvasGroup.blocksRaycasts = true;

        if (target != null && Vector2.Distance(rectTransform.position, target.position) <= snapDistance)
        {
            StickToTarget();
        }
        else if (returnToStartIfMissed)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }

    private void StickToTarget()
    {
        hasStuck = true;

        // Snap exactly onto the target's position
        rectTransform.position = target.position;

        if (prevParent != null)
            prevParent.SetActive(false);

        if (nextParent != null)
            nextParent.SetActive(true);
    }
}