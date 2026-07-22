using UnityEngine;

public class BabyNameTest : MonoBehaviour
{
    public BabyNamePanel panel;

    void Start()
    {
        panel.OnNameConfirmed += BabyChosen;

        panel.Open();
    }

    void BabyChosen(string babyName)
    {
        Debug.Log("Baby Name = " + babyName);
    }
}