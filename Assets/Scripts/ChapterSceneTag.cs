using UnityEngine;

/// <summary>
/// Attach this to the FIRST scene of each chapter.
/// It tells SaveManager which chapter is now active so that
/// per-chapter scene progress is tracked correctly — even when
/// the game auto-transitions between chapters without going
/// through the Chapters hub.
///
/// SETUP:
///   - Add this component to ONE GameObject in Scene1  → set Chapter Name = "Chapter1"
///   - Add this component to ONE GameObject in Scene13 → set Chapter Name = "Chapter2"
///   - And so on for each chapter's first scene.
/// </summary>
public class ChapterSceneTag : MonoBehaviour
{
    [Tooltip("Must match exactly: Chapter1, Chapter2, Chapter3, Chapter4, or Chapter5")]
    public string chapterName = "Chapter1";

    private void Awake()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetActiveChapter(chapterName);
            Debug.Log("[ChapterSceneTag] Chapter started: " + chapterName);
        }
    }
}
