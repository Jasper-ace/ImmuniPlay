using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
    [SerializeField] private Transform mobileTarget;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private float bounceAmount = 20f;

    private RectTransform arrowRect;
    private Vector3 startPosition;
    private float elapsedTime = 0f;

    private void Start()
    {
        arrowRect = GetComponent<RectTransform>();
        startPosition = arrowRect.anchoredPosition;
    }

    private void Update()
    {
        if (mobileTarget != null)
        {
            // Point arrow toward mobile
            PointTowardMobile();

            // Bounce animation
            BounceAnimation();
        }
    }

    private void PointTowardMobile()
    {
        // Calculate direction from arrow to mobile
        Vector3 direction = mobileTarget.position - transform.position;
        
        // Calculate angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Apply rotation
        arrowRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void BounceAnimation()
    {
        elapsedTime += Time.deltaTime;

        // Bounce up and down
        float bounceY = Mathf.Sin(elapsedTime * animationSpeed) * bounceAmount;
        
        arrowRect.anchoredPosition = startPosition + new Vector3(0, bounceY, 0);
    }
}