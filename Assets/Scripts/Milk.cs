using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class Milk : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    private RectTransform milkRect;
    private CanvasGroup milkCanvasGroup;

    [SerializeField] private RectTransform targetArea;
    [SerializeField] private Image targetImage;
    [SerializeField] private float targetProximity = 80f;

    [Header("Baby Asset States")]
    [Tooltip("Baby's default/idle look before the bottle reaches the target.")]
    [SerializeField] private GameObject idleBaby;

    [Tooltip("Baby holding the bottled milk (shown immediately on target hit).")]
    [SerializeField] private GameObject holdingMilkBaby;

    [Tooltip("Baby's final look after drinking (shown after the 2nd 1.5s delay).")]
    [SerializeField] private GameObject finishedMilkBaby;

    [Header("Timing")]
    [SerializeField] private float holdingMilkDuration = 1.5f;
    [SerializeField] private float finishedMilkDuration = 1.5f;

    [Header("Completion Callback")]
    [Tooltip("Fired after both delays finish. Wire this to whatever parent/manager should be notified (not BabyCareManager).")]
    public UnityEvent onMilkSequenceComplete;

    //====================================================
    // STATE
    //====================================================
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool completed = false;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        milkRect = GetComponent<RectTransform>();
        milkCanvasGroup = GetComponent<CanvasGroup>();

        if (milkCanvasGroup == null)
        {
            milkCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = milkRect.anchoredPosition;
        completed = false;

        SetBabyState(idle: true);
    }

    //====================================================
    // DRAG HANDLERS
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;
        if (milkCanvasGroup != null)
            milkCanvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging)
            return;

        // Move milk bottle with pointer
        if (milkRect != null)
            milkRect.anchoredPosition += eventData.delta;

        // Check if bottle reached target (only once)
        if (targetArea != null && milkRect != null && !completed)
        {
            float distanceToTarget = Vector3.Distance(
                milkRect.position,
                targetArea.position
            );

            Debug.Log($"Milk distance to target: {distanceToTarget:F2} (threshold: {targetProximity})");

            if (distanceToTarget < targetProximity)
            {
                Debug.Log("Milk reached target! Completing...");
                CompleteMilk();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (milkCanvasGroup != null)
            milkCanvasGroup.alpha = 1f;

        if (targetImage != null)
        {
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    //====================================================
    // COMPLETE MILK (target reached)
    //====================================================
    void CompleteMilk()
    {
        completed = true;
        isDragging = false;

        if (milkCanvasGroup != null)
            milkCanvasGroup.blocksRaycasts = false;

        // Hide the bottle itself
        Image milkImage = GetComponent<Image>();
        if (milkImage != null)
        {
            milkImage.enabled = false;
        }

        // Hide target indicator
        if (targetImage != null)
        {
            targetImage.enabled = false;
        }

        // Stage 1: baby now holding the bottled milk
        SetBabyState(holding: true);

        StartCoroutine(MilkSequence());
    }

    //====================================================
    // MILK SEQUENCE (two staged delays)
    //====================================================
    IEnumerator MilkSequence()
    {
        // Wait while baby is shown holding the milk
        yield return new WaitForSeconds(holdingMilkDuration);

        // Stage 2: baby's final look after drinking
        SetBabyState(finished: true);

        yield return new WaitForSeconds(finishedMilkDuration);

        // Notify whatever parent/manager needs to know (NOT BabyCareManager)
        onMilkSequenceComplete?.Invoke();

        Debug.Log("Milk sequence complete! onMilkSequenceComplete invoked.");
    }

    //====================================================
    // BABY STATE HELPER
    //====================================================
    void SetBabyState(bool idle = false, bool holding = false, bool finished = false)
    {
        if (idleBaby != null) idleBaby.SetActive(idle);
        if (holdingMilkBaby != null) holdingMilkBaby.SetActive(holding);
        if (finishedMilkBaby != null) finishedMilkBaby.SetActive(finished);
    }

    //====================================================
    // PUBLIC RESET (for manual reset)
    //====================================================
    public void Reset()
    {
        completed = false;
        isDragging = false;

        if (milkCanvasGroup != null)
        {
            milkCanvasGroup.blocksRaycasts = true;
            milkCanvasGroup.alpha = 1f;
        }

        if (milkRect != null)
        {
            milkRect.anchoredPosition = originalPosition;
        }

        if (targetImage != null)
        {
            targetImage.enabled = true;
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        Image milkImage = GetComponent<Image>();
        if (milkImage != null)
        {
            milkImage.enabled = true;
        }

        SetBabyState(idle: true);
    }
}