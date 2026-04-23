using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject successPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI descriptionText;

    [Header("Baby States (0–100)")]
    public GameObject[] babyStates; // 11 elements (0,10,...100)

    [Header("Game Settings")]
    public int score = 0;
    public int totalGerms = 10;

    private int processedGerms = 0;

    // ✅ Called when player taps germ
    public void AddScore()
    {
        score++;
        processedGerms++;

        CheckGameEnd();
    }

    // ❌ Called when germ reaches baby
    public void GermMissed()
    {
        processedGerms++;

        CheckGameEnd();
    }

    void CheckGameEnd()
    {
        if (processedGerms >= totalGerms)
        {
            ShowSuccess();
        }
    }

    void ShowSuccess()
    {
        float percent = ((float)score / totalGerms) * 100f;
        int finalScore = Mathf.RoundToInt(percent);

        successPanel.SetActive(true);

        // ✅ FIXED: add %
        scoreText.text = "Score: " + finalScore;

        // ✅ description
        descriptionText.text = GetDescription(finalScore);

        // ✅ baby image switch
        ShowBabyState(finalScore);

        Time.timeScale = 0f;
    }

    void ShowBabyState(int score)
    {
        int index = Mathf.Clamp(score / 10, 0, 10);

        for (int i = 0; i < babyStates.Length; i++)
        {
            babyStates[i].SetActive(false);
        }

        babyStates[index].SetActive(true);
    }

    string GetDescription(int score)
    {
        if (score >= 100)
            return "Perfect protection! Your baby is fully protected.";

        else if (score >= 90)
            return "Excellent! Your baby is well protected.";

        else if (score >= 80)
            return "Great job! Your baby is mostly protected.";

        else if (score >= 70)
            return "Good effort! Some protection is present.";

        else if (score >= 60)
            return "Fair result. Needs improvement.";

        else if (score >= 50)
            return "Average. Your baby is at risk.";

        else if (score >= 40)
            return "Below average. Improve protection.";

        else if (score >= 30)
            return "Poor protection. Baby is vulnerable.";

        else if (score >= 20)
            return "Very poor. Immediate care needed.";

        else if (score >= 10)
            return "Critical condition.";

        else
            return "Danger! No protection at all.";
    }
}