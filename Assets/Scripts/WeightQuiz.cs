using UnityEngine;

public class WeightQuiz : MonoBehaviour
{
    public RectTransform selector;

    public float minGreenX = 200f;
    public float maxGreenX = 350f;

    void Update()
    {
        float x = selector.anchoredPosition.x;

        if (x >= minGreenX && x <= maxGreenX)
        {
            Debug.Log("Correct!");

            // Show success popup
            // Fade to next scene
        }
    }
}