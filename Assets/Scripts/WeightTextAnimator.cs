using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WeightTextAnimator : MonoBehaviour
{
    public float startWeight = 0f;
    public float countDuration = 1.2f;      // seconds to count up
    public float punchScale = 0.35f;        // how big the bounce gets
    public float punchDuration = 0.5f;
    public Color flashColor = new Color(0.3f, 1f, 0.4f);
    public string suffix = " kg";

    private TMP_Text label;
    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine routine;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        baseScale = transform.localScale;
        baseColor = label.color;
    }

    public void Play(float target, Action onComplete = null)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(target, onComplete));
    }

    IEnumerator Animate(float target, Action onComplete)
    {
        label.color = flashColor;

        // 1. count up, fast at first then slowing down like a real scale settling
        float t = 0f;
        while (t < countDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / countDuration);
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            SetValue(Mathf.Lerp(startWeight, target, eased));
            yield return null;
        }
        SetValue(target);

        // 2. bounce and fade the color back
        t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / punchDuration);
            float s = 1f + punchScale * Mathf.Sin(k * Mathf.PI * 2f) * (1f - k);
            transform.localScale = baseScale * s;
            label.color = Color.Lerp(flashColor, baseColor, k);
            yield return null;
        }

        transform.localScale = baseScale;
        label.color = baseColor;

        onComplete?.Invoke();               // animation finished
    }

    void SetValue(float v)
    {
        label.text = v.ToString("0.00", CultureInfo.InvariantCulture) + suffix;
    }
}