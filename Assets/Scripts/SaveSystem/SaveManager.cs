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

    /// <summary>
    /// Fires automatically whenever any scene finishes loading.
    /// Saves the new scene name — except TitleScene (main menu).
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Never overwrite a real game scene with the main menu
        if (scene.name == "TitleScene") return;

        CurrentSave.currentScene = scene.name;
        SaveGame();

        Debug.Log("[SaveManager] Scene loaded and saved: " + scene.name);
    }

    /// <summary>
    /// Safety net for PC/Editor — fires when the player quits.
    /// SceneManager.sceneLoaded already handles this in normal play.
    /// </summary>
    private void OnApplicationQuit()
    {
        string active = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(active) && active != "TitleScene")
        {
            CurrentSave.currentScene = active;
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
        if (!string.IsNullOrEmpty(active) && active != "TitleScene")
        {
            CurrentSave.currentScene = active;
            SaveGame();
        }
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
