using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    // =========================================================
    // ACTION TYPE
    // =========================================================

    public enum ActionType
    {
        None,
        NextScene,
        GameObject,
        Quiz
    }

    // =========================================================
    // BUTTON ACTION
    // =========================================================

    [Header("BUTTON ACTION")]
    [Tooltip("Choose what happens when this button is tapped.")]
    public ActionType actionType = ActionType.None;


    // =========================================================
    // NEXT SCENE
    // =========================================================

    [Header("NEXT SCENE")]
    [Tooltip("Scene to load when Action Type is NextScene.")]
    public string nextSceneName = "";


    // =========================================================
    // GAMEOBJECT TRANSITION
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW")]
    [Tooltip("GameObjects to activate when this button is tapped.")]
    public GameObject[] gameObjectsToShow;

    [Header("GAMEOBJECTS TO HIDE")]
    [Tooltip("GameObjects to deactivate when this button is tapped.")]
    public GameObject[] gameObjectsToHide;


    // =========================================================
    // QUIZ
    // =========================================================

    [Header("QUIZ")]
    [Tooltip("Quiz JSON filename without .json. Example: Chapter1")]
    public string quizChapterFileName = "Chapter1";

    [Tooltip("Scene containing the shared ChapterQuiz.")]
    public string quizSceneName = "ChapterQuiz";


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(ExecuteAction);
        }
        else
        {
            Debug.LogWarning(
                "ActionButton: No Button component found.",
                this
            );
        }
    }


    // =========================================================
    // EXECUTE ACTION
    // =========================================================

    public void ExecuteAction()
    {
        switch (actionType)
        {
            case ActionType.None:
                break;

            case ActionType.NextScene:
                GoToNextScene();
                break;

            case ActionType.GameObject:
                GameObjectTransition();
                break;

            case ActionType.Quiz:
                StartQuiz();
                break;
        }
    }


    // =========================================================
    // NEXT SCENE
    // =========================================================

    private void GoToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError(
                "ActionButton: Next Scene Name is empty!",
                this
            );

            return;
        }

        Debug.Log(
            "ActionButton: Loading scene: " +
            nextSceneName,
            this
        );

        SceneManager.LoadScene(nextSceneName);
    }


    // =========================================================
    // GAMEOBJECT TRANSITION
    // =========================================================

    private void GameObjectTransition()
    {
        // -----------------------------------------------------
        // HIDE GAMEOBJECTS
        // -----------------------------------------------------

        if (gameObjectsToHide != null)
        {
            foreach (GameObject obj in gameObjectsToHide)
            {
                if (obj == null)
                    continue;

                obj.SetActive(false);

                Debug.Log(
                    "ActionButton: Hidden: " +
                    obj.name,
                    this
                );
            }
        }


        // -----------------------------------------------------
        // SHOW GAMEOBJECTS
        // -----------------------------------------------------

        if (gameObjectsToShow != null)
        {
            foreach (GameObject obj in gameObjectsToShow)
            {
                if (obj == null)
                    continue;

                obj.SetActive(true);

                Debug.Log(
                    "ActionButton: Shown: " +
                    obj.name,
                    this
                );
            }
        }
    }


    // =========================================================
    // QUIZ
    // =========================================================

    private void StartQuiz()
    {
        if (string.IsNullOrEmpty(quizChapterFileName))
        {
            Debug.LogError(
                "ActionButton: Quiz Chapter File Name is empty!",
                this
            );

            return;
        }

        if (string.IsNullOrEmpty(quizSceneName))
        {
            Debug.LogError(
                "ActionButton: Quiz Scene Name is empty!",
                this
            );

            return;
        }

        Debug.Log(
            "ActionButton: Starting Quiz - " +
            quizChapterFileName,
            this
        );

        QuizSceneData.chapterFileName =
            quizChapterFileName;

        SceneManager.LoadScene(quizSceneName);
    }
}