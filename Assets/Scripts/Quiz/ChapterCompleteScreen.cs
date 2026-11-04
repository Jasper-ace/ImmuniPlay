using UnityEngine;
using TMPro;

public class ChapterCompleteScreen : MonoBehaviour
{
    //====================================================
    // UI
    //====================================================

    [Header("Chapter Complete UI")]
    public TMP_Text chapterTitleText;
    public TMP_Text descriptionText;

    [Header("Character")]
    public Transform characterContainer;

    [Header("Quiz")]
    public ChapterQuizManager quizManager;

    [Header("Next Screen")]
    public GameObject greatWorkScreen;


    //====================================================
    // QUIZ
    //====================================================

    [Header("Chapter Data")]
    [HideInInspector]
    public string chapterFileName;


    //====================================================
    // PRIVATE
    //====================================================

    private QuizData chapterData;
    private GameObject currentCharacter;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        // Get the chapter selected by the quiz button
        chapterFileName = QuizSceneData.chapterFileName;

        Debug.Log(
            "[ChapterComplete] Loading chapter: " +
            chapterFileName
        );

        LoadChapterData();
        ShowChapterComplete();
    }


    //====================================================
    // LOAD CHAPTER DATA
    //====================================================

    void LoadChapterData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(
            "Quizzes/" + chapterFileName
        );

        if (jsonFile == null)
        {
            Debug.LogError(
                "[ChapterComplete] Could not find quiz: " +
                chapterFileName
            );

            return;
        }

        chapterData = JsonUtility.FromJson<QuizData>(
            jsonFile.text
        );

        if (chapterData == null)
        {
            Debug.LogError(
                "[ChapterComplete] Failed to read JSON."
            );

            return;
        }
    }

    public void ContinueToGreatwork()
    {
        // Hide Chapter Complete screen
        gameObject.SetActive(false);

        // Show Great Work screen
        if (greatWorkScreen != null)
        {
            greatWorkScreen.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "[ChapterComplete] Great Work Screen is not assigned."
            );
        }
    }


    //====================================================
    // SHOW CHAPTER COMPLETE
    //====================================================

    void ShowChapterComplete()
    {
        if (chapterData == null)
            return;


        //================================================
        // CHAPTER TITLE
        //================================================

        if (chapterTitleText != null)
        {
            chapterTitleText.text = chapterData.title;
        }


        //================================================
        // DESCRIPTION
        //================================================

        if (descriptionText != null)
        {
            descriptionText.text = chapterData.subtitle;
        }


        //================================================
        // CHARACTER
        //================================================

        ShowCharacter(
            chapterData.introCharacterPrefab
        );
    }


    //====================================================
    // SHOW CHARACTER
    //====================================================

    void ShowCharacter(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogWarning(
                "[ChapterComplete] No character prefab assigned."
            );

            return;
        }


        // Remove previous character
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }


        // Load character from Resources
        GameObject characterPrefab =
            Resources.Load<GameObject>(
                "Quiz Character Intros/" + prefabName
            );


        if (characterPrefab == null)
        {
            Debug.LogError(
                "[ChapterComplete] Could not find character prefab: " +
                prefabName
            );

            return;
        }


        // Create character
        if (characterContainer != null)
        {
            currentCharacter = Instantiate(
                characterPrefab,
                characterContainer
            );

            currentCharacter.transform.localPosition =
                Vector3.zero;

            currentCharacter.transform.localScale =
                Vector3.one;
        }
        else
        {
            currentCharacter = Instantiate(
                characterPrefab
            );
        }
    }
}