using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs the retake quiz in the RetakeQuiz scene.
/// Flow: Quiz Scene -> Result Scene (per question) -> Quiz Completion Scene.
/// Attach to an empty GameObject (outside the Canvas) called "RetakeQuizManager".
/// On completion, the score overwrites quizScoreChapterX in save.json via SaveManager.
/// </summary>
public class RetakeQuizManager : MonoBehaviour
{
    [Header("Scene Roots (the big parent objects)")]
    [SerializeField] private GameObject chapterCompletionScene;   // hidden on start
    [SerializeField] private GameObject greatworkScene;           // hidden on start
    [SerializeField] private GameObject quizScene;
    [SerializeField] private GameObject resultScene;
    [SerializeField] private GameObject quizCompletionScene;

    [Header("Quiz Scene")]
    [SerializeField] private TMP_Text questionNumberText;         // QuestionNumber > Text
    [SerializeField] private TMP_Text questionText;               // QuestionText
    [SerializeField] private Button[] answerButtons;              // Answer Button 1-4

    [Header("Result Scene")]
    [SerializeField] private GameObject nurseCorrect;
    [SerializeField] private GameObject nurseWrong;
    [SerializeField] private Image resultIcon;                    // optional
    [SerializeField] private Sprite correctSprite;                // optional
    [SerializeField] private Sprite wrongSprite;                  // optional
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private Button nextButton;

    [Header("Quiz Completion Scene")]
    [SerializeField] private GameObject nursePassed;
    [SerializeField] private GameObject nurseFailed;
    [SerializeField] private TMP_Text completionTitleText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button replayQuizButton;
    [SerializeField] private Button nextChapterButton;

    [Header("Settings")]
    [SerializeField] private string chaptersSceneName = "ChaptersScene";  // where Next Chapter goes
    [Range(0f, 1f)] [SerializeField] private float passPercent = 0.6f;
    [SerializeField] private bool shuffleQuestions = true;
    [SerializeField] private bool shuffleAnswers = true;   // your JSON lists the correct answer first
    [SerializeField] private string correctMessage = "Correct!";
    [SerializeField] private string wrongMessage = "Not quite!";

    [Header("Saving")]
    [Tooltip("ON: always overwrite the saved score with the retake result.\nOFF: only overwrite if the new score is higher.")]
    [SerializeField] private bool alwaysOverwriteScore = true;
    [Tooltip("ON: only save the score when the player passes the retake.")]
    [SerializeField] private bool saveOnlyIfPassed = false;

    private RetakeChapterData data;
    private List<RetakeQuestion> questions = new List<RetakeQuestion>();
    private List<RetakeAnswer> currentAnswers;
    private int currentIndex;
    private int score;
    private int chapter;

    private void Start()
    {
        chapter = RetakeQuizSession.SelectedChapter;
        data = RetakeQuizSession.LoadChapter(chapter);

        Hook(nextButton, OnNextClicked);
        Hook(replayQuizButton, StartQuiz);
        Hook(nextChapterButton, () => SceneManager.LoadScene(chaptersSceneName));

        if (data == null)
        {
            Debug.LogError($"[RetakeQuizManager] Could not load Chapter {chapter}.");
            return;
        }

        StartQuiz();
    }

    private void StartQuiz()
    {
        questions = new List<RetakeQuestion>(data.questions);
        if (shuffleQuestions) Shuffle(questions);

        currentIndex = 0;
        score = 0;

        Show(chapterCompletionScene, false);
        Show(greatworkScene, false);
        Show(resultScene, false);
        Show(quizCompletionScene, false);
        Show(quizScene, true);

        ShowQuestion();
    }

    // ---------------- QUESTION ----------------
    private void ShowQuestion()
    {
        Show(resultScene, false);
        Show(quizScene, true);

        RetakeQuestion q = questions[currentIndex];

        if (questionNumberText != null)
            questionNumberText.text = $"Question {currentIndex + 1}/{questions.Count}";
        if (questionText != null) questionText.text = q.question;

        currentAnswers = new List<RetakeAnswer>(q.answers);
        if (shuffleAnswers) Shuffle(currentAnswers);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            bool hasAnswer = i < currentAnswers.Count;
            btn.gameObject.SetActive(hasAnswer);
            if (!hasAnswer) continue;

            btn.interactable = true;
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = currentAnswers[i].text;

            int index = i; // capture for lambda
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    private void OnAnswerSelected(int selected)
    {
        bool correct = currentAnswers[selected].isCorrect;
        if (correct) score++;

        foreach (Button b in answerButtons) b.interactable = false;
        ShowResult(correct);
    }

    // ---------------- RESULT (per question) ----------------
    private void ShowResult(bool correct)
    {
        Show(quizScene, false);
        Show(resultScene, true);

        Show(nurseCorrect, correct);
        Show(nurseWrong, !correct);

        if (resultIcon != null)
        {
            Sprite s = correct ? correctSprite : wrongSprite;
            if (s != null) resultIcon.sprite = s;
        }

        if (resultText != null) resultText.text = correct ? correctMessage : wrongMessage;
        if (explanationText != null) explanationText.text = questions[currentIndex].explanation;
    }

    private void OnNextClicked()
    {
        currentIndex++;
        if (currentIndex < questions.Count) ShowQuestion();
        else ShowCompletion();
    }

    // ---------------- COMPLETION ----------------
    private void ShowCompletion()
    {
        Show(quizScene, false);
        Show(resultScene, false);
        Show(quizCompletionScene, true);

        bool passed = questions.Count > 0 && (float)score / questions.Count >= passPercent;
        Show(nursePassed, passed);
        Show(nurseFailed, !passed);

        if (completionTitleText != null) completionTitleText.text = data.title;
        if (scoreText != null) scoreText.text = $"{score} / {questions.Count}";

        SaveScore(passed);
    }

    // ---------------- SAVE ----------------
    /// <summary>
    /// Writes the retake score into save.json through SaveManager.
    /// SaveManager.SetQuizScore already writes the file, so no extra save call is needed.
    /// </summary>
    private void SaveScore(bool passed)
    {
        if (saveOnlyIfPassed && !passed) return;

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[RetakeQuizManager] SaveManager not found. " +
                             "Start the game from TitleScene so it exists. Score not saved.");
            return;
        }

        if (chapter < 1 || chapter > 5)
        {
            Debug.LogWarning($"[RetakeQuizManager] Invalid chapter {chapter}. Score not saved.");
            return;
        }

        string chapterName = $"Chapter{chapter}";
        int oldScore = SaveManager.Instance.GetQuizScore(chapterName);

        if (alwaysOverwriteScore || score > oldScore)
        {
            SaveManager.Instance.SetQuizScore(chapterName, score);
            Debug.Log($"[RetakeQuizManager] {chapterName} score {oldScore} -> {score}");
        }
        else
        {
            Debug.Log($"[RetakeQuizManager] {chapterName} score kept at {oldScore} (new {score} is not higher).");
        }
    }

    // ---------------- HELPERS ----------------
    private static void Show(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private static void Hook(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}