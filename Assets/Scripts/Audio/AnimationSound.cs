using UnityEngine;

public class AnimationSound : MonoBehaviour
{
    /// <summary>
    /// Plays any AudioClip passed from an Animation Event
    /// through the central AudioManager.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        Debug.Log("ANIMATION SOUND CALLED");

        if (clip == null)
        {
            Debug.LogWarning(
                $"[{gameObject.name}] AnimationSound: AudioClip is empty."
            );
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                $"[{gameObject.name}] AnimationSound: AudioManager is not available."
            );
            return;
        }

        Debug.Log("Playing: " + clip.name);

        AudioManager.Instance.PlaySFX(clip);
    }
}
