using UnityEngine;
using System.Collections;

public class BusMove : MonoBehaviour
{
    [Header("Bus")]
    public RectTransform stopPoint;
    public float speed = 500f;

    [Header("Animation")]
    [Tooltip("How much the bus eases in/out of its movement (higher = snappier, lower = floatier).")]
    public float easeSharpness = 3f;

    [Tooltip("How much the bus squashes when it stops (0 = no bounce).")]
    public float stopBounceAmount = 0.15f;

    [Tooltip("How long the stop bounce takes to settle.")]
    public float stopBounceDuration = 0.25f;

    [Header("UI")]
    public GameObject couple;
    public GameObject walkButton;

    private RectTransform bus;
    private Vector3 baseScale;

    void Start()
    {
        bus = GetComponent<RectTransform>();
        baseScale = bus.localScale;

        // Hide couple and button at start
        if (couple != null)
            couple.SetActive(false);

        if (walkButton != null)
            walkButton.SetActive(false);

        StartCoroutine(MoveBusSequence());
    }

    IEnumerator MoveBusSequence()
    {
        // Move bus to stop point, easing out (starts fast, slows into the stop)
        yield return MoveEased(stopPoint.anchoredPosition, easeOut: true);

        // Little squash-bounce to sell the stop
        yield return StopBounce();

        // Stop for 1 second
        yield return new WaitForSeconds(1f);

        // Show couple and WALK button
        if (couple != null)
            couple.SetActive(true);

        if (walkButton != null)
            walkButton.SetActive(true);

        // Bus leaves to the left, easing in (starts slow, speeds up as it exits)
        Vector2 exitPoint = new Vector2(-2000f, bus.anchoredPosition.y);
        yield return MoveEased(exitPoint, easeOut: false);

        // Hide bus after it leaves screen
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Moves the bus toward targetPos with easing instead of constant speed.
    /// easeOut = true  -> starts fast, decelerates into the target (good for arriving/stopping).
    /// easeOut = false -> starts slow, accelerates away from the start (good for leaving).
    /// </summary>
    private IEnumerator MoveEased(Vector2 targetPos, bool easeOut)
    {
        Vector2 startPos = bus.anchoredPosition;
        float distance = Vector2.Distance(startPos, targetPos);

        // Estimate a duration from distance/speed so easeSharpness stays consistent
        // regardless of how far the bus needs to travel.
        float duration = Mathf.Max(distance / speed, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out: fast start, slow finish. Ease-in: slow start, fast finish.
            float easedT = easeOut
                ? 1f - Mathf.Pow(1f - t, easeSharpness)
                : Mathf.Pow(t, easeSharpness);

            bus.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        bus.anchoredPosition = targetPos;
    }

    /// <summary>
    /// Quick squash-and-recover scale animation to sell the impact of stopping.
    /// Squashes horizontally (as if compressing from momentum), then springs back.
    /// </summary>
    private IEnumerator StopBounce()
    {
        if (stopBounceAmount <= 0f)
            yield break;

        float elapsed = 0f;
        Vector3 squashedScale = new Vector3(
            baseScale.x * (1f + stopBounceAmount),
            baseScale.y * (1f - stopBounceAmount * 0.5f),
            baseScale.z);

        // Squash
        float squashTime = stopBounceDuration * 0.4f;
        while (elapsed < squashTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / squashTime);
            bus.localScale = Vector3.Lerp(baseScale, squashedScale, t);
            yield return null;
        }

        // Recover back to normal, slightly overshooting for a springy feel
        elapsed = 0f;
        float recoverTime = stopBounceDuration * 0.6f;
        while (elapsed < recoverTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / recoverTime);
            float overshoot = Mathf.Sin(t * Mathf.PI) * 0.05f; // small extra wobble
            bus.localScale = Vector3.Lerp(squashedScale, baseScale, t) + Vector3.one * overshoot;
            yield return null;
        }

        bus.localScale = baseScale;
    }
}