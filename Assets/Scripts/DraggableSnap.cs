using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class DraggableSnap : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("TARGET")]
    [SerializeField] private RectTransform target;

    [Header("SNAP DISTANCE")]
    [SerializeField] private float snapDistance = 80f;

    [Header("CANVAS")]
    [SerializeField] private Canvas canvas;

    [Header("CANVAS GROUP")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("SECOND OBJECT")]
    [Tooltip("An object (e.g. 'Game 2') to show once snapped.")]
    [SerializeField] private GameObject secondObject;

    [Header("OBJECT TO HIDE ON SNAP (optional)")]
    [SerializeField] private GameObject objectToHide;

    [Header("SNAP SOUND")]
    [SerializeField] private AudioClip snapSound;

    [Range(0f, 1f)]
    [SerializeField] private float snapSoundVolume = 1f;

    [Header("TYPEWRITER SOUND")]
    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the typewriter sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Range(0f, 1f)]
    [Tooltip("Volume of the typewriter sound.")]
    [SerializeField] private float typewriterVolume = 0.6f;

    [Header("DIALOGUE BUBBLE")]
    [SerializeField] private GameObject dialogueBubble;

    [SerializeField] private TMP_Text dialogueText;

    [Header("DIALOGUE ANIMATIONS")]
    [Tooltip("Automatically resizes the dialogue bubble based on the text.")]
    [SerializeField] private DialogueBoxAutoSize autoSize;

    [Tooltip("Plays the pop animation when the dialogue appears.")]
    [SerializeField] private DialoguePopAnimation popAnimation;

    [Tooltip("Plays the triangle animation when the dialogue appears.")]
    [SerializeField] private DialogueTriangleAnimation triangleAnimation;

    [Header("DIALOGUE")]
    [TextArea(2, 5)]
    [SerializeField] private string instructionMessage =
        "Drag the Sphygmomanometer onto Maria's arm";

    [Tooltip("Seconds between each typed character. Lower = faster typing.")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;

    private string currentFullMessage;

    private bool isTyping;

    private RectTransform rectTransform;

    private Vector2 originalPosition;

    private bool isSnapped = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Find Canvas automatically
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        // Find CanvasGroup automatically
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Remember original position
        originalPosition = rectTransform.anchoredPosition;
    }


    // =========================================================
    // START - SHOW INITIAL INSTRUCTION
    // =========================================================

    private void Start()
    {
        ShowDialogue(instructionMessage);
    }


    // =========================================================
    // UPDATE
    //
    // TAP 1:
    // If text is typing -> finish typing
    //
    // TAP 2:
    // If text has finished -> hide dialogue
    // =========================================================

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (isTyping)
        {
            // First tap finishes the typewriter.
            CompleteTyping();
        }
        else
        {
            // Second tap hides the dialogue.
            HideDialogue();
        }
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSnapped)
            return;

        if (canvasGroup != null)
        {
            // Allows the Target to receive the pointer.
            canvasGroup.blocksRaycasts = false;
        }
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (isSnapped)
            return;

        if (canvas == null)
        {
            Debug.LogError(
                "DraggableSnap: Canvas is not assigned!",
                this
            );

            return;
        }

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }


    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (isSnapped)
            return;

        if (target == null)
        {
            Debug.LogError(
                "DraggableSnap: Target is not assigned!",
                this
            );

            return;
        }

        // Calculate distance
        float distance = Vector2.Distance(
            rectTransform.position,
            target.position
        );

        Debug.Log(
            "Distance: " +
            distance +
            " | Snap Distance: " +
            snapDistance
        );

        if (distance <= snapDistance)
        {
            SnapToTarget();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }


    // =========================================================
    // SNAP TO TARGET
    // =========================================================

    private void SnapToTarget()
    {
        Debug.Log(
            "SPHYGMOMANOMETER HIT TARGET!"
        );

        // Snap its own position onto the target.
        // Parent remains unchanged.
        rectTransform.position = target.position;


        // -----------------------------------------------------
        // PLAY SNAP SOUND
        // -----------------------------------------------------
        // Uses AudioManager -> SFXSource.
        // No AudioSource is needed on this object.

        if (
            snapSound != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                snapSound,
                snapSoundVolume
            );
        }


        // Draw on top of its siblings.
        rectTransform.SetAsLastSibling();


        // -----------------------------------------------------
        // SHOW SECOND OBJECT
        // -----------------------------------------------------

        if (secondObject != null)
        {
            secondObject.SetActive(true);

            Debug.Log(
                "Second Object ENABLED: " +
                secondObject.name
            );
        }


        // -----------------------------------------------------
        // HIDE OLD OBJECT
        // -----------------------------------------------------

        if (objectToHide != null)
        {
            objectToHide.SetActive(false);

            Debug.Log(
                "Object To Hide DISABLED: " +
                objectToHide.name
            );
        }


        // -----------------------------------------------------
        // MARK AS SNAPPED
        // -----------------------------------------------------

        isSnapped = true;


        // -----------------------------------------------------
        // HIDE DIALOGUE
        // -----------------------------------------------------

        HideDialogue();


        Debug.Log(
            "SPHYGMOMANOMETER SNAPPED IN PLACE " +
            "(parent unchanged)."
        );
    }


    // =========================================================
    // SHOW DIALOGUE
    // =========================================================

    private void ShowDialogue(string message)
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }


        // -----------------------------------------------------
        // AUTO SIZE
        // -----------------------------------------------------

        if (autoSize != null)
        {
            if (
                autoSize.enabled &&
                autoSize.gameObject.activeInHierarchy
            )
            {
                autoSize.ResizeForText(message);
            }
        }


        // -----------------------------------------------------
        // POP ANIMATION
        // -----------------------------------------------------

        if (popAnimation != null)
        {
            if (
                popAnimation.enabled &&
                popAnimation.gameObject.activeInHierarchy
            )
            {
                popAnimation.PlayPop();
            }
        }


        // -----------------------------------------------------
        // TRIANGLE ANIMATION
        // -----------------------------------------------------

        if (triangleAnimation != null)
        {
            if (
                triangleAnimation.enabled &&
                triangleAnimation.gameObject.activeInHierarchy
            )
            {
                triangleAnimation.PlayAnimation();
            }
        }


        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);

            currentFullMessage = message ?? "";

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine =
                StartCoroutine(
                    TypeText(currentFullMessage)
                );
        }
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

   private IEnumerator TypeText(string message)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        string safeMessage = message ?? "";

        float safeTypingSpeed =
            Mathf.Max(typingSpeed, 0.001f);

        int characterCount = 0;

        foreach (char c in safeMessage)
        {
            if (dialogueText != null)
            {
                dialogueText.text += c;
            }

            // -----------------------------------------------------
            // TYPEWRITER SOUND
            // Play sound every X non-space characters
            // -----------------------------------------------------

            if (!char.IsWhiteSpace(c))
            {
                characterCount++;

                if (
                    typewriterSound != null &&
                    characterCount %
                    Mathf.Max(soundEveryCharacters, 1) == 0
                )
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDialogue(typewriterSound, typewriterVolume);
                    }
                }
            }

            yield return new WaitForSeconds(
                safeTypingSpeed
            );
        }

        // Typing has finished.
        isTyping = false;

        typingCoroutine = null;
    }


    // =========================================================
    // COMPLETE TYPING
    // =========================================================

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

        // Dialogue stays visible.
        // The NEXT tap will hide it.
    }


    // =========================================================
    // HIDE DIALOGUE
    // =========================================================

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
    }


    // =========================================================
    // RETURN TO ORIGINAL POSITION
    // =========================================================

    private void ReturnToOriginalPosition()
    {
        Debug.Log(
            "Missed Target - Returning to original position."
        );

        rectTransform.anchoredPosition =
            originalPosition;
    }
}