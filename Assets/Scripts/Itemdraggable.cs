using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to every object under MiniGame/Items (VaccinationCard, WetWipes, MilkBottle, etc.)
/// and to the EPI vaccine object. Requires an Image (Raycast Target ON) on the same object.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ItemDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Tick this ONLY on the EPI vaccine.")]
    public bool isVaccine;

    [Tooltip("Drag the Hand object (with HandDropZone) here.")]
    public HandDropZone dropZone;

    [HideInInspector] public bool locked;

    RectTransform rect;
    RectTransform parentRect;
    Canvas canvas;
    CanvasGroup group;
    Vector2 startPos;
    Vector2 grabOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = rect.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (locked) return;

        startPos = rect.anchoredPosition;
        group.blocksRaycasts = false;
        rect.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, GetCamera(), out Vector2 localPoint);
        grabOffset = rect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (locked) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, GetCamera(), out Vector2 localPoint))
        {
            rect.anchoredPosition = localPoint + grabOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
        if (locked) return;

        if (dropZone != null && dropZone.IsOverHand(rect, GetCamera()))
        {
            dropZone.HandleDrop(this);
        }
    }

    public void ReturnToStart()
    {
        rect.anchoredPosition = startPos;
    }

    public RectTransform Rect => rect;

    Camera GetCamera()
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return canvas.worldCamera;
    }
}