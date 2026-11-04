using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChapterQuizManager : MonoBehaviour
{
    //====================================================
    // QUESTION UI
    //====================================================

    [Header("Question UI")]
    public TMP_Text questionNumberText;
    public TMP_Text questionText;


    //====================================================
    // ANSWER BUTTONS
    //====================================================

    [Header("Answer Buttons")]
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;


    //====================================================
    // QUIZ / RESULT SCENES
    //====================================================

    [Header("Quiz / Result Scenes")]
    public GameObject quizScene;
    public GameObject resultScene;


    //====================================================
    // NURSE CHARACTERS
    //====================================================

    [Header("Nurse Characters")]
    [Tooltip("Nurse shown while the player is answering.")]
    public GameObject quizNurse;

    [Tooltip("Nurse shown when the player answers correctly.")]
    public GameObject correctNurse;

    [Tooltip("Nurse shown when the player answers incorrectly.")]
    public GameObject wrongNurse;


    //====================================================
    // RESULT PANEL
    //====================================================

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public TMP_Text explanationText;
    public Image resultIcon;


    //====================================================
    // RESULT ICONS
    //====================================================

    [Header("Result Icons")]
    public Sprite correctIcon;
    public Sprite wrongIcon;


    //====================================================
    // NEXT BUTTON
    //====================================================

    [Header("Next Button")]
    public Button nextButton;


    //====================================================
    // QUIZ
    //====================================================

    [Header("Quiz")]
    public string chapterFileName = "Chapter1";

    [Tooltip("Maximum number of questions to show in the quiz.")]
    public int maxQuestions = 5;

    [Tooltip("Minimum score required to pass the quiz.")]
    public int passingScore = 3;


    //====================================================
    // QUIZ COMPLETION
    //====================================================

    public GameObject quizCompletion;
    public GameObject passedNurse;
    public GameObject failedNurse;
    public TMP_Text completionTitleText;
    public TMP_Text completionScoreText;

    [Header("Completion Buttons")]
    public Button replayQuizButton;
    public Button nextChapterButton;


    //====================================================
    // PRIVATE VARIABLES
    //====================================================

    private QuizData quizData;
    private int currentQuestion = 0;
    private int score = 0;
    private bool answerSelected = false;


    //====================================================
    // START
    //====================================================

    void Start()
        {
        Debug.Log("=================================");
        Debug.Log("[QuizManager] START");
        Debug.Log("[QuizManager] QuizSceneData.chapterFileName = "
            + QuizSceneData.chapterFileName);
        Debug.Log("[QuizManager] QuizSceneData.nextChapterScene = "
            + QuizSceneData.nextChapterScene);
        Debug.Log("[QuizManager] Inspector chapterFileName = "
            + chapterFileName);
        Debug.Log("=================================");

        // rest of your Start...


        // IMPORTANT:
        // Use the chapter selected by the previous scene
        if (!string.IsNullOrEmpty(QuizSceneData.chapterFileName))
        {
            chapterFileName = QuizSceneData.chapterFileName;
        }


        Debug.Log("[Quiz] FINAL chapterFileName = " + chapterFileName);


        // Hide quiz scene at the beginning
        if (quizScene != null)
            quizScene.SetActive(false);

        // Hide result scene
        if (resultScene != null)
            resultScene.SetActive(false);

        // Make sure the correct/wrong nurses are hidden
        if (correctNurse != null)
            correctNurse.SetActive(false);

        if (wrongNurse != null)
            wrongNurse.SetActive(false);

        // Hide quiz nurse
        if (quizNurse != null)
            quizNurse.SetActive(false);

        // Hide quiz completion
        if (quizCompletion != null)
            quizCompletion.SetActive(false);

        if (passedNurse != null)
            passedNurse.SetActive(false);

        if (failedNurse != null)
            failedNurse.SetActive(false);


        // Load selected JSON
        LoadQuiz();


        // Connect Next button
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextQuestion);
        }
    }

    public void StartQuiz()
    {
        currentQuestion = 0;
        score = 0;
        answerSelected = false;

        // Show quiz scene
        if (quizScene != null)
            quizScene.SetActive(true);

        // Hide completion screen
        if (quizCompletion != null)
            quizCompletion.SetActive(false);

        // Show quiz nurse
        if (quizNurse != null)
            quizNurse.SetActive(true);

        // Show first question
        ShowQuestion();
    }


    //====================================================
    // LOAD QUIZ
    //====================================================

    void LoadQuiz()
    {

         Debug.Log(
            "[QuizManager] ACTUALLY LOADING: Resources/Quizzes/" +
            chapterFileName +
            ".json"
        );
        
        TextAsset jsonFile = Resources.Load<TextAsset>(
            "Quizzes/" + chapterFileName
        );

        if (jsonFile == null)
        {
            Debug.LogError(
                "[QuizManager] Could not find quiz: " +
                chapterFileName
            );

            return;
        }

        quizData = JsonUtility.FromJson<QuizData>(
            jsonFile.text
        );

        if (quizData == null)
        {
            Debug.LogError(
                "[QuizManager] Failed to read quiz JSON."
            );

            return;
        }

        if (quizData.questions == null ||
            quizData.questions.Length == 0)
        {
            Debug.LogError(
                "[QuizManager] No questions found in JSON."
            );

            return;
        }

        //================================================
        // SHUFFLE QUESTIONS
        //================================================

        List<Question> shuffledQuestions =
            new List<Question>(quizData.questions);

        ShuffleList(shuffledQuestions);

        //================================================
        // LIMIT NUMBER OF QUESTIONS
        //================================================

        int questionCount = maxQuestions;

        // If maxQuestions is 0 or less,
        // use all available questions.
        if (questionCount <= 0)
            questionCount = shuffledQuestions.Count;

        questionCount = Mathf.Min(
            questionCount,
            shuffledQuestions.Count
        );

        // Create the final question list
        List<Question> selectedQuestions =
            shuffledQuestions.GetRange(0, questionCount);

        quizData.questions = selectedQuestions.ToArray();

        //================================================
        // SHUFFLE ANSWERS
        //================================================

        foreach (Question question in quizData.questions)
        {
            if (question.answers != null &&
                question.answers.Length > 1)
            {
                List<Answer> shuffledAnswers =
                    new List<Answer>(question.answers);

                ShuffleList(shuffledAnswers);

                question.answers = shuffledAnswers.ToArray();
            }
        }

        // Safety check for passing score
        if (passingScore < 0)
            passingScore = 0;

        if (passingScore > quizData.questions.Length)
            passingScore = quizData.questions.Length;

        Debug.Log(
            "[QuizManager] Loaded " +
            quizData.questions.Length +
            " questions."
        );

        Debug.Log(
            "[QuizManager] Passing score: " +
            passingScore
        );
    }


    //====================================================
    // SHUFFLE LIST
    //====================================================

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    //====================================================
    // SHOW QUESTION
    //====================================================

    void ShowQuestion()
    {
        if (quizData == null)
            return;

        if (currentQuestion >= quizData.questions.Length)
            return;

        answerSelected = false;

        // Make sure quiz scene is visible
        if (quizScene != null)
            quizScene.SetActive(true);

        // Hide result scene
        if (resultScene != null)
            resultScene.SetActive(false);

        // Show quiz nurse
        if (quizNurse != null)
            quizNurse.SetActive(true);

        // Hide result nurses
        if (correctNurse != null)
            correctNurse.SetActive(false);

        if (wrongNurse != null)
            wrongNurse.SetActive(false);


        Question current =
            quizData.questions[currentQuestion];


        //================================================
        // QUESTION NUMBER
        //================================================

        if (questionNumberText != null)
        {
            questionNumberText.text =
                "Question " +
                (currentQuestion + 1) +
                " / " +
                quizData.questions.Length;
        }


        //================================================
        // QUESTION TEXT
        //================================================

        if (questionText != null)
        {
            questionText.text = current.question;
        }


        //================================================
        // ANSWERS
        //================================================

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < current.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);

                if (answerTexts[i] != null)
                {
                    answerTexts[i].text =
                        current.answers[i].text;
                }

                // Remove previous listeners
                answerButtons[i].onClick.RemoveAllListeners();

                // Store answer index
                int answerIndex = i;

                // Add new listener
                answerButtons[i].onClick.AddListener(
                    () => SelectAnswer(answerIndex)
                );

                // Enable button
                answerButtons[i].interactable = true;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }


        // Hide result panel if assigned
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }


    //====================================================
    // SELECT ANSWER
    //====================================================

    public void SelectAnswer(int answerIndex)
    {
        Debug.Log(
            "ANSWER BUTTON PRESSED: " +
            answerIndex
        );


        // Prevent selecting multiple answers
        if (answerSelected)
            return;

        answerSelected = true;


        Question current =
            quizData.questions[currentQuestion];


        // Safety check
        if (answerIndex < 0 ||
            answerIndex >= current.answers.Length)
        {
            Debug.LogError(
                "Invalid answer index: " +
                answerIndex
            );

            return;
        }


        Answer selectedAnswer =
            current.answers[answerIndex];


        //================================================
        // DISABLE ANSWER BUTTONS
        //================================================

        foreach (Button button in answerButtons)
        {
            if (button != null)
                button.interactable = false;
        }


        //================================================
        // CHECK ANSWER
        //================================================

        if (selectedAnswer.isCorrect)
        {
            score++;

            Debug.Log("CORRECT ANSWER!");

            ShowCorrectResult(
                current.explanation
            );
        }
        else
        {
            Debug.Log("WRONG ANSWER!");

            ShowWrongResult(
                current.explanation
            );
        }
    }


    //====================================================
    // SHOW CORRECT RESULT
    //====================================================

    void ShowCorrectResult(string explanation)
    {
        // Hide quiz scene
        if (quizScene != null)
            quizScene.SetActive(false);


        // Show result scene
        if (resultScene != null)
            resultScene.SetActive(true);


        // Show result panel
        if (resultPanel != null)
            resultPanel.SetActive(true);


        // Correct text
        if (resultText != null)
            resultText.text = "<color=#5CA938>Correct!</color>";


        // Explanation
        if (explanationText != null)
            explanationText.text = explanation;


        // Correct icon
        if (resultIcon != null &&
            correctIcon != null)
        {
            resultIcon.sprite = correctIcon;
        }


        //================================================
        // NURSE
        //================================================

        // Hide wrong nurse
        if (wrongNurse != null)
            wrongNurse.SetActive(false);

        // Hide quiz nurse
        if (quizNurse != null)
            quizNurse.SetActive(false);

        // Show correct nurse
        if (correctNurse != null)
            correctNurse.SetActive(true);
    }


    //====================================================
    // SHOW WRONG RESULT
    //====================================================

    void ShowWrongResult(string explanation)
    {
        // Hide quiz scene
        if (quizScene != null)
            quizScene.SetActive(false);


        // Show result scene
        if (resultScene != null)
            resultScene.SetActive(true);


        // Show result panel
        if (resultPanel != null)
            resultPanel.SetActive(true);


        // Wrong text
        if (resultText != null)
            resultText.text = "<color=#EB7560>Wrong!</color>";


        // Explanation
        if (explanationText != null)
            explanationText.text = explanation;


        // Wrong icon
        if (resultIcon != null &&
            wrongIcon != null)
        {
            resultIcon.sprite = wrongIcon;
        }


        //================================================
        // NURSE
        //================================================

        // Hide correct nurse
        if (correctNurse != null)
            correctNurse.SetActive(false);

        // Hide quiz nurse
        if (quizNurse != null)
            quizNurse.SetActive(false);

        // Show wrong nurse
        if (wrongNurse != null)
            wrongNurse.SetActive(true);
    }


    //====================================================
    // NEXT QUESTION
    //====================================================

    public void NextQuestion()
    {
        currentQuestion++;

        if (currentQuestion >= quizData.questions.Length)
        {
            Debug.Log(
                "Quiz Finished! Score: " +
                score +
                "/" +
                quizData.questions.Length
            );

            // Hide result scene
            if (resultPanel != null)
                resultPanel.SetActive(false);

            // Show quiz completion
            ShowQuizCompletion();

            return;
        }

        ShowQuestion();
    }


    //====================================================
    // QUIZ COMPLETION
    //====================================================

    void ShowQuizCompletion()
    {
        if (quizCompletion != null)
            quizCompletion.SetActive(true);


        if (completionScoreText != null)
        {
            completionScoreText.text =
                "You got " +
                score +
                " out of " +
                quizData.questions.Length +
                " correct.";
        }


        //================================================
        // PASS / FAIL
        //================================================

        bool passed = score >= passingScore;


        if (passed)
        {
            if (completionTitleText != null)
                completionTitleText.text = "Congratulations!";


            if (passedNurse != null)
                passedNurse.SetActive(true);

            if (failedNurse != null)
                failedNurse.SetActive(false);
        }
        else
        {
            if (completionTitleText != null)
                completionTitleText.text = "Nice Try!";


            if (passedNurse != null)
                passedNurse.SetActive(false);

            if (failedNurse != null)
                failedNurse.SetActive(true);
        }
    }


    //====================================================
    // REPLAY QUIZ
    //====================================================

    public void ReplayQuiz()
    {
        currentQuestion = 0;
        score = 0;
        answerSelected = false;


        if (quizCompletion != null)
            quizCompletion.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);


        // Reload the JSON and shuffle again
        LoadQuiz();

        ShowQuestion();
    }


    //====================================================
    // GET SCORE
    //====================================================

    public int GetScore()
    {
        return score;
    }

   public void NextChapter()
    {
        // Get the chapter and next scene from QuizSceneData
        string currentChapter = QuizSceneData.chapterFileName;
        string nextScene = QuizSceneData.nextChapterScene;

        // Check current chapter
        if (string.IsNullOrEmpty(currentChapter))
        {
            Debug.LogError("[Quiz] Current chapter is not set.");
            return;
        }

        // Check next scene
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[Quiz] Next Chapter Scene is not assigned.");
            return;
        }

        // Check SaveManager
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[Quiz] SaveManager instance not found.");
            return;
        }

        //================================================
        // SAVE QUIZ DATA
        //================================================

        SaveManager.Instance.SetQuizScore(
            currentChapter,
            score
        );

        SaveManager.Instance.SetQuizAttempted(
            currentChapter
        );

        SaveManager.Instance.SetChapterCompleted(
            currentChapter
        );

        Debug.Log(
            "[Quiz] " +
            currentChapter +
            " completed with score: " +
            score
        );

        // TODO: added this
        //================================================
        // CLEAR OLD CHAPTER RESUME POINT
        //================================================

        Debug.Log(
            "[Quiz] Clearing saved scene for completed chapter: " +
            currentChapter
        );

        SaveManager.Instance.ClearChapterScene(
            currentChapter
        );

        SaveManager.Instance.ClearReplayChapterScene(
            currentChapter
        );

        //================================================
        // SAVE NEXT SCENE
        //================================================

        SaveManager.Instance.SaveCurrentScene(
            nextScene
        );

        Debug.Log(
            "[Quiz] Loading next chapter scene: " +
            nextScene
        );

        //================================================
        // LOAD NEXT CHAPTER
        //================================================

        SceneManager.LoadScene(nextScene);
    }

}


//========================================================
// QUIZ DATA
//========================================================

[System.Serializable]
public class QuizData
{
    public int chapterID;
    public string title;
    public string subtitle;
    public string introCharacterPrefab;

    public Question[] questions;
}


//========================================================
// QUESTION
//========================================================

[System.Serializable]
public class Question
{
    public string question;
    public Answer[] answers;
    public string explanation;
}


//========================================================
// ANSWER
//========================================================

[System.Serializable]
public class Answer
{
    public string text;
    public bool isCorrect;
}