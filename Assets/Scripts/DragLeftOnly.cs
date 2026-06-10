using UnityEngine;
using UnityEngine.EventSystems;

public class DragLeftOnly : MonoBehaviour, IDragHandler
{
    public RectTransform rectTransform;
    public SceneFade sceneFade;

    public string nextScene = "Scene3";

    float startX;

    bool hasTriggered = false;

    void Start()
    {
        startX = rectTransform.anchoredPosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasTriggered)
            return;

        Vector2 pos = rectTransform.anchoredPosition;

        // Move only LEFT
        pos.x += eventData.delta.x;

        // Prevent moving right
        pos.x = Mathf.Clamp(pos.x, -600f, startX);

        rectTransform.anchoredPosition = pos;

        // Trigger only ONCE
        if (pos.x <= -200f)
        {
            hasTriggered = true;

            sceneFade.FadeToScene(nextScene);
        }
    }
}