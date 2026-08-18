using UnityEngine;

public class RetakeButtonController : MonoBehaviour
{
    [Header("Retake Buttons")]
    public GameObject chapter1RetakeButton;
    public GameObject chapter2RetakeButton;
    public GameObject chapter3RetakeButton;
    public GameObject chapter4RetakeButton;
    public GameObject chapter5RetakeButton;

    private void Start()
    {
        UpdateRetakeButtons();
    }

    public void UpdateRetakeButtons()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }

        // Get scores
        int score1 = SaveManager.Instance.GetQuizScore("Chapter1");
        int score2 = SaveManager.Instance.GetQuizScore("Chapter2");
        int score3 = SaveManager.Instance.GetQuizScore("Chapter3");
        int score4 = SaveManager.Instance.GetQuizScore("Chapter4");
        int score5 = SaveManager.Instance.GetQuizScore("Chapter5");

        // Show RETAKE only when score is less than 5
        chapter1RetakeButton.SetActive(score1 < 5);
        chapter2RetakeButton.SetActive(score2 < 5);
        chapter3RetakeButton.SetActive(score3 < 5);
        chapter4RetakeButton.SetActive(score4 < 5);
        chapter5RetakeButton.SetActive(score5 < 5);
    }
}