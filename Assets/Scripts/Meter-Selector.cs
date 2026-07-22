using UnityEngine;

public class MeterSelector : MonoBehaviour
{
    [Header("Selector")]
    public RectTransform selector;

    [Header("Timer")]
    public GreenBarTimer timer;

    [Header("Movement")]
    public float speed = 300f;
    public float minY = -250f;
    public float maxY = 250f;

    [Header("Result Zones")]
    public RectTransform goodZone;
    public RectTransform badHighZone;
    public RectTransform badLowZone;

    [Header("Result Indicators")]
    public GameObject goodIndicator;
    public GameObject badHighIndicator;
    public GameObject badLowIndicator;

    [Header("Result Parents")]
    public GameObject goodParent;
    public GameObject badParent;

    private bool movingUp = true;
    private bool stopped = false;
    private bool resultChecked = false;

    void Start()
    {
        if (goodIndicator != null)
            goodIndicator.SetActive(false);

        if (badHighIndicator != null)
            badHighIndicator.SetActive(false);

        if (badLowIndicator != null)
            badLowIndicator.SetActive(false);

        if (goodParent != null)
            goodParent.SetActive(false);

        if (badParent != null)
            badParent.SetActive(false);
    }

    void Update()
    {
        // Stop when player taps
        if (Input.GetMouseButtonDown(0))
        {
            stopped = true;
        }

        // Stop when timer ends
        if (timer != null && timer.IsTimeUp())
        {
            stopped = true;
        }

        // Check result once
        if (stopped)
        {
            if (!resultChecked)
            {
                CheckResult();
                resultChecked = true;
            }

            return;
        }

        Vector2 pos = selector.anchoredPosition;

        if (movingUp)
        {
            pos.y += speed * Time.deltaTime;

            if (pos.y >= maxY)
            {
                pos.y = maxY;
                movingUp = false;
            }
        }
        else
        {
            pos.y -= speed * Time.deltaTime;

            if (pos.y <= minY)
            {
                pos.y = minY;
                movingUp = true;
            }
        }

        selector.anchoredPosition = pos;
    }

    void CheckResult()
    {
        if (IsInside(goodZone))
        {
            ShowGoodResult();
        }
        else if (IsInside(badHighZone))
        {
            ShowBadHighResult();
        }
        else if (IsInside(badLowZone))
        {
            ShowBadLowResult();
        }
        else
        {
            Debug.LogWarning("Selector is not inside any result zone.");
        }
    }

    bool IsInside(RectTransform zone)
    {
        if (zone == null)
            return false;

        Vector3[] corners = new Vector3[4];
        zone.GetWorldCorners(corners);

        Vector3 selectorPos = selector.position;

        return selectorPos.x >= corners[0].x &&
               selectorPos.x <= corners[2].x &&
               selectorPos.y >= corners[0].y &&
               selectorPos.y <= corners[2].y;
    }

    public void ShowGoodResult()
    {
        if (goodIndicator != null)
            goodIndicator.SetActive(true);

        if (badHighIndicator != null)
            badHighIndicator.SetActive(false);

        if (badLowIndicator != null)
            badLowIndicator.SetActive(false);

        if (goodParent != null)
            goodParent.SetActive(true);

        if (badParent != null)
            badParent.SetActive(false);
    }

    public void ShowBadHighResult()
    {
        if (goodIndicator != null)
            goodIndicator.SetActive(false);

        if (badHighIndicator != null)
            badHighIndicator.SetActive(true);

        if (badLowIndicator != null)
            badLowIndicator.SetActive(false);

        if (goodParent != null)
            goodParent.SetActive(false);

        if (badParent != null)
            badParent.SetActive(true);
    }

    public void ShowBadLowResult()
    {
        if (goodIndicator != null)
            goodIndicator.SetActive(false);

        if (badHighIndicator != null)
            badHighIndicator.SetActive(false);

        if (badLowIndicator != null)
            badLowIndicator.SetActive(true);

        if (goodParent != null)
            goodParent.SetActive(false);

        if (badParent != null)
            badParent.SetActive(true);
    }
}