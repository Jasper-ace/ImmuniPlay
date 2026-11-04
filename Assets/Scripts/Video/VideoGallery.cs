using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoGallery : MonoBehaviour
{
    [Serializable]
    public class VideoThumbnailData
    {
        [Tooltip("Must match the video ID saved in SaveManager.")]
        public string videoID;

        [Tooltip("Thumbnail image for this video.")]
        public Sprite thumbnail;

        [Tooltip("Title displayed below the thumbnail.")]
        public string videoTitle;

        [Tooltip("Video clip that will play when this thumbnail is pressed.")]
        public VideoClip videoClip;
    }

    // =========================================================
    // GALLERY
    // =========================================================

    [Header("GALLERY")]
    [SerializeField] private Transform content;

    // =========================================================
    // THUMBNAIL PREFAB
    // =========================================================

    [Header("THUMBNAIL PREFAB")]
    [SerializeField] private GameObject videoThumbnailPrefab;

    // =========================================================
    // VIDEO PLAYER
    // =========================================================

    [Header("VIDEO PLAYER")]
    [Tooltip("The VideoPlayer component from your existing Video Player prefab.")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("The GameObject/panel that contains your Video Player UI.")]
    [SerializeField] private GameObject videoPlayerPanel;

    [Tooltip("Button used to skip the currently playing video.")]
    [SerializeField] private Button skipVideoButton;

    // =========================================================
    // VIDEO LIST
    // =========================================================

    [Header("VIDEO THUMBNAILS")]
    [SerializeField]
    private List<VideoThumbnailData> videoThumbnails =
        new List<VideoThumbnailData>();

    // =========================================================
    // INTERNAL
    // =========================================================

    private string currentVideoID = "";

    private bool videoFinished = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Debug.Log(
            "========== VIDEO GALLERY START ==========",
            this
        );

        SetupVideoPlayer();

        LoadViewedVideos();

        Debug.Log(
            "========== VIDEO GALLERY START COMPLETE ==========",
            this
        );
    }

    // =========================================================
    // SETUP VIDEO PLAYER
    // =========================================================

    private void SetupVideoPlayer()
    {
        // -----------------------------------------------------
        // CHECK VIDEO PLAYER
        // -----------------------------------------------------

        if (videoPlayer == null)
        {
            Debug.LogError(
                "VideoGallery: Video Player is NOT assigned!",
                this
            );
        }
        else
        {
            Debug.Log(
                "VideoGallery: Video Player found: " +
                videoPlayer.name,
                this
            );

            // Listen for natural video completion
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // -----------------------------------------------------
        // HIDE VIDEO PLAYER PANEL
        // -----------------------------------------------------

        if (videoPlayerPanel != null)
        {
            videoPlayerPanel.SetActive(false);

            Debug.Log(
                "VideoGallery: Video Player Panel hidden at start.",
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: Video Player Panel is NOT assigned!",
                this
            );
        }

        // -----------------------------------------------------
        // SETUP SKIP BUTTON
        // -----------------------------------------------------

        if (skipVideoButton != null)
        {
            skipVideoButton.onClick.RemoveListener(SkipVideo);
            skipVideoButton.onClick.AddListener(SkipVideo);

            skipVideoButton.gameObject.SetActive(false);

            Debug.Log(
                "VideoGallery: Skip Button found and hidden at start.",
                this
            );
        }
        else
        {
            Debug.LogError(
                "VideoGallery: Skip Button is NOT assigned!",
                this
            );
        }
    }

    // =========================================================
    // LOAD VIEWED VIDEOS
    // =========================================================

    private void LoadViewedVideos()
    {
        // -----------------------------------------------------
        // CHECK SAVE MANAGER
        // -----------------------------------------------------

        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "VideoGallery: SaveManager.Instance is NULL!",
                this
            );

            return;
        }

        Debug.Log(
            "VideoGallery: SaveManager.Instance found.",
            this
        );

        // -----------------------------------------------------
        // CHECK CONTENT
        // -----------------------------------------------------

        if (content == null)
        {
            Debug.LogError(
                "VideoGallery: Content is not assigned.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK PREFAB
        // -----------------------------------------------------

        if (videoThumbnailPrefab == null)
        {
            Debug.LogError(
                "VideoGallery: Video Thumbnail Prefab is not assigned.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK VIEWED VIDEOS
        // -----------------------------------------------------

        if (SaveManager.Instance.CurrentSave.viewedVideos == null)
        {
            Debug.Log(
                "VideoGallery: No viewed videos found.",
                this
            );

            return;
        }

        Debug.Log(
            "VideoGallery: SaveManager contains " +
            SaveManager.Instance.CurrentSave.viewedVideos.Count +
            " viewed videos.",
            this
        );

        // -----------------------------------------------------
        // PRINT ALL VIEWED VIDEO IDS
        // -----------------------------------------------------

        foreach (
            string videoID
            in SaveManager.Instance.CurrentSave.viewedVideos
        )
        {
            Debug.Log(
                "VideoGallery: Viewed Video ID = [" +
                videoID +
                "]",
                this
            );

            CreateVideoThumbnail(videoID);
        }

        Debug.Log(
            "VideoGallery: Loaded " +
            SaveManager.Instance.CurrentSave.viewedVideos.Count +
            " viewed videos.",
            this
        );
    }

    // =========================================================
    // CREATE THUMBNAIL
    // =========================================================

    private void CreateVideoThumbnail(string videoID)
    {
        if (string.IsNullOrEmpty(videoID))
        {
            Debug.LogWarning(
                "VideoGallery: Tried to create thumbnail with empty Video ID.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // FIND MATCHING VIDEO
        // -----------------------------------------------------

        VideoThumbnailData matchingVideo = null;

        foreach (
            VideoThumbnailData video
            in videoThumbnails
        )
        {
            if (video == null)
                continue;

            if (video.videoID == videoID)
            {
                matchingVideo = video;
                break;
            }
        }

        // -----------------------------------------------------
        // NO VIDEO FOUND
        // -----------------------------------------------------

        if (matchingVideo == null)
        {
            Debug.LogWarning(
                "VideoGallery: No video registered for Video ID: [" +
                videoID +
                "]",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK THUMBNAIL
        // -----------------------------------------------------

        if (matchingVideo.thumbnail == null)
        {
            Debug.LogWarning(
                "VideoGallery: Thumbnail is empty for Video ID: [" +
                videoID +
                "]",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK VIDEO CLIP
        // -----------------------------------------------------

        if (matchingVideo.videoClip == null)
        {
            Debug.LogError(
                "VideoGallery: Video Clip is empty for Video ID: [" +
                videoID +
                "]",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CREATE PREFAB
        // -----------------------------------------------------

        GameObject thumbnailObject =
            Instantiate(
                videoThumbnailPrefab,
                content
            );

        // -----------------------------------------------------
        // FIND THUMBNAIL IMAGE
        // -----------------------------------------------------

        Image image =
            thumbnailObject.GetComponentInChildren<Image>();

        if (image == null)
        {
            Debug.LogError(
                "VideoGallery: VideoThumbnail prefab does not " +
                "contain an Image component.",
                thumbnailObject
            );

            Destroy(thumbnailObject);

            return;
        }

        // -----------------------------------------------------
        // SET THUMBNAIL IMAGE
        // -----------------------------------------------------

        image.sprite = matchingVideo.thumbnail;
        image.preserveAspect = true;

        // -----------------------------------------------------
        // FIND VIDEO TITLE
        // -----------------------------------------------------

        TMP_Text titleText =
            thumbnailObject.GetComponentInChildren<TMP_Text>();

        if (titleText != null)
        {
            titleText.text = matchingVideo.videoTitle;

            Debug.Log(
                "VideoGallery: Set title for [" +
                videoID +
                "] = [" +
                matchingVideo.videoTitle +
                "]",
                thumbnailObject
            );
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: No TMP_Text found for Video Title " +
                "in thumbnail prefab.",
                thumbnailObject
            );
        }

        // -----------------------------------------------------
        // FIND BUTTON
        // -----------------------------------------------------

        Button button =
            thumbnailObject.GetComponent<Button>();

        if (button == null)
        {
            button =
                thumbnailObject.GetComponentInChildren<Button>();
        }

        // -----------------------------------------------------
        // CHECK BUTTON
        // -----------------------------------------------------

        if (button == null)
        {
            Debug.LogError(
                "VideoGallery: VideoThumbnail prefab does not " +
                "contain a Button component.",
                thumbnailObject
            );

            Destroy(thumbnailObject);

            return;
        }

        // -----------------------------------------------------
        // SET BUTTON
        // -----------------------------------------------------

        VideoClip selectedVideo =
            matchingVideo.videoClip;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            PlayVideo(
                selectedVideo,
                videoID
            );
        });

        // -----------------------------------------------------
        // LOG
        // -----------------------------------------------------

        Debug.Log(
            "VideoGallery: Created clickable thumbnail for [" +
            videoID +
            "] with title [" +
            matchingVideo.videoTitle +
            "]",
            thumbnailObject
        );
    }

    // =========================================================
    // PLAY VIDEO
    // =========================================================

    private void PlayVideo(
        VideoClip videoClip,
        string videoID
    )
    {
        Debug.Log(
            "========== VIDEO GALLERY PLAY ==========",
            this
        );

        // -----------------------------------------------------
        // STORE CURRENT VIDEO
        // -----------------------------------------------------

        currentVideoID = videoID;

        videoFinished = false;

        Debug.Log(
            "VideoGallery: Current Video ID = [" +
            currentVideoID +
            "]",
            this
        );

        // -----------------------------------------------------
        // CHECK VIDEO PLAYER
        // -----------------------------------------------------

        if (videoPlayer == null)
        {
            Debug.LogError(
                "VideoGallery: Video Player is NOT assigned!",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK VIDEO CLIP
        // -----------------------------------------------------

        if (videoClip == null)
        {
            Debug.LogError(
                "VideoGallery: Video Clip is NULL for Video ID: [" +
                videoID +
                "]",
                this
            );

            return;
        }

        Debug.Log(
            "VideoGallery: Video Clip = [" +
            videoClip.name +
            "]",
            this
        );

        // -----------------------------------------------------
        // CHECK SAVE MANAGER
        // -----------------------------------------------------

        bool alreadyViewed = false;

        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "VideoGallery: SaveManager.Instance is NULL!",
                this
            );
        }
        else
        {
            alreadyViewed =
                SaveManager.Instance.IsVideoViewed(videoID);

            Debug.Log(
                "VideoGallery: IsVideoViewed([" +
                videoID +
                "]) = " +
                alreadyViewed,
                this
            );
        }

        // -----------------------------------------------------
        // SHOW / HIDE SKIP BUTTON
        // -----------------------------------------------------

        if (skipVideoButton != null)
        {
            skipVideoButton.gameObject.SetActive(
                alreadyViewed
            );

            Debug.Log(
                "VideoGallery: Skip Button SetActive = " +
                alreadyViewed,
                this
            );

            Debug.Log(
                "VideoGallery: Skip Button actual activeSelf = " +
                skipVideoButton.gameObject.activeSelf,
                this
            );
        }
        else
        {
            Debug.LogError(
                "VideoGallery: Skip Button reference is NULL!",
                this
            );
        }

        // -----------------------------------------------------
        // STOP PREVIOUS VIDEO
        // -----------------------------------------------------

        Debug.Log(
            "VideoGallery: Stopping previous video.",
            this
        );

        videoPlayer.Stop();

        // -----------------------------------------------------
        // CLEAR OLD FRAME
        // -----------------------------------------------------

        Debug.Log(
            "VideoGallery: Clearing old Render Texture frame.",
            this
        );

        ClearRenderTexture();

        // -----------------------------------------------------
        // ASSIGN VIDEO
        // -----------------------------------------------------

        videoPlayer.clip = videoClip;

        Debug.Log(
            "VideoGallery: Assigned clip to VideoPlayer.",
            this
        );

        // -----------------------------------------------------
        // SHOW VIDEO PLAYER
        // -----------------------------------------------------

        if (videoPlayerPanel != null)
        {
            videoPlayerPanel.SetActive(true);

            Debug.Log(
                "VideoGallery: Video Player Panel enabled.",
                this
            );
        }

        // -----------------------------------------------------
        // PAUSE BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            Debug.Log(
                "VideoGallery: Pausing background music.",
                this
            );

            AudioManager.Instance.PauseMusic();
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: AudioManager.Instance is NULL. " +
                "Music cannot be paused.",
                this
            );
        }

        // -----------------------------------------------------
        // PLAY
        // -----------------------------------------------------

        videoPlayer.Play();

        Debug.Log(
            "VideoGallery: Playing video [" +
            videoID +
            "]",
            this
        );

        Debug.Log(
            "========== VIDEO GALLERY PLAY END ==========",
            this
        );
    }

    // =========================================================
    // SKIP VIDEO
    // =========================================================

    public void SkipVideo()
    {
        Debug.Log(
            "========== VIDEO GALLERY SKIP ==========",
            this
        );

        // -----------------------------------------------------
        // CHECK CURRENT VIDEO
        // -----------------------------------------------------

        Debug.Log(
            "VideoGallery: Current Video ID = [" +
            currentVideoID +
            "]",
            this
        );

        // -----------------------------------------------------
        // PREVENT DOUBLE PRESS
        // -----------------------------------------------------

        if (videoFinished)
        {
            Debug.Log(
                "VideoGallery: Video is already finished/skipped.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK SAVE MANAGER
        // -----------------------------------------------------

        if (SaveManager.Instance != null &&
            !string.IsNullOrEmpty(currentVideoID))
        {
            bool alreadyViewed =
                SaveManager.Instance.IsVideoViewed(
                    currentVideoID
                );

            Debug.Log(
                "VideoGallery: Skip check - IsVideoViewed([" +
                currentVideoID +
                "]) = " +
                alreadyViewed,
                this
            );

            if (!alreadyViewed)
            {
                Debug.LogWarning(
                    "VideoGallery: Skip blocked because this video " +
                    "has not been viewed yet.",
                    this
                );

                return;
            }
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: Cannot verify viewed status. " +
                "SaveManager or Video ID is missing.",
                this
            );
        }

        // -----------------------------------------------------
        // MARK AS FINISHED
        // -----------------------------------------------------

        videoFinished = true;

        Debug.Log(
            "VideoGallery: Video skipped.",
            this
        );

        // -----------------------------------------------------
        // STOP VIDEO
        // -----------------------------------------------------

        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            Debug.Log(
                "VideoGallery: VideoPlayer stopped.",
                this
            );
        }

        // -----------------------------------------------------
        // CLEAR RENDER TEXTURE
        // -----------------------------------------------------

        ClearRenderTexture();

        // -----------------------------------------------------
        // HIDE SKIP BUTTON
        // -----------------------------------------------------

        if (skipVideoButton != null)
        {
            skipVideoButton.gameObject.SetActive(false);

            Debug.Log(
                "VideoGallery: Skip Button hidden.",
                this
            );
        }

        // -----------------------------------------------------
        // HIDE VIDEO PANEL
        // -----------------------------------------------------

        if (videoPlayerPanel != null)
        {
            videoPlayerPanel.SetActive(false);

            Debug.Log(
                "VideoGallery: Video Player Panel hidden.",
                this
            );
        }

        // -----------------------------------------------------
        // RESUME BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            Debug.Log(
                "VideoGallery: Resuming background music after skip.",
                this
            );

            AudioManager.Instance.ResumeMusic();
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: AudioManager.Instance is NULL. " +
                "Music cannot be resumed.",
                this
            );
        }

        Debug.Log(
            "========== VIDEO GALLERY SKIP END ==========",
            this
        );
    }

    // =========================================================
    // VIDEO FINISHED NATURALLY
    // =========================================================

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log(
            "========== VIDEO GALLERY VIDEO FINISHED ==========",
            this
        );

        if (videoFinished)
        {
            Debug.Log(
                "VideoGallery: Video was already finished/skipped.",
                this
            );

            return;
        }

        videoFinished = true;

        Debug.Log(
            "VideoGallery: Video finished naturally.",
            this
        );

        Debug.Log(
            "VideoGallery: Finished Video ID = [" +
            currentVideoID +
            "]",
            this
        );

        // -----------------------------------------------------
        // HIDE SKIP BUTTON
        // -----------------------------------------------------

        if (skipVideoButton != null)
        {
            skipVideoButton.gameObject.SetActive(false);

            Debug.Log(
                "VideoGallery: Skip Button hidden after video finished.",
                this
            );
        }

        // -----------------------------------------------------
        // HIDE VIDEO PANEL
        // -----------------------------------------------------

        if (videoPlayerPanel != null)
        {
            videoPlayerPanel.SetActive(false);

            Debug.Log(
                "VideoGallery: Video Player Panel hidden after video finished.",
                this
            );
        }

        // -----------------------------------------------------
        // CLEAR RENDER TEXTURE
        // -----------------------------------------------------

        ClearRenderTexture();

        Debug.Log(
            "VideoGallery: Render Texture cleared.",
            this
        );

        // -----------------------------------------------------
        // RESUME BACKGROUND MUSIC
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
        {
            Debug.Log(
                "VideoGallery: Resuming background music after video finished.",
                this
            );

            AudioManager.Instance.ResumeMusic();
        }
        else
        {
            Debug.LogWarning(
                "VideoGallery: AudioManager.Instance is NULL. " +
                "Music cannot be resumed.",
                this
            );
        }

        Debug.Log(
            "========== VIDEO GALLERY VIDEO FINISHED END ==========",
            this
        );
    }

    // =========================================================
    // CLEAR RENDER TEXTURE
    // =========================================================

    private void ClearRenderTexture()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning(
                "VideoGallery: Cannot clear Render Texture. " +
                "VideoPlayer is NULL.",
                this
            );

            return;
        }

        if (videoPlayer.targetTexture == null)
        {
            Debug.LogWarning(
                "VideoGallery: VideoPlayer has no Target Texture.",
                this
            );

            return;
        }

        RenderTexture renderTexture =
            videoPlayer.targetTexture;

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active =
            renderTexture;

        GL.Clear(
            true,
            true,
            Color.clear
        );

        RenderTexture.active =
            previous;

        Debug.Log(
            "VideoGallery: Render Texture successfully cleared.",
            this
        );
    }

    // =========================================================
    // ON DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }
    }

    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }

        if (skipVideoButton != null)
        {
            skipVideoButton.onClick.RemoveListener(
                SkipVideo
            );
        }
    }
}