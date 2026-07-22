using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject settingsPanel;
    public GameObject confirmationPanel;

    [Header("Sound Settings")]
    public Slider volumeSlider;
    public Toggle muteToggle;

    [Header("Status Feedback")]
    public TMP_Text statusText;

    private const string VolumeKey = "GlobalVolume";
    private const string MuteKey = "GlobalMute";

    private void Start()
    {
        // Set up initial sound settings
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1.0f);
        bool savedMute = PlayerPrefs.GetInt(MuteKey, 0) == 1;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = savedMute;
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        }

        // Apply loaded settings
        ApplySettings(savedVolume, savedMute);

        // Hide panels by default
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (statusText != null) statusText.text = "";
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();

        // If not muted, update volume
        bool isMuted = muteToggle != null ? muteToggle.isOn : false;
        if (!isMuted)
        {
            AudioListener.volume = value;
        }
    }

    public void OnMuteToggled(bool isMuted)
    {
        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            float savedVolume = volumeSlider != null ? volumeSlider.value : 1.0f;
            AudioListener.volume = savedVolume;
        }
    }

    // Reset Progress Actions
    public void PromptResetProgress()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void CancelResetProgress()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void ConfirmResetProgress()
    {
        // Retrieve volume and mute keys so we don't clear the player's custom settings
        float currentVolume = PlayerPrefs.GetFloat(VolumeKey, 1.0f);
        int currentMute = PlayerPrefs.GetInt(MuteKey, 0);

        // Delete all progress
        PlayerPrefs.DeleteAll();

        // Reapply settings keys
        PlayerPrefs.SetFloat(VolumeKey, currentVolume);
        PlayerPrefs.SetInt(MuteKey, currentMute);
        PlayerPrefs.Save();

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = "PROGRESS RESET SUCCESSFULLY!";
            statusText.color = Color.green;
            Invoke("ClearStatusText", 2.0f);
        }
    }

    public void UnlockAllChapters()
    {
        PlayerPrefs.SetInt("Chapter1Done", 1);
        PlayerPrefs.SetInt("Chapter2Done", 1);
        PlayerPrefs.SetInt("Chapter3Done", 1);
        PlayerPrefs.SetInt("Chapter4Done", 1);
        PlayerPrefs.Save();

        if (statusText != null)
        {
            statusText.text = "ALL CHAPTERS UNLOCKED SUCCESSFULLY!";
            statusText.color = Color.yellow;
            Invoke("ClearStatusText", 2.0f);
        }
    }

    private void ClearStatusText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    private void ApplySettings(float volume, bool mute)
    {
        if (mute)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = volume;
        }
    }
}
