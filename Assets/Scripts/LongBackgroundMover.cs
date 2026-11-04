using UnityEngine;

public class LongBackgroundMover : MonoBehaviour
{
    [Header("References")]
    public RectTransform longBackground;
    public GameObject walkingCouple;
    public GameObject standingCouple;
    public GameObject scene1;
    public GameObject backyard;

    [Header("Button")]
    public GameObject walkToHospitalButton;

    [Header("Movement")]
    public float moveSpeed = 250f;

    [Header("Stop Position")]
    public float stopPositionX = 0.9659424f;

    private bool moving = false;

    private void Start()
    {
        // Couple should NOT appear when the scene starts.
        walkingCouple.SetActive(false);
        standingCouple.SetActive(false);
        backyard.SetActive(false);
        scene1.SetActive(true);

        // DO NOT activate the button here.
        // BusAnimatorController will show it after the bus animation.
    }

    private void Update()
    {
        if (!moving)
            return;

        // Move background to the right
        longBackground.anchoredPosition +=
            Vector2.right * moveSpeed * Time.deltaTime;

        // Stop when destination is reached
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

    // Called by the Walk to Hospital button
    public void StartWalkSequence()
    {
        // Hide the button
        walkToHospitalButton.SetActive(false);

        // Hide standing couple
        standingCouple.SetActive(false);

        // Show walking couple
        walkingCouple.SetActive(true);

        // Start background movement
        moving = true;
    }
}