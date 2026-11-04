using UnityEngine;

/// <summary>
/// Attach this component to the Settings panel GameObject.
/// Audio stays paused for as long as this GameObject is active/enabled.
/// </summary>
public class AudioPauseWhileEnabled : MonoBehaviour
{
    private void OnEnable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSettingsAudioPaused(true);
    }

    private void Start()
    {
        // Also handle cases where the panel was already active before
        // AudioManager finished initializing.
        if (isActiveAndEnabled && AudioManager.Instance != null)
            AudioManager.Instance.SetSettingsAudioPaused(true);
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSettingsAudioPaused(false);
    }
}
