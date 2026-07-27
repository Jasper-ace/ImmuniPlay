using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("Chapter (for saving quiz score)")]
    [Tooltip("Must match exactly: Chapter1, Chapter2, Chapter3, Chapter4, or Chapter5")]
    public string chapterName = "Chapter1";

    [Header("Question Panels")]
    public GameObject[] questions;

    [Header("Finish Panel")]
    public GameObject finishPanel;

    public GameObject congratulationsPanel;
    public GameObject niceTryPanel;

    [Header("Finish Text")]
    public TMP_Text congratulationsScoreText;
    public TMP_Text niceTryScoreText;

    private int currentQuestion = 0;
    private int score = 0;
    private bool answered = false;

    void Start()
    {
        finishPanel.SetActive(false);

        for (int i = 0; i < questions.Length; i++)
        {
            questions[i].SetActive(i == 0);

            Transform correct = questions[i].transform.Find("CorrectPanel");
            if (correct) correct.gameObject.SetActive(false);

            Transform wrong = questions[i].transform.Find("WrongPanel");
            if (wrong) wrong.gameObject.SetActive(false);

            ShuffleButtons(questions[i]);
            RegisterButtons(questions[i]);
        }
    }
void RegisterButtons(GameObject question)
{
    Button[] buttons = question.GetComponentsInChildren<Button>(true);

    Debug.Log($"Question: {question.name} | Buttons found: {buttons.Length}");

    foreach (Button btn in buttons)
    {
        Debug.Log($"Checking {btn.name}");

        AnswerButton answer = btn.GetComponent<AnswerButton>();

        if (answer == null)
        {
            Debug.Log("❌ No AnswerButton on " + btn.name);
            continue;
        }

        Debug.Log("✅ Registered " + btn.name);

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SelectAnswer(btn));
    }
}
    void SelectAnswer(Button clickedButton)
    {
    Debug.Log("SelectAnswer called: " + clickedButton.name);
    Debug.Log("Current Question Index: " + currentQuestion);


        if (answered)
            return;

        answered = true;

        AnswerButton answer = clickedButton.GetComponent<AnswerButton>();

        GameObject current = questions[currentQuestion];

        Button[] buttons = current.GetComponentsInChildren<Button>();

        foreach (Button b in buttons)
            b.interactable = false;

        if (answer.isCorrect)
        {
            score++;

            Transform correct = FindDeepChild(current.transform, "CorrectPanel");

            if (correct != null)
                correct.gameObject.SetActive(true);
        }
        else
        {
Transform wrong = FindDeepChild(current.transform, "WrongPanel");

if (wrong != null)
    wrong.gameObject.SetActive(true);        }
    }
   public void Continue()
{
    GameObject current = questions[currentQuestion];

    Transform correct = FindDeepChild(current.transform, "CorrectPanel");
    if (correct) correct.gameObject.SetActive(false);

    Transform wrong = FindDeepChild(current.transform, "WrongPanel");
    if (wrong) wrong.gameObject.SetActive(false);

    Button[] buttons = current.GetComponentsInChildren<Button>();

    foreach (Button b in buttons)
        b.interactable = true;

    current.SetActive(false);

    currentQuestion++;
    answered = false;

    if (currentQuestion >= questions.Length)
    {
        // Auto-save quiz score and mark chapter as completed
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetQuizScore(chapterName, score);
            SaveManager.Instance.SetChapterCompleted(chapterName);
        }


        finishPanel.SetActive(true);

        // Hide both result panels first
        congratulationsPanel.SetActive(false);
        niceTryPanel.SetActive(false);

        // Update score text
        string scoreMessage = $"You got {score} out of {questions.Length} correct.";

        // Show the appropriate finish panel
        if (score >= 3)
        {
            congratulationsScoreText.text = scoreMessage;
            congratulationsPanel.SetActive(true);
        }
        else
        {
            niceTryScoreText.text = scoreMessage;
            niceTryPanel.SetActive(true);
        }

        Debug.Log($"Final Score: {score}/{questions.Length}");
        return;
    }

    questions[currentQuestion].SetActive(true);
}
public void RestartQuiz()
{
    score = 0;
    currentQuestion = 0;
    answered = false;

    // Hide finish screen
    finishPanel.SetActive(false);
    congratulationsPanel.SetActive(false);
    niceTryPanel.SetActive(false);

    // Reset every question
    foreach (GameObject question in questions)
    {
        question.SetActive(false);

        Transform correct = FindDeepChild(question.transform, "CorrectPanel");
        if (correct) correct.gameObject.SetActive(false);

        Transform wrong = FindDeepChild(question.transform, "WrongPanel");
        if (wrong) wrong.gameObject.SetActive(false);

        ShuffleButtons(question);

        Button[] buttons = question.GetComponentsInChildren<Button>(true);

        foreach (Button b in buttons)
            b.interactable = true;
    }

    // Show first question
    questions[0].SetActive(true);
}

   void ShuffleButtons(GameObject question)
{
    Transform buttons = question.transform.Find("Buttons");

    if (buttons == null)
        return;

    int count = buttons.childCount;

    for (int i = 0; i < count; i++)
    {
        int random = Random.Range(i, count);
        buttons.GetChild(random).SetSiblingIndex(i);
    }
}

Transform FindDeepChild(Transform parent, string name)
{
    foreach (Transform child in parent)
    {
        if (child.name == name)
            return child;

        Transform result = FindDeepChild(child, name);

        if (result != null)
            return result;
    }

    return null;
}
}