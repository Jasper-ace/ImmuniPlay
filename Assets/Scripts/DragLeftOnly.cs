using UnityEngine;
using UnityEngine.EventSystems;

public class DragLeftOnly : MonoBehaviour, IDragHandler
{
    public RectTransform rectTransform;
    public SceneFade sceneFade;

    public string nextScene = "Scene3";

    float startX;

    void Start()
    {
        startX = rectTransform.anchoredPosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = rectTransform.anchoredPosition;

        // Move only LEFT
        pos.x += eventData.delta.x;

        // Prevent moving right
        pos.x = Mathf.Clamp(pos.x, -600f, startX);

        rectTransform.anchoredPosition = pos;

        // If dragged far enough left
        if (pos.x <= -500f)
        {
            sceneFade.FadeToScene(nextScene);
        }
    }
}