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
            progressText.text = "10/10";
            feedbackText.text = "CHECKUP COMPLETE!";
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

        progressText.text =
            currentHeartNumber + "/" + totalHearts;
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
            feedbackText.text = "PERFECT!";
        }
        else if (distance <= 120)
        {
            feedbackText.text = "GOOD!";
        }
        else
        {
            feedbackText.text = "MISS!";
        }

        Destroy(currentHeart);

        currentHeart = null;

        SpawnHeart();
    }

    public void HeartMissed()
    {
        feedbackText.text = "MISS!";

        currentHeart = null;

        SpawnHeart();
    }
}