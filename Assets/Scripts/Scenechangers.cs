using UnityEngine;

public class SceneChangers : MonoBehaviour
{
    [SerializeField] private string targetNextScene;
    [SerializeField] private GameObject fadeManagerGameObject;

    private SceneFade sceneFade;

    private void Start()
    {
        sceneFade = fadeManagerGameObject.GetComponent<SceneFade>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ChangeScene();
        }
    }

    private void ChangeScene()
    {
        if (sceneFade != null)
        {
            sceneFade.FadeToScene(targetNextScene);
        }
    }
}