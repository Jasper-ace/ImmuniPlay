using UnityEngine;

public class DelayButtonController : MonoBehaviour
{
    public GameObject epiCard;

    public void OnDelayPressed()
    {
        epiCard.SetActive(true);
    }

    public void CloseEPICard()
    {
        epiCard.SetActive(false);
    }
}