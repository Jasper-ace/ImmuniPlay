using UnityEngine;

public class ShowWeightOnTap : MonoBehaviour
{
    public GameObject weightObject;

    private bool shown = false;

    void Update()
    {
        if (shown) return;

        // Mouse click (PC)
        if (Input.GetMouseButtonDown(0))
        {
            ShowWeight();
        }

        // Touch (Mobile)
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ShowWeight();
        }
    }

    void ShowWeight()
    {
        weightObject.SetActive(true);
        shown = true;
    }
}