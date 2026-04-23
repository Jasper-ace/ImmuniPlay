using UnityEngine;

public class BabyAnimationUI : MonoBehaviour
{
    [Header("Idle Bounce")]
    public float bounceSpeed = 2f;
    public float bounceHeight = 10f;

    [Header("Wiggle")]
    public float wiggleSpeed = 3f;
    public float wiggleAngle = 5f;

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 🟢 Bounce (looping)
        float newY = startPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);

        // 🔵 Wiggle (looping)
        float rotation = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAngle;
        rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}   