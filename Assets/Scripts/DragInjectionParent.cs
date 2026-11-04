using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DragInjectionParent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================

    [Tooltip("GameObject to activate once this task completes (e.g. the next task's parent).")]
    [SerializeField] private GameObject nextParent;

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
    // INJECTION SFX
    //====================================================

    [Header("Injection SFX")]

    [Tooltip("Pop sound played when the syringe snaps onto the injection target.")]
    [SerializeField] private AudioClip snapPopSound;

    [Tooltip("Volume of the syringe snap pop sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float snapPopVolume = 1f;


    //====================================================
    // STATE
    //====================================================

    private Vector3 syringeOriginalPosition;
    private Vector3 plungerOriginalPosition;
    private Vector3 liquidOriginalScale;

    private float liquidOriginalHeight;

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
        {
            plungerOriginalPosition = plunger.anchoredPosition;
        }

        if (liquidImage != null)
        {
            // Move the pivot to one edge so scaling on Y
            // shrinks from one side instead of both sides.

            Vector2 newPivot = liquidShrinksFromTop
                ? new Vector2(0.5f, 1f)
                : new Vector2(0.5f, 0f);

            SetPivotPreservingPosition(
                liquidImage.rectTransform,
                newPivot
            );

            liquidOriginalScale = liquidImage.rectTransform.localScale;
            liquidOriginalHeight = liquidImage.rectTransform.rect.height;
        }


        //================================================
        // HIDE INJECT BUTTON INITIALLY
        //================================================

        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);

            EventTrigger trigger =
                injectButton.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger =
                    injectButton.gameObject.AddComponent<EventTrigger>();
            }


            //============================================
            // POINTER DOWN
            //============================================

            EventTrigger.Entry pointerDown =
                new EventTrigger.Entry();

            pointerDown.eventID =
                EventTriggerType.PointerDown;

            pointerDown.callback.AddListener(
                (data) => { BeginInjection(); }
            );

            trigger.triggers.Add(pointerDown);


            //============================================
            // POINTER UP
            //============================================

            EventTrigger.Entry pointerUp =
                new EventTrigger.Entry();

            pointerUp.eventID =
                EventTriggerType.PointerUp;

            pointerUp.callback.AddListener(
                (data) => { EndInjection(); }
            );

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

    private void SetPivotPreservingPosition(
        RectTransform rt,
        Vector2 newPivot
    )
    {
        Vector2 size = rt.rect.size;

        Vector3 worldPosOfNewPivotPoint =
            rt.TransformPoint(
                new Vector3(
                    (newPivot.x - rt.pivot.x) * size.x,
                    (newPivot.y - rt.pivot.y) * size.y,
                    0f
                )
            );

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


        //============================================
        // CHECK TARGET DISTANCE
        //============================================

        if (injectionTarget != null)
        {
            float distanceToTarget =
                Vector3.Distance(
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

        // Snap syringe to target
        syringeRect.position = injectionTarget.position;


        //================================================
        // SNAP POP SOUND
        //================================================

        if (snapPopSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                snapPopSound,
                snapPopVolume
            );
        }


        //================================================
        // SHOW INJECT BUTTON
        //================================================

        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(true);

            Debug.Log(
                "Syringe stuck! Press and hold INJECT button to inject."
            );
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

        Debug.Log(
            "Injection started - holding button..."
        );

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

        Debug.Log(
            "Injection button released"
        );
    }


    //====================================================
    // ANIMATE INJECTION
    //====================================================

    IEnumerator AnimateInjection()
    {
        float elapsed = 0f;


        while (
            elapsed < injectionDuration &&
            isInjecting
        )
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / injectionDuration
                );


            //============================================
            // MOVE PLUNGER LEFT
            //============================================

            if (plunger != null)
            {
                Vector3 newPlungerPos =
                    plungerOriginalPosition;

                newPlungerPos.x -=
                    plungerMaxDistance * progress;

                plunger.anchoredPosition =
                    newPlungerPos;
            }


            //============================================
            // SHRINK LIQUID
            //============================================

            if (liquidImage != null)
            {
                float displayedHeight =
                    liquidOriginalHeight *
                    liquidOriginalScale.y;

                float shrunkAmount =
                    Mathf.Min(
                        plungerMaxDistance * progress,
                        displayedHeight
                    );

                float newDisplayedHeight =
                    displayedHeight - shrunkAmount;

                Vector3 newLiquidScale =
                    liquidOriginalScale;

                newLiquidScale.y =
                    liquidOriginalHeight > 0f
                        ? newDisplayedHeight /
                          liquidOriginalHeight
                        : 0f;

                liquidImage.rectTransform.localScale =
                    newLiquidScale;
            }

            yield return null;
        }


        //============================================
        // COMPLETE INJECTION
        //============================================

        if (
            elapsed >= injectionDuration &&
            isInjecting
        )
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


        //============================================
        // DISABLE BUTTON
        //============================================

        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);
        }


        //============================================
        // DISABLE SYRINGE
        //============================================

        Image syringeImage =
            GetComponent<Image>();

        if (syringeImage != null)
        {
            syringeImage.enabled = false;
        }


        //============================================
        // SHOW NEXT PARENT
        //============================================

        if (nextParent != null)
        {
            nextParent.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "DragInjectionParent: no nextParent assigned - " +
                "nothing will be shown after this task."
            );
        }

        Debug.Log("Injection complete!");
    }


    //====================================================
    // RESET SYRINGE
    //====================================================

    void ResetSyringe()
    {
        syringeRect.anchoredPosition =
            syringeOriginalPosition;

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


        //============================================
        // RESET SYRINGE
        //============================================

        syringeCanvasGroup.blocksRaycasts = true;
        syringeCanvasGroup.alpha = 1f;

        syringeRect.anchoredPosition =
            syringeOriginalPosition;


        //============================================
        // RESET PLUNGER
        //============================================

        if (plunger != null)
        {
            plunger.anchoredPosition =
                plungerOriginalPosition;
        }


        //============================================
        // RESET LIQUID
        //============================================

        if (liquidImage != null)
        {
            liquidImage.rectTransform.localScale =
                liquidOriginalScale;
        }


        //============================================
        // HIDE INJECT BUTTON
        //============================================

        if (injectButton != null)
        {
            injectButton.gameObject.SetActive(false);
        }


        //============================================
        // SHOW SYRINGE
        //============================================

        Image syringeImage =
            GetComponent<Image>();

        if (syringeImage != null)
        {
            syringeImage.enabled = true;
        }
    }
}