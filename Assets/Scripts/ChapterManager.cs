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
        // Chapter 2
        chapter2Button.interactable =
            PlayerPrefs.GetInt("Chapter1Done", 0) == 1;

        // Chapter 3
        chapter3Button.interactable =
            PlayerPrefs.GetInt("Chapter2Done", 0) == 1;

        // Chapter 4
        chapter4Button.interactable =
            PlayerPrefs.GetInt("Chapter3Done", 0) == 1;

        // Chapter 5
        chapter5Button.interactable =
            PlayerPrefs.GetInt("Chapter4Done", 0) == 1;
    }

    public void CompleteChapter1()
    {
        PlayerPrefs.SetInt("Chapter1Done", 1);
        PlayerPrefs.Save();
    }

    public void CompleteChapter2()
    {
        PlayerPrefs.SetInt("Chapter2Done", 1);
        PlayerPrefs.Save();
    }

    public void CompleteChapter3()
    {
        PlayerPrefs.SetInt("Chapter3Done", 1);
        PlayerPrefs.Save();
    }

    public void CompleteChapter4()
    {
        PlayerPrefs.SetInt("Chapter4Done", 1);
        PlayerPrefs.Save();
    }
}