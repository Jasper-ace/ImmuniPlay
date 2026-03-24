using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ✅ IMPORTANT


public class ButtonHandler : MonoBehaviour
{
    // ✅ Correct answers
    public string[] correctAnswers = {
        "Spoiled Burger",
        "Moldy Closet",
        "Dirty Shoes",
        "Dusty Keyboard"
    };

    // 🎥 Camera for shake
    public Camera mainCamera;

    // 🎨 Animation settings
    [Header("Animation Settings")]
    public float clickScaleAmount = 0.9f;
    public float clickDuration = 0.1f;
    public float colorTransitionDuration = 0.3f;
    public AnimationCurve clickCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Camera Shake Settings")]
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeMagnitude = 0.3f;
    public bool enableCameraShake = true;

    // 🔘 Button click
    public void OnClick(Button btn)
    {
        // 🎬 Start click animation
        StartCoroutine(AnimateButtonClick(btn));
    }

    // 🎬 Button click animation
    IEnumerator AnimateButtonClick(Button btn)
    {
        // ✅ Get TMP text
        TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();

        // ✅ Get button image
        Image img = btn.GetComponent<Image>();

        // 🛑 Safety check
        if (txt == null || img == null)
        {
            Debug.LogError("Missing TextMeshPro or Image on button!");
            yield break;
        }

        // 🎯 Disable button during animation
        btn.interactable = false;

        // 📏 Store original scale
        Vector3 originalScale = btn.transform.localScale;
        Color originalColor = img.color;

        // � Click scale animation
        yield return StartCoroutine(ScaleAnimation(btn.transform, originalScale, clickScaleAmount, clickDuration));

        // 🔍 Check answer logic
        string buttonText = txt.text.Trim().ToLower();
        bool isCorrect = false;

        foreach (string answer in correctAnswers)
        {
            if (buttonText == answer.Trim().ToLower())
            {
                isCorrect = true;
                break;
            }
        }

        // 🎯 Result with animation
        if (isCorrect)
        {
            yield return StartCoroutine(CorrectAnswerAnimation(img, txt, originalColor));
        }
        else
        {
            yield return StartCoroutine(WrongAnswerAnimation(img, txt, originalColor));
        }

        // 🔄 Re-enable button
        btn.interactable = true;
    }

    // ✅ Correct answer animation
    IEnumerator CorrectAnswerAnimation(Image img, TMP_Text txt, Color originalColor)
    {
        // 🎨 Smooth color transition to green
        yield return StartCoroutine(ColorTransition(img, originalColor, Color.green, colorTransitionDuration));
        
        // ✨ Success pulse effect
        yield return StartCoroutine(PulseAnimation(img.transform, 1.1f, 0.2f));
        
        // 🎉 Optional: Add particle effect or sound here
        Debug.Log("Correct Answer! ✅");
    }

    // ❌ Wrong answer animation
    IEnumerator WrongAnswerAnimation(Image img, TMP_Text txt, Color originalColor)
    {
        // 🎨 Smooth color transition to red
        yield return StartCoroutine(ColorTransition(img, originalColor, Color.red, colorTransitionDuration));
        
        // 📳 Shake effect on button
        yield return StartCoroutine(ShakeAnimation(img.transform, 0.1f, 0.3f));
        
        // 📳 CAMERA SHAKE for wrong answers
        if (mainCamera != null && enableCameraShake)
        {
            StartCoroutine(ShakeCamera());
            Debug.Log("Wrong Answer! Camera Shake Triggered! ❌📳");
        }

        yield return new WaitForSeconds(0.2f);

        // 🎨 Smooth color transition back to original
        yield return StartCoroutine(ColorTransition(img, Color.red, originalColor, colorTransitionDuration));
    }

    // 📏 Scale animation
    IEnumerator ScaleAnimation(Transform target, Vector3 originalScale, float scaleAmount, float duration)
    {
        Vector3 targetScale = originalScale * scaleAmount;
        float elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float curveValue = clickCurve.Evaluate(progress);
            target.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        // Scale back up
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float curveValue = clickCurve.Evaluate(progress);
            target.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    // 🎨 Color transition animation
    IEnumerator ColorTransition(Image img, Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            img.color = Color.Lerp(fromColor, toColor, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        img.color = toColor;
    }

    // ✨ Pulse animation for correct answers
    IEnumerator PulseAnimation(Transform target, float pulseScale, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * pulseScale;
        float elapsed = 0f;

        // Pulse up
        while (elapsed < duration / 2)
        {
            float progress = (elapsed / (duration / 2));
            target.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        // Pulse down
        while (elapsed < duration / 2)
        {
            float progress = (elapsed / (duration / 2));
            target.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    // 📳 Shake animation for wrong answers
    IEnumerator ShakeAnimation(Transform target, float magnitude, float duration)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            target.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    // ❌ Legacy wrong answer effect (kept for compatibility)
    IEnumerator WrongEffect(Image img)
    {
        img.color = Color.red;

        // 📳 Shake camera
        if (mainCamera != null)
        {
            StartCoroutine(ShakeCamera());
        }

        yield return new WaitForSeconds(0.5f);

        img.color = Color.white;
    }

    // 📳 Camera shake
    IEnumerator ShakeCamera()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not assigned for shake effect!");
            yield break;
        }

        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        Debug.Log($"🎬 Camera shake started! Duration: {cameraShakeDuration}s, Magnitude: {cameraShakeMagnitude}");

        while (elapsed < cameraShakeDuration)
        {
            // Create random shake offset
            float x = Random.Range(-cameraShakeMagnitude, cameraShakeMagnitude);
            float y = Random.Range(-cameraShakeMagnitude, cameraShakeMagnitude);

            // Apply shake with decreasing intensity over time
            float intensity = 1f - (elapsed / cameraShakeDuration);
            
            mainCamera.transform.position = new Vector3(
                originalPos.x + (x * intensity),
                originalPos.y + (y * intensity),
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset camera to original position
        mainCamera.transform.position = originalPos;
        Debug.Log("📳 Camera shake completed!");
    }
}