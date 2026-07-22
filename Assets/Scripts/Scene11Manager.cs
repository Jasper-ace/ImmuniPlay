using UnityEngine;

public class Scene11Manager : MonoBehaviour
{
    [Header("Transition")]
    public SceneFade fadeManager;

    public void CompleteChapter()
    {
        // Mark Chapter 1 as completed
        PlayerPrefs.SetInt("Chapter1Done", 1);
        PlayerPrefs.Save();

        // Fade to Chapters scene
        if (fadeManager != null)
        {
            fadeManager.FadeToScene("Chapters");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Chapters");
        }
    }
}
