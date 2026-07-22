using UnityEngine;

public class WalkingCoupleMover : MonoBehaviour
{
    public GameObject standingCouple;
    public GameObject walkButton;

    public void StartWalking()
    {
        // Hide standing couple
        standingCouple.SetActive(false);

        // Show walking couple
        gameObject.SetActive(true);

        // Hide button
        walkButton.SetActive(false);
    }
}