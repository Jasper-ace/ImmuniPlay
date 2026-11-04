using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float volume = 1f;

    public void PlayClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                clickSound,
                volume
            );
        }
    }
}