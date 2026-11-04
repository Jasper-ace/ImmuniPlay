using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    // ImmuniPlay's fixed aspect ratio: 1920 x 1080
    private const float TARGET_ASPECT = 16f / 9f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        ApplyAspectRatio();
    }

    private void Update()
    {
        // Recalculate only when the screen resolution/orientation changes
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            ApplyAspectRatio();
        }
    }

    private void ApplyAspectRatio()
    {
        float screenAspect =
            (float)Screen.width / Screen.height;

        Rect viewportRect;

        if (screenAspect > TARGET_ASPECT)
        {
            // ==========================================
            // SCREEN IS WIDER THAN 16:9
            // Black bars on LEFT and RIGHT
            // ==========================================

            float width = TARGET_ASPECT / screenAspect;

            viewportRect = new Rect(
                (1f - width) / 2f, // X
                0f,                // Y
                width,             // Width
                1f                 // Height
            );
        }
        else
        {
            // ==========================================
            // SCREEN IS TALLER/NARROWER THAN 16:9
            // Black bars on TOP and BOTTOM
            // ==========================================

            float height = screenAspect / TARGET_ASPECT;

            viewportRect = new Rect(
                0f,                 // X
                (1f - height) / 2f, // Y
                1f,                 // Width
                height              // Height
            );
        }

        cam.rect = viewportRect;
    }
}