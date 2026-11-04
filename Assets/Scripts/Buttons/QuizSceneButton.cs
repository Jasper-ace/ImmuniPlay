using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizSceneButton : MonoBehaviour
{
    [Header("Quiz Settings")]
    [Tooltip("JSON filename inside Resources/Quizzes, without .json")]
    public string chapterFileName;

    [Tooltip("Scene containing the shared ChapterQuizManager")]
    public string quizSceneName = "ChapterQuiz";

    [Tooltip("Scene to load after this quiz is completed")]
    public string nextChapterScene;
    
    public void OpenQuiz()
    {
        // Safety checks FIRST
        if (string.IsNullOrEmpty(chapterFileName))
        {
            Debug.LogError("[Quiz] Chapter File Name is empty!");
            return;
        }

        if (string.IsNullOrEmpty(quizSceneName))
        {
            Debug.LogError("[Quiz] Quiz Scene Name is empty!");
            return;
        }

        if (string.IsNullOrEmpty(nextChapterScene))
        {
            Debug.LogError("[Quiz] Next Chapter Scene is empty!");
            return;
        }


        // ============================================
        // SEND DATA TO CHAPTER QUIZ SCENE
        // ============================================

        QuizSceneData.chapterFileName = chapterFileName;
        QuizSceneData.nextChapterScene = nextChapterScene;


        // Debug
        Debug.Log("=================================");
        Debug.Log("[Quiz Button] PRESSED");
        Debug.Log("[Quiz Button] chapterFileName = " + chapterFileName);
        Debug.Log("[Quiz Button] nextChapterScene = " + nextChapterScene);

        Debug.Log("[Quiz Button] QuizSceneData.chapterFileName = "
            + QuizSceneData.chapterFileName);

        Debug.Log("[Quiz Button] QuizSceneData.nextChapterScene = "
            + QuizSceneData.nextChapterScene);

        Debug.Log("[Quiz Button] Loading scene: " + quizSceneName);
        Debug.Log("=================================");


        // Load shared quiz scene
        SceneManager.LoadScene(quizSceneName);
    }
}