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

        belowPanel.SetActive(false);
        healthyPanel.SetActive(false);
        abovePanel.SetActive(false);
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

        // Timer
        currentTime -= Time.deltaTime;

        timerText.text = Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        canTilt = false;

        timerText.text = "0";

        float finalX = rect.anchoredPosition.x;

        // BELOW
        if (finalX < 180f)
        {
            belowPanel.SetActive(true);
        }
        // HEALTHY
        else if (finalX >= 180f && finalX <= 350f)
        {
            healthyPanel.SetActive(true);
        }
        // ABOVE
        else
        {
            abovePanel.SetActive(true);
        }
    }
}