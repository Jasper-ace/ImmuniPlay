using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuizProgressBar : MonoBehaviour
{
    [Header("Progress Bars")]
    public Slider chapter1ProgressBar;
    public Slider chapter2ProgressBar;
    public Slider chapter3ProgressBar;
    public Slider chapter4ProgressBar;
    public Slider chapter5ProgressBar;

    [Header("Animation")]
    public float animationSpeed = 1f;

    private void Start()
    {
        StartCoroutine(AnimateAllProgressBars());
    }

    private IEnumerator AnimateAllProgressBars()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            yield break;
        }

        // Get saved scores
        int score1 = SaveManager.Instance.GetQuizScore("Chapter1");
        int score2 = SaveManager.Instance.GetQuizScore("Chapter2");
        int score3 = SaveManager.Instance.GetQuizScore("Chapter3");
        int score4 = SaveManager.Instance.GetQuizScore("Chapter4");
        int score5 = SaveManager.Instance.GetQuizScore("Chapter5");

        // Start all bars at 0
        SetupBar(chapter1ProgressBar);
        SetupBar(chapter2ProgressBar);
        SetupBar(chapter3ProgressBar);
        SetupBar(chapter4ProgressBar);
        SetupBar(chapter5ProgressBar);

        // Animate all bars at the same time
        while (
            chapter1ProgressBar.value < score1 ||
            chapter2ProgressBar.value < score2 ||
            chapter3ProgressBar.value < score3 ||
            chapter4ProgressBar.value < score4 ||
            chapter5ProgressBar.value < score5
        )
        {
            chapter1ProgressBar.value = Mathf.MoveTowards(
                chapter1ProgressBar.value,
                score1,
                animationSpeed * Time.deltaTime
            );

            chapter2ProgressBar.value = Mathf.MoveTowards(
                chapter2ProgressBar.value,
                score2,
                animationSpeed * Time.deltaTime
            );

            chapter3ProgressBar.value = Mathf.MoveTowards(
                chapter3ProgressBar.value,
                score3,
                animationSpeed * Time.deltaTime
            );

            chapter4ProgressBar.value = Mathf.MoveTowards(
                chapter4ProgressBar.value,
                score4,
                animationSpeed * Time.deltaTime
            );

            chapter5ProgressBar.value = Mathf.MoveTowards(
                chapter5ProgressBar.value,
                score5,
                animationSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Make sure they end exactly on the saved scores
        chapter1ProgressBar.value = score1;
        chapter2ProgressBar.value = score2;
        chapter3ProgressBar.value = score3;
        chapter4ProgressBar.value = score4;
        chapter5ProgressBar.value = score5;
    }

    private void SetupBar(Slider bar)
    {
        if (bar == null)
            return;

        bar.minValue = 0;
        bar.maxValue = 5;
        bar.value = 0;
    }
}