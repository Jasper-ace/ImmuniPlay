using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach to the Main Menu / Title Scene GameObject.
///
/// HOW TO WIRE IN INSPECTOR
/// ------------------------
/// startButton      -> the Button currently labelled "START"
/// startButtonText  -> the TMP_Text child of that button
/// newGameScene     -> name of the first scene for a brand-new game (e.g. "Chapter1Scene1")
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Start / Continue Button")]
    public Button startButton;
    public TMP_Text startButtonText;

    [Header("Scene Names")]
    [Tooltip("Scene to load when starting a brand-new game")]
    public string newGameScene = "Chapter1Scene1";

    // -----------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------
    void Start()
    {
        // SaveManager auto-loads save.json in its own Awake(),
        // so HasSave() / currentScene is reliable by the time Start() runs.
        bool hasValidScene = HasValidSavedScene();

        startButtonText.text = hasValidScene ? "CONTINUE" : "START";
    }

    // -----------------------------------------------------------------------
    //  Button callbacks  (wire these to OnClick in the Inspector)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by the START / CONTINUE button.
    /// New player  -> loads newGameScene.
    /// Returning   -> resumes the last saved scene.
    /// </summary>
    public void OnStartOrContinue()
    {
        if (HasValidSavedScene())
        {
            // Returning player — jump straight to the last saved scene
            Debug.Log("[MainMenu] Continuing from: " + SaveManager.Instance.CurrentSave.currentScene);
            SaveManager.Instance.LoadSavedScene();
        }
        else
        {
            // New player OR save exists but no scene was recorded yet
            Debug.Log("[MainMenu] Starting new game: " + newGameScene);

            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveCurrentScene(newGameScene);

            SceneManager.LoadScene(newGameScene);
        }
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns true only when a save file exists AND a non-empty scene name
    /// is recorded inside it. Both conditions must be true to show CONTINUE.
    /// </summary>
    private bool HasValidSavedScene()
    {
        if (SaveManager.Instance == null) return false;
        if (!SaveManager.Instance.HasSave()) return false;

        string savedScene = SaveManager.Instance.CurrentSave.currentScene;
        return !string.IsNullOrEmpty(savedScene);
    }

    /// <summary>
    /// Called by the QUIT button.
    /// </summary>
    public void OnQuit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
