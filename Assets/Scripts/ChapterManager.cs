using UnityEngine;
using UnityEngine.UI;

public class ChapterManager : MonoBehaviour
{
    public Button chapter2Button;
    public Button chapter3Button;
    public Button chapter4Button;
    public Button chapter5Button;

    void Start()
    {
        // Lock/unlock chapter buttons based on what has been completed.
        // SaveManager loads the save file automatically on Awake,
        // so the data is ready here.
        chapter2Button.interactable = SaveManager.Instance.IsChapterCompleted("Chapter1");
        chapter3Button.interactable = SaveManager.Instance.IsChapterCompleted("Chapter2");
        chapter4Button.interactable = SaveManager.Instance.IsChapterCompleted("Chapter3");
        chapter5Button.interactable = SaveManager.Instance.IsChapterCompleted("Chapter4");
    }

    // -----------------------------------------------------------------------
    //  Call these from your game when a chapter ends
    // -----------------------------------------------------------------------

    public void CompleteChapter1()
    {
        SaveManager.Instance.SetChapterCompleted("Chapter1");
    }

    public void CompleteChapter2()
    {
        SaveManager.Instance.SetChapterCompleted("Chapter2");
    }

    public void CompleteChapter3()
    {
        SaveManager.Instance.SetChapterCompleted("Chapter3");
    }

    public void CompleteChapter4()
    {
        SaveManager.Instance.SetChapterCompleted("Chapter4");
    }

    public void CompleteChapter5()
    {
        SaveManager.Instance.SetChapterCompleted("Chapter5");
    }
}
