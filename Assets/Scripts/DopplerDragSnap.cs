using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class DopplerDragSnap : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // =========================================================
    // SETUP
    // =========================================================

    [Header("Setup")]

    public RectTransform targetArea;
    public TargetSpawner targetSpawner;

    public GameObject sceneParentToEnable;
    public GameObject dragParentToDisable;

    public float snapDistance = 100f;
    public bool lockAfterSnap = true;
    public bool returnToStartOnMiss = true;
    public float snapSpeed = 15f;

    public UnityEngine.Events.UnityEvent onSnapped;


    // =========================================================
    // DIALOGUE SYSTEM
    // =========================================================

    [Header("Dialogue System")]

    [Tooltip("The dialogue panel GameObject.")]
    public GameObject dialoguePanelObject;

    [Tooltip("Text inside the dialogue bubble.")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Time between each character.")]
    public float typewriterSpeed = 0.05f;

    [Tooltip("Sound played while the dialogue is typing.")]
    public AudioClip typewriterSound;

    [Tooltip("Play the typewriter sound every X non-space characters.")]
    [Min(1)]
    public int soundEveryCharacters = 2;

    [Tooltip("Volume of the typewriter sound.")]
    [Range(0f, 1f)]
    public float typewriterVolume = 1f;


    // =========================================================
    // DIALOGUE ANIMATIONS
    // =========================================================

    [Header("Dialogue Animations")]

    [Tooltip("Automatically resize the dialogue box based on the text.")]
    public DialogueBoxAutoSize autoSize;

    [Tooltip("Pop animation for the dialogue box.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Triangle animation for the dialogue bubble.")]
    public DialogueTriangleAnimation triangleAnimation;


    // =========================================================
    // DIALOGUES
    // =========================================================

    [Header("Dialogues")]

    [TextArea(2, 3)]
    public string[] dialogues;


    [Header("GameObjects To Hide After Dialogue")]

    [Tooltip("GameObjects to hide when all dialogue is finished.")]
    public GameObject[] gameObjectsToHideAfterDialogue;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private RectTransform rect;
    private Canvas canvas;
    private Vector2 startPos;

    private bool snapped = false;
    private bool animating = false;
    private Vector2 animTarget;


    // =========================================================
    // DIALOGUE VARIABLES
    // =========================================================

    private int currentDialogueIndex = 0;

    private bool isTyping = false;
    private bool dialogueComplete = false;
    private bool allDialoguesComplete = false;

    private bool dialogueLocked = true;

    private float inputCooldown = 0f;

    private const float INPUT_COOLDOWN_TIME = 0.1f;

    // New safer typewriter handling
    private Coroutine typingCoroutine;
    private string currentFullText = "";


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        startPos = rect.anchoredPosition;


        // -----------------------------------------------------
        // Start with dialogue panel visible
        // -----------------------------------------------------

        if (dialoguePanelObject != null)
        {
            dialoguePanelObject.SetActive(true);
        }


        // -----------------------------------------------------
        // Start first dialogue
        // -----------------------------------------------------

        if (dialogues != null && dialogues.Length > 0)
        {
            currentDialogueIndex = 0;

            ShowDialogue();
        }
        else
        {
            FinishDialogue();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // -----------------------------------------------------
        // Probe snapping animation
        // -----------------------------------------------------

        if (animating)
        {
            rect.anchoredPosition =
                Vector2.Lerp(
                    rect.anchoredPosition,
                    animTarget,
                    Time.deltaTime * snapSpeed
                );

            if (
                Vector2.Distance(
                    rect.anchoredPosition,
                    animTarget
                ) < 2f
            )
            {
                rect.anchoredPosition = animTarget;
                animating = false;
            }
        }


        // -----------------------------------------------------
        // Keep snapped probe locked
        // -----------------------------------------------------

        if (snapped && lockAfterSnap)
        {
            rect.anchoredPosition =
                targetArea.anchoredPosition;
        }


        // -----------------------------------------------------
        // Input cooldown
        // -----------------------------------------------------

        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
        }


    }


    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    public void AdvanceDialogue()
    {
        if (allDialoguesComplete)
            return;

        // If text is still typing, finish it immediately.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // If current dialogue is complete, move to the next one.
        if (dialogueComplete)
        {
            currentDialogueIndex++;

            if (
                dialogues != null &&
                currentDialogueIndex < dialogues.Length
            )
            {
                dialogueComplete = false;
                ShowDialogue();

                Debug.Log(
                    "Moving to dialogue: " +
                    currentDialogueIndex
                );
            }
            else
            {
                FinishDialogue();
            }
        }
    }


    // =========================================================
    // SHOW DIALOGUE
    // =========================================================

    private void ShowDialogue()
    {
        if (
            dialogues == null ||
            dialogues.Length == 0 ||
            currentDialogueIndex < 0 ||
            currentDialogueIndex >= dialogues.Length
        )
        {
            return;
        }


        // -----------------------------------------------------
        // Get current dialogue
        // -----------------------------------------------------

        string text =
            dialogues[currentDialogueIndex] ?? "";


        // -----------------------------------------------------
        // Set dialogue state
        // -----------------------------------------------------

        isTyping = true;
        dialogueComplete = false;


        // -----------------------------------------------------
        // Show dialogue panel
        // -----------------------------------------------------

        if (dialoguePanelObject != null)
        {
            dialoguePanelObject.SetActive(true);
        }


        // -----------------------------------------------------
        // Clear previous text
        // -----------------------------------------------------

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }


        // -----------------------------------------------------
        // Store complete text
        // -----------------------------------------------------

        currentFullText = text;


        // =====================================================
        // AUTO SIZE
        // =====================================================

        if (autoSize != null)
        {
            if (
                autoSize.enabled &&
                autoSize.gameObject.activeInHierarchy
            )
            {
                autoSize.ResizeForText(text);
            }
        }


        // =====================================================
        // POP ANIMATION
        // =====================================================

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


        // =====================================================
        // TRIANGLE ANIMATION
        // =====================================================

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
        // Stop previous typewriter
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // -----------------------------------------------------
        // Start exactly one typewriter
        // -----------------------------------------------------

        typingCoroutine =
            StartCoroutine(
                TypeText(text)
            );
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueComplete = false;


        // -----------------------------------------------------
        // Clear text before typing
        // -----------------------------------------------------

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }


        string safeText =
            text ?? "";


        float safeTypingSpeed =
            Mathf.Max(
                typewriterSpeed,
                0.001f
            );


        int characterCounter = 0;


        // -----------------------------------------------------
        // Type each character
        // -----------------------------------------------------

        foreach (char c in safeText)
        {
            if (dialogueText != null)
            {
                dialogueText.text += c;
            }


            // -------------------------------------------------
            // Typewriter sound
            // -------------------------------------------------

            if (!char.IsWhiteSpace(c))
            {
                characterCounter++;

                if (
                    characterCounter >=
                    Mathf.Max(
                        1,
                        soundEveryCharacters
                    )
                )
                {
                    if (
                        typewriterSound != null &&
                        AudioManager.Instance != null
                    )
                    {
                        AudioManager.Instance.PlayDialogue(
                            typewriterSound,
                            typewriterVolume
                        );
                    }

                    characterCounter = 0;
                }
            }


            // -------------------------------------------------
            // Wait before next character
            // -------------------------------------------------

            yield return new WaitForSeconds(
                safeTypingSpeed
            );
        }


        // -----------------------------------------------------
        // Finished typing
        // -----------------------------------------------------

        isTyping = false;
        dialogueComplete = true;

        typingCoroutine = null;
    }


    // =========================================================
    // FINISH CURRENT TYPING
    // =========================================================

    private void FinishTyping()
    {
        // -----------------------------------------------------
        // Stop ONLY the typewriter coroutine
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }


        // -----------------------------------------------------
        // Immediately show complete text
        // -----------------------------------------------------

        if (dialogueText != null)
        {
            dialogueText.text =
                currentFullText;
        }


        isTyping = false;
        dialogueComplete = true;


        Debug.Log(
            "Text completed instantly"
        );
    }


    // =========================================================
    // FINISH DIALOGUE
    // =========================================================

    void FinishDialogue()
    {
        // -----------------------------------------------------
        // Make sure typewriter is stopped
        // -----------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;
        }

        isTyping = false;


        // -----------------------------------------------------
        // Dialogue is completely finished
        // -----------------------------------------------------

        allDialoguesComplete = true;
        dialogueLocked = false;


        // -----------------------------------------------------
        // Hide dialogue panel
        // -----------------------------------------------------

        if (dialoguePanelObject != null)
        {
            dialoguePanelObject.SetActive(false);
        }


        // -----------------------------------------------------
        // Hide assigned GameObjects after dialogue
        // -----------------------------------------------------

        if (gameObjectsToHideAfterDialogue != null)
        {
            foreach (GameObject obj in gameObjectsToHideAfterDialogue)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }


        Debug.Log(
            "Dialogue complete! Probe is now draggable."
        );
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData data)
    {
        // -----------------------------------------------------
        // Block dragging during dialogue
        // -----------------------------------------------------

        if (dialogueLocked)
            return;


        Debug.Log(
            "OnBeginDrag called"
        );


        // -----------------------------------------------------
        // Don't allow dragging after snap
        // -----------------------------------------------------

        if (snapped && lockAfterSnap)
        {
            Debug.Log(
                "Already snapped - cannot drag!"
            );

            return;
        }


        animating = false;
    }


    // =========================================================
    // DRAG PROBE
    // =========================================================

    public void OnDrag(PointerEventData data)
    {
        // -----------------------------------------------------
        // Block dragging during dialogue
        // -----------------------------------------------------

        if (dialogueLocked)
            return;


        if (snapped && lockAfterSnap)
        {
            return;
        }


        // -----------------------------------------------------
        // Move probe
        // -----------------------------------------------------

        rect.anchoredPosition +=
            data.delta / canvas.scaleFactor;


        Debug.Log(
            "Dragging: " +
            rect.anchoredPosition
        );
    }


    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(PointerEventData data)
    {
        // -----------------------------------------------------
        // Block dragging during dialogue
        // -----------------------------------------------------

        if (dialogueLocked)
            return;


        Debug.Log(
            "OnEndDrag called"
        );


        if (snapped && lockAfterSnap)
            return;


        // -----------------------------------------------------
        // Check distance
        // -----------------------------------------------------

        float dist =
            Vector3.Distance(
                rect.position,
                targetArea.position
            );


        Debug.Log(
            "Distance: " +
            dist +
            " vs snapDistance: " +
            snapDistance
        );


        // -----------------------------------------------------
        // Snap
        // -----------------------------------------------------

        if (dist <= snapDistance)
        {
            Snap();
        }


        // -----------------------------------------------------
        // Return to start
        // -----------------------------------------------------

        else if (returnToStartOnMiss)
        {
            animTarget = startPos;
            animating = true;
        }
    }


    // =========================================================
    // SNAP
    // =========================================================

    void Snap()
    {
        snapped = true;


        // -----------------------------------------------------
        // Get target world position
        // -----------------------------------------------------

        Vector3 targetWorldPos =
            targetArea.position;


        // -----------------------------------------------------
        // Get probe canvas
        // -----------------------------------------------------

        Canvas probeCanvas =
            rect.GetComponentInParent<Canvas>();


        RectTransform probeParent =
            rect.parent as RectTransform;


        // -----------------------------------------------------
        // Get target canvas
        // -----------------------------------------------------

        Canvas targetCanvas =
            targetArea.GetComponentInParent<Canvas>();


        // -----------------------------------------------------
        // Convert target world position to screen
        // -----------------------------------------------------

        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(
                targetCanvas.worldCamera,
                targetWorldPos
            );


        // -----------------------------------------------------
        // Convert screen position to probe parent
        // -----------------------------------------------------

        Vector2 localPoint;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                probeParent,
                screenPoint,
                probeCanvas.renderMode ==
                    RenderMode.ScreenSpaceOverlay
                    ? null
                    : probeCanvas.worldCamera,
                out localPoint
            );


        rect.anchoredPosition =
            localPoint;

        animating = false;


        // =====================================================
        // TOGGLE SCENE PARENTS
        // =====================================================

        if (dragParentToDisable != null)
        {
            dragParentToDisable.SetActive(false);

            Debug.Log(
                "Dragging UI Hidden"
            );
        }


        if (sceneParentToEnable != null)
        {
            sceneParentToEnable.SetActive(true);

            Debug.Log(
                "Scene Parent Enabled"
            );
        }


        // =====================================================
        // REVEAL TARGET
        // =====================================================

        if (targetSpawner != null)
        {
            targetSpawner.RevealTarget();
        }


        Debug.Log(
            "TARGET FOUND AND SCENE ACTIVATED!"
        );


        // =====================================================
        // EVENT
        // =====================================================

        onSnapped?.Invoke();
    }
}