using System.Collections;
using UnityEngine;

public class WrongItemFeedback : MonoBehaviour
{
    [SerializeField] private GameObject xObject;          // the X image
    [SerializeField] private GameObject arrowObject;      // the arrow to hide while X is showing
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float hideDuration = 0.15f;

    private Coroutine routine;
    private Vector3 originalScale;
    private bool arrowWasActive;

    void Awake()
    {
        originalScale = xObject.transform.localScale;
        xObject.SetActive(false);
    }

    public void ShowX()
    {
        // Remember if the arrow was on, but only on the first call
        // (so a quick second wrong drop doesn't overwrite it)
        if (routine == null && arrowObject != null)
            arrowWasActive = arrowObject.activeSelf;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        Transform t = xObject.transform;
        t.localScale = Vector3.zero;

        if (arrowObject != null) arrowObject.SetActive(false);   // arrow OFF
        xObject.SetActive(true);                                 // X ON

        // Pop in
        for (float time = 0; time < popDuration; time += Time.unscaledDeltaTime)
        {
            t.localScale = originalScale * EaseOutBack(time / popDuration);
            yield return null;
        }
        t.localScale = originalScale;

        yield return new WaitForSecondsRealtime(holdDuration);

        // Shrink away
        for (float time = 0; time < hideDuration; time += Time.unscaledDeltaTime)
        {
            t.localScale = originalScale * (1f - time / hideDuration);
            yield return null;
        }

        xObject.SetActive(false);                                // X OFF
        t.localScale = originalScale;

        if (arrowObject != null) arrowObject.SetActive(arrowWasActive);   // arrow back ON

        routine = null;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }
}