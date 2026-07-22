using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float score = 0;

    public TMP_Text scoreText;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(float points)
    {
        score += points;
        score = Mathf.Clamp(score, 0, 100);

        UpdateScoreUI();

        Debug.Log("Score: " + score);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(score) + "%";
        }
    }
}