using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Towel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    [SerializeField] private BabyCareManager babyCareManager;
    private RectTransform towelRect;
    private CanvasGroup towelCanvasGroup;

    [SerializeField] private RectTransform targetArea;
    [SerializeField] private Image targetImage;
    [SerializeField] private GameObject wetBaby;
    [SerializeField] private GameObject driedBaby;
    [SerializeField] private float targetProximity = 80f;

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
        towelRect = GetComponent<RectTransform>();
        towelCanvasGroup = GetComponent<CanvasGroup>();

        if (towelCanvasGroup == null)
        {
            towelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = towelRect.anchoredPosition;
        completed = false;
    }

    //====================================================
    // DRAG HANDLERS
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;
        if (towelCanvasGroup != null)
            towelCanvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging)
            return;

        // Move towel with mouse
        if (towelRect != null)
            towelRect.anchoredPosition += eventData.delta;

        // Check if towel reached target (only once)
        if (targetArea != null && towelRect != null && !completed)
        {
            float distanceToTarget = Vector3.Distance(
                towelRect.position,
                targetArea.position
            );

            Debug.Log($"Towel distance to target: {distanceToTarget:F2} (threshold: {targetProximity})");

            // If towel reaches target, complete immediately
            if (distanceToTarget < targetProximity)
            {
                Debug.Log("Towel reached target! Completing...");
                CompleteToweling();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (towelCanvasGroup != null)
            towelCanvasGroup.alpha = 1f;

        // Reset target color
        if (targetImage != null)
        {
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    //====================================================
    // COMPLETE TOWELING
    //====================================================
    void CompleteToweling()
    {
        completed = true;
        isDragging = false;

        if (towelCanvasGroup != null)
            towelCanvasGroup.blocksRaycasts = false;

        // Disable towel immediately
        Image towelImage = GetComponent<Image>();
        if (towelImage != null)
        {
            towelImage.enabled = false;
        }

        // Disable target indicator
        if (targetImage != null)
        {
            targetImage.enabled = false;
        }

        // Disable wet baby
        if (wetBaby != null)
        {
            wetBaby.SetActive(false);
            Debug.Log("Wet baby disabled");
        }

        // Enable dried baby
        if (driedBaby != null)
        {
            driedBaby.SetActive(true);
            Debug.Log("Dried baby enabled");
        }

        // Wait a moment, then redirect to BabyCareManager
        StartCoroutine(WaitThenRedirect());
    }

    //====================================================
    // WAIT THEN REDIRECT
    //====================================================
    IEnumerator WaitThenRedirect()
    {
        yield return new WaitForSeconds(1.5f); // Wait 1.5 seconds to see the change
        RedirectToBabyCareManager();
    }

    //====================================================
    // REDIRECT TO BABY CARE MANAGER
    //====================================================
    void RedirectToBabyCareManager()
    {
        if (babyCareManager != null)
        {
            babyCareManager.TowelCompleted();
            Debug.Log("Towel complete! Redirected to BabyCareManager");
        }
        else
        {
            Debug.LogError("BabyCareManager not assigned!");
        }
    }

    //====================================================
    // PUBLIC RESET (for manual reset)
    //====================================================
    public void Reset()
    {
        completed = false;
        isDragging = false;

        if (towelCanvasGroup != null)
        {
            towelCanvasGroup.blocksRaycasts = true;
            towelCanvasGroup.alpha = 1f;
        }

        if (towelRect != null)
        {
            towelRect.anchoredPosition = originalPosition;
        }

        if (targetImage != null)
        {
            targetImage.enabled = true;
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        Image towelImage = GetComponent<Image>();
        if (towelImage != null)
        {
            towelImage.enabled = true;
        }

        // Reset babies
        if (wetBaby != null)
        {
            wetBaby.SetActive(true);
        }

        if (driedBaby != null)
        {
            driedBaby.SetActive(false);
        }
    }
}