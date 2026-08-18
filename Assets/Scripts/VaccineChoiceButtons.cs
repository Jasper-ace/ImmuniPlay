using UnityEngine;

public class VaccineChoiceButtons : MonoBehaviour
{
    public void GiveVaccines()
    {
        SaveManager.Instance.SetGiveVaccine(true);

        Debug.Log("PLAYER CHOSE: GIVE VACCINES");
    }

    public void DelayVaccines()
    {
        SaveManager.Instance.SetGiveVaccine(false);

        Debug.Log("PLAYER CHOSE: DELAY VACCINES");
    }
}