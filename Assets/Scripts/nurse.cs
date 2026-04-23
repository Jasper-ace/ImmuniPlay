using UnityEngine;
using System.Collections;

public class NurseAnimation : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatAmount = 5f;

    [Header("Breathing Scale")]
    public float scaleSpeed = 2f;
    public float scaleAmount = 0.02f;

    [Header("Blink Settings")]
    public GameObject eyes; // Assign eyes object (optional)
    public float minBlinkTime = 2f;
    public float maxBlinkTime = 5f;

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;

        // Start blinking if eyes assigned
        if (eyes != null)
        {
            StartCoroutine(Blink());
        }
    }

    void Update()
    {
        AnimateFloating();
        AnimateBreathing();
    }

    void AnimateFloating()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = startPos + new Vector3(0, newY, 0);
    }

    void AnimateBreathing()
    {
        float scale = 1 + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = startScale * scale;
    }

    IEnumerator Blink()
    {
        while (true)
        {
            float waitTime = Random.Range(minBlinkTime, maxBlinkTime);
            yield return new WaitForSeconds(waitTime);

            if (eyes != null)
            {
                eyes.SetActive(false);
                yield return new WaitForSeconds(0.1f);
                eyes.SetActive(true);
            }
        }
    }
}