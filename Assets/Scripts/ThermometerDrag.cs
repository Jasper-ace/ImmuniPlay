using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Attach this to the Thermometer UI object.
/// Drag it near the "Target" (white square on the baby's forehead).
/// If dropped close enough, it snaps/sticks to the forehead for
/// "stickDuration" seconds, then shows a popup and disables itself.
/// Once the popup is closed, "onSequenceComplete" fires so you can
/// tell BabyCareManager to advance to the correct dialogue index.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ThermometerDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Target Settings")]
    [Tooltip("The white square Target object attached to the baby's forehead")]
    public RectTransform target;

    [Tooltip("How close (in pixels, screen space) the thermometer must be dropped to the target to count as a hit")]
    public float snapDistance = 80f;

    [Header("Stick Settings")]
    [Tooltip("How long the thermometer stays stuck on the forehead before the popup appears. Adjust freely in the Inspector.")]
    public float stickDuration = 1.5f;

    [Tooltip("Snap the thermometer exactly onto the target's position once it sticks")]
    public bool snapToTargetPosition = true;

    [Header("Popup")]
    [Tooltip("Popup panel shown once the sticking timer finishes. Should NOT be a child of the Thermometer object.")]
    public GameObject popupPanel;

    [Header("Events")]
    [Tooltip("Fires after the popup is closed (call ClosePopupAndContinue from your popup's button). " +
             "Hook this to BabyCareManager's method that advances dialogue using Thermometer Dialogue Index.")]
    public UnityEvent onSequenceComplete;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 originalAnchoredPosition;
    private bool isLocked = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        originalAnchoredPosition = rectTransform.anchoredPosition;

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        if (target != null && IsNearTarget())
        {
            StickToForehead();
        }
        else
        {
            ReturnToStart();
        }
    }

    private bool IsNearTarget()
    {
        float distance = Vector2.Distance(rectTransform.position, target.position);
        return distance <= snapDistance;
    }

    private void ReturnToStart()
    {
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    private void StickToForehead()
    {
        isLocked = true;

        if (snapToTargetPosition)
        {
            rectTransform.position = target.position;
        }

        // Stop further dragging/interaction while it's "stuck"
        canvasGroup.blocksRaycasts = false;

        StartCoroutine(StickRoutine());
    }

    private IEnumerator StickRoutine()
    {
        yield return new WaitForSeconds(stickDuration);
        ShowPopupAndDisable();
    }

    private void ShowPopupAndDisable()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        // Thermometer disabled once the popup shows
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hook this up to your popup's "OK / Continue" button OnClick().
    /// It closes the popup and fires onSequenceComplete, which you wire
    /// in the Inspector to BabyCareManager's dialogue-advance method.
    /// </summary>
    public void ClosePopupAndContinue()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        onSequenceComplete?.Invoke();
    }
}