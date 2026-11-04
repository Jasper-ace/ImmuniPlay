using UnityEngine;
using UnityEngine.EventSystems;

public class LubricantDrag : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // =========================================================
    // TARGETS
    // =========================================================

    [Header("Targets")]
    public DopplerTarget[] targets;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    public float requiredOverlap = 0.1f;


    // =========================================================
    // LUBRICANT SOUND
    // =========================================================

    [Header("Lubricant SFX")]

    [Tooltip("Wet/squishy sound played when lubricant is successfully applied.")]
    public AudioClip lubricantApplySound;

    [Tooltip("Volume of the lubricant application sound.")]
    [Range(0f, 1f)]
    public float lubricantApplyVolume = 1f;


    // =========================================================
    // INTERNAL
    // =========================================================

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool isDragging = false;
    private bool completed = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;

        canvasGroup.blocksRaycasts = false;

        Debug.Log("Started dragging lubricant.");
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || completed)
            return;

        MoveLubricant(eventData);

        CheckTargets();
    }


    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = false;

        canvasGroup.blocksRaycasts = true;

        CheckTargets();

        Debug.Log("Stopped dragging lubricant.");
    }


    // =========================================================
    // MOVE LUBRICANT
    // =========================================================

    private void MoveLubricant(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        RectTransform parentRect =
            rectTransform.parent as RectTransform;

        if (parentRect == null)
            return;

        Vector2 localPoint;

        Camera eventCamera =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventCamera,
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }


    // =========================================================
    // CHECK TARGETS
    // =========================================================

    private void CheckTargets()
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
                continue;

            if (targets[i].TryApplyLubricant(rectTransform))
            {
                // -------------------------------------------------
                // LUBRICANT SUCCESSFULLY APPLIED
                // -------------------------------------------------

                PlayLubricantApplySound();

                Debug.Log(
                    "Target " +
                    (i + 1) +
                    " completed."
                );
            }
        }

        CheckCompletion();
    }


    // =========================================================
    // PLAY LUBRICANT SFX
    // =========================================================

    private void PlayLubricantApplySound()
    {
        if (
            lubricantApplySound != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                lubricantApplySound,
                lubricantApplyVolume
            );
        }
    }


    // =========================================================
    // CHECK COMPLETION
    // =========================================================

    private void CheckCompletion()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
                continue;

            if (!targets[i].IsCompleted())
            {
                return;
            }
        }

        completed = true;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;

        Debug.Log("ALL DOPPLER TARGETS COMPLETED!");

        if (DopplerMiniGameManager.Instance != null)
        {
            DopplerMiniGameManager.Instance.CompleteMiniGame();
        }
    }
}