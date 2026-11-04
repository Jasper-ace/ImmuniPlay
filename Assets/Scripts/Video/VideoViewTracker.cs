using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Attach this to the same GameObject as a Unity VideoPlayer.
/// Set a unique "Video Id" in the Inspector per video — that's the
/// variable name that ties this video to its Gallery unlock state.
///
/// When the video finishes playing, it's marked as viewed and saved.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoViewTracker : MonoBehaviour
{
    [Tooltip("Unique ID for this video. Must match the ID used on its Gallery button entry.")]
    [SerializeField] private string videoId;

    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        _videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        _videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (string.IsNullOrEmpty(videoId))
        {
            Debug.LogWarning("[VideoViewTracker] No videoId set on " + gameObject.name);
            return;
        }

        SaveManager.Instance.MarkVideoViewed(videoId);
    }
}