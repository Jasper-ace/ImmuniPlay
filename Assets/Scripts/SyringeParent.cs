using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SyringeParent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    [SerializeField] private Scene3GameManager gameManager;
    private RectTransform syringeRect;
    private CanvasGroup syringeCanvasGroup;

    [SerializeField] private RectTransform injectionTarget;
    [SerializeField] private Button injectButton;
    [SerializeField] private RectTransform plunger;
    [SerializeField] private Image liquidImage;
    [SerializeField] private float targetProximity = 100f;
    [SerializeField] private float injectionDuration = 3f;
    [SerializeField] private float plungerMaxDistance = 100f;

    [Tooltip("If the liquid should shrink away FROM the top edge (top stays fixed, bottom shrinks up), leave true. " +
             "If it looks wrong in play mode, flip this to shrink from the other side instead.")]
    [SerializeField] private bool liquidShrinksFromTop = true;

    //====================================================
    // STATE
    //====================================================
    private Vector3 syringeOriginalPosition;
    private Vector3 plungerOriginalPosition;
    private Vector3 liquidOriginalScale;
    private float liquidOriginalHeight; // unscaled rect height, used to match shrink speed to plunger speed
    
    private bool isDraggingSyringe = false;
    private bool syringeStuck = false;
    private bool isInjecting = false;
    private bool completed = false;

    //====================================================
    // START
    //====================================================
    void Start()
    {
        syringeRect = GetComponent<RectTransform>();
        syringeCanvasGroup = GetComponent<CanvasGroup>();

        if (syringeCanvasGroup == null)
        {
            syringeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        syringeOriginalPosition = syringeRect.anchoredPosition;

        if (plunger != null)
            plungerOriginalPosition = plunger.anchoredPosition;

        if (liquidImage != null)
        {
            // Move the pivot to one edge (without moving the sprite on screen) so that
            // scaling on Y only eats into the side away from the pivot, instead of
            // shrinking symmetrically from both ends.
            Vector2 newPivot = liquidShrinksFromTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            SetPivotPreservingPosition(liquidImage.rectTransform, newPivot);

            liquidOriginalScale = liquidImage.rectTransform.localScale;
            liquidOriginalHeight = liquidImage.rectTransform.rect.height;
        }

        // Hide inject button initially
        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);
            // Add button listeners
            EventTrigger trigger = injectButton.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = injectButton.gameObject.AddComponent<EventTrigger>();
            }

            // On Pointer Down - Start injection
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { BeginInjection(); });
            trigger.triggers.Add(pointerDown);

            // On Pointer Up - End injection
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { EndInjection(); });
            trigger.triggers.Add(pointerUp);
        }

        completed = false;
        syringeStuck = false;
        isDraggingSyringe = false;
        isInjecting = false;
    }

    //====================================================
    // PIVOT HELPER
    //====================================================
    // Changes a RectTransform's pivot without moving it on screen.
    // Normally setting .pivot directly shifts the rect visually, because the
    // rect's position is measured relative to its pivot. This compensates for
    // that shift so only the scaling behavior changes, not the layout.
    private void SetPivotPreservingPosition(RectTransform rt, Vector2 newPivot)
    {
        Vector2 size = rt.rect.size;

        // Find the world-space location of the point that is ABOUT TO BECOME the new
        // pivot, while the OLD pivot/position are still in effect. Local point (0,0,0)
        // is always the current pivot; offsetting by (newPivot - oldPivot) * size in the
        // rect's own local space gives the point at the new pivot's fractional location.
        Vector3 worldPosOfNewPivotPoint = rt.TransformPoint(
            new Vector3((newPivot.x - rt.pivot.x) * size.x, (newPivot.y - rt.pivot.y) * size.y, 0f));

        // Changing pivot alone doesn't move anything on screen at this point - Unity just
        // re-defines which point inside the rect the Transform's position now refers to.
        // So immediately after this, rt.position is silently treated as "the new pivot's
        // location" - which visually yanks the rect. Snapping position back to where that
        // exact point used to be undoes that shift, leaving the rect visually untouched.
        rt.pivot = newPivot;
        rt.position = worldPosOfNewPivotPoint;
    }

    //====================================================
    // DRAG SYRINGE
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed || syringeStuck)
            return;

        isDraggingSyringe = true;
        syringeCanvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDraggingSyringe)
            return;

        syringeRect.anchoredPosition += eventData.delta;

        if (injectionTarget != null)
        {
            float distanceToTarget = Vector3.Distance(
                syringeRect.position,
                injectionTarget.position
            );

            if (distanceToTarget < targetProximity)
            {
                StickSyringe();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraggingSyringe = false;

        if (!syringeStuck)
        {
            syringeCanvasGroup.alpha = 1f;
            ResetSyringe();
        }
    }

    //====================================================
    // STICK SYRINGE
    //====================================================
    void StickSyringe()
    {
        syringeStuck = true;
        isDraggingSyringe = false;
        syringeCanvasGroup.alpha = 1f;
        syringeCanvasGroup.blocksRaycasts = false;

        syringeRect.position = injectionTarget.position;

        // Show inject button
        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(true);
            Debug.Log("Syringe stuck! Press and hold INJECT button to inject.");
        }
    }

    //====================================================
    // BEGIN INJECTION
    //====================================================
    void BeginInjection()
    {
        if (completed || isInjecting)
            return;

        isInjecting = true;
        Debug.Log("Injection started - holding button...");
        StartCoroutine(AnimateInjection());
    }

    //====================================================
    // END INJECTION
    //====================================================
    void EndInjection()
    {
        if (!isInjecting)
            return;

        isInjecting = false;
        Debug.Log("Injection button released");
    }

    //====================================================
    // ANIMATE INJECTION
    //====================================================
    IEnumerator AnimateInjection()
    {
        float elapsed = 0f;

        while (elapsed < injectionDuration && isInjecting)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / injectionDuration);

            // Animate plunger moving LEFT (X axis, since it's rotated)
            if (plunger != null)
            {
                Vector3 newPlungerPos = plungerOriginalPosition;
                newPlungerPos.x -= plungerMaxDistance * progress;  // Move LEFT on X-axis
                plunger.anchoredPosition = newPlungerPos;
            }

            // Animate liquid squeezing RIGHT-TO-LEFT (scale on Y-axis)
            if (liquidImage != null)
            {
                // Move the shrinking edge at the SAME pixels-per-second rate as the plunger,
                // rather than just making both finish at the same time. The plunger travels
                // plungerMaxDistance total, so the liquid's displayed height should shrink
                // by that same distance over the same duration.
                float displayedHeight = liquidOriginalHeight * liquidOriginalScale.y;
                float shrunkAmount = Mathf.Min(plungerMaxDistance * progress, displayedHeight);
                float newDisplayedHeight = displayedHeight - shrunkAmount;

                Vector3 newLiquidScale = liquidOriginalScale;
                newLiquidScale.y = liquidOriginalHeight > 0f
                    ? newDisplayedHeight / liquidOriginalHeight
                    : 0f;
                liquidImage.rectTransform.localScale = newLiquidScale;
            }

            yield return null;
        }

        // If button was held for full duration, complete injection
        if (elapsed >= injectionDuration && isInjecting)
        {
            CompleteInjection();
        }
    }

    //====================================================
    // COMPLETE INJECTION
    //====================================================
    void CompleteInjection()
    {
        completed = true;
        isInjecting = false;

        // Disable button
        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);
        }

        // Disable syringe
        Image syringeImage = GetComponent<Image>();
        if (syringeImage != null)
        {
            syringeImage.enabled = false;
        }

        // Call game manager
        if (gameManager != null)
        {
            gameManager.SyringeCompleted();
        }
        else
        {
            Debug.LogError("Scene3GameManager not found!");
        }

        Debug.Log("Injection complete!");
    }

    //====================================================
    // RESET SYRINGE
    //====================================================
    void ResetSyringe()
    {
        syringeRect.anchoredPosition = syringeOriginalPosition;
        syringeStuck = false;
    }

    //====================================================
    // PUBLIC RESET
    //====================================================
    public void Reset()
    {
        completed = false;
        isDraggingSyringe = false;
        syringeStuck = false;
        isInjecting = false;

        syringeCanvasGroup.blocksRaycasts = true;
        syringeCanvasGroup.alpha = 1f;
        syringeRect.anchoredPosition = syringeOriginalPosition;

        if (plunger != null)
        {
            plunger.anchoredPosition = plungerOriginalPosition;
        }

        if (liquidImage != null)
        {
            liquidImage.rectTransform.localScale = liquidOriginalScale;
        }

        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);
        }

        Image syringeImage = GetComponent<Image>();
        if (syringeImage != null)
        {
            syringeImage.enabled = true;
        }
    }
}