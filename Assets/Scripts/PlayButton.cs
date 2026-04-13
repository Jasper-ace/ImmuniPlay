using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public Animator transition;

    public void PlayGame()
    {
        transition.SetTrigger("Play");
        Invoke("LoadScene", 1f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}