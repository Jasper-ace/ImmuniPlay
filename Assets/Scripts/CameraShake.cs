using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        originalPos = transform.localPosition;
    }

    // 🔥 Public function to call
    public void Shake()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float duration = 0.3f;
        float magnitude = 0.3f; // ✅ adjusted for camera

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 🔄 Reset position
        transform.localPosition = originalPos;
    }
}