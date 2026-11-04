using UnityEngine;
using System.Collections;

public class DialoguePopAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private float startScale = 0.8f;

    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = transform.localScale;
    }

    public void PlayPop()
    {
        StopAllCoroutines();
        StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        float time = 0f;

        // Start small
        transform.localScale = normalScale * startScale;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // Smooth easing
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(
                normalScale * startScale,
                normalScale,
                t
            );

            yield return null;
        }

        transform.localScale = normalScale;
    }
}
