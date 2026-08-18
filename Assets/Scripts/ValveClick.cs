using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Attach this to the Valve GameObject. It needs a Button component
// (Unity UI) - drag that same Button into the "Valve Button" field below.
public class ValveClick : MonoBehaviour
{
    [Tooltip("Drag the Button component on this same GameObject (or wherever your valve's Button lives).")]
    [SerializeField] private Button valveButton;

    [Header("NEEDLE")]
    [Tooltip("The needle's RectTransform - it will be rotated back to Needle Angle At Zero when the valve is clicked.")]
    [SerializeField] private RectTransform needle;

    [Tooltip("The needle's Z angle for a reading of 0 (same value you used on PressureGauge).")]
    [SerializeField] private float needleAngleAtZero = 0f;

    [Tooltip("Seconds it takes the needle to smoothly sweep back to zero.")]
    [SerializeField] private float needleMoveDuration = 0.4f;

    [Header("REVEAL AFTER NEEDLE HITS 0")]
    [Tooltip("Optional. Drag a GameObject here to enable it once the needle animation finishes. The Valve itself is NOT reparented - it stays exactly where it is.")]
    [SerializeField] private GameObject objectToEnable;

    private Coroutine needleCoroutine;

    [Header("DIALOGUE BUBBLE")]
    [Tooltip("The chat bubble GameObject to show/hide (e.g. a panel with a TMP text inside).")]
    [SerializeField] private GameObject dialogueBubble;

    [SerializeField] private TMP_Text dialogueText;

    [TextArea]
    [SerializeField] private string instructionMessage = "Tap the valve to release the pressure.";

    [Tooltip("Seconds between each typed character. Lower = faster typing.")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private string currentFullMessage;
    private bool isTyping;
    private bool canInteract = false;

    private void Awake()
    {
        if (valveButton == null)
        {
            valveButton = GetComponent<Button>();
        }

        if (valveButton != null)
        {
            valveButton.onClick.AddListener(OnValveClicked);
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
            HideDialogue();
        }
    }

    // Wired up to valveButton.onClick in Awake (and also visible/assignable
    // in the Button's own OnClick() list in the Inspector if you prefer).
    public void OnValveClicked()
    {
        // While the text is mid-type, this click just finished it (via
        // Update above, same frame) - don't also treat it as opening the valve.
        if (isTyping)
            return;

        // If the bubble is still showing, this click dismisses it AND
        // performs the valve action in one go - no extra click needed.
        if (dialogueBubble != null && dialogueBubble.activeSelf)
        {
            HideDialogue();
        }

        if (!canInteract)
            return;

        if (needle == null)
        {
            Debug.LogError("ValveClick: Needle is not assigned!", this);
            return;
        }

        if (needleCoroutine != null)
        {
            StopCoroutine(needleCoroutine);
        }

        needleCoroutine = StartCoroutine(AnimateNeedleToZero());
    }

    private IEnumerator AnimateNeedleToZero()
    {
        float startAngle = needle.localEulerAngles.z;
        float elapsed = 0f;

        while (elapsed < needleMoveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / needleMoveDuration);
            // ease-out so it settles smoothly instead of stopping abruptly
            progress = 1f - Mathf.Pow(1f - progress, 3f);

            float angle = Mathf.LerpAngle(startAngle, needleAngleAtZero, progress);
            needle.localEulerAngles = new Vector3(0f, 0f, angle);

            yield return null;
        }

        needle.localEulerAngles = new Vector3(0f, 0f, needleAngleAtZero);
        needleCoroutine = null;

        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            Debug.Log("ValveClick: enabled " + objectToEnable.name + " after needle hit 0. Valve's own parent is unchanged.");
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