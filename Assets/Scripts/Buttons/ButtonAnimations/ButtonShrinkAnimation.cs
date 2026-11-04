using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonShrinkAnimation : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Shrink Settings")]
    [Range(0.8f, 1f)]
    public float pressedScale = 0.92f;

    [Header("Animation Speed")]
    public float pressDuration = 0.08f;
    public float releaseDuration = 0.10f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateTo(originalScale * pressedScale, pressDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(originalScale, releaseDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(originalScale, releaseDuration);
    }

    private void AnimateTo(Vector3 targetScale, float duration)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleButton(targetScale, duration));
    }

    private System.Collections.IEnumerator ScaleButton(
        Vector3 targetScale,
        float duration
    )
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth ease-out
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t
            );

            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}