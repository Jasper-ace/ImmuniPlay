using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to each Retake button. Set Chapter Number (1-5) in the Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class RetakeButton : MonoBehaviour
{
    [SerializeField] private int chapterNumber = 1;
    [SerializeField] private string quizSceneName = "RetakeQuiz";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        RetakeQuizSession.SelectedChapter = chapterNumber;
        RetakeQuizSession.IsRetake = true;
        SceneManager.LoadScene(quizSceneName);
    }
}