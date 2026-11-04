using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class DraggableBaby : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop target")]
    public RectTransform arrow;                 // Arrow Pointing 1
    public float snapDistance = 100f;           // in UI units

    [Header("Objects to swap")]
    public GameObject motherHoldingBaby;        // turned OFF when you pick up the baby
    public GameObject motherStanding;           // turned ON when you pick up the baby
    public GameObject scaleBaby;                // WeightingScale/Baby (turned ON when placed)

    [Header("Scale text")]
    public WeightTextAnimator weightAnimator;   // drag WeightTxt here
    public float newWeight = 3.5f;

    [Header("Next mini game")]
    public MiniGameSwitcher switcher;           // drag GameManager here

    private Image img;
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;
    private Transform startParent;
    private int startSiblingIndex;
    private Vector2 startAnchoredPos;
    private float originalAlpha;
    private bool isPlaced;

    void Awake()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // lets the drop check see through the baby while dragging
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        startParent = transform.parent;
        startSiblingIndex = transform.GetSiblingIndex();
        startAnchoredPos = rect.anchoredPosition;
        originalAlpha = img.color.a;

        if (canvas == null) Debug.LogError("DraggableBaby: not inside a Canvas.", this);
        if (arrow == null) Debug.LogError("DraggableBaby: 'arrow' is not assigned.", this);
        if (motherHoldingBaby == null || motherStanding == null)
            Debug.LogError("DraggableBaby: mother objects are not assigned.", this);
        if (scaleBaby == null) Debug.LogError("DraggableBaby: 'scaleBaby' is not assigned.", this);
        if (weightAnimator == null) Debug.LogError("DraggableBaby: 'weightAnimator' is not assigned.", this);
        if (switcher == null) Debug.LogError("DraggableBaby: 'switcher' is not assigned.", this);
        if (motherHoldingBaby == gameObject)
            Debug.LogError("DraggableBaby: this script is ON MotherHoldingBaby. Put it on the baby.", this);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (isPlaced) return;

        // Move baby to the top of the Canvas so it draws over everything
        // and isn't switched off when the mother is.
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        group.blocksRaycasts = false;
        SetAlpha(1f);                            // 100% opacity

        motherHoldingBaby.SetActive(false);      // change the mother
        motherStanding.SetActive(true);
    }

    public void OnDrag(PointerEventData e)
    {
        if (isPlaced) return;
        rect.anchoredPosition += e.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (isPlaced) return;
        group.blocksRaycasts = true;

        float dist = Vector2.Distance(rect.position, arrow.position);
        float threshold = snapDistance * canvas.scaleFactor;

        if (dist <= threshold) OnPlaced();
        else CancelDrag();
    }

    void CancelDrag()
    {
        transform.SetParent(startParent, false);
        transform.SetSiblingIndex(startSiblingIndex);
        rect.anchoredPosition = startAnchoredPos;
        SetAlpha(originalAlpha);

        motherStanding.SetActive(false);
        motherHoldingBaby.SetActive(true);
    }

    void OnPlaced()
    {
        isPlaced = true;

        arrow.gameObject.SetActive(false);
        scaleBaby.SetActive(true);
        weightAnimator.Play(newWeight, switcher.GoToMiniGame2);   // go to game 2 when done
        gameObject.SetActive(false);                              // hide the dragged baby
    }

    void SetAlpha(float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}