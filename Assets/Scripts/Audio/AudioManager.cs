
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource dialogueSource;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string UI_VOLUME_KEY = "UIVolume";
    private const string DIALOGUE_VOLUME_KEY = "DialogueVolume";

    // Independent pause reasons prevent one overlay from resuming audio
    // while another overlay is still open.
    private bool sideMenuPauseRequested;
    private bool settingsPauseRequested;

    private void Awake()
    {
        // Prevent duplicate AudioManagers when changing scenes.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep AudioManager alive between scenes.
        DontDestroyOnLoad(gameObject);

        // Configure audio sources.
        SetupAudioSource(musicSource);
        SetupAudioSource(sfxSource);
        SetupAudioSource(uiSource);
        SetupAudioSource(dialogueSource);

        // Apply saved volume settings immediately.
        LoadSavedVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check whether the new scene has a MusicPlayer.
        MusicPlayer musicPlayer = FindFirstObjectByType<MusicPlayer>();

        // Stop previous music if no MusicPlayer exists.
        if (musicPlayer == null)
        {
            StopMusic();
        }

        // Reapply saved volumes after loading a scene.
        LoadSavedVolumes();
    }

    private void SetupAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    // ==========================================
    // LOAD SAVED VOLUMES
    // ==========================================

    private void LoadSavedVolumes()
    {
        float musicVolume =
            PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);

        float sfxVolume =
            PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        float uiVolume =
            PlayerPrefs.GetFloat(UI_VOLUME_KEY, 1f);

        float dialogueVolume =
            PlayerPrefs.GetFloat(DIALOGUE_VOLUME_KEY, 1f);

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetUIVolume(uiVolume);
        SetDialogueVolume(dialogueVolume);

        Debug.Log(
            "[AudioManager] Loaded saved volumes: " +
            $"Music={musicVolume}, " +
            $"SFX={sfxVolume}, " +
            $"UI={uiVolume}, " +
            $"Dialogue={dialogueVolume}"
        );
    }

    // ==========================================
    // MUSIC
    // ==========================================

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null)
            return;

        // Same music is already playing. Do not restart it.
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.loop = loop;
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // ==========================================
    // SFX
    // ==========================================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    // ==========================================
    // UI
    // ==========================================

    public void PlayUI(AudioClip clip)
    {
        if (clip == null || uiSource == null)
            return;

        uiSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip, float volume)
    {
        if (clip == null || uiSource == null)
            return;

        uiSource.PlayOneShot(clip, volume);
    }

    // ==========================================
    // DIALOGUE
    // ==========================================

    public void PlayDialogue(AudioClip clip)
    {
        if (clip == null || dialogueSource == null)
            return;

        dialogueSource.PlayOneShot(clip);
    }

    public void PlayDialogue(AudioClip clip, float volume)
    {
        if (clip == null || dialogueSource == null)
            return;

        dialogueSource.PlayOneShot(clip, volume);
    }

    public void StopDialogue()
    {
        if (dialogueSource == null)
            return;

        dialogueSource.Stop();
    }

    // ==========================================
    // VOLUME CONTROLS
    // ==========================================

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetUIVolume(float volume)
    {
        if (uiSource != null)
            uiSource.volume = Mathf.Clamp01(volume);
    }

    public void SetDialogueVolume(float volume)
    {
        if (dialogueSource != null)
            dialogueSource.volume = Mathf.Clamp01(volume);
    }

    // ==========================================
    // GLOBAL AUDIO PAUSE / STOP
    // ==========================================

    public void SetSideMenuAudioPaused(bool paused)
    {
        sideMenuPauseRequested = paused;
        UpdateGlobalAudioPause();
    }

    public void SetSettingsAudioPaused(bool paused)
    {
        settingsPauseRequested = paused;
        UpdateGlobalAudioPause();
    }

    private void UpdateGlobalAudioPause()
    {
        AudioListener.pause = sideMenuPauseRequested || settingsPauseRequested;
    }

    public void PauseAllSounds()
    {
        AudioListener.pause = true;
    }

    public void ResumeAllSounds()
    {
        // Use the pause reasons instead of blindly resuming. This keeps
        // audio paused if either overlay is still active.
        UpdateGlobalAudioPause();
    }

    public void StopAllSounds()
    {
        // Clear overlay pause states before leaving the scene.
        sideMenuPauseRequested = false;
        settingsPauseRequested = false;
        AudioListener.pause = false;

        if (musicSource != null)
            musicSource.Stop();

        if (sfxSource != null)
            sfxSource.Stop();

        if (uiSource != null)
            uiSource.Stop();

        if (dialogueSource != null)
            dialogueSource.Stop();
    }

    // ==========================================
    // HANDLE MUSIC IN SCENES
    // ==========================================

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null)
            return;

        musicSource.UnPause();
    }
}
