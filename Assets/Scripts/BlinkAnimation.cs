using UnityEngine;

public class BlinkAnimation : MonoBehaviour
{
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float bounceAmount = 30f;

    private RectTransform rectTransform;
    private Vector3 startPosition;
    private float elapsedTime = 0f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Bounce up and down
        float bounceY = Mathf.Sin(elapsedTime * bounceSpeed) * bounceAmount;
        
        rectTransform.anchoredPosition = startPosition + new Vector3(0, bounceY, 0);
    }
}