using UnityEngine;

public class BabyMeterController : MonoBehaviour
{
    [Header("Baby States")]
    public GameObject withBaby;
    public GameObject withBabyNoVaccine;

    [Header("Name States")]
    public GameObject noName;
    public GameObject withName;

    private void Start()
    {
        UpdateBabyMeter();
    }

    public void UpdateBabyMeter()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }

        // Get saved data
        bool chapter1Completed =
            SaveManager.Instance.IsChapterCompleted("Chapter1");

        string babyName =
            SaveManager.Instance.GetBabyName();

        string vaccineChoice =
            SaveManager.Instance.GetGiveVaccine();


        // ------------------------------------------------
        // DISABLE EVERYTHING FIRST
        // ------------------------------------------------

        withBaby.SetActive(false);
        withBabyNoVaccine.SetActive(false);
        noName.SetActive(false);
        withName.SetActive(false);


        // ------------------------------------------------
        // CHAPTER 1 NOT COMPLETED
        // ------------------------------------------------

        if (!chapter1Completed)
        {
            return;
        }


        // ------------------------------------------------
        // VACCINE CHOICE
        // ------------------------------------------------

        if (vaccineChoice == "false")
        {
            // Player chose DELAY VACCINES
            withBabyNoVaccine.SetActive(true);
        }
        else
        {
            // Player chose GIVE VACCINES
            withBaby.SetActive(true);
        }


        // ------------------------------------------------
        // BABY NAME
        // ------------------------------------------------

        if (string.IsNullOrEmpty(babyName))
        {
            // Baby has no name
            noName.SetActive(true);
        }
        else
        {
            // Baby has a name
            withName.SetActive(true);
        }
    }
}