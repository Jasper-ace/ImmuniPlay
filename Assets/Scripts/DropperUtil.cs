using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pointer input that works with the old Input Manager AND the new Input System.
/// </summary>
public static class PointerInput
{
    public static bool Down
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }

    public static bool Held
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
            return false;
#else
            return Input.GetMouseButton(0);
#endif
        }
    }

    public static bool Up
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) return true;
            return false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }
    }

    public static Vector2 Position
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }
}

/// <summary>
/// Helpers that work for both UI (Canvas / Image) and 2D sprites (SpriteRenderer).
/// No colliders required.
/// </summary>
public static class DropperUtil
{
    public static Camera GetCamera(Transform t)
    {
        Canvas c = t.GetComponentInParent<Canvas>();
        if (c == null) return Camera.main;

        c = c.rootCanvas;
        if (c.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return c.worldCamera != null ? c.worldCamera : Camera.main;
    }

    /// <summary>Is the screen point over this object's Image or Sprite?</summary>
    public static bool IsOver(Component comp, Vector2 screen)
    {
        Camera cam = GetCamera(comp.transform);

        Graphic g = comp.GetComponentInChildren<Graphic>();
        if (g != null)
            return RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, screen, cam);

        SpriteRenderer sr = comp.GetComponentInChildren<SpriteRenderer>();
        if (sr == null || cam == null) return false;

        Bounds b = sr.bounds;
        float depth = Mathf.Abs(cam.transform.position.z - b.center.z);
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        w.z = b.center.z;
        return b.Contains(w);
    }

    /// <summary>Converts a screen point to a world position on the object's plane.</summary>
    public static Vector3 ScreenToWorld(Transform t, Vector2 screen)
    {
        Camera cam = GetCamera(t);

        Canvas c = t.GetComponentInParent<Canvas>();
        if (c != null)
        {
            Vector3 world;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)c.rootCanvas.transform, screen, cam, out world);
            return world;
        }

        float depth = Mathf.Abs(cam.transform.position.z - t.position.z);
        return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
    }

    public static Vector2 WorldToScreen(Transform t, Vector3 world)
    {
        return RectTransformUtility.WorldToScreenPoint(GetCamera(t), world);
    }
}