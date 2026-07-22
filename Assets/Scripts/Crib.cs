using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class Crib : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //====================================================
    // REFERENCES
    //====================================================
    private RectTransform cribRect;
    private Canvas canvas;

    [Header("Crib Swing Physics")]
    [Tooltip("Pivot the crib rotates around. (0.5, 1) = hanging top-center. (0.5, 0) = rocking bottom.")]
    [SerializeField] private Vector2 pivot = new Vector2(0.5f, 1f);

    [Tooltip("How natural/fast the crib swings back and forth in Hz (higher = faster oscillation).")]
    [SerializeField] private float frequency = 1.2f;

    [Tooltip("Damping ratio. 0.0 = infinite oscillation, 1.0 = critically damped (no bounce back), 0.15 = smooth swinging with momentum.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float dampingRatio = 0.18f;

    [Tooltip("Maximum tilt angle in either direction (degrees).")]
    [SerializeField] private float maxAngle = 18f;

    [Header("Swing Detection Logic")]
    [Tooltip("Minimum horizontal drag change (pixels) to count as directional change.")]
    [SerializeField] private float swipeThreshold = 15f;

    [Tooltip("Total accumulated active-swing time needed to complete the mini-game.")]
    [SerializeField] private float requiredSwipeDuration = 2.0f;

    [Header("Baby GameObjects")]
    [Tooltip("Baby asset shown while awake/crying.")]
    [SerializeField] private GameObject beforeBaby;

    [Tooltip("Baby asset shown after falling asleep.")]
    [SerializeField] private GameObject afterBaby;

    [Tooltip("Slight scale popup duration when transitioning baby state.")]
    [SerializeField] private float transitionDuration = 0.4f;

    [Header("Scene Transition")]
    [SerializeField] private float sceneLoadDelay = 1.2f;
    [SerializeField] private string nextSceneName;

    //====================================================
    // INTERNAL STATE
    //====================================================
    private bool isDragging = false;
    private bool completed = false;

    private float currentAngle = 0f;
    private float angularVelocity = 0f;
    private float targetAngle = 0f;

    private float lastPointerX;
    private float pointerVelocityX;
    private int lastDirection = 0;
    private float swipeProgress = 0f;

    //====================================================
    // INITIALIZATION
    //====================================================
    private void Awake()
    {
        cribRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cribRect.pivot = pivot;
    }

    private void Start()
    {
        ResetState();
    }

    //====================================================
    // UPDATE
    //====================================================
    private void Update()
    {
        if (completed)
        {
            UpdateSpringPhysics(0f); // Rest naturally at 0 degrees
            return;
        }

        if (isDragging)
        {
            // Calculate target angle based on drag position relative to crib pivot
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cribRect.parent as RectTransform,
                Input.mousePosition,
                canvas != null ? canvas.worldCamera : null,
                out localPoint
            );

            Vector2 pivotOffset = cribRect.anchoredPosition;
            float dragDeltaX = localPoint.x - pivotOffset.x;
            targetAngle = Mathf.Clamp(dragDeltaX * 0.15f, -maxAngle, maxAngle);

            // Track drag velocity for release momentum
            pointerVelocityX = Mathf.Lerp(pointerVelocityX, (Input.mousePosition.x - lastPointerX) / Time.deltaTime, Time.deltaTime * 20f);
            lastPointerX = Input.mousePosition.x;
        }
        else
        {
            // Natural return to center when released
            targetAngle = 0f;

            // Decay progress over time if stopped swinging
            if (swipeProgress > 0f)
            {
                swipeProgress = Mathf.Max(0f, swipeProgress - Time.deltaTime * 0.4f);
            }
        }

        UpdateSpringPhysics(targetAngle);
    }

    //====================================================
    // SMOOTH SPRING PHYSICS INTEGRATOR
    //====================================================
    private void UpdateSpringPhysics(float target)
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 2nd-order Harmonic Oscillator
        float f = frequency * 2f * Mathf.PI;
        float force = (target - currentAngle) * (f * f);
        float dampForce = -2f * dt * dampingRatio * f * angularVelocity;

        angularVelocity += (force * dt) + dampForce;
        currentAngle += angularVelocity * dt;

        // Boundary bounce clamp
        if (Mathf.Abs(currentAngle) > maxAngle)
        {
            currentAngle = Mathf.Sign(currentAngle) * maxAngle;
            angularVelocity *= -0.2f; // Soft bounce off edge limits
        }

        cribRect.localEulerAngles = new Vector3(0f, 0f, currentAngle);
    }

    //====================================================
    // DRAG INTERACTION HANDLERS
    //====================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed) return;

        isDragging = true;
        lastPointerX = eventData.position.x;
        pointerVelocityX = 0f;
        lastDirection = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging) return;

        float deltaX = eventData.position.x - lastPointerX;

        if (Mathf.Abs(deltaX) >= swipeThreshold)
        {
            int currentDir = deltaX > 0 ? 1 : -1;

            if (lastDirection != 0 && currentDir != lastDirection)
            {
                // Small physics push on direction change
                angularVelocity += currentDir * 40f;
                swipeProgress += 0.25f;

                Debug.Log($"Swing Reversal Detected! Progress: {swipeProgress:F2}/{requiredSwipeDuration}");
            }

            lastDirection = currentDir;
        }

        if (swipeProgress >= requiredSwipeDuration && !completed)
        {
            CompleteCribSwinging();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (completed) return;

        isDragging = false;
        lastDirection = 0;

        // Transfer hand release momentum directly into swing energy
        angularVelocity += -pointerVelocityX * 0.08f;
    }

    //====================================================
    // GAME COMPLETION & SMOOTH GAMEOBJECT TRANSITION
    //====================================================
    private void CompleteCribSwinging()
    {
        completed = true;
        isDragging = false;

        StartCoroutine(TransitionGameObjectStates());
    }

    private IEnumerator TransitionGameObjectStates()
    {
        // Animated transition using transform scale (works on standard GameObjects)
        if (beforeBaby != null && afterBaby != null)
        {
            Vector3 originalScale = afterBaby.transform.localScale;
            float timer = 0f;

            // Shrink awake baby slightly
            while (timer < transitionDuration * 0.5f)
            {
                timer += Time.deltaTime;
                float progress = timer / (transitionDuration * 0.5f);
                beforeBaby.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, progress);
                yield return null;
            }

            beforeBaby.SetActive(false);
            beforeBaby.transform.localScale = Vector3.one;

            // Activate sleeping baby and expand smoothly
            afterBaby.SetActive(true);
            afterBaby.transform.localScale = Vector3.zero;

            timer = 0f;
            while (timer < transitionDuration * 0.5f)
            {
                timer += Time.deltaTime;
                float progress = timer / (transitionDuration * 0.5f);
                // EaseOutBack for a cute, soft pop-in feel
                float scale = Mathf.Sin(progress * Mathf.PI * 0.5f); 
                afterBaby.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, scale);
                yield return null;
            }

            afterBaby.transform.localScale = originalScale;
        }
        else
        {
            // Fallback direct swap if scale animation isn't needed
            if (beforeBaby != null) beforeBaby.SetActive(false);
            if (afterBaby != null) afterBaby.SetActive(true);
        }

        yield return new WaitForSeconds(sceneLoadDelay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    //====================================================
    // RESET UTILITY
    //====================================================
    public void ResetState()
    {
        completed = false;
        isDragging = false;
        swipeProgress = 0f;
        lastDirection = 0;
        currentAngle = 0f;
        angularVelocity = 0f;

        if (cribRect != null)
            cribRect.localEulerAngles = Vector3.zero;

        if (beforeBaby != null)
        {
            beforeBaby.SetActive(true);
            beforeBaby.transform.localScale = Vector3.one;
        }

        if (afterBaby != null)
        {
            afterBaby.SetActive(false);
            afterBaby.transform.localScale = Vector3.one;
        }
    }
}