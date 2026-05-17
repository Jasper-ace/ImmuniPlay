using UnityEngine;

public class TouchToContinue : MonoBehaviour
{
    public SceneFade sceneFade;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            sceneFade.FadeToScene("Scene2");
        }
    }
}