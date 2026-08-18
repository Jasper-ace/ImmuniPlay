using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Attach this to the "Pump" GameObject. It needs an Image component
// (with "Raycast Target" checked) so it can receive clicks.
public class PressureGauge : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("PUMP IMAGE SWAP")]
    [Tooltip("The Image component showing the pump (auto-found on this GameObject if left empty).")]
    [SerializeField] private Image pumpImage;

    [Tooltip("Drag the 'not pressed' sprite here.")]
    [SerializeField] private Sprite notPressedSprite;

    [Tooltip("Drag the 'pressed' sprite here.")]
    [SerializeField] private Sprite pressedSprite;

    [Header("GAUGE NEEDLE")]
    [Tooltip("The needle's RectTransform - it will be rotated around its pivot.")]
    [SerializeField] private RectTransform needle;

    [Tooltip("Needle's local Z angle at a reading of 0 (its resting/starting pose).")]
    [SerializeField] private float needleAngleAtZero = 90f;

    [Tooltip("Exact needle Z angle to animate to after each click, in order. Must have at least Clicks To Reach Target entries, e.g. [-84, -129, -180].")]
    [SerializeField] private float[] clickAngles = new float[] { -84f, -129f, -180f };

    [Tooltip("The highest number printed on the gauge face (matches your dial artwork, e.g. 300).")]
    [SerializeField] private float dialMaxValue = 300f;

    [Tooltip("Seconds it takes the needle to smoothly sweep from its current angle to the new one per pump.")]
    [SerializeField] private float needleMoveDuration = 0.4f;

    [Header("TARGET PRESSURE")]
    [Tooltip("The reading the needle should reach, e.g. 160.")]
    [SerializeField] private float targetPressure = 160f;

    [Tooltip("Number of pump clicks needed to reach Target Pressure.")]
    [SerializeField] private int clicksToReachTarget = 3;

    [Header("REVEAL ON TARGET REACHED")]
    [Tooltip("Optional. Drag a GameObject here to enable it once the target pressure is reached (e.g. after 3 clicks). The pump itself is NOT reparented - it stays exactly where it is.")]
    [SerializeField] private GameObject objectToEnable;

    [Header("EVENTS")]
    [Tooltip("Fired once, the moment the target pressure is reached.")]
    [SerializeField] private UnityEvent onTargetReached;

    [Tooltip("Fired on every click, passing the current reading (0..targetPressure).")]
    [SerializeField] private UnityEvent<float> onPump;

    [Header("DIALOGUE BUBBLE")]
    [Tooltip("The chat bubble GameObject to show/hide (e.g. a panel with a TMP text inside).")]
    [SerializeField] private GameObject dialogueBubble;

    [SerializeField] private TMP_Text dialogueText;

    [TextArea]
    [SerializeField] private string instructionMessage = "Tap the pump 3 times to inflate the cuff.";

    [Tooltip("Seconds between each typed character. Lower = faster typing.")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Tooltip("Seconds to wait after the text finishes typing (or is skipped) before hiding the bubble. Set to 0 to hide immediately.")]
    [SerializeField] private float autoHideDelay = 1.5f;

    private Coroutine typingCoroutine;
    private string currentFullMessage;
    private bool isTyping;
    private bool canInteract = false;

    private int clickCount;
    private float currentPressure;
    private bool targetReached;
    private Coroutine needleCoroutine;

    private void Awake()
    {
        if (pumpImage == null)
        {
            pumpImage = GetComponent<Image>();
        }

        if (pumpImage != null && notPressedSprite != null)
        {
            pumpImage.sprite = notPressedSprite;
        }

        // Force the needle to its known "reading = 0" pose so the very
        // first click always sweeps from a consistent starting angle,
        // regardless of whatever rotation was left on it in the Editor.
        if (needle != null)
        {
            needle.localEulerAngles = new Vector3(0f, 0f, needleAngleAtZero);
        }
    }

    private void Start()
    {
        ShowDialogue(instructionMessage);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (isTyping)
        {
            CompleteTyping();
        }
        else if (dialogueBubble != null && dialogueBubble.activeSelf)
        {
            CancelInvoke(nameof(HideDialogue));
            HideDialogue();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canInteract)
            return;

        if (pumpImage != null && pressedSprite != null)
        {
            pumpImage.sprite = pressedSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canInteract)
            return;

        if (pumpImage != null && notPressedSprite != null)
        {
            pumpImage.sprite = notPressedSprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canInteract || targetReached)
            return;

        clickCount++;

        float step = targetPressure / clicksToReachTarget;
        currentPressure = Mathf.Min(targetPressure, step * clickCount);

        UpdateNeedle();
        onPump?.Invoke(currentPressure);

        if (clickCount >= clicksToReachTarget)
        {
            targetReached = true;
            onTargetReached?.Invoke();
            Debug.Log("PressureGauge: target of " + targetPressure + " reached after " + clickCount + " clicks.");

            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
                Debug.Log("PressureGauge: enabled " + objectToEnable.name + ". Pump's own parent is unchanged.");
            }
        }
    }

    private void UpdateNeedle()
    {
        if (needle == null)
            return;

        float targetAngle = needleAngleAtZero;

        if (clickAngles != null && clickAngles.Length > 0)
        {
            int index = Mathf.Clamp(clickCount - 1, 0, clickAngles.Length - 1);
            targetAngle = clickAngles[index];
        }

        if (needleCoroutine != null)
        {
            StopCoroutine(needleCoroutine);
        }

        needleCoroutine = StartCoroutine(AnimateNeedle(targetAngle));
    }

    private IEnumerator AnimateNeedle(float targetAngle)
    {
        float startAngle = needle.localEulerAngles.z;
        float elapsed = 0f;

        while (elapsed < needleMoveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / needleMoveDuration);
            // ease-out so it settles smoothly instead of stopping abruptly
            progress = 1f - Mathf.Pow(1f - progress, 3f);

            float angle = Mathf.LerpAngle(startAngle, targetAngle, progress);
            needle.localEulerAngles = new Vector3(0f, 0f, angle);

            yield return null;
        }

        needle.localEulerAngles = new Vector3(0f, 0f, targetAngle);
        needleCoroutine = null;
    }

    // Call this (e.g. from a "Reset" button) to start the gauge over.
    public void ResetGauge()
    {
        if (needleCoroutine != null)
        {
            StopCoroutine(needleCoroutine);
            needleCoroutine = null;
        }

        clickCount = 0;
        currentPressure = 0f;
        targetReached = false;

        if (needle != null)
        {
            needleCoroutine = StartCoroutine(AnimateNeedle(needleAngleAtZero));
        }
    }


    // =========================================================
    // DIALOGUE BUBBLE HELPERS
    // =========================================================

    private void ShowDialogue(string message)
    {
        CancelInvoke(nameof(HideDialogue));

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);

            currentFullMessage = message;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(message));
        }
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        QueueAutoHide();
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullMessage;
        }

        isTyping = false;

        QueueAutoHide();
    }

    private void QueueAutoHide()
    {
        CancelInvoke(nameof(HideDialogue));

        if (autoHideDelay > 0f)
        {
            Invoke(nameof(HideDialogue), autoHideDelay);
        }
        else
        {
            HideDialogue();
        }
    }

    private void HideDialogue()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }

        canInteract = true;
    }
}