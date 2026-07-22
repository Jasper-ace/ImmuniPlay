using UnityEngine;
using UnityEngine.EventSystems;

public class BandAidDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    private RectTransform bandAidRect;
    private CanvasGroup bandAidCanvasGroup;
    private Canvas parentCanvas;

    [Tooltip("The dot/target on the arm you drag the band-aid to.")]
    [SerializeField] private RectTransform target;

    [Tooltip("How close (in UI pixels) the band-aid needs to get to the target to count as a hit.")]
    [SerializeField] private float targetProximity = 100f;

    [Tooltip("Multiplier on drag movement, in case it still feels too fast/slow after the canvas-scale fix. 1 = matches finger/mouse movement exactly.")]
    [SerializeField] private float dragSensitivity = 1f;

    [Header("What happens when the band-aid lands on target")]
    [Tooltip("Hide the target dot once the band-aid is applied, so it doesn't show through/around the band-aid sitting on top of it.")]
    [SerializeField] private bool hideTargetOnApply = true;

    [Tooltip("The static 'band-aid already on the arm' image that lives on the baby (e.g. baby > bandaid). It starts OFF and gets switched ON once the drag lands on target.")]
    [SerializeField] private GameObject appliedBandAidVisual;

    [Tooltip("Hide the draggable band-aid itself once applied. Leave this ON if appliedBandAidVisual already shows the result - you don't need both visible at once.")]
    [SerializeField] private bool hideDraggableOnApply = true;

    [Header("Next Scene")]
    [Tooltip("The GameObject in your scene that handles fading/scene transitions.")]
    [SerializeField] private GameObject fadeManager;

    [Tooltip("The name of the scene to load once the band-aid is applied.")]
    [SerializeField] private string nextScene;

    //====================================================
    // STATE
    //====================================================
    private Vector2 originalAnchoredPosition;
    private bool isDragging = false;
    private bool completed = false;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        bandAidRect = GetComponent<RectTransform>();
        bandAidCanvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        if (bandAidCanvasGroup == null)
        {
            bandAidCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalAnchoredPosition = bandAidRect.anchoredPosition;

        if (appliedBandAidVisual != null)
        {
            appliedBandAidVisual.SetActive(false);
        }

        completed = false;
        isDragging = false;
    }

    //====================================================
    // DRAG
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;
        bandAidCanvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging)
            return;

        // eventData.delta is in raw screen pixels, but anchoredPosition is in canvas units.
        // If the Canvas Scaler's scale factor isn't 1 (e.g. "Scale With Screen Size" on a
        // high-res device), adding the raw delta directly makes the drag feel way too fast.
        // Dividing by the scale factor converts it back to canvas units first.
        float scaleFactor = (parentCanvas != null) ? parentCanvas.scaleFactor : 1f;
        if (scaleFactor <= 0f) scaleFactor = 1f;

        bandAidRect.anchoredPosition += (eventData.delta / scaleFactor) * dragSensitivity;

        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(bandAidRect.position, target.position);

            if (distanceToTarget < targetProximity)
            {
                ApplyBandAid();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (!completed)
        {
            bandAidCanvasGroup.alpha = 1f;
            SnapBackToStart();
        }
    }

    //====================================================
    // APPLY BAND-AID
    //====================================================
    void ApplyBandAid()
    {
        if (completed)
            return;

        completed = true;
        isDragging = false;
        bandAidCanvasGroup.alpha = 1f;

        // Snap it exactly onto the target so it looks placed, not just "close enough"
        bandAidRect.position = target.position;

        if (hideDraggableOnApply)
        {
            gameObject.SetActive(false);
        }

        if (hideTargetOnApply && target != null)
        {
            target.gameObject.SetActive(false);
        }

        if (appliedBandAidVisual != null)
        {
            appliedBandAidVisual.SetActive(true);
        }

        Debug.Log("Band-aid applied!");

        if (fadeManager != null)
        {
            // Calls a method named "FadeToScene(string)" if the FadeManager component has
            // one; harmlessly does nothing if it doesn't (DontRequireReceiver). If your
            // FadeManager's method is named differently, tell me and I'll match it exactly.
            fadeManager.SendMessage("FadeToScene", nextScene, SendMessageOptions.DontRequireReceiver);
        }
    }

    //====================================================
    // RESET (missed drag)
    //====================================================
    void SnapBackToStart()
    {
        bandAidRect.anchoredPosition = originalAnchoredPosition;
    }

    //====================================================
    // PUBLIC RESET (e.g. for retrying the scene)
    //====================================================
    public void ResetBandAid()
    {
        completed = false;
        isDragging = false;

        gameObject.SetActive(true);
        bandAidCanvasGroup.alpha = 1f;
        bandAidCanvasGroup.blocksRaycasts = true;
        bandAidRect.anchoredPosition = originalAnchoredPosition;

        if (target != null)
        {
            target.gameObject.SetActive(true);
        }

        if (appliedBandAidVisual != null)
        {
            appliedBandAidVisual.SetActive(false);
        }
    }
}