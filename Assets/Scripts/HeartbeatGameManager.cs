using UnityEngine;
using TMPro;

public class HeartbeatGameManager : MonoBehaviour
{
    [Header("Heart")]
    public GameObject heartPrefab;
    public RectTransform spawnPoint;
    public Transform heartLane;

    [Header("Lines")]
    public RectTransform targetLine;
    public RectTransform missedLine;

    [Header("UI")]
    public TMP_Text feedbackText;
    public TMP_Text progressText;
    public GameObject resultPanel;
    public TMP_Text resultScoreText;

    private GameObject currentHeart;

    private int currentHeartNumber = 0;
    private int totalHearts = 10;

    void Start()
    {
        SpawnHeart();
    }

    void SpawnHeart()
    {
        if (currentHeartNumber >= totalHearts)
        {
            if (progressText != null) progressText.text = totalHearts + "/" + totalHearts;
            if (feedbackText != null) feedbackText.text = "CHECKUP COMPLETE!";
            
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
                if (resultScoreText != null && ScoreManager.Instance != null)
                {
                    resultScoreText.text = "Final Score: " + Mathf.FloorToInt(ScoreManager.Instance.score) + "%";
                }
            }

            // Execute the branching transition based on score
            HeartbeatMinigameCondition condition = GetComponent<HeartbeatMinigameCondition>();
            if (condition != null)
            {
                condition.ExecuteTransition();
            }

            return;
        }

        // Spawn inside HeartLane
        currentHeart = Instantiate(heartPrefab, heartLane);

        RectTransform heartRect =
            currentHeart.GetComponent<RectTransform>();

        // Copy SpawnPoint position
        heartRect.anchorMin = spawnPoint.anchorMin;
        heartRect.anchorMax = spawnPoint.anchorMax;
        heartRect.pivot = spawnPoint.pivot;
        heartRect.anchoredPosition = spawnPoint.anchoredPosition;
        heartRect.localScale = Vector3.one;

        // Give references to HeartController
        HeartController heartScript =
            currentHeart.GetComponent<HeartController>();

        if (heartScript != null)
        {
            heartScript.missLine = missedLine;
            heartScript.gameManager = this;
        }

        currentHeartNumber++;

        if (progressText != null)
        {
            progressText.text =
                currentHeartNumber + "/" + totalHearts;
        }
    }

    public void TapHeart()
    {
        if (currentHeart == null)
            return;

        RectTransform heartRect =
            currentHeart.GetComponent<RectTransform>();

        float distance =
            Mathf.Abs(
                heartRect.position.y -
                targetLine.position.y
            );

        if (distance <= 50)
        {
            if (feedbackText != null) feedbackText.text = "PERFECT!";
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(10);
        }
        else if (distance <= 120)
        {
            if (feedbackText != null) feedbackText.text = "GOOD!";
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(7);
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "MISS!";
        }

        Destroy(currentHeart);

        currentHeart = null;

        SpawnHeart();
    }

    public void HeartMissed()
    {
        if (feedbackText != null) feedbackText.text = "MISS!";

        currentHeart = null;

        SpawnHeart();
    }
}