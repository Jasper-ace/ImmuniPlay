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
    /// </summary>
    public void SetActiveChapter(string chapterName)
    {
        _activeChapter = chapterName;
        Debug.Log("[SaveManager] Active chapter set to: " + chapterName);
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
    /// Updates both the global currentScene (CONTINUE button) and
    /// the per-chapter last scene (chapter buttons).
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsHubScene(scene.name))
        {
            // Player returned to hub — clear chapter tracking and replay mode
            _activeChapter = "";
            _isReplayMode = false;
            return;
        }

        // Global resume point (used by CONTINUE button)
        CurrentSave.currentScene = scene.name;

        if (!string.IsNullOrEmpty(_activeChapter))
        {
            if (_isReplayMode)
                // Replay mode — save to replay slot, not the main progress slot
                SaveReplayChapterScene(_activeChapter, scene.name);
            else
                // Normal play — save to main progress slot
                SaveChapterScene(_activeChapter, scene.name);
        }

        SaveGame();

        Debug.Log("[SaveManager] Scene saved: " + scene.name
            + " | Chapter: " + (_activeChapter == "" ? "none" : _activeChapter)
            + " | Replay: " + _isReplayMode);
    }

    /// <summary>
    /// Safety net for PC/Editor — fires when the player quits.
    /// </summary>
    private void OnApplicationQuit()
    {
        string active = SceneManager.GetActiveScene().name;
        if (!IsHubScene(active))
        {
            CurrentSave.currentScene = active;
            if (!string.IsNullOrEmpty(_activeChapter) && !_isReplayMode)
                SaveChapterScene(_activeChapter, active);
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
        if (!IsHubScene(active))
        {
            CurrentSave.currentScene = active;
            if (!string.IsNullOrEmpty(_activeChapter) && !_isReplayMode)
                SaveChapterScene(_activeChapter, active);
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
            || sceneName == "Chapters";
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

    /// <summary>
    /// Returns the saved baby name (empty string if not yet set).
    /// </summary>
    /// 
    /// // -----------------------------------------------------------------------
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
    public string GetBabyName() => CurrentSave.babyName;

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
