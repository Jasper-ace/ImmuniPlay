using UnityEngine;

public class HeartController : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 250f;

    [HideInInspector]
    public RectTransform missLine;

    [HideInInspector]
    public HeartbeatGameManager gameManager;

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Move heart downward
        rect.anchoredPosition +=
            Vector2.down *
            fallSpeed *
            Time.deltaTime;

        // Heart passed the miss line
        if (missLine != null &&
            rect.position.y <= missLine.position.y)
        {
            if (gameManager != null)
            {
                gameManager.HeartMissed();
            }

            Destroy(gameObject);
        }
    }
}