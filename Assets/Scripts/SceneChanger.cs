using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class SceneChanger : MonoBehaviour
{
    // Call this method from your Button's OnClick() in the Inspector
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Please enter a valid scene name.");
        }
    }
}