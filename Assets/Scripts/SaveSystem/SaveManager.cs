using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that owns all JSON save/load operations.
/// Lives across scene loads (DontDestroyOnLoad).
///
/// HOW TO USE
/// ----------
/// Chapter completed  : SaveManager.Instance.SetChapterCompleted("Chapter1");
/// Scene changed      : SaveManager.Instance.SaveCurrentScene();   (or pass name)
/// Quiz score         : SaveManager.Instance.SetQuizScore("Chapter1", 10);
/// Baby name          : SaveManager.Instance.SetBabyName("Sophia");
/// Load saved scene   : SaveManager.Instance.LoadSavedScene();
/// Video watched      : SaveManager.Instance.MarkVideoViewed("chapter1_intro");
/// </summary>
public class SaveManager : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Singleton
    // -----------------------------------------------------------------------
    public static SaveManager Instance { get; private set; }

    // -----------------------------------------------------------------------
    //  State
    // -----------------------------------------------------------------------
    public SaveData CurrentSave { get; private set; } = new SaveData();

    private string _savePath;

    // -----------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // MUST be a root object for DontDestroyOnLoad to work.
            // This detaches from any parent so it always works.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, "save.json");
            Debug.Log("[SaveManager] Save path: " + _savePath);

            // Subscribe ONCE here — persists for the whole game session
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Auto-load on game start
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Only unsubscribe if this is the real singleton instance
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // -----------------------------------------------------------------------
    //  Active chapter tracking
    // -----------------------------------------------------------------------

    // Set by ChapterResumeButton when a chapter is entered
    private string _activeChapter = "";

    // True while the player is replaying a completed chapter.
    // Per-chapter scene progress is frozen during replay so that
    // visiting Chapter 1 cannot overwrite Chapter 2's saved scene.
    private bool _isReplayMode = false;

    /// <summary>
    /// Called by ChapterResumeButton to tell SaveManager which chapter
    /// the player is about to enter. Used to save per-chapter progress.
    /// Also immediately syncs currentScene to that chapter's last saved
    /// scene (or empty if the chapter was never started), so the value
    /// is always chapter-scoped from the moment a chapter is selected.
    /// </summary>
    public void SetActiveChapter(string chapterName)
    {
        _activeChapter = chapterName;

        // Sync currentScene to the selected chapter's progress slot.
        // This ensures currentScene == "" for chapters with no saved scene,
        // and currentScene == lastSceneChapterX for chapters that were started.
        // Replay mode is excluded — during replay, currentScene is not updated.
        if (!_isReplayMode)
        {
            CurrentSave.currentScene = GetChapterScene(chapterName);
            SaveGame();
        }

        Debug.Log("[SaveManager] Active chapter set to: " + chapterName
            + " | currentScene: " + CurrentSave.currentScene);
    }

    /// <summary>
    /// Enable replay mode when re-entering a completed chapter.
    /// Per-chapter scene data is not written during replay.
    /// </summary>
    public void SetReplayMode(bool isReplaying)
    {
        _isReplayMode = isReplaying;
        Debug.Log("[SaveManager] Replay mode: " + isReplaying);
    }

    /// <summary>
    /// Save the last scene reached inside a specific chapter.
    /// </summary>
    public void SaveChapterScene(string chapterName, string sceneName)
    {
        switch (chapterName)
        {
            case "Chapter1": CurrentSave.lastSceneChapter1 = sceneName; break;
            case "Chapter2": CurrentSave.lastSceneChapter2 = sceneName; break;
            case "Chapter3": CurrentSave.lastSceneChapter3 = sceneName; break;
            case "Chapter4": CurrentSave.lastSceneChapter4 = sceneName; break;
            case "Chapter5": CurrentSave.lastSceneChapter5 = sceneName; break;
        }
    }

    /// <summary>
    /// Get the last scene reached inside a specific chapter.
    /// Returns empty string if the player has never entered that chapter.
    /// </summary>
    public string GetChapterScene(string chapterName)
    {
        return chapterName switch
        {
            "Chapter1" => CurrentSave.lastSceneChapter1,
            "Chapter2" => CurrentSave.lastSceneChapter2,
            "Chapter3" => CurrentSave.lastSceneChapter3,
            "Chapter4" => CurrentSave.lastSceneChapter4,
            "Chapter5" => CurrentSave.lastSceneChapter5,
            _ => ""
        };
    }

    /// <summary>
    /// Clears the saved scene for a chapter.
    /// Used when a chapter has been completely finished,
    /// so replaying the chapter starts from the beginning.
    /// </summary>
    /// TODO: added this
    public void ClearChapterScene(string chapterName)
    {
        switch (chapterName)
        {
            case "Chapter1":
                CurrentSave.lastSceneChapter1 = "";
                break;

            case "Chapter2":
                CurrentSave.lastSceneChapter2 = "";
                break;

            case "Chapter3":
                CurrentSave.lastSceneChapter3 = "";
                break;

            case "Chapter4":
                CurrentSave.lastSceneChapter4 = "";
                break;

            case "Chapter5":
                CurrentSave.lastSceneChapter5 = "";
                break;

            default:
                Debug.LogWarning(
                    "[SaveManager] Cannot clear scene. Unknown chapter: "
                    + chapterName
                );
                return;
        }

        SaveGame();

        Debug.Log(
            "[SaveManager] Cleared saved scene for "
            + chapterName
        );
    }

    /// <summary>
    /// Clears the saved replay-scene checkpoint for a chapter.
    /// Called when a chapter replay is completed so that the
    /// next replay always starts fresh from the beginning.
    /// </summary>
    public void ClearReplayChapterScene(string chapterName)
    {
        switch (chapterName)
        {
            case "Chapter1":
                CurrentSave.lastSceneReplayChapter1 = "";
                break;

            case "Chapter2":
                CurrentSave.lastSceneReplayChapter2 = "";
                break;

            case "Chapter3":
                CurrentSave.lastSceneReplayChapter3 = "";
                break;

            case "Chapter4":
                CurrentSave.lastSceneReplayChapter4 = "";
                break;

            case "Chapter5":
                CurrentSave.lastSceneReplayChapter5 = "";
                break;

            default:
                Debug.LogWarning(
                    "[SaveManager] Cannot clear replay scene. Unknown chapter: "
                    + chapterName
                );
                return;
        }

        SaveGame();

        Debug.Log(
            "[SaveManager] Cleared replay scene for "
            + chapterName
        );
    }

    /// <summary>
    /// Save the last scene reached while replaying a completed chapter.
    /// </summary>
    public void SaveReplayChapterScene(string chapterName, string sceneName)
    {
        switch (chapterName)
        {
            case "Chapter1": CurrentSave.lastSceneReplayChapter1 = sceneName; break;
            case "Chapter2": CurrentSave.lastSceneReplayChapter2 = sceneName; break;
            case "Chapter3": CurrentSave.lastSceneReplayChapter3 = sceneName; break;
            case "Chapter4": CurrentSave.lastSceneReplayChapter4 = sceneName; break;
            case "Chapter5": CurrentSave.lastSceneReplayChapter5 = sceneName; break;
        }
    }

    /// <summary>
    /// Get the last scene reached while replaying a completed chapter.
    /// Returns empty string if the chapter has never been replayed.
    /// </summary>
    public string GetReplayChapterScene(string chapterName)
    {
        return chapterName switch
        {
            "Chapter1" => CurrentSave.lastSceneReplayChapter1,
            "Chapter2" => CurrentSave.lastSceneReplayChapter2,
            "Chapter3" => CurrentSave.lastSceneReplayChapter3,
            "Chapter4" => CurrentSave.lastSceneReplayChapter4,
            "Chapter5" => CurrentSave.lastSceneReplayChapter5,
            _ => ""
        };
    }

    // -----------------------------------------------------------------------
    //  Scene lifecycle hooks
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires automatically whenever any scene finishes loading.
    /// Updates the per-chapter last scene (chapter buttons) and derives
    /// currentScene from the active chapter's progress slot so that
    /// currentScene always reflects lastSceneChapterX for the selected
    /// chapter — never a scene from a different chapter.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Do not save navigation/progress when entering ChapterQuiz.
        // The previous gameplay scene should remain the resume point.
        if (IsNonSaveScene(scene.name))
        {
            Debug.Log("[SaveManager] " + scene.name + " loaded — scene progress NOT saved.");
            return;
        }

        // Hub scenes
        if (IsHubScene(scene.name))
        {
            // Player returned to hub — clear chapter tracking and replay mode
            _activeChapter = "";
            _isReplayMode = false;
            return;
        }

        if (!string.IsNullOrEmpty(_activeChapter))
        {
            if (_isReplayMode)
            {
                // Replay mode — save to replay slot only; do not touch currentScene
                SaveReplayChapterScene(_activeChapter, scene.name);
            }
            else
            {
                // Normal play — save to the chapter's progress slot
                SaveChapterScene(_activeChapter, scene.name);

                // currentScene always mirrors the active chapter's progress slot,
                // so CONTINUE resumes the correct chapter (not another chapter's scene)
                CurrentSave.currentScene = GetChapterScene(_activeChapter);
            }
        }

        SaveGame();

        Debug.Log("[SaveManager] Scene saved: " + scene.name
            + " | Chapter: " + (_activeChapter == "" ? "none" : _activeChapter)
            + " | Replay: " + _isReplayMode
            + " | currentScene: " + CurrentSave.currentScene);
    }

    /// <summary>
    /// Safety net for PC/Editor — fires when the player quits.
    /// </summary>
    private void OnApplicationQuit()
    {
        string active = SceneManager.GetActiveScene().name;

        // Never save ChapterQuiz as a resume point
        if (active == "ChapterQuiz")
        {
            Debug.Log("[SaveManager] Game quitting from ChapterQuiz — keeping previous scene.");
            return;
        }

        if (!IsHubScene(active) && !IsNonSaveScene(active))
        {
            if (!string.IsNullOrEmpty(_activeChapter) && !_isReplayMode)
            {
                SaveChapterScene(_activeChapter, active);

                // Keep currentScene in sync with the active chapter's slot
                CurrentSave.currentScene = GetChapterScene(_activeChapter);
            }

            SaveGame();
        }
    }

    /// <summary>
    /// Safety net for Android/iOS — fires when app goes to background.
    /// </summary>
    private void OnApplicationPause(bool isPaused)
    {
        if (!isPaused) return;

        string active = SceneManager.GetActiveScene().name;

        // Never save ChapterQuiz as a resume point
        if (active == "ChapterQuiz")
        {
            Debug.Log("[SaveManager] App paused in ChapterQuiz — keeping previous scene.");
            return;
        }

        if (!IsHubScene(active) && !IsNonSaveScene(active))
        {
            if (!string.IsNullOrEmpty(_activeChapter) && !_isReplayMode)
            {
                SaveChapterScene(_activeChapter, active);

                // Keep currentScene in sync with the active chapter's slot
                CurrentSave.currentScene = GetChapterScene(_activeChapter);
            }

            SaveGame();
        }
    }

    /// <summary>
    /// Returns true for scenes that are navigation hubs, not gameplay scenes.
    /// These should never overwrite real gameplay progress.
    /// </summary>
    private bool IsHubScene(string sceneName)
    {
        return string.IsNullOrEmpty(sceneName)
            || sceneName == "TitleScene"
            || sceneName == "ChaptersScene";
    }

    private bool IsNonSaveScene(string sceneName)
    {
        return string.IsNullOrEmpty(sceneName)
            || sceneName == "TitleScene"
            || sceneName == "ChaptersScene"
            || sceneName == "ChapterQuiz"
            || sceneName == "Scene15";
    }


    // -----------------------------------------------------------------------
    //  Chapter progress
    // -----------------------------------------------------------------------

    /// <summary>
    /// Mark a chapter as completed and immediately save.
    /// chapterName must be "Chapter1" … "Chapter5".
    /// </summary>
    public void SetChapterCompleted(string chapterName)
    {
        switch (chapterName)
        {
            case "Chapter1": CurrentSave.chapter1Completed = true; break;
            case "Chapter2": CurrentSave.chapter2Completed = true; break;
            case "Chapter3": CurrentSave.chapter3Completed = true; break;
            case "Chapter4": CurrentSave.chapter4Completed = true; break;
            case "Chapter5": CurrentSave.chapter5Completed = true; break;
            default:
                Debug.LogWarning("[SaveManager] Unknown chapter: " + chapterName);
                return;
        }
        SaveGame();
    }

    /// <summary>
    /// Returns whether a chapter has been completed.
    /// </summary>
    public bool IsChapterCompleted(string chapterName)
    {
        return chapterName switch
        {
            "Chapter1" => CurrentSave.chapter1Completed,
            "Chapter2" => CurrentSave.chapter2Completed,
            "Chapter3" => CurrentSave.chapter3Completed,
            "Chapter4" => CurrentSave.chapter4Completed,
            "Chapter5" => CurrentSave.chapter5Completed,
            _ => false
        };
    }

    // -----------------------------------------------------------------------
    //  Scene tracking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Save the currently active scene name and write to disk.
    /// </summary>
    public void SaveCurrentScene()
    {
        CurrentSave.currentScene = SceneManager.GetActiveScene().name;
        SaveGame();
    }

    /// <summary>
    /// Save a specific scene name and write to disk.
    /// </summary>
    public void SaveCurrentScene(string sceneName)
    {
        CurrentSave.currentScene = sceneName;
        SaveGame();
    }

    /// <summary>
    /// Load the scene stored in the save file.
    /// Does nothing if no save exists or currentScene is empty.
    /// </summary>
    public void LoadSavedScene()
    {
        if (!string.IsNullOrEmpty(CurrentSave.currentScene))
        {
            Debug.Log("[SaveManager] Loading saved scene: " + CurrentSave.currentScene);
            SceneManager.LoadScene(CurrentSave.currentScene);
        }
        else
        {
            Debug.Log("[SaveManager] No saved scene found.");
        }
    }

    // -----------------------------------------------------------------------
    //  Quiz scores
    // -----------------------------------------------------------------------

    /// <summary>
    /// Save a quiz score for a specific chapter.
    /// chapterName must be "Chapter1" … "Chapter5".
    /// </summary>
    public void SetQuizScore(string chapterName, int score)
    {
        switch (chapterName)
        {
            case "Chapter1": CurrentSave.quizScoreChapter1 = score; break;
            case "Chapter2": CurrentSave.quizScoreChapter2 = score; break;
            case "Chapter3": CurrentSave.quizScoreChapter3 = score; break;
            case "Chapter4": CurrentSave.quizScoreChapter4 = score; break;
            case "Chapter5": CurrentSave.quizScoreChapter5 = score; break;
            default:
                Debug.LogWarning("[SaveManager] Unknown chapter for quiz score: " + chapterName);
                return;
        }
        SaveGame();
    }

    /// <summary>
    /// Get the saved quiz score for a chapter.
    /// </summary>
    public int GetQuizScore(string chapterName)
    {
        return chapterName switch
        {
            "Chapter1" => CurrentSave.quizScoreChapter1,
            "Chapter2" => CurrentSave.quizScoreChapter2,
            "Chapter3" => CurrentSave.quizScoreChapter3,
            "Chapter4" => CurrentSave.quizScoreChapter4,
            "Chapter5" => CurrentSave.quizScoreChapter5,
            _ => 0
        };
    }

    // -----------------------------------------------------------------------
    //  Quiz attempted
    // -----------------------------------------------------------------------

    /// <summary>
    /// Mark that the player has attempted (opened) a chapter's quiz.
    /// Call this in QuizManager.Start().
    /// </summary>
    public void SetQuizAttempted(string chapterName)
    {
        switch (chapterName)
        {
            case "Chapter1": CurrentSave.quizAttemptedChapter1 = true; break;
            case "Chapter2": CurrentSave.quizAttemptedChapter2 = true; break;
            case "Chapter3": CurrentSave.quizAttemptedChapter3 = true; break;
            case "Chapter4": CurrentSave.quizAttemptedChapter4 = true; break;
            case "Chapter5": CurrentSave.quizAttemptedChapter5 = true; break;
            default:
                Debug.LogWarning("[SaveManager] Unknown chapter for quiz attempted: " + chapterName);
                return;
        }
        SaveGame();
        Debug.Log("[SaveManager] Quiz attempted: " + chapterName);
    }

    /// <summary>
    /// Returns true if the player has ever opened this chapter's quiz.
    /// </summary>
    public bool IsQuizAttempted(string chapterName)
    {
        return chapterName switch
        {
            "Chapter1" => CurrentSave.quizAttemptedChapter1,
            "Chapter2" => CurrentSave.quizAttemptedChapter2,
            "Chapter3" => CurrentSave.quizAttemptedChapter3,
            "Chapter4" => CurrentSave.quizAttemptedChapter4,
            "Chapter5" => CurrentSave.quizAttemptedChapter5,
            _ => false
        };
    }

    // -----------------------------------------------------------------------
    //  Baby name
    // -----------------------------------------------------------------------

    /// <summary>
    /// Save the baby name and write to disk.
    /// </summary>
    public void SetBabyName(string name)
    {
        CurrentSave.babyName = name;
        SaveGame();
    }

    public void SetTermsAccepted()
    {
        CurrentSave.termsAccepted = true;
        SaveGame();
    }

    // -----------------------------------------------------------------------
    //  Vaccine choice
    // -----------------------------------------------------------------------

    /// <summary>
    /// Save whether the player chose to give vaccines.
    /// true = Give Vaccines
    /// false = Delay Vaccines
    /// </summary>
    public void SetGiveVaccine(bool giveVaccine)
    {
        CurrentSave.IsGiveVaccine = giveVaccine ? "true" : "false";

        // Choosing "Delay Vaccines" (false) completes Chapter 2
        if (!giveVaccine)
        {
            CurrentSave.chapter2Completed = true;
            Debug.Log("[SaveManager] Delay Vaccines chosen — chapter2Completed set to true.");
        }

        SaveGame();
        Debug.Log("[SaveManager] IsGiveVaccine = " + giveVaccine);
    }

    /// <summary>
    /// Returns the saved vaccine choice.
    /// </summary>
    public string GetGiveVaccine()
    {
        return CurrentSave.IsGiveVaccine;
    }

    /// <summary>
    /// Returns the saved baby name (empty string if not yet set).
    /// </summary>
    public string GetBabyName() => CurrentSave.babyName;

    // -----------------------------------------------------------------------
    //  Gallery videos
    // -----------------------------------------------------------------------

    /// <summary>
    /// Marks a video as viewed (unlocking it in the Gallery) and saves.
    /// Safe to call multiple times — won't add duplicates.
    /// </summary>
    public void MarkVideoViewed(string videoId)
    {
        if (string.IsNullOrEmpty(videoId))
        {
            Debug.LogWarning("[SaveManager] MarkVideoViewed called with empty videoId.");
            return;
        }

        if (!CurrentSave.viewedVideos.Contains(videoId))
        {
            CurrentSave.viewedVideos.Add(videoId);
            SaveGame();
            Debug.Log("[SaveManager] Video marked as viewed: " + videoId);
        }
    }

    /// <summary>
    /// Returns true if the player has watched this video before.
    /// </summary>
    public bool IsVideoViewed(string videoId)
    {
        return CurrentSave.viewedVideos.Contains(videoId);
    }

    // -----------------------------------------------------------------------
    //  Core IO
    // -----------------------------------------------------------------------

    /// <summary>
    /// Write CurrentSave to save.json.
    /// Called automatically by every setter — do not call manually.
    /// </summary>
    private void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSave, true);
        File.WriteAllText(_savePath, json);

        Debug.Log("[SaveManager] SAVED:\n" + json);
    }

    /// <summary>
    /// Read save.json into CurrentSave.
    /// Called automatically on Awake — do not call manually.
    /// </summary>
    private void LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[SaveManager] No save file found — starting fresh.");
            CurrentSave = new SaveData();
            return;
        }

        string json = File.ReadAllText(_savePath);
        CurrentSave = JsonUtility.FromJson<SaveData>(json);

        Debug.Log("[SaveManager] LOADED:\n" + json);
    }

    /// <summary>
    /// Delete save.json and reset state.
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("[SaveManager] Save file deleted.");
        }

        CurrentSave = new SaveData();
    }

    /// <summary>
    /// Returns true if a save file exists on disk.
    /// </summary>
    public bool HasSave() => File.Exists(_savePath);
}