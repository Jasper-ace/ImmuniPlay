using UnityEngine;
using TMPro;

public class QuizScoreDisplay : MonoBehaviour
{
    [Header("Chapter Parents")]
    public GameObject chapter1;
    public GameObject chapter2;
    public GameObject chapter3;
    public GameObject chapter4;
    public GameObject chapter5;

    private void Start()
    {
        UpdateQuizScores();
    }

    public void UpdateQuizScores()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[QuizScoreDisplay] SaveManager not found!");
            return;
        }

        // Find TMP inside each chapter
        TMP_Text chapter1Text = chapter1.GetComponentInChildren<TMP_Text>();
        TMP_Text chapter2Text = chapter2.GetComponentInChildren<TMP_Text>();
        TMP_Text chapter3Text = chapter3.GetComponentInChildren<TMP_Text>();
        TMP_Text chapter4Text = chapter4.GetComponentInChildren<TMP_Text>();
        TMP_Text chapter5Text = chapter5.GetComponentInChildren<TMP_Text>();

        // Get scores
        int score1 = SaveManager.Instance.GetQuizScore("Chapter1");
        int score2 = SaveManager.Instance.GetQuizScore("Chapter2");
        int score3 = SaveManager.Instance.GetQuizScore("Chapter3");
        int score4 = SaveManager.Instance.GetQuizScore("Chapter4");
        int score5 = SaveManager.Instance.GetQuizScore("Chapter5");

        // Display
        if (chapter1Text != null)
            chapter1Text.text = "Chapter 1 Quiz: " + SaveManager.Instance.GetQuizScore("Chapter1") + "/5";

        if (chapter2Text != null)
            chapter2Text.text = "Chapter 2 Quiz: " + SaveManager.Instance.GetQuizScore("Chapter2") + "/5";

        if (chapter3Text != null)
            chapter3Text.text = "Chapter 3 Quiz: " + SaveManager.Instance.GetQuizScore("Chapter3") + "/5";

        if (chapter4Text != null)
            chapter4Text.text = "Chapter 4 Quiz: " + SaveManager.Instance.GetQuizScore("Chapter4") + "/5";

        if (chapter5Text != null)
            chapter5Text.text = "Chapter 5 Quiz: " + SaveManager.Instance.GetQuizScore("Chapter5") + "/5";
    }
}