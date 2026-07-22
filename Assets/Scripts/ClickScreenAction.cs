using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickScreenAction : MonoBehaviour
{
    [Header("Parent Transition")]
    public GameObject currentParent;
    public GameObject nextParent;

    [Header("Scene Transition (Optional)")]
    public bool loadScene = false;
    public string sceneName;

    [Header("Fade (Optional)")]
    public SceneFade fadeManager;

    private bool clicked = false;

    void Update()
    {
        if (clicked) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Ignore clicks on UI Buttons
            if (EventSystem.current != null)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.GetComponentInParent<Button>() != null)
                        return;
                }
            }

            clicked = true;

            // Load Scene
            if (loadScene)
            {
                if (fadeManager != null)
                    fadeManager.FadeToScene(sceneName);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

                return;
            }

            // Change Parent
            if (currentParent != null)
                currentParent.SetActive(false);

            if (nextParent != null)
                nextParent.SetActive(true);
        }
    }
}