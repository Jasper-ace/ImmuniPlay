using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to each video thumbnail button in the Gallery.
/// Set the Scene Name to the name of the scene that plays that specific
/// video fullscreen. Hook this component's PlayVideo() method up to the
/// button's OnClick() event in the Inspector.
/// </summary>
public class GalleryVideoButton : MonoBehaviour
{
    [Tooltip("Exact name of the scene that plays this video fullscreen (must be added to Build Settings).")]
    [SerializeField] private string videoSceneName;

    /// <summary>
    /// Call this from the Button's OnClick() event.
    /// </summary>
    public void PlayVideo()
    {
        if (string.IsNullOrEmpty(videoSceneName))
        {
            Debug.LogWarning("[GalleryVideoButton] No videoSceneName set on " + gameObject.name);
            return;
        }

        SceneManager.LoadScene(videoSceneName);
    }
}