using UnityEngine;

/// <summary>
/// Attach this anywhere in the scene (e.g. an empty "PauseManager" object,
/// or your UI Canvas). Hook the Pause button's OnClick() to TogglePause(),
/// or use Pause()/Resume() directly if you have separate buttons for each.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The panel that appears when the game is paused (e.g. a pause menu).")]
    public GameObject pausePanel;

    public bool IsPaused { get; private set; } = false;

    /// <summary>
    /// Call this from the Pause button's OnClick() event.
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}