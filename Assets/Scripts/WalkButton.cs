using UnityEngine;

public class WalkButtonTest : MonoBehaviour
{
    public GameObject standingCouple;
    public GameObject walkingCouple;
    public GameObject walkButton;

    public void StartWalking()
    {
        standingCouple.SetActive(false);
        walkingCouple.SetActive(true);
        walkButton.SetActive(false);

        WalkingCoupleMover mover =
            walkingCouple.GetComponent<WalkingCoupleMover>();

        if (mover != null)
        {
            mover.StartWalking();
        }
        else
        {
            Debug.LogError(
                "WalkingCoupleMover script not found on Walking_Couple (1)");
        }
    }
}