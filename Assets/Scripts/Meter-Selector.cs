using UnityEngine;

public class MeterSelector : MonoBehaviour
{
    public RectTransform selector;

    public GreenBarTimer timer;

    public float speed = 300f;

    public float minY = -250f;
    public float maxY = 250f;

    private bool movingUp = true;

    private bool stopped = false;

    void Update()
    {
        // STOP if tapped
        if (Input.GetMouseButtonDown(0))
        {
            stopped = true;
        }

        // STOP if timer ends
        if (timer != null && timer.IsTimeUp())
        {
            stopped = true;
        }

        if (stopped)
            return;

        Vector2 pos = selector.anchoredPosition;

        // MOVE UP
        if (movingUp)
        {
            pos.y += speed * Time.deltaTime;

            if (pos.y >= maxY)
            {
                pos.y = maxY;

                movingUp = false;
            }
        }
        // MOVE DOWN
        else
        {
            pos.y -= speed * Time.deltaTime;

            if (pos.y <= minY)
            {
                pos.y = minY;

                movingUp = true;
            }
        }

        selector.anchoredPosition = pos;
    }
}