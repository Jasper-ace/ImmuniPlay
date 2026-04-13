using UnityEngine;

public class HandPointer : MonoBehaviour
{
    public float moveDistance = 40f; // how far it taps
    public float speed = 3f; // speed of animation
    public float scaleAmount = 0.05f; // bounce size

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Tap motion (down-up)
        float offset = Mathf.Sin(Time.time * speed) * moveDistance;
        transform.localPosition = startPos + new Vector3(0, -Mathf.Abs(offset), 0);

        // Bounce scale effect
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = new Vector3(scale, scale, 1);
    }
}