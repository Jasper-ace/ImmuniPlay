using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.Events;

using UnityEngine.UI;

using TMPro;

using System.Collections;

public class PressureGauge : MonoBehaviour,

    IPointerClickHandler,

    IPointerDownHandler,

    IPointerUpHandler

{

    [Header("GAMEOBJECTS TO SHOW ON DIALOGUE START")]

    [Tooltip("GameObjects to show when the dialogue starts.")]

    [SerializeField] private GameObject[] gameObjectsToShowOnDialogueStart;

    [Header("GAMEOBJECTS TO HIDE ON DIALOGUE START")]

    [Tooltip("GameObjects to hide when the dialogue starts.")]

    [SerializeField] private GameObject[] gameObjectsToHideOnDialogueStart;

    [Header("PUMP IMAGE")]

    [SerializeField] private Image pumpImage;

    [SerializeField] private Sprite notPressedSprite;

    [SerializeField] private Sprite pressedSprite;


    [Header("PUMP SOUND")]
    [Tooltip("Sound played each time the player taps the pump.")]
    [SerializeField] private AudioClip pumpSound;

    [Range(0f, 1f)]
    [SerializeField] private float pumpSoundVolume = 1f;

    [Header("GAUGE")]

    [SerializeField] private RectTransform needle;

    [Header("NEEDLE TICK SOUND")]
    [Tooltip("Short tick sound played each time the gauge needle moves.")]
    [SerializeField] private AudioClip needleTickSound;

    [Range(0f, 1f)]
    [SerializeField] private float needleTickVolume = 1f;

    [SerializeField] private float needleAngleAtZero = 0f;

    [SerializeField] private float[] clickAngles =

    {

        45f,

        90f,

        135f

    };

    [SerializeField] private float dialMaxValue = 120f;

    [SerializeField] private float needleMoveDuration = 0.2f;

    [Header("TARGET PRESSURE")]

    [SerializeField] private float targetPressure = 120f;

    [SerializeField] private int clicksToReachTarget = 3;

    [Header("SCENE TRANSITION")]

    [Tooltip("The current GameObject/scene to disable when the target pressure is reached.")]

    [SerializeField] private GameObject objectToDisable;

    [Tooltip("The next GameObject/scene to enable when the target pressure is reached.")]

    [SerializeField] private GameObject objectToEnable;

    [Header("EVENTS")]

    public UnityEvent onTargetReached;

    public UnityEvent onPump;

    [Header("DIALOGUE")]

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

    [SerializeField]

    private string instructionMessage =

        "Tap the pump 3 times to inflate the cuff.";

    [SerializeField] private float typingSpeed = 0.03f;

    [Header("TYPEWRITER SOUND")]

    [SerializeField] private AudioClip typewriterSound;

    [Tooltip("Play the typewriter sound every X non-space characters.")]

    [SerializeField] private int soundEveryCharacters = 2;

    [Range(0f, 1f)]

    [SerializeField] private float typewriterVolume = 0.25f;

    // State

    private Coroutine typingCoroutine;

    private string currentFullMessage;

    private bool isTyping;

    private bool canInteract;

    private int clickCount;

    private float currentPressure;

    private bool targetReached;

    private Coroutine needleCoroutine;

    private bool needleMoving;



    private void Awake()

    {

        // Find pump image automatically if not assigned

        if (pumpImage == null)

        {

            pumpImage = GetComponent<Image>();

        }

        // Store original pump sprite

        if (pumpImage != null && notPressedSprite == null)

        {

            notPressedSprite = pumpImage.sprite;

        }

        // Reset gauge

        if (needle != null)

        {

            needle.localRotation =

                Quaternion.Euler(0f, 0f, needleAngleAtZero);

        }

    }

    private void ApplyDialogueStartGameObjects()

    {

        if (gameObjectsToHideOnDialogueStart != null)

        {

            foreach (GameObject obj in gameObjectsToHideOnDialogueStart)

            {

                if (obj != null)

                    obj.SetActive(false);

            }

        }

        if (gameObjectsToShowOnDialogueStart != null)

        {

            foreach (GameObject obj in gameObjectsToShowOnDialogueStart)

            {

                if (obj != null)

                    obj.SetActive(true);

            }

        }

    }

    private void Start()

    {

        ShowDialogue(instructionMessage);

    }

    // Called by the DialogueInput UI Button.

    // This replaces screen-wide mouse/touch input so tapping other UI

    // elements (such as a hamburger/menu button) will not advance dialogue.

    public void AdvanceDialogue()

    {

        if (isTyping)

        {

            CompleteTyping();

            return;

        }

        if (dialogueBubble != null && dialogueBubble.activeSelf)

        {

            CancelInvoke(nameof(HideDialogue));

            HideDialogue();

        }

    }

    // --------------------------------------------------

    // PUMP PRESS

    // --------------------------------------------------

    public void OnPointerDown(PointerEventData eventData)

    {

        if (!canInteract || targetReached || needleMoving)

            return;

        if (pumpImage != null && pressedSprite != null)

        {

            pumpImage.sprite = pressedSprite;

        }

    }

    public void OnPointerUp(PointerEventData eventData)

    {

        if (targetReached)

            return;

        if (pumpImage != null && notPressedSprite != null)

        {

            pumpImage.sprite = notPressedSprite;

        }

    }

    public void OnPointerClick(PointerEventData eventData)

    {

        if (!canInteract || targetReached || needleMoving)

            return;

        clickCount++;


        // Play the pump sound through AudioManager so it follows the game's SFX volume.
        if (pumpSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(pumpSound, pumpSoundVolume);
        }

        // Calculate pressure

        currentPressure =

            (targetPressure / clicksToReachTarget) * clickCount;

        currentPressure =

            Mathf.Clamp(currentPressure, 0f, dialMaxValue);

        // Check target before starting the needle animation so the final
        // animation is the one that controls the scene/object transition.
        if (clickCount >= clicksToReachTarget)
        {
            currentPressure = targetPressure;
            targetReached = true;
        }

        UpdateNeedle();

        // Trigger pump event

        onPump?.Invoke();

    }

    // --------------------------------------------------

    // NEEDLE

    // --------------------------------------------------

    private void UpdateNeedle()

    {

        if (needle == null)
        {
            FinishNeedleMovement();
            return;
        }

        float targetAngle;

        if (clickCount <= 0)

        {

            targetAngle = needleAngleAtZero;

        }

        else

        {

            int angleIndex =

                Mathf.Clamp(clickCount - 1, 0, clickAngles.Length - 1);

            targetAngle = clickAngles[angleIndex];

        }

        if (needleCoroutine != null)
        {
            StopCoroutine(needleCoroutine);
        }

        needleMoving = true;

        needleCoroutine =

            StartCoroutine(

                AnimateNeedle(targetAngle)

            );

    }

    private IEnumerator AnimateNeedle(float targetAngle)

    {

        float startAngle =

            needle.localEulerAngles.z;

        // Play one short tick through AudioManager when the needle starts moving.
        if (needleTickSound != null &&
            Mathf.Abs(Mathf.DeltaAngle(startAngle, targetAngle)) > 0.01f)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    needleTickSound,
                    needleTickVolume
                );
            }
        }

        // Convert 0-360 to -180 to 180

        if (startAngle > 180f)

            startAngle -= 360f;

        float elapsed = 0f;

        while (elapsed < needleMoveDuration)

        {

            elapsed += Time.deltaTime;

            float t =

                Mathf.Clamp01(

                    elapsed / needleMoveDuration

                );

            // Smooth movement

            t = Mathf.SmoothStep(0f, 1f, t);

            float currentAngle =

                Mathf.Lerp(

                    startAngle,

                    targetAngle,

                    t

                );

            needle.localRotation =

                Quaternion.Euler(

                    0f,

                    0f,

                    currentAngle

                );

            yield return null;

        }

        needle.localRotation =

            Quaternion.Euler(

                0f,

                0f,

                targetAngle

            );

        needleCoroutine = null;
        needleMoving = false;

        FinishNeedleMovement();

    }

    private void FinishNeedleMovement()
    {
        needleMoving = false;

        // Do not change scene/object until the final needle animation is complete.
        if (targetReached)
        {
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }

            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            onTargetReached?.Invoke();
            canInteract = false;
            return;
        }

        // Allow the next pump only after the needle has finished moving.
        canInteract = true;
    }

    // --------------------------------------------------

    // RESET

    // --------------------------------------------------

    public void ResetGauge()

    {

        clickCount = 0;

        currentPressure = 0f;

        targetReached = false;

        canInteract = false;

        if (pumpImage != null &&

            notPressedSprite != null)

        {

            pumpImage.sprite = notPressedSprite;

        }

        if (needleCoroutine != null)

        {

            StopCoroutine(needleCoroutine);

            needleCoroutine = null;

        }

        if (needle != null)

        {

            needle.localRotation =

                Quaternion.Euler(

                    0f,

                    0f,

                    needleAngleAtZero

                );

        }

        ShowDialogue(instructionMessage);

    }

    // --------------------------------------------------

    // DIALOGUE

    // --------------------------------------------------

    public void ShowDialogue(string message)

    {

        if (dialogueBubble == null ||

            dialogueText == null)

        {

            return;

        }

        CancelInvoke(nameof(HideDialogue));

        ApplyDialogueStartGameObjects();

        dialogueBubble.SetActive(true);

        dialogueText.text = "";

        currentFullMessage = message;

        canInteract = false;

        if (typingCoroutine != null)

        {

            StopCoroutine(typingCoroutine);

        }

        // Prepare the bubble before typing:

        // auto-resize, pop animation, and triangle animation.

        PrepareDialogue(message);

        typingCoroutine =

            StartCoroutine(

                TypeText(message)

            );

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

    private IEnumerator TypeText(string message)

    {

        isTyping = true;

        int characterCount = 0;

        if (dialogueText != null)

        {

            dialogueText.text = "";

        }

        foreach (char c in message)

        {

            if (dialogueText != null)

            {

                dialogueText.text += c;

            }

            // Count only non-space characters.

            // The sound plays every X characters, like SingleCharDialogue.

            if (!char.IsWhiteSpace(c))

            {

                characterCount++;

                if (typewriterSound != null &&

                    soundEveryCharacters > 0 &&

                    characterCount % soundEveryCharacters == 0)

                {

                    // Play through AudioManager so the dialogue volume

                    // follows the game's Dialogue Volume setting.

                    if (AudioManager.Instance != null)

                    {

                        AudioManager.Instance.PlayDialogue(

                            typewriterSound,

                            typewriterVolume

                        );

                    }

                }

            }

            // typingSpeed only controls how fast the text appears.

            yield return new WaitForSeconds(typingSpeed);

        }

        isTyping = false;

        typingCoroutine = null;

    }

    private void CompleteTyping()

    {

        if (!isTyping)

            return;

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

        CancelInvoke(nameof(HideDialogue));

        if (dialogueBubble != null)

        {

            dialogueBubble.SetActive(false);

        }

        if (dialogueText != null)

        {

            dialogueText.text = "";

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