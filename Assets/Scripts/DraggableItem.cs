using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemName;
    [TextArea] public string description;
    public bool isCorrectItem;

    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Vector3 startPos;
    private Transform startParent;
    private int startSibling;
    private Vector3 grabOffset;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        canvasRect = (RectTransform)canvas.transform;

        startPos = rectTransform.position;
        startParent = transform.parent;
        startSibling = transform.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = false;

        // Move to the top of the canvas so the item is drawn above everything
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        // Keep the spot you grabbed under your finger (no snapping to center)
        if (TryGetWorldPoint(e, out Vector3 pointerWorld))
            grabOffset = rectTransform.position - pointerWorld;
    }

    public void OnDrag(PointerEventData e)
    {
        if (TryGetWorldPoint(e, out Vector3 pointerWorld))
            rectTransform.position = pointerWorld + grabOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;

        // If not dropped on the bag, go back to the original position
        if (e.pointerEnter == null || e.pointerEnter.GetComponentInParent<BabyBagDropZone>() == null)
            ReturnToStart();
    }

    private bool TryGetWorldPoint(PointerEventData e, out Vector3 worldPoint)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, e.position, cam, out worldPoint);
    }

    public void ReturnToStart()
    {
        transform.SetParent(startParent, true);
        transform.SetSiblingIndex(startSibling);
        rectTransform.position = startPos;
    }

    public void MarkAsPacked() => gameObject.SetActive(false);
}