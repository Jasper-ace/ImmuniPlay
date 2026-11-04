using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the Gallery parent object. For each video, add one entry
/// in the list (Inspector) with the video's ID (matching the ID set on its
/// VideoViewTracker), the button GameObject that plays/opens the video, and
/// the locked placeholder GameObject (the grey box with the padlock icon).
///
/// - Not viewed  -> show lockedIcon, hide videoButton
/// - Viewed      -> show videoButton, hide lockedIcon
/// </summary>
public class GalleryButtonController : MonoBehaviour
{
    [Serializable]
    public class GalleryVideoEntry
    {
        [Tooltip("Must match the Video Id set on the matching VideoViewTracker.")]
        public string videoId;

        [Tooltip("The actual video thumbnail/button — shown once the video has been watched.")]
        public GameObject videoButton;

        [Tooltip("The locked placeholder (grey box + padlock) — shown until the video has been watched.")]
        public GameObject lockedIcon;
    }

    [SerializeField] private List<GalleryVideoEntry> videoEntries = new List<GalleryVideoEntry>();

    private void OnEnable()
    {
        RefreshGallery();
    }

    /// <summary>
    /// Call this whenever a video is watched (or when the Gallery screen
    /// opens) to refresh which buttons/lock icons are visible.
    /// </summary>
    public void RefreshGallery()
    {
        SaveManager sm = SaveManager.Instance;

        foreach (GalleryVideoEntry entry in videoEntries)
        {
            bool viewed = sm.IsVideoViewed(entry.videoId);

            if (entry.videoButton != null)
                entry.videoButton.SetActive(viewed);

            if (entry.lockedIcon != null)
                entry.lockedIcon.SetActive(!viewed);
        }
    }
}