using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [Header("Scene Manager")]
    public SceneFade sceneManager;

    [Header("Scene Settings")]
    public string nextSceneName;

    public void ChangeScene()
    {
        if (sceneManager == null)
        {
            Debug.LogError("Scene Manager is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Next Scene Name is empty!");
            return;
        }

        sceneManager.FadeToScene(nextSceneName);
    }
}