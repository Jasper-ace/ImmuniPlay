using UnityEngine;
using UnityEngine.UI;

public class GreenBarTimer : MonoBehaviour
{
    public RectTransform greenBar;

    public float totalTime = 45f;

    private float currentTime;

    private float startWidth;

    private Image greenBarImage;

    private bool stopped = false;

    void Start()
    {
        currentTime = totalTime;

        startWidth = greenBar.sizeDelta.x;

        greenBar.pivot = new Vector2(0, 0.5f);

        greenBarImage = greenBar.GetComponent<Image>();
    }

    void Update()
    {
        // STOP TIMER WHEN PLAYER TAPS
        if (Input.GetMouseButtonDown(0))
        {
            stopped = true;
        }

        if (stopped)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime < 0)
            currentTime = 0;

        // SHRINK BAR
        float width = (currentTime / totalTime) * startWidth;

        greenBar.sizeDelta =
            new Vector2(width, greenBar.sizeDelta.y);

        // COLOR CHANGE
        if (currentTime <= 3f && currentTime > 1f)
        {
            greenBarImage.color =
                new Color(1f, 0.5f, 0f); // Orange
        }
        else if (currentTime <= 1f)
        {
            greenBarImage.color = Color.red;
        }
        else
        {
            greenBarImage.color = Color.green;
        }
    }

    public bool IsTimeUp()
    {
        return currentTime <= 0;
    }
}