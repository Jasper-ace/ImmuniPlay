using UnityEngine;

public class GreatWorkScreen : MonoBehaviour
{
    [Header("Quiz")]
    public ChapterQuizManager quizManager;


    //====================================================
    // START QUIZ
    //====================================================

    public void StartQuiz()
    {
        if (quizManager != null)
        {
            quizManager.StartQuiz();

            // Hide Great Work screen
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "[GreatWork] Quiz Manager is not assigned."
            );
        }
    }
}