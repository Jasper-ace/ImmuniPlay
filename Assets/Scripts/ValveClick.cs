using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ValveClick : MonoBehaviour
{
    // =========================================================
    // GAMEOBJECTS TO SHOW / HIDE ON DIALOGUE START
    // =========================================================

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE START")]
    [Tooltip("GameObjects to show when the dialogue starts.")]
    [SerializeField] private GameObject[] gameObjectsToShowOnDialogueStart;

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE START")]
    [Tooltip("GameObjects to hide when the dialogue starts.")]
    [SerializeField] private GameObject[] gameObjectsToHideOnDialogueStart;


    // =========================================================
    // VALVE
    // =========================================================

    [Header("VALVE")]
    [Tooltip("Drag the Button component on this same GameObject (or wherever your valve's Button lives).")]
    [SerializeField] private Button valveButton;


    // =========================================================
    // NEEDLE
    // =========================================================

    [Header("NEEDLE")]

    [Tooltip("The needle's RectTransform.")]
    [SerializeField] private RectTransform needle;

    [Tooltip("The needle's Z angle for a reading of 0.")]
    [SerializeField] private float needleAngleAtZero = 0f;

    [Tooltip("Total time for the needle to slowly travel from its starting position to 0.")]
    [SerializeField] private float needleMoveDuration = 5f;


    // =========================================================
    // PITIK ANGLES
    // =========================================================

    [Header("PITIK ANGLES")]

    [Tooltip("First angle where a pitik happens.")]
    [SerializeField] private float pitikAngle1 = 140f;

    [Tooltip("Second angle where a pitik happens.")]
    [SerializeField] private float pitikAngle2 = 100f;

    [Tooltip("Third angle where a pitik happens.")]
    [SerializeField] private float pitikAngle3 = 60f;


    // =========================================================
    // PITIK ANIMATION
    // =========================================================

    [Header("PITIK ANIMATION")]

    [Tooltip("How far the needle quickly moves backward during a pitik.")]
    [SerializeField] private float pitikAmount = 6f;

    [Tooltip("How quickly the pitik happens.")]
    [SerializeField] private float pitikDuration = 0.08f;

    [Tooltip("Optional sound played whenever a pitik happens.")]
    [SerializeField] private AudioClip pitikSound;

    [Range(0f, 1f)]
    [SerializeField] private float pitikSoundVolume = 1f;

    // =========================================================
    // DIALOGUE BUBBLE
    // =========================================================

    [Header("DIALOGUE BUBBLE")]

    [Tooltip("The chat bubble GameObject to show/hide.")]
    [SerializeField] private GameObject dialogueBubble;

    [SerializeField] private TMP_Text dialogueText;

    [Header("DIALOGUE ANIMATIONS")]

    [Tooltip("Automatically resizes the dialogue bubble.")]
    [SerializeField] private DialogueBoxAutoSize autoSize;

    [Tooltip("Pop animation for the dialogue bubble.")]
    [SerializeField] private DialoguePopAnimation popAnimation;

    [Tooltip("Triangle animation for the dialogue bubble.")]
    [SerializeField] private DialogueTriangleAnimation triangleAnimation;

    [Header("GAMEOBJECTS TO HIDE AFTER DIALOGUE")]
    [Tooltip("GameObjects to hide when the dialogue is dismissed.")]
    [SerializeField] private GameObject[] gameObjectsToHideAfterDialogue;

    [TextArea]
    [SerializeField] private string instructionMessage =
        "Tap the valve to release the pressure.";

    [Tooltip("Seconds between each typed character.")]
    [SerializeField] private float typingSpeed = 0.04f;


    // =========================================================
    // DIALOGUE SOUND
    // =========================================================

    [Header("DIALOGUE SOUND")]

    [SerializeField] private AudioClip dialogueSound;

    [Tooltip("Play the dialogue sound every X characters.")]
    [SerializeField] private int soundEveryCharacters = 2;

    [Range(0f, 1f)]
    [Tooltip("Volume of the dialogue typing sound.")]
    [SerializeField] private float dialogueSoundVolume = 0.6f;

    [Header("SCENE TRANSITION")]

    [Tooltip("GameObjects to disable after the needle reaches 0.")]
    [SerializeField] private GameObject[] objectsToDisable;

    [Tooltip("GameObjects to enable after the needle reaches 0.")]
    [SerializeField] private GameObject[] objectsToEnable;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private Coroutine needleCoroutine;
    private Coroutine typingCoroutine;

    private string currentFullMessage;

    private bool isTyping;
    private bool canInteract = false;


    // =========================================================
    // AWAKE
    // =========================================================

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


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ShowDialogue(instructionMessage);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public void AdvanceDialogue()
    {
        if (isTyping)
        {
            CompleteTyping();
        }
        else if (dialogueBubble != null && dialogueBubble.activeSelf)
        {
            HideDialogue();
        }
    }


    // =========================================================
    // VALVE CLICK
    // =========================================================

    public void OnValveClicked()
    {
        // Already running or already completed.
        // Ignore any additional taps.
        if (needleCoroutine != null)
            return;

        // While the text is typing, don't activate the valve.
        if (isTyping)
            return;

        // If dialogue is still showing, hide it.
        if (dialogueBubble != null && dialogueBubble.activeSelf)
        {
            HideDialogue();
        }

        if (!canInteract)
            return;

        if (needle == null)
        {
            Debug.LogError(
                "ValveClick: Needle is not assigned!",
                this
            );

            return;
        }

        // ---------------------------------------------------------
        // LOCK THE BUTTON
        // ---------------------------------------------------------

        if (valveButton != null)
        {
            valveButton.interactable = false;
        }

        // ---------------------------------------------------------
        // START THE FULL ANIMATION
        // ---------------------------------------------------------

        needleCoroutine = StartCoroutine(AnimateNeedle());
    }


    // =========================================================
    // NEEDLE ANIMATION
    // =========================================================

    private IEnumerator AnimateNeedle()
    {
        float startAngle = needle.localEulerAngles.z;

        // Make sure all three pitik angles are treated as
        // positions between the starting angle and zero.
        float[] pitikAngles =
        {
            pitikAngle1,
            pitikAngle2,
            pitikAngle3
        };

        // Calculate the duration of each section.
        float sectionDuration = needleMoveDuration / 4f;

        // -----------------------------------------------------
        // MOVE TO PITIK 1
        // -----------------------------------------------------

        yield return StartCoroutine(
            MoveNeedle(
                startAngle,
                pitikAngle1,
                sectionDuration
            )
        );

        yield return StartCoroutine(
            PlayPitik(pitikAngle1)
        );


        // -----------------------------------------------------
        // MOVE TO PITIK 2
        // -----------------------------------------------------

        yield return StartCoroutine(
            MoveNeedle(
                pitikAngle1,
                pitikAngle2,
                sectionDuration
            )
        );

        yield return StartCoroutine(
            PlayPitik(pitikAngle2)
        );


        // -----------------------------------------------------
        // MOVE TO PITIK 3
        // -----------------------------------------------------

        yield return StartCoroutine(
            MoveNeedle(
                pitikAngle2,
                pitikAngle3,
                sectionDuration
            )
        );

        yield return StartCoroutine(
            PlayPitik(pitikAngle3)
        );


        // -----------------------------------------------------
        // MOVE TO ZERO
        // -----------------------------------------------------

        yield return StartCoroutine(
            MoveNeedle(
                pitikAngle3,
                needleAngleAtZero,
                sectionDuration
            )
        );


        // Make absolutely sure the needle ends at zero.
        needle.localEulerAngles = new Vector3(
            0f,
            0f,
            needleAngleAtZero
        );

        needleCoroutine = null;

        // Disable all assigned GameObjects.
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // Enable all assigned GameObjects.
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }


    // =========================================================
    // MOVE NEEDLE
    // =========================================================

    private IEnumerator MoveNeedle(
        float fromAngle,
        float toAngle,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Smooth movement.
            progress =
                1f - Mathf.Pow(1f - progress, 3f);

            float angle =
                Mathf.LerpAngle(
                    fromAngle,
                    toAngle,
                    progress
                );

            needle.localEulerAngles = new Vector3(
                0f,
                0f,
                angle
            );

            yield return null;
        }

        needle.localEulerAngles = new Vector3(
            0f,
            0f,
            toAngle
        );
    }


    // =========================================================
    // PITIK ANIMATION
    // =========================================================

    private IEnumerator PlayPitik(float angle)
    {
        // Play pitik sound.
        if (pitikSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDialogue(
                    pitikSound,
                    pitikSoundVolume
                );
            }
            else
            {
                AudioSource.PlayClipAtPoint(
                    pitikSound,
                    transform.position,
                    pitikSoundVolume
                );
            }
        }


        // -----------------------------------------------------
        // QUICK BACKWARD MOVEMENT
        // -----------------------------------------------------

        float backwardAngle = angle + pitikAmount;

        float halfDuration =
            Mathf.Max(pitikDuration / 2f, 0.01f);

        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / halfDuration);

            float currentAngle =
                Mathf.LerpAngle(
                    angle,
                    backwardAngle,
                    progress
                );

            needle.localEulerAngles = new Vector3(
                0f,
                0f,
                currentAngle
            );

            yield return null;
        }


        // -----------------------------------------------------
        // RETURN TO ORIGINAL ANGLE
        // -----------------------------------------------------

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / halfDuration);

            float currentAngle =
                Mathf.LerpAngle(
                    backwardAngle,
                    angle,
                    progress
                );

            needle.localEulerAngles = new Vector3(
                0f,
                0f,
                currentAngle
            );

            yield return null;
        }


        // Make sure it ends exactly at the pitik angle.
        needle.localEulerAngles = new Vector3(
            0f,
            0f,
            angle
        );
    }


    // =========================================================
    // DIALOGUE START GAMEOBJECTS
    // =========================================================

    private void ApplyDialogueStartGameObjects()
    {
        if (gameObjectsToShowOnDialogueStart != null)
        {
            foreach (GameObject obj in gameObjectsToShowOnDialogueStart)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        if (gameObjectsToHideOnDialogueStart != null)
        {
            foreach (GameObject obj in gameObjectsToHideOnDialogueStart)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }


    // =========================================================
    // DIALOGUE BUBBLE
    // =========================================================

    private void ShowDialogue(string message)
    {
        CancelInvoke(nameof(HideDialogue));

        ApplyDialogueStartGameObjects();

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            currentFullMessage = message;

            // Prepare the bubble before typing:
            // auto-resize, pop animation, and triangle animation.
            PrepareDialogue(message);

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine =
                StartCoroutine(TypeText(message));
        }   
    }

    private void PrepareDialogue(string text)
    {
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
                autoSize.ResizeForText(text);
            }
        }

        // -----------------------------------------------------
        // POP
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
        // TRIANGLE
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
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

    private IEnumerator TypeText(string message)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

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

            // Count only visible characters.
            if (!char.IsWhiteSpace(c))
            {
                characterCount++;

                if (
                    dialogueSound != null &&
                    characterCount %
                    Mathf.Max(soundEveryCharacters, 1) == 0
                )
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDialogue(
                            dialogueSound,
                            dialogueSoundVolume
                        );
                    }
                }
            }

            yield return new WaitForSeconds(
                safeTypingSpeed
            );
        }

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

        // Hide selected GameObjects after the dialogue is dismissed.
        if (gameObjectsToHideAfterDialogue != null)
        {
            foreach (GameObject obj in gameObjectsToHideAfterDialogue)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        canInteract = true;
    }
}