using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows a chat bubble notification after a delay, with a spawn (pop-in) animation
/// and a typewriter text effect.
/// 
/// Setup:
/// - Attach this to your chat bubble root GameObject (the panel/image that contains the text).
/// - Assign the TextMeshProUGUI field to the text component inside the bubble.
/// - Make sure the bubble root is INACTIVE in the scene by default (or it will flash before the delay).
/// </summary>
public class ChatBubbleNotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubbleRoot;      // The chat bubble object (parent panel/image)
    [SerializeField] private TextMeshProUGUI tmpText;    // The TMP text inside the bubble

    [Header("Timing")]
    [SerializeField] private float delayBeforeShow = 1.5f;
    [SerializeField] private float typewriterSpeed = 0.03f; // seconds per character

    [Header("Content")]
    [SerializeField] private string message = "DING...";

    [Header("Loop (dot cycle)")]
    [SerializeField] private bool loop = true;
    [SerializeField] private string[] loopMessages = { "ding...", "ding..", "ding..", "ding.." };
    [SerializeField] private float loopStepDelay = 0.3f; // time each dot-state stays on screen

    [Header("Spawn Animation")]
    [SerializeField] private float spawnDuration = 0.25f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 spawnStartScale = Vector3.zero;
    [SerializeField] private Vector3 spawnEndScale = Vector3.one;

    private void Awake()
    {
        if (bubbleRoot == null) bubbleRoot = gameObject;
        bubbleRoot.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        // Wait before showing anything
        yield return new WaitForSeconds(delayBeforeShow);

        // Activate and reset state
        bubbleRoot.SetActive(true);
        bubbleRoot.transform.localScale = spawnStartScale;

        if (tmpText != null)
            tmpText.text = "";

        // Play spawn (pop-in) animation
        yield return StartCoroutine(SpawnAnimation());

        // Play typewriter effect for the initial message
        if (tmpText != null)
            yield return StartCoroutine(TypewriterEffect(message));

        // Loop through the dot-cycle messages (e.g. ding... / ding.. / ding.. / ding..)
        if (loop && tmpText != null && loopMessages.Length > 0)
        {
            while (true)
            {
                foreach (string step in loopMessages)
                {
                    tmpText.text = step;
                    yield return new WaitForSeconds(loopStepDelay);
                }
            }
        }
    }

    private IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnDuration);
            float curved = spawnCurve.Evaluate(t);

            bubbleRoot.transform.localScale = Vector3.LerpUnclamped(spawnStartScale, spawnEndScale, curved);
            yield return null;
        }

        bubbleRoot.transform.localScale = spawnEndScale;
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        tmpText.text = "";

        foreach (char c in fullText)
        {
            tmpText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    /// <summary>
    /// Call this externally to trigger the bubble on demand instead of on Start().
    /// </summary>
    public void TriggerBubble(string newMessage = null)
    {
        if (!string.IsNullOrEmpty(newMessage))
            message = newMessage;

        StopAllCoroutines();
        StartCoroutine(ShowSequence());
    }

    /// <summary>
    /// Stops the loop and hides the bubble. Call this when the notification should disappear
    /// (e.g. player opens the phone/message).
    /// </summary>
    public void HideBubble()
    {
        StopAllCoroutines();
        bubbleRoot.SetActive(false);
    }
}