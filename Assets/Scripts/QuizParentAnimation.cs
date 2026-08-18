using UnityEngine;
using System.Collections;

public class QuizParentAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float duration = 0.35f;
    public float startScale = 0.8f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PopUp());
    }

    // Call this instead of gameObject.SetActive(false)
    public void Close()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PopOut());
    }

    private IEnumerator PopUp()
    {
        transform.localScale = originalScale * startScale;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);

            // Ease Out Back
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            float easedT =
                1f + c3 * Mathf.Pow(t - 1f, 3f)
                + c1 * Mathf.Pow(t - 1f, 2f);

            transform.localScale = Vector3.LerpUnclamped(
                originalScale * startScale,
                originalScale,
                easedT
            );

            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator PopOut()
    {
        Vector3 startScaleVector = transform.localScale;
        Vector3 endScaleVector = originalScale * startScale;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);

            // Ease In Back
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            float easedT =
                c3 * t * t * t
                - c1 * t * t;

            transform.localScale = Vector3.LerpUnclamped(
                startScaleVector,
                endScaleVector,
                easedT
            );

            yield return null;
        }

        transform.localScale = endScaleVector;

        // Disable AFTER animation finishes
        gameObject.SetActive(false);
    }
}