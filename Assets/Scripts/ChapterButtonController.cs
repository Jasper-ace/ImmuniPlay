using UnityEngine;

/// <summary>
/// Enables/disables the "check" and "lock" buttons under each Chapter object
/// based on chapter completion state.
///
/// Rule (per chapter N, N > 1):
/// - Previous chapter NOT completed  -> show "lock", hide "check"   (locked)
/// - Previous chapter completed,
///   this chapter NOT yet completed  -> hide both                  (in progress)
/// - This chapter completed          -> show "check", hide "lock"  (completed)
///
/// Chapter 1 has no lock object (always accessible), so it only ever
/// shows "check" once chapter1Completed is true, otherwise nothing.
///
/// Attach this to a GameObject in the scene (e.g. the menu/canvas) and drag
/// each button GameObject into the matching slot in the Inspector.
/// </summary>
public class ChapterButtonController : MonoBehaviour
{
    [Header("Chapter 1 (no lock)")]
    [SerializeField] private GameObject chapter1Check;

    [Header("Chapter 2")]
    [SerializeField] private GameObject chapter2Check;
    [SerializeField] private GameObject chapter2Lock;

    [Header("Chapter 3")]
    [SerializeField] private GameObject chapter3Check;
    [SerializeField] private GameObject chapter3Lock;

    [Header("Chapter 4")]
    [SerializeField] private GameObject chapter4Check;
    [SerializeField] private GameObject chapter4Lock;

    [Header("Chapter 5")]
    [SerializeField] private GameObject chapter5Check;
    [SerializeField] private GameObject chapter5Lock;

    private void Start()
    {
        RefreshButtons();
    }

    /// <summary>
    /// Call this any time chapter progress changes (e.g. right after
    /// SaveManager.Instance.CompleteChapter(x)) to refresh the button states.
    /// </summary>
    public void RefreshButtons()
    {
        SaveManager sm = SaveManager.Instance;

        // Chapter 1 has no lock object — it's always accessible.
        // "previous completed" is treated as true since there's no prior chapter.
        SetTriState(chapter1Check, null, previousCompleted: true, thisCompleted: sm.IsChapterCompleted("Chapter1"));

        SetTriState(chapter2Check, chapter2Lock, sm.IsChapterCompleted("Chapter1"), sm.IsChapterCompleted("Chapter2"));
        SetTriState(chapter3Check, chapter3Lock, sm.IsChapterCompleted("Chapter2"), sm.IsChapterCompleted("Chapter3"));
        SetTriState(chapter4Check, chapter4Lock, sm.IsChapterCompleted("Chapter3"), sm.IsChapterCompleted("Chapter4"));
        SetTriState(chapter5Check, chapter5Lock, sm.IsChapterCompleted("Chapter4"), sm.IsChapterCompleted("Chapter5"));
    }

    /// <summary>
    /// Three possible states for a chapter's icons:
    /// - Locked (previous chapter not completed): show lock, hide check.
    /// - In progress (previous completed, this one not yet): hide both.
    /// - Completed (this chapter completed): show check, hide lock.
    /// </summary>
    private void SetTriState(GameObject checkBtn, GameObject lockBtn, bool previousCompleted, bool thisCompleted)
    {
        if (!previousCompleted)
        {
            // Locked
            SetActive(checkBtn, false);
            SetActive(lockBtn, true);
        }
        else if (thisCompleted)
        {
            // Completed
            SetActive(checkBtn, true);
            SetActive(lockBtn, false);
        }
        else
        {
            // In progress — neither icon shows
            SetActive(checkBtn, false);
            SetActive(lockBtn, false);
        }
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }
}