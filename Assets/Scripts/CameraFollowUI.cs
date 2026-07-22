using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public RectTransform walkingCouple;
    public float followSpeed = 5f;

    private RectTransform sceneRect;
    private bool follow = false;

    void Start()
    {
        sceneRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!follow || walkingCouple == null)
            return;

        Vector2 targetPos = sceneRect.anchoredPosition;

        targetPos.x = Mathf.Lerp(
            sceneRect.anchoredPosition.x,
            -walkingCouple.anchoredPosition.x,
            followSpeed * Time.deltaTime
        );

        sceneRect.anchoredPosition = targetPos;
    }

    public void StartFollowing()
    {
        follow = true;
    }
}