using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to each Chapter button in the Chapters scene.
///
/// HOW TO SET UP IN INSPECTOR
/// --------------------------
/// 1. Set "Chapter Name"        -> must match exactly: Chapter1, Chapter2, etc.
/// 2. Set "Default Start Scene" -> the very first scene of this chapter (e.g. "Scene1")
/// 3. Fill "Chapter Scenes"     -> ALL scene names that belong to this chapter
///    (e.g. Scene1, Scene2 ... Scene17 for Chapter 1)
/// 4. (Optional) Assign "Scene Fade" for fade transitions.
///
/// ON THE BUTTON:
///   Remove the old SceneFade.FadeToScene listener.
///   Add -> ChapterResumeButton -> OnChapterButtonClicked()
/// </summary>
public class ChapterResumeButton : MonoBehaviour
{
    [Header("Chapter Settings")]
    [Tooltip("Must match exactly: Chapter1, Chapter2, Chapter3, Chapter4, or Chapter5")]
    public string chapterName = "Chapter1";

    [Tooltip("The very first scene of this chapter.")]
    public string defaultStartScene = "Scene1";

    [Tooltip("Every scene name that belongs to this chapter.")]
    public string[] chapterScenes;

    [Header("Transition (optional)")]
    [Tooltip("Assign the SceneFade object if you want fade transitions.")]
    public SceneFade sceneFade;

    /// <summary>
    /// Wire this to the Chapter button's OnClick().
    /// </summary>
    public void OnChapterButtonClicked()
    {
        string targetScene = defaultStartScene;

        if (SaveManager.Instance != null)
        {
            bool isCompleted = SaveManager.Instance.IsChapterCompleted(chapterName);

            if (isCompleted)
            {
                // Chapter finished — check if there's a saved replay position first
                string replayScene = SaveManager.Instance.GetReplayChapterScene(chapterName);

                if (!string.IsNullOrEmpty(replayScene))
                {
                    targetScene = replayScene;
                    Debug.Log("[ChapterResumeButton] " + chapterName + " replay resuming from: " + targetScene);
                }
                else
                {
                    targetScene = defaultStartScene;
                    Debug.Log("[ChapterResumeButton] " + chapterName + " replay starting fresh from: " + targetScene);
                }

                SaveManager.Instance.SetReplayMode(true);
            }
            else
            {
                // Chapter in progress — resume and make sure replay mode is OFF
                SaveManager.Instance.SetReplayMode(false);
                string chapterLastScene = SaveManager.Instance.GetChapterScene(chapterName);

                if (!string.IsNullOrEmpty(chapterLastScene))
                {
                    targetScene = chapterLastScene;
                    Debug.Log("[ChapterResumeButton] Resuming " + chapterName + " from: " + targetScene);
                }
                else
                {
                    Debug.Log("[ChapterResumeButton] No save for " + chapterName + ". Starting from: " + targetScene);
                }
            }

            // Tell SaveManager which chapter is now active so it tracks
            // per-chapter progress as the player moves through scenes
            SaveManager.Instance.SetActiveChapter(chapterName);
        }

        LoadScene(targetScene);
    }

    private bool IsInThisChapter(string sceneName)
    {
        foreach (string s in chapterScenes)
        {
            if (s == sceneName)
                return true;
        }
        return false;
    }

    private void LoadScene(string sceneName)
    {
        if (sceneFade != null)
            sceneFade.FadeToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}

