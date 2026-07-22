using UnityEngine;
using UnityEngine.EventSystems;

public class ShoeDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    public bool isLeft;
    public float snapSpeed = 1200f;

    [Header("References")]
    public Canvas canvas;
    public GameObject footWithShoe;
    public GameObject bareFoot;

    [HideInInspector]
    public bool isPlaced = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private bool revealed = false;

    private bool isSnapping = false;
    private Vector3 targetPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        startPosition = rectTransform.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void Update()
    {
        if (isSnapping)
        {
            rectTransform.position = Vector3.MoveTowards(
                rectTransform.position,
                targetPosition,
                snapSpeed * Time.deltaTime);

            if (Vector3.Distance(rectTransform.position, targetPosition) < 1f)
{
    rectTransform.position = targetPosition;
    isSnapping = false;

    ShoeLandingEffect effect = GetComponent<ShoeLandingEffect>();

    if (effect != null)
        effect.PlayLandingEffect();

    enabled = false;
}
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced)
            return;

        if (!revealed)
        {
            revealed = true;

            canvasGroup.alpha = 1f;

            transform.SetAsLastSibling();

            footWithShoe.SetActive(false);
            bareFoot.SetActive(true);
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (isPlaced)
            return;

        // Return shoe
        rectTransform.anchoredPosition = startPosition;

        // Hide shoe again
        canvasGroup.alpha = 0f;

        // Restore original foot
        footWithShoe.SetActive(true);
        bareFoot.SetActive(false);

        revealed = false;
    }

    public void StartSnap(Vector3 snapPosition)
    {
        isPlaced = true;
        targetPosition = snapPosition;
        isSnapping = true;
    }
}