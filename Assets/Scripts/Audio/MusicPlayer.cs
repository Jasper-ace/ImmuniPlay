using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip music;

    [Header("Playback")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Tooltip("Delay before the music starts when the scene loads. Set to 0 for instant playback. The delay is only used for the initial play and does not affect looping.")]
    [SerializeField] private float startDelay = 0f;

    [Tooltip("Stops the currently playing music before this MusicPlayer starts its music.")]
    [SerializeField] private bool stopPreviousMusic = false;

    private void Start()
    {
        if (playOnStart)
        {
            if (stopPreviousMusic && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }

            if (startDelay > 0f)
                Invoke(nameof(PlayMusicDelayed), startDelay);
            else
                PlayMusic();
        }
    }

    private void PlayMusicDelayed()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (music == null)
        {
            Debug.LogWarning("MusicPlayer: No music clip assigned.");
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("MusicPlayer: AudioManager instance not found.");
            return;
        }

        AudioManager.Instance.PlayMusic(music, loop);
    }

    public void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
}
