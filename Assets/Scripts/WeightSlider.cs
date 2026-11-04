using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class WeightSlider : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Ruler Bounds")]
    public RectTransform leftBound;
    public RectTransform rightBound;

    [Header("Snap Targets")]
    public RectTransform targetOnRuler;
    public RectTransform targetInsideIndicator;

    [Tooltip("Distance in SCREEN PIXELS needed to snap.")]
    public float snapTolerance = 45f;

    [Header("State")]
    public bool isLocked = true;
    public bool hasSnapped = false;

    [Header("Events")]
    public UnityEvent onSnapped;
    public UnityEvent onInvalidDrop;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 pointerStartLocal;
    private Vector2 indicatorStartPosition;

    private bool currentlyDragging = false;

    [Header("Sliding Sound")]
    public AudioClip slidingSound;

    [Tooltip("Base volume before applying the player's SFX setting.")]
    [Range(0f, 1f)] public float slidingSoundVolume = 0.4f;

    // Must match GameSettings.SFX_VOLUME_KEY.
    private const string SFX_VOLUME_KEY = "SFXVolume";

    [Tooltip("Minimum actual indicator movement required before the sliding sound starts.")]
    public float slidingSoundMovementThreshold = 2f;

    [Tooltip("How long the indicator can remain still before the sliding sound starts fading out.")]
    public float slidingSoundStopDelay = 0.05f;

    [Tooltip("How long the sliding sound takes to fade from its current volume to 0.")]
    public float slidingSoundFadeOutDuration = 0.2f;

    private AudioSource slidingAudioSource;
    private float lastMovementTime = -999f;
    private Coroutine slidingSoundFadeCoroutine;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        parentRect = rectTransform.parent as RectTransform;

        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Dedicated AudioSource for the sliding sound.
        // This is NOT the AudioManager.
        slidingAudioSource = GetComponent<AudioSource>();

        if (slidingAudioSource == null)
        {
            slidingAudioSource = gameObject.AddComponent<AudioSource>();
        }

        slidingAudioSource.playOnAwake = false;
        slidingAudioSource.loop = true;
        slidingAudioSource.spatialBlend = 0f;
        UpdateSlidingSoundVolume();
    }


    // =========================================================
    // UPDATE SLIDING SOUND VOLUME
    // =========================================================

    private void UpdateSlidingSoundVolume()
    {
        if (slidingAudioSource == null)
            return;

        float savedSFXVolume =
            PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        slidingAudioSource.volume =
            slidingSoundVolume * savedSFXVolume;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateSlidingSoundVolume();

        if (slidingAudioSource == null)
            return;

        if (!currentlyDragging ||
            Time.unscaledTime - lastMovementTime > slidingSoundStopDelay)
        {
            StartFadeOutSlidingSound();
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        hasSnapped = false;
        currentlyDragging = false;
    }


    // =========================================================
    // UNLOCK
    // =========================================================

    public void Unlock()
    {
        if (hasSnapped)
            return;

        isLocked = false;
        currentlyDragging = false;
        StopSlidingSound();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }


    // =========================================================
    // LOCK
    // =========================================================

    public void Lock()
    {
        isLocked = true;
        currentlyDragging = false;
        StopSlidingSound();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked || hasSnapped)
            return;

        if (parentRect == null)
            return;

        Camera cam = GetUICamera();

        Vector2 pointerLocal;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            cam,
            out pointerLocal))
        {
            pointerStartLocal = pointerLocal;

            indicatorStartPosition =
                rectTransform.anchoredPosition;

            currentlyDragging = true;
            lastMovementTime = -999f;
            StopSlidingSound();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked || hasSnapped || !currentlyDragging)
            return;

        Camera cam = GetUICamera();

        Vector2 pointerLocal;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            cam,
            out pointerLocal))
        {
            return;
        }


        // =====================================================
        // FREE MOVEMENT
        // =====================================================

        float movement =
            pointerLocal.x -
            pointerStartLocal.x;

        Vector2 newPosition =
            indicatorStartPosition;

        newPosition.x += movement;


        // =====================================================
        // CLAMP USING INTERNAL TARGET
        // =====================================================

        if (leftBound != null &&
            rightBound != null &&
            targetInsideIndicator != null)
        {
            newPosition.x =
                ClampIndicatorUsingInternalTarget(
                    newPosition.x
                );
        }


        // =====================================================
        // CHECK ACTUAL MOVEMENT
        // =====================================================

        float actualMovement =
            Mathf.Abs(
                newPosition.x -
                rectTransform.anchoredPosition.x
            );

        // =====================================================
        // MOVE INDICATOR
        // =====================================================

        rectTransform.anchoredPosition =
            newPosition;

        // Only play the sound when the indicator actually moved.
        // Keeping the movement threshold prevents tiny touch jitter
        // from repeatedly triggering the sound.
        if (actualMovement >= slidingSoundMovementThreshold)
        {
            PlaySlidingSound();
        }
        else if (currentlyDragging)
        {
            // No meaningful movement this frame.
            // Update() will stop the sound after the short delay.
        }
    }


    // =========================================================
    // SLIDING SOUND
    // =========================================================

    private void PlaySlidingSound()
    {
        if (slidingSound == null)
            return;

        if (slidingAudioSource == null)
            return;

        // If the sound is currently fading out and the player starts moving again,
        // cancel the fade and smoothly return to the normal volume.
        if (slidingSoundFadeCoroutine != null)
        {
            StopCoroutine(slidingSoundFadeCoroutine);
            slidingSoundFadeCoroutine = null;
        }

        slidingAudioSource.clip = slidingSound;
        UpdateSlidingSoundVolume();

        lastMovementTime = Time.unscaledTime;

        if (!slidingAudioSource.isPlaying)
        {
            slidingAudioSource.volume = slidingSoundVolume * PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            slidingAudioSource.Play();
        }
    }


    private void StartFadeOutSlidingSound()
    {
        if (slidingAudioSource == null || !slidingAudioSource.isPlaying)
            return;

        // Do not start multiple fade coroutines at the same time.
        if (slidingSoundFadeCoroutine != null)
            return;

        slidingSoundFadeCoroutine = StartCoroutine(FadeOutSlidingSound());
    }


    private System.Collections.IEnumerator FadeOutSlidingSound()
    {
        float startVolume = slidingAudioSource.volume;
        float duration = Mathf.Max(0.01f, slidingSoundFadeOutDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // If the player starts dragging again, stop the fade immediately.
            if (currentlyDragging &&
                Time.unscaledTime - lastMovementTime <= slidingSoundStopDelay)
            {
                UpdateSlidingSoundVolume();
                slidingSoundFadeCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            slidingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        slidingAudioSource.volume = 0f;
        slidingAudioSource.Stop();
        UpdateSlidingSoundVolume();

        slidingSoundFadeCoroutine = null;
    }


    private void StopSlidingSound()
    {
        if (slidingAudioSource == null)
            return;

        if (slidingSoundFadeCoroutine != null)
        {
            StopCoroutine(slidingSoundFadeCoroutine);
            slidingSoundFadeCoroutine = null;
        }

        if (slidingAudioSource.isPlaying)
        {
            slidingAudioSource.Stop();
        }

        UpdateSlidingSoundVolume();
    }


    // =========================================================
    // RELEASE FINGER / MOUSE
    // =========================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked || hasSnapped)
            return;

        currentlyDragging = false;
        StartFadeOutSlidingSound();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }


        // =====================================================
        // MAKE SURE TARGETS EXIST
        // =====================================================

        if (targetOnRuler == null ||
            targetInsideIndicator == null)
        {
            HandleInvalidDrop();
            return;
        }


        // =====================================================
        // CHECK DISTANCE
        // =====================================================

        float distance =
            GetTargetDistanceInPixels();

        Debug.Log(
            gameObject.name +
            " released. Target distance = " +
            distance +
            " px"
        );


        // =====================================================
        // SUCCESS
        // =====================================================

        if (distance <= snapTolerance)
        {
            SnapToTarget();
        }

        // =====================================================
        // INVALID DROP
        // =====================================================

        else
        {
            HandleInvalidDrop();
        }
    }


    // =========================================================
    // INVALID DROP
    // =========================================================

    private void HandleInvalidDrop()
    {
        Debug.Log(
            gameObject.name +
            " was released outside the target."
        );

        // IMPORTANT:
        // Do NOT move the weight back to its original position.
        // It stays exactly where the player released it.

        currentlyDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // Tell ScaleGameManager to show the error message.
        onInvalidDrop?.Invoke();
    }


    // =========================================================
    // GET DISTANCE BETWEEN TARGETS
    // =========================================================

    private float GetTargetDistanceInPixels()
    {
        Camera cam = GetUICamera();


        // =====================================================
        // FIXED TARGET ON RULER
        // =====================================================

        Vector3 rulerTargetWorld =
            targetOnRuler.TransformPoint(
                targetOnRuler.rect.center
            );


        // =====================================================
        // TARGET INSIDE DRAGGABLE INDICATOR
        // =====================================================

        Vector3 indicatorTargetWorld =
            targetInsideIndicator.TransformPoint(
                targetInsideIndicator.rect.center
            );


        // =====================================================
        // WORLD → SCREEN
        // =====================================================

        Vector2 rulerTargetScreen =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                rulerTargetWorld
            );

        Vector2 indicatorTargetScreen =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                indicatorTargetWorld
            );


        // =====================================================
        // ONLY HORIZONTAL DISTANCE
        // =====================================================

        return Mathf.Abs(
            rulerTargetScreen.x -
            indicatorTargetScreen.x
        );
    }


    // =========================================================
    // SNAP
    // =========================================================

    private void SnapToTarget()
    {
        if (hasSnapped)
            return;

        Camera cam = GetUICamera();


        // =====================================================
        // GET FIXED TARGET
        // =====================================================

        Vector3 rulerTargetWorld =
            targetOnRuler.TransformPoint(
                targetOnRuler.rect.center
            );


        // =====================================================
        // GET TARGET INSIDE INDICATOR
        // =====================================================

        Vector3 indicatorTargetWorld =
            targetInsideIndicator.TransformPoint(
                targetInsideIndicator.rect.center
            );


        // =====================================================
        // DIFFERENCE
        // =====================================================

        Vector3 difference =
            rulerTargetWorld -
            indicatorTargetWorld;


        // Horizontal ruler.
        difference.y = 0f;
        difference.z = 0f;


        // =====================================================
        // MOVE INDICATOR
        // =====================================================

        rectTransform.position += difference;


        // =====================================================
        // LOCK
        // =====================================================

        hasSnapped = true;
        isLocked = true;
        currentlyDragging = false;
        StopSlidingSound();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }


        Debug.Log(
            gameObject.name +
            " SNAPPED AND LOCKED"
        );


        // =====================================================
        // SUCCESS EVENT
        // =====================================================

        onSnapped?.Invoke();
    }


    // =========================================================
    // CLAMP USING TARGET INSIDE INDICATOR
    // =========================================================

    private float ClampIndicatorUsingInternalTarget(
        float indicatorPositionX)
    {
        float leftX =
            GetParentSpaceX(leftBound);

        float rightX =
            GetParentSpaceX(rightBound);

        float minX =
            Mathf.Min(leftX, rightX);

        float maxX =
            Mathf.Max(leftX, rightX);


        // Current position of internal target
        float currentTargetX =
            GetInternalTargetParentX();


        // Offset between Indicator and its target
        float targetOffset =
            currentTargetX -
            rectTransform.anchoredPosition.x;


        // Where the internal target WOULD be
        // after moving the Indicator.
        float newTargetX =
            indicatorPositionX +
            targetOffset;


        // Keep target inside ruler.
        if (newTargetX < minX)
        {
            indicatorPositionX +=
                minX - newTargetX;
        }


        if (newTargetX > maxX)
        {
            indicatorPositionX +=
                maxX - newTargetX;
        }


        return indicatorPositionX;
    }


    // =========================================================
    // INTERNAL TARGET X
    // =========================================================

    private float GetInternalTargetParentX()
    {
        Vector3 worldPosition =
            targetInsideIndicator.TransformPoint(
                targetInsideIndicator.rect.center
            );

        Vector3 localPosition =
            parentRect.InverseTransformPoint(
                worldPosition
            );

        return localPosition.x;
    }


    // =========================================================
    // TARGET X IN PARENT
    // =========================================================

    private float GetParentSpaceX(
        RectTransform target)
    {
        if (target == null)
            return 0f;

        Vector3 worldPosition =
            target.TransformPoint(
                target.rect.center
            );

        Vector3 localPosition =
            parentRect.InverseTransformPoint(
                worldPosition
            );

        return localPosition.x;
    }


    // =========================================================
    // CAMERA
    // =========================================================

    private Camera GetUICamera()
    {
        if (canvas == null)
            return null;

        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetSlider()
    {
        isLocked = true;
        hasSnapped = false;
        currentlyDragging = false;
        StopSlidingSound();
        lastMovementTime = -999f;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }
}