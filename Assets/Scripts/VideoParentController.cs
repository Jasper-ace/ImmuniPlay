using UnityEngine;
using UnityEngine.Video;

public class VideoParentController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Parent to Enable After Video")]
    public GameObject nextParent;

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Stop the video
        vp.Stop();

        // Disable this current parent
        gameObject.SetActive(false);

        // Enable the next parent
        if (nextParent != null)
        {
            nextParent.SetActive(true);
        }
    }
}