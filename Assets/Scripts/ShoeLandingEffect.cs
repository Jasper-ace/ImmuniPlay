using System.Collections;
using UnityEngine;

public class ShoeLandingEffect : MonoBehaviour
{
    [Header("Animation")]
    public float popScale = 1.15f;
    public float popDuration = 0.12f;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayLandingEffect()
    {
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    IEnumerator PopAnimation()
    {
        Vector3 enlarged = originalScale * popScale;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector3.Lerp(
                originalScale,
                enlarged,
                timer / popDuration);

            yield return null;
        }

        timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector3.Lerp(
                enlarged,
                originalScale,
                timer / popDuration);

            yield return null;
        }

        transform.localScale = originalScale;
    }
}