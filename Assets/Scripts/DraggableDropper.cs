using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to: MiniGame1/Dropper (the one OUTSIDE the Baby).
/// Works with UI Images or Sprites. No collider needed.
/// Prints debug info to the Console when you release the dropper.
/// </summary>
public class DraggableDropper : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Baby/Target - placed where the dropper should be released (can stay disabled)")]
    [SerializeField] private Transform target;

    [Tooltip("Baby/Dropper - the disabled dropper nested inside the baby (NOT this object)")]
    [SerializeField] private GameObject nestedDropper;

    [Header("Settings")]
    [Tooltip("Max distance in SCREEN PIXELS between dropper and Target to count as placed")]
    [SerializeField] private float snapRadius = 300f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private bool debugLog = true;

    [Header("Events")]
    public UnityEvent onPlaced;

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool isReturning;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        transform.position = startPosition;
        isDragging = false;
        isReturning = false;
    }

    private void Update()
    {
        if (isReturning) return;

        Vector2 screen = PointerInput.Position;

        if (PointerInput.Down && DropperUtil.IsOver(this, screen))
        {
            isDragging = true;
            dragOffset = transform.position - DropperUtil.ScreenToWorld(transform, screen);
            if (debugLog) Debug.Log("[DraggableDropper] Drag started");
        }

        if (isDragging && PointerInput.Held)
        {
            transform.position = DropperUtil.ScreenToWorld(transform, screen) + dragOffset;
        }

        if (isDragging && PointerInput.Up)
        {
            isDragging = false;
            OnReleased();
        }
    }

    private void OnReleased()
    {
        if (target == null)
        {
            Debug.LogError("[DraggableDropper] Target field is EMPTY. Assign Baby/Target.", this);
            StartCoroutine(ReturnToStart());
            return;
        }

        if (nestedDropper == null)
        {
            Debug.LogError("[DraggableDropper] Nested Dropper field is EMPTY. Assign Baby/Dropper.", this);
            StartCoroutine(ReturnToStart());
            return;
        }

        if (nestedDropper == gameObject)
        {
            Debug.LogError("[DraggableDropper] Nested Dropper is set to THIS object. " +
                           "It must be the other Dropper inside Baby.", this);
            StartCoroutine(ReturnToStart());
            return;
        }

        Vector2 dropperScreen = DropperUtil.WorldToScreen(transform, transform.position);
        Vector2 targetScreen = DropperUtil.WorldToScreen(transform, target.position);
        float distance = Vector2.Distance(dropperScreen, targetScreen);

        if (debugLog)
        {
            Debug.Log($"[DraggableDropper] Released. Distance to Target = {distance:F0}px, " +
                      $"Snap Radius = {snapRadius:F0}px. Target = '{target.name}', " +
                      $"Nested = '{nestedDropper.name}'");
        }

        if (distance <= snapRadius)
            PlaceOnBaby();
        else
            StartCoroutine(ReturnToStart());
    }

    private void PlaceOnBaby()
    {
        // Enable the nested dropper first, then disable this one
        nestedDropper.SetActive(true);

        if (debugLog)
        {
            Debug.Log($"[DraggableDropper] Nested dropper '{nestedDropper.name}' enabled. " +
                      $"activeInHierarchy = {nestedDropper.activeInHierarchy}");
        }

        onPlaced?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator ReturnToStart()
    {
        isReturning = true;
        Vector3 from = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / returnDuration;
            transform.position = Vector3.Lerp(from, startPosition, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.position = startPosition;
        isReturning = false;
    }
}