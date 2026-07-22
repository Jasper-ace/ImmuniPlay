using UnityEngine;
using TMPro;

public class TiltWeightGame : MonoBehaviour
{
    [Header("Tilt")]
    public bool canTilt = false;
    public float tiltSpeed = 500f;

    [Header("Limits")]
    public float minX = -20.6f;
    public float maxX = 519.4f;

    [Header("Timer")]
    public float gameTime = 5f;
    private float currentTime;

    public TMP_Text timerText;

    [Header("Verify Zones (Invisible Images)")]
    public RectTransform belowZone;
    public RectTransform healthyZone;
    public RectTransform aboveZone;

    [Header("Result Panels")]
    public GameObject belowPanel;
    public GameObject healthyPanel;
    public GameObject abovePanel;

    private RectTransform rect;
    private bool gameEnded = false;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        currentTime = gameTime;

        if (belowPanel != null)
            belowPanel.SetActive(false);

        if (healthyPanel != null)
            healthyPanel.SetActive(false);

        if (abovePanel != null)
            abovePanel.SetActive(false);

        if (timerText != null)
            timerText.text = Mathf.Ceil(currentTime).ToString();
    }

    void Update()
    {
        if (!canTilt || gameEnded)
            return;

#if UNITY_EDITOR
        float tilt = Input.GetAxis("Horizontal");
#else
        float tilt = Input.acceleration.x;
#endif

        // Move selector
        Vector2 pos = rect.anchoredPosition;
        pos.x += tilt * tiltSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        rect.anchoredPosition = pos;

        // Countdown
        currentTime -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.Ceil(Mathf.Max(currentTime, 0)).ToString();

        if (currentTime <= 0f)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        canTilt = false;

        if (timerText != null)
            timerText.text = "0";

        if (IsInside(belowZone))
        {
            if (belowPanel != null)
                belowPanel.SetActive(true);
        }
        else if (IsInside(healthyZone))
        {
            if (healthyPanel != null)
                healthyPanel.SetActive(true);
        }
        else if (IsInside(aboveZone))
        {
            if (abovePanel != null)
                abovePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Selector is not inside any verify zone.");
        }
    }

    bool IsInside(RectTransform zone)
    {
        if (zone == null)
            return false;

        Vector3[] corners = new Vector3[4];
        zone.GetWorldCorners(corners);

        Vector3 selectorPos = rect.position;

        return selectorPos.x >= corners[0].x &&
               selectorPos.x <= corners[2].x &&
               selectorPos.y >= corners[0].y &&
               selectorPos.y <= corners[2].y;
    }
}