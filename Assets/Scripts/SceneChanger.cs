using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    /// <summary>
    /// Call this from a Button's OnClick() in the Inspector.
    /// The destination scene name is saved to save.json before loading.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneChanger] Scene name is empty! Please enter a valid scene name.");
            return;
        }

        // Auto-save the scene we are about to enter
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveCurrentScene(sceneName);

        SceneManager.LoadScene(sceneName);
    }
}
