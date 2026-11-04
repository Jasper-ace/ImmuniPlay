using UnityEngine;
using System.Collections;

public class FadeInTransition : TransitionBase
{
    [Header("Fade Target")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float duration = 0.5f;

    public override IEnumerator Play()
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("FadeInTransition: CanvasGroup is not assigned.");
            yield break;
        }

        float elapsed = 0f;

        canvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}