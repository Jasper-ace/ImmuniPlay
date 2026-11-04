
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager1 : MonoBehaviour
{
    public static AudioManager1 Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource dialogueSource;

    [Header("Character Voice")]
    [SerializeField] private AudioSource voiceSource;

    private void Awake()
    {
        // Prevent duplicates when changing scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep AudioManager alive between scenes
        DontDestroyOnLoad(gameObject);

        // Safety setup
        SetupAudioSource(musicSource);
        SetupAudioSource(sfxSource);
        SetupAudioSource(uiSource);
        SetupAudioSource(dialogueSource);
        SetupAudioSource(voiceSource);
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
        // Check if the new scene has a MusicPlayer
        MusicPlayer musicPlayer = FindFirstObjectByType<MusicPlayer>();

        // If there is no MusicPlayer, stop the previous music
        if (musicPlayer == null)
        {
            StopMusic();
        }
    }

    private void SetupAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    // ==========================================
    // MUSIC
    // ==========================================

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null)
            return;

        // Don't restart the same music if it is already playing
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.loop = loop;
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

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
    // DIALOGUE SOUND EFFECTS
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
    // CHARACTER VOICE
    // ==========================================

    public void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (voiceSource == null)
        {
            Debug.LogWarning(
                "AudioManager: Voice Source is not assigned.",
                this
            );
            return;
        }

        // Stop the previous character voice
        voiceSource.Stop();

        if (clip == null)
            return;

        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.volume = Mathf.Clamp01(volume);
        voiceSource.Play();
    }

    public void StopVoice()
    {
        if (voiceSource == null)
            return;

        voiceSource.Stop();
        voiceSource.clip = null;
    }

    // ==========================================
    // VOLUMES
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

    public void SetVoiceVolume(float volume)
    {
        if (voiceSource != null)
            voiceSource.volume = Mathf.Clamp01(volume);
    }
}
