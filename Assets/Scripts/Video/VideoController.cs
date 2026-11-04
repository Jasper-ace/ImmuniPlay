using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class VideoController : MonoBehaviour
{
    // =========================================================
    // ENUMS
    // =========================================================

    public enum TransitionType
    {
        DisableAndEnable,
        LoadScene
    }

    public enum PlayTrigger
    {
        OnSceneLoaded,
        OnButtonPressed
    }

    // =========================================================
    // VIDEO
    // =========================================================

    [Header("VIDEO")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoPanel;

    // =========================================================
    // PLAY SETTINGS
    // =========================================================

    [Header("PLAY SETTINGS")]
    [SerializeField]
    private PlayTrigger playTrigger = PlayTrigger.OnButtonPressed;

    // =========================================================
    // VIDEO SAVE
    // =========================================================

    [Header("VIDEO SAVE")]
    [Tooltip("Must match the Video Id in VideoViewTracker.")]
    [SerializeField] private string videoID;

    // =========================================================
    // SKIP BUTTON
    // =========================================================

    [Header("SKIP BUTTON")]
    [Tooltip("Skip button shown only if this video was already viewed.")]
    [SerializeField] private GameObject skipButton;

    // =========================================================
    // AFTER VIDEO
    // =========================================================

    [Header("AFTER VIDEO")]
    [SerializeField]
    private TransitionType transitionType =
        TransitionType.DisableAndEnable;

    // =========================================================
    // UNITY SCENE
    // =========================================================

    [Header("UNITY SCENE")]
    [SerializeField] private string nextSceneName;

    // =========================================================
    // GAMEOBJECTS TO SHOW
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW")]
    [SerializeField]
    private List<GameObject> gameObjectsToShow =
        new List<GameObject>();

    // =========================================================
    // GAMEOBJECTS TO HIDE
    // =========================================================

    [Header("GAMEOBJECTS TO HIDE")]
    [SerializeField]
    private List<GameObject> gameObjectsToHide =
        new List<GameObject>();

    // =========================================================
    // INTERNAL
    // =========================================================

    private bool videoFinished = false;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // Automatically find VideoPlayer if not assigned
        if (videoPlayer == null)
        {
            videoPlayer =
                GetComponentInChildren<VideoPlayer>();
        }

        // Subscribe to video finished event
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            Debug.LogError(
                "VideoController: No VideoPlayer found.",
                this
            );
        }

        // Hide Skip button when the scene starts
        if (skipButton != null)
        {
            skipButton.SetActive(false);

            Debug.Log(
                "VideoController: Skip button found in Awake. " +
                "Initial Active = " + skipButton.activeSelf,
                this
            );
        }
        else
        {
            Debug.LogError(
                "VideoController: Skip button is NOT assigned in Inspector!",
                this
            );
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Only automatically play when configured
        // as OnSceneLoaded.
        if (playTrigger == PlayTrigger.OnSceneLoaded)
        {
            Debug.Log(
                "VideoController: Play Trigger = OnSceneLoaded. " +
                "Starting video automatically.",
                this
            );

            PlayVideo();
        }
        else
        {
            Debug.Log(
                "VideoController: Play Trigger = OnButtonPressed. " +
                "Waiting for button.",
                this
            );
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }
    }

    // =========================================================
    // PLAY VIDEO
    // =========================================================

    public void PlayVideo()
    {
        Debug.Log(
            "========== VIDEO PLAY DEBUG START ==========",
            this
        );

        // -----------------------------------------------------
        // CHECK VIDEO PLAYER
        // -----------------------------------------------------

        if (videoPlayer == null)
        {
            Debug.LogError(
                "VideoController: VideoPlayer is NOT assigned.",
                this
            );

            return;
        }

        Debug.Log(
            "VideoController: VideoPlayer found.",
            this
        );

        // -----------------------------------------------------
        // CHECK VIDEO ID
        // -----------------------------------------------------

        Debug.Log(
            "VideoController: Current Video ID = [" +
            videoID + "]",
            this
        );

        if (string.IsNullOrEmpty(videoID))
        {
            Debug.LogError(
                "VideoController: VIDEO ID IS EMPTY!",
                this
            );
        }

        // -----------------------------------------------------
        // CHECK SAVE MANAGER
        // -----------------------------------------------------

        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "VideoController: SaveManager.Instance is NULL!",
                this
            );
        }
        else
        {
            Debug.Log(
                "VideoController: SaveManager.Instance FOUND.",
                this
            );
        }

        // -----------------------------------------------------
        // RESET FINISHED STATE
        // -----------------------------------------------------

        videoFinished = false;

        Debug.Log(
            "VideoController: videoFinished reset to FALSE.",
            this
        );

        // -----------------------------------------------------
        // SHOW VIDEO PANEL
        // -----------------------------------------------------

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);

            Debug.Log(
                "VideoController: Video panel ENABLED.",
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "VideoController: videoPanel is NULL!",
                this
            );
        }

        // -----------------------------------------------------
        // CHECK IF VIDEO WAS ALREADY WATCHED
        // -----------------------------------------------------

        bool alreadyViewed = false;

        if (SaveManager.Instance != null &&
            !string.IsNullOrEmpty(videoID))
        {
            Debug.Log(
                "VideoController: Checking SaveManager for Video ID [" +
                videoID + "]",
                this
            );

            alreadyViewed =
                SaveManager.Instance.IsVideoViewed(videoID);

            Debug.Log(
                "VideoController: IsVideoViewed returned = " +
                alreadyViewed,
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "VideoController: Cannot check viewed status because " +
                "SaveManager or Video ID is missing.",
                this
            );
        }

        // -----------------------------------------------------
        // SHOW / HIDE SKIP BUTTON
        // -----------------------------------------------------

        if (skipButton != null)
        {
            Debug.Log(
                "VideoController: Skip Button reference FOUND.",
                this
            );

            Debug.Log(
                "VideoController: Setting Skip Button Active = " +
                alreadyViewed,
                this
            );

            skipButton.SetActive(alreadyViewed);

            Debug.Log(
                "VideoController: Skip Button actual activeSelf = " +
                skipButton.activeSelf,
                this
            );
        }
        else
        {
            Debug.LogError(
                "VideoController: SKIP BUTTON REFERENCE IS NULL!",
                this
            );
        }

        // -----------------------------------------------------
        // VIEWED STATUS MESSAGE
        // -----------------------------------------------------

        if (alreadyViewed)
        {
            Debug.Log(
                "VideoController: VIDEO ALREADY VIEWED.",
                this
            );

            Debug.Log(
                "VideoController: SKIP BUTTON SHOULD BE VISIBLE.",
                this
            );

            Debug.Log(
                "VideoController: Video ID = [" +
                videoID + "]",
                this
            );
        }
        else
        {
            Debug.Log(
                "VideoController: VIDEO NOT FOUND AS VIEWED.",
                this
            );

            Debug.Log(
                "VideoController: SKIP BUTTON WILL BE HIDDEN.",
                this
            );

            Debug.Log(
                "VideoController: Video ID checked = [" +
                videoID + "]",
                this
            );
        }

        // -----------------------------------------------------
        // STOP PREVIOUS PLAYBACK
        // -----------------------------------------------------

        Debug.Log(
            "VideoController: Stopping previous video.",
            this
        );

        videoPlayer.Stop();

        // -----------------------------------------------------
        // CLEAR OLD VIDEO FRAME
        // -----------------------------------------------------

        Debug.Log(
            "VideoController: Clearing Render Texture.",
            this
        );

        ClearRenderTexture();

        // -----------------------------------------------------
        // PAUSE BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }

        // -----------------------------------------------------
        // PLAY VIDEO
        // -----------------------------------------------------

        Debug.Log(
            "VideoController: Starting video playback.",
            this
        );

        videoPlayer.Play();

        Debug.Log(
            "VideoController: Video playback started.",
            this
        );
    }

    // =========================================================
    // VIDEO FINISHED NATURALLY
    // =========================================================

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished)
            return;

        videoFinished = true;

        Debug.Log(
            "VideoController: Video finished.",
            this
        );

        // Hide Skip button
        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }

        // Hide video panel
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        ClearRenderTexture();

        // -----------------------------------------------------
        // RESUME BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        // Follow normal After Video action
        PerformAfterVideoAction();
    }

    // =========================================================
    // SKIP VIDEO
    // =========================================================

    public void SkipVideo()
    {
        // Prevent multiple presses
        if (videoFinished)
            return;

        // -----------------------------------------------------
        // SAFETY CHECK
        // -----------------------------------------------------

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "VideoController: SaveManager.Instance is null. " +
                "Skip is not available.",
                this
            );

            return;
        }

        if (string.IsNullOrEmpty(videoID))
        {
            Debug.LogWarning(
                "VideoController: Video ID is empty. " +
                "Skip is not available.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // ONLY ALLOW SKIP IF ALREADY VIEWED
        // -----------------------------------------------------

        if (!SaveManager.Instance.IsVideoViewed(videoID))
        {
            Debug.LogWarning(
                "VideoController: Cannot skip video because it " +
                "has not been viewed yet: " +
                videoID,
                this
            );

            return;
        }

        videoFinished = true;

        Debug.Log(
            "VideoController: Video skipped: " +
            videoID,
            this
        );

        // -----------------------------------------------------
        // STOP VIDEO
        // -----------------------------------------------------

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        ClearRenderTexture();

        // -----------------------------------------------------
        // HIDE SKIP BUTTON
        // -----------------------------------------------------

        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }

        // -----------------------------------------------------
        // HIDE VIDEO PANEL
        // -----------------------------------------------------

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }

        // -----------------------------------------------------
        // RESUME BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        // -----------------------------------------------------
        // FOLLOW SAME AFTER VIDEO ACTION
        // -----------------------------------------------------

        PerformAfterVideoAction();
    }

    // =========================================================
    // CLEAR RENDER TEXTURE
    // =========================================================

    private void ClearRenderTexture()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.targetTexture == null)
            return;

        RenderTexture renderTexture =
            videoPlayer.targetTexture;

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active = renderTexture;

        GL.Clear(
            true,
            true,
            Color.clear
        );

        RenderTexture.active = previous;
    }

    // =========================================================
    // AFTER VIDEO ACTION
    // =========================================================

    private void PerformAfterVideoAction()
    {
        switch (transitionType)
        {
            case TransitionType.DisableAndEnable:

                DisableAndEnableObjects();

                break;

            case TransitionType.LoadScene:

                LoadNextScene();

                break;
        }
    }

    // =========================================================
    // DISABLE AND ENABLE OBJECTS
    // =========================================================

    private void DisableAndEnableObjects()
    {
        // -----------------------------------------------------
        // SHOW
        // -----------------------------------------------------

        foreach (GameObject obj in gameObjectsToShow)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // -----------------------------------------------------
        // HIDE
        // -----------------------------------------------------

        foreach (GameObject obj in gameObjectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // =========================================================
    // LOAD NEXT SCENE
    // =========================================================

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "VideoController: Next Scene Name is empty."
            );

            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}