using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Slider dialogueSlider;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string UI_VOLUME_KEY = "UIVolume";
    private const string DIALOGUE_VOLUME_KEY = "DialogueVolume";

    [Header("Volume Labels")]
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI sfxLabel;
    [SerializeField] private TextMeshProUGUI uiLabel;
    [SerializeField] private TextMeshProUGUI dialogueLabel;

    private void Start()
    {
        LoadSettings();
    }

    // ==========================================
    // MUSIC
    // ==========================================

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();

        if (musicLabel != null)
            musicLabel.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // ==========================================
    // SFX
    // ==========================================

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();

        if (sfxLabel != null)
            sfxLabel.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // ==========================================
    // UI
    // ==========================================

    public void SetUIVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetUIVolume(value);
        }

        PlayerPrefs.SetFloat(UI_VOLUME_KEY, value);
        PlayerPrefs.Save();

        if (uiLabel != null)
            uiLabel.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // ==========================================
    // DIALOGUE
    // ==========================================

    public void SetDialogueVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetDialogueVolume(value);
        }

        PlayerPrefs.SetFloat(DIALOGUE_VOLUME_KEY, value);
        PlayerPrefs.Save();

        if (dialogueLabel != null)
            dialogueLabel.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // ==========================================
    // LOAD SETTINGS
    // ==========================================

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        float uiVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, 1f);
        float dialogueVolume = PlayerPrefs.GetFloat(DIALOGUE_VOLUME_KEY, 1f);

        // Update sliders without triggering OnValueChanged
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(musicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfxVolume);

        if (uiSlider != null)
            uiSlider.SetValueWithoutNotify(uiVolume);

        if (dialogueSlider != null)
            dialogueSlider.SetValueWithoutNotify(dialogueVolume);

        if (musicLabel != null)
            musicLabel.text = Mathf.RoundToInt(musicVolume * 100f) + "%";

        if (sfxLabel != null)
            sfxLabel.text = Mathf.RoundToInt(sfxVolume * 100f) + "%";

        if (uiLabel != null)
            uiLabel.text = Mathf.RoundToInt(uiVolume * 100f) + "%";

        if (dialogueLabel != null)
            dialogueLabel.text = Mathf.RoundToInt(dialogueVolume * 100f) + "%";

        // Apply saved values to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
            AudioManager.Instance.SetUIVolume(uiVolume);
            AudioManager.Instance.SetDialogueVolume(dialogueVolume);
        }
    }
}