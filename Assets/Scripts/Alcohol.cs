using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class Alcohol : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    [SerializeField] private GameObject nextParent;
    private RectTransform alcoholWipeRect;
    private CanvasGroup alcoholWipeCanvasGroup;
    private CanvasGroup residueCanvasGroup;

    [SerializeField] private RectTransform targetArea;
    [SerializeField] private GameObject residueImage;
    [SerializeField] private Image targetImage;
    [SerializeField] private float rubbingDuration = 3f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float targetProximity = 80f;

    //====================================================
    // STATE
    //====================================================
    private Vector3 originalPosition;
    private bool isDragging = false;
    private float rubbingTimeElapsed = 0f;
    private bool isOverTarget = false;
    private bool completed = false;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        alcoholWipeRect = GetComponent<RectTransform>();
        alcoholWipeCanvasGroup = GetComponent<CanvasGroup>();

        if (alcoholWipeCanvasGroup == null)
        {
            alcoholWipeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Get CanvasGroup for residue fade
        if (residueImage != null)
        {
            residueCanvasGroup = residueImage.GetComponent<CanvasGroup>();
            if (residueCanvasGroup == null)
            {
                residueCanvasGroup = residueImage.AddComponent<CanvasGroup>();
            }
            residueCanvasGroup.alpha = 0f;
            residueImage.SetActive(false);
        }

        originalPosition = alcoholWipeRect.anchoredPosition;
        completed = false;
        rubbingTimeElapsed = 0f;
    }

    //====================================================
    // DRAG HANDLERS
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;
        alcoholWipeCanvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging)
            return;

        // Move the alcohol wipe with the mouse
        alcoholWipeRect.anchoredPosition += eventData.delta;

        // Check if wipe is over target using world positions
        if (targetArea != null)
        {
            float distanceToTarget = Vector3.Distance(
                alcoholWipeRect.position,
                targetArea.position
            );

            isOverTarget = distanceToTarget < targetProximity;

            // Visual feedback - highlight target
            if (targetImage != null)
            {
                if (isOverTarget)
                {
                    targetImage.color = new Color(1f, 0.5f, 0.5f, 0.7f); // Red tint
                }
                else
                {
                    targetImage.color = new Color(1f, 1f, 1f, 0.5f); // Default
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        alcoholWipeCanvasGroup.alpha = 1f;

        // Stop wiping sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Reset target color
        if (targetImage != null)
        {
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        isOverTarget = false;
    }

    //====================================================
    // UPDATE - Track rubbing time
    //====================================================
    void Update()
    {
        if (completed || !isDragging || !isOverTarget)
            return;

        // Increment rubbing time
        rubbingTimeElapsed += Time.deltaTime;

        // Check if rubbing is complete
        if (rubbingTimeElapsed >= rubbingDuration)
        {
            CompleteRubbing();
        }
    }

    //====================================================
    // COMPLETE RUBBING
    //====================================================
    void CompleteRubbing()
    {
        completed = true;
        isDragging = false;
        alcoholWipeCanvasGroup.blocksRaycasts = false;

        // Disable alcohol wipe
        Image alcoholImage = GetComponent<Image>();
        if (alcoholImage != null)
        {
            alcoholImage.enabled = false;
        }

        // Disable target
        if (targetImage != null)
        {
            targetImage.enabled = false;
        }

        // Enable residue
        if (residueImage != null)
        {
            residueImage.SetActive(true);
            if (residueCanvasGroup != null)
            {
                residueCanvasGroup.alpha = 1f;
            }
        }

        // Start fade out
        StartCoroutine(FadeOutResidue());
    }

    //====================================================
    // FADE OUT RESIDUE
    //====================================================
    IEnumerator FadeOutResidue()
    {
        yield return new WaitForSeconds(0.5f); // Brief delay before fading

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (residueCanvasGroup != null)
            {
                residueCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        // Ensure it's completely invisible
        if (residueCanvasGroup != null)
        {
            residueCanvasGroup.alpha = 0f;
        }

        if (residueImage != null)
        {
            residueImage.SetActive(false);
        }

        // Redirect to next parent
        RedirectToNextParent();
    }

    //====================================================
    // REDIRECT TO NEXT PARENT
    //====================================================
    void RedirectToNextParent()
    {
        // Deactivate alcohol parent
        gameObject.SetActive(false);

        // Activate next parent
        if (nextParent != null)
        {
            nextParent.SetActive(true);
            Debug.Log($"Alcohol complete! Activated: {nextParent.name}");
        }
        else
        {
            Debug.LogWarning("Next Parent not assigned!");
        }
    }

    //====================================================
    // PUBLIC RESET (for manual reset)
    //====================================================
    public void Reset()
    {
        completed = false;
        isDragging = false;
        isOverTarget = false;
        rubbingTimeElapsed = 0f;
        alcoholWipeCanvasGroup.blocksRaycasts = true;
        alcoholWipeCanvasGroup.alpha = 1f;
        alcoholWipeRect.anchoredPosition = originalPosition;

        if (targetImage != null)
        {
            targetImage.enabled = true;
            targetImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        if (residueImage != null)
        {
            residueImage.SetActive(false);
            if (residueCanvasGroup != null)
            {
                residueCanvasGroup.alpha = 0f;
            }
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}