using UnityEngine;

public class QuizChapterVisibility : MonoBehaviour
{
    [Header("Chapter Objects")]
    public GameObject chapter1;
    public GameObject chapter2;
    public GameObject chapter3;
    public GameObject chapter4;
    public GameObject chapter5;

    private void Start()
    {
        UpdateChapterVisibility();
    }

    public void UpdateChapterVisibility()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }

        chapter1.SetActive(
            SaveManager.Instance.IsQuizAttempted("Chapter1")
        );

        chapter2.SetActive(
            SaveManager.Instance.IsQuizAttempted("Chapter2")
        );

        chapter3.SetActive(
            SaveManager.Instance.IsQuizAttempted("Chapter3")
        );

        chapter4.SetActive(
            SaveManager.Instance.IsQuizAttempted("Chapter4")
        );

        chapter5.SetActive(
            SaveManager.Instance.IsQuizAttempted("Chapter5")
        );
    }
}