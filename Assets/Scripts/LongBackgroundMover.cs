using UnityEngine;

public class LongBackgroundMover : MonoBehaviour
{
    [Header("References")]
    public RectTransform longBackground;

    public GameObject walkingCouple;
    public GameObject standingCouple;

    public GameObject scene1;
    public GameObject backyard;

    [Header("Movement")]
    public float moveSpeed = 250f;

    [Header("Stop Position")]
    public float stopPositionX = 0.9659424f;

    private bool moving = false;

    void Update()
    {
        if (!moving)
            return;

        // Move background to the right
        longBackground.anchoredPosition +=
            Vector2.right * moveSpeed * Time.deltaTime;

        // Stop when bench reaches center
        if (longBackground.anchoredPosition.x >= stopPositionX)
        {
            moving = false;

            // Hide walking couple
            walkingCouple.SetActive(false);

            // Show standing couple
            standingCouple.SetActive(true);

            // Switch to backyard
            scene1.SetActive(false);
            backyard.SetActive(true);
        }
    }

    public void StartWalkSequence()
    {
        moving = true;
    }
}