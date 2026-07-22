using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SmoothButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scaling")]
    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;
    public float scaleSpeed = 12f;

    [Header("Color Tinting")]
    public Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color hoverColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
    public Color pressColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public float colorSpeed = 12f;

    private UnityEngine.UI.Image targetImage;
    private Vector3 targetScale = Vector3.one;
    private Color targetColor;
    private bool isHovered = false;
    private bool isPressed = false;

    private void Awake()
    {
        targetImage = GetComponent<UnityEngine.UI.Image>();
        targetColor = normalColor;
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        isHovered = false;
        isPressed = false;
        targetScale = Vector3.one;
        targetColor = normalColor;
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    private void Update()
    {
        // Smoothly interpolate scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        // Smoothly interpolate color
        if (targetImage != null)
        {
            targetImage.color = Color.Lerp(targetImage.color, targetColor, Time.deltaTime * colorSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTargetState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTargetState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateTargetState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateTargetState();
    }

    private void UpdateTargetState()
    {
        if (isPressed)
        {
            targetScale = Vector3.one * pressScale;
            targetColor = pressColor;
        }
        else if (isHovered)
        {
            targetScale = Vector3.one * hoverScale;
            targetColor = hoverColor;
        }
        else
        {
            targetScale = Vector3.one;
            targetColor = normalColor;
        }
    }
}