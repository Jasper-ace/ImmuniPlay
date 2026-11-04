using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueTyper : MonoBehaviour
{
    //====================================================
    // AUTO START
    //====================================================

    [Header("Auto Start")]
    public bool autoStartDialogue = false;


    //====================================================
    // DIALOGUE TEXT
    //====================================================

    [Header("Dialogue Text")]
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;


    //====================================================
    // OPTIONAL DIALOGUE BUBBLE
    //====================================================

    [Header("Optional Dialogue Bubble")]
    [Tooltip("Leave empty if not needed.")]
    public GameObject dialogueBubble;


    //====================================================
    // OPTIONAL DIALOGUE ANIMATIONS
    //====================================================

    [Header("Optional Dialogue Animations")]

    [Tooltip("Leave empty if Auto Resize is not needed.")]
    public DialogueBoxAutoSize dialogueAutoSize;

    [Tooltip("Leave empty if Pop Animation is not needed.")]
    public DialoguePopAnimation popAnimation;

    [Tooltip("Leave empty if Triangle Animation is not needed.")]
    public DialogueTriangleAnimation triangleAnimation;


    //====================================================
    // OBJECTS TO SHOW AFTER DIALOGUE
    //====================================================

    [Header("Objects To Show After Dialogue")]
    public GameObject[] objectsToShow;


    //====================================================
    // OPTIONAL OBJECT FADE
    //====================================================

    [Header("Optional Object Fade")]
    [Tooltip("Assign a black UI Image. Leave empty if not needed.")]
    public Image objectFadeImage;

    public float objectFadeDuration = 1f;


    //====================================================
    // HIDE MAIN PARENT AFTER DIALOGUE
    //====================================================

    [Header("Hide Main Parent After Dialogue")]
    public bool hideMainParent = false;

    public GameObject mainParent;


    //====================================================
    // TYPING
    //====================================================

    [Header("Typing")]
    public float typingSpeed = 0.03f;


    //====================================================
    // OPTIONAL SCENE FADE
    //====================================================

    [Header("Optional Scene Fade")]
    [Tooltip("Assign a black UI Image. Leave empty if not needed.")]
    public Image fadeImage;

    [Tooltip("Leave empty if you don't want to load another scene.")]
    public string nextSceneName = "";

    public float fadeDuration = 1f;


    //====================================================
    // EVENTS
    //====================================================

    [Header("Events")]
    public UnityEvent onFinish;


    //====================================================
    // PRIVATE VARIABLES
    //====================================================

    private bool canClick = true;
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool ignoreNextClick = false;
    private bool isTransitioning = false;

    private Coroutine typingCoroutine;


    //====================================================
    // START
    //====================================================

    void Start()
    {
        currentIndex = 0;

        // Optional scene fade
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;

            fadeImage.gameObject.SetActive(true);
        }

        // Optional object fade
        if (objectFadeImage != null)
        {
            Color c = objectFadeImage.color;
            c.a = 0f;
            objectFadeImage.color = c;

            objectFadeImage.gameObject.SetActive(true);
        }

        // Make sure Dialogue Text exists
        if (dialogueText == null)
        {
            Debug.LogError(
                "[DialogueTyper] Dialogue Text is not assigned!",
                this
            );

            return;
        }

        // Make sure there are dialogue lines
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[DialogueTyper] No dialogue lines assigned.",
                this
            );

            return;
        }

        if (autoStartDialogue)
        {
            RestartDialogue();
        }
    }


    //====================================================
    // UPDATE
    //====================================================

    void Update()
    {
        if (isTransitioning)
            return;

        if (!canClick)
            return;

        // Ignore the click that originally started the dialogue
        if (ignoreNextClick)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ignoreNextClick = false;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //================================================
            // IF CURRENT LINE IS STILL TYPING
            //================================================

            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                dialogueText.text = dialogues[currentIndex];

                isTyping = false;
                typingCoroutine = null;
            }

            //================================================
            // OTHERWISE GO TO NEXT DIALOGUE
            //================================================

            else
            {
                NextDialogue();
            }
        }
    }


    //====================================================
    // NEXT DIALOGUE
    //====================================================

    void NextDialogue()
    {
        Debug.Log("NextDialogue BEFORE: " + currentIndex);

        currentIndex++;

        Debug.Log("NextDialogue AFTER: " + currentIndex);

        // End of dialogue
        if (currentIndex >= dialogues.Length)
        {
            FinishDialogue();
            return;
        }

        // Prepare optional features
        PrepareDialogue(dialogues[currentIndex]);

        // Start typing
        typingCoroutine = StartCoroutine(TypeText());
    }


    //====================================================
    // OPTIONAL DIALOGUE FEATURES
    //====================================================

    void PrepareDialogue(string text)
    {
        //================================================
        // AUTO RESIZE
        //================================================

        if (dialogueAutoSize != null)
        {
            // If the component is disabled, do nothing.
            if (dialogueAutoSize.enabled &&
                dialogueAutoSize.gameObject.activeInHierarchy)
            {
                dialogueAutoSize.ResizeForText(text);
            }
        }


        //================================================
        // POP ANIMATION
        //================================================

        if (popAnimation != null)
        {
            // If the component is disabled, do nothing.
            if (popAnimation.enabled &&
                popAnimation.gameObject.activeInHierarchy)
            {
                popAnimation.PlayPop();
            }
        }


        //================================================
        // TRIANGLE ANIMATION
        //================================================

        if (triangleAnimation != null)
        {
            // If the component is disabled, do nothing.
            if (triangleAnimation.enabled &&
                triangleAnimation.gameObject.activeInHierarchy)
            {
                triangleAnimation.PlayAnimation();
            }
        }
    }


    //====================================================
    // FINISH DIALOGUE
    //====================================================

    void FinishDialogue()
    {
        isTransitioning = true;

        // Optional dialogue bubble
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }


        //================================================
        // LOAD ANOTHER SCENE
        //================================================

        if (fadeImage != null &&
            !string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(FadeAndLoadScene());
            return;
        }


        //================================================
        // FADE OBJECTS
        //================================================

        if (objectFadeImage != null)
        {
            StartCoroutine(FadeAndShowObjects());
            return;
        }


        //================================================
        // SHOW OBJECTS
        //================================================

        ShowObjects();


        //================================================
        // EVENT
        //================================================

        if (onFinish != null)
        {
            onFinish.Invoke();
        }


        currentIndex = 0;
        isTransitioning = false;
    }


    //====================================================
    // RESTART DIALOGUE
    //====================================================

    public void RestartDialogue()
    {
        Debug.Log("RestartDialogue called");

        // Safety check
        if (dialogueText == null)
        {
            Debug.LogError(
                "[DialogueTyper] Cannot start dialogue. Dialogue Text is missing!",
                this
            );

            return;
        }

        if (dialogues == null ||
            dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[DialogueTyper] Cannot start dialogue. No dialogue lines exist.",
                this
            );

            return;
        }


        // Stop previous typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        // Reset variables
        currentIndex = 0;
        isTyping = false;
        isTransitioning = false;

        // Prevent the original button click from
        // immediately advancing the dialogue
        ignoreNextClick = true;


        // Clear text
        dialogueText.text = "";


        //================================================
        // OPTIONAL DIALOGUE BUBBLE
        //================================================

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }


        //================================================
        // OPTIONAL FEATURES
        //================================================

        PrepareDialogue(dialogues[currentIndex]);


        //================================================
        // START TYPING
        //================================================

        typingCoroutine = StartCoroutine(TypeText());


        //================================================
        // ENABLE CLICK AFTER SHORT DELAY
        //================================================

        StartCoroutine(EnableClickAfterDelay());
    }


    //====================================================
    // SHOW OBJECTS
    //====================================================

    void ShowObjects()
    {
        if (objectsToShow != null)
        {
            foreach (GameObject obj in objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }


        // Optional main parent
        if (hideMainParent &&
            mainParent != null)
        {
            mainParent.SetActive(false);
        }
    }


    //====================================================
    // TYPE TEXT
    //====================================================

    IEnumerator TypeText()
    {
        Debug.Log(
            "TypeText START - currentIndex = " +
            currentIndex
        );


        // Safety check
        if (dialogueText == null)
        {
            Debug.LogError(
                "[DialogueTyper] Dialogue Text became missing.",
                this
            );

            yield break;
        }


        if (dialogues == null ||
            currentIndex < 0 ||
            currentIndex >= dialogues.Length)
        {
            yield break;
        }


        isTyping = true;

        dialogueText.text = "";


        //================================================
        // TYPE EACH CHARACTER
        //================================================

        foreach (char letter in dialogues[currentIndex])
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }


        // Finished typing
        isTyping = false;
        typingCoroutine = null;


        Debug.Log(
            "TypeText END - currentIndex = " +
            currentIndex
        );
    }


    //====================================================
    // CLICK DELAY
    //====================================================

    IEnumerator EnableClickAfterDelay()
    {
        canClick = false;

        yield return new WaitForSeconds(0.2f);

        canClick = true;
    }


    //====================================================
    // FADE AND SHOW OBJECTS
    //====================================================

    IEnumerator FadeAndShowObjects()
    {
        if (objectFadeImage != null)
        {
            yield return StartCoroutine(
                FadeOut(
                    objectFadeImage,
                    objectFadeDuration
                )
            );
        }


        ShowObjects();

        // Disable this script after everything is finished
        enabled = false;
    }


    //====================================================
    // FADE AND LOAD SCENE
    //====================================================

    IEnumerator FadeAndLoadScene()
    {
        if (fadeImage != null)
        {
            yield return StartCoroutine(
                FadeOut(
                    fadeImage,
                    fadeDuration
                )
            );
        }


        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }


    //====================================================
    // FADE OUT
    //====================================================

    IEnumerator FadeOut(
        Image image,
        float duration)
    {
        if (image == null)
            yield break;


        float t = 0f;

        Color c = image.color;


        while (t < duration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(
                0f,
                1f,
                t / duration
            );

            image.color = c;

            yield return null;
        }


        c.a = 1f;

        image.color = c;
    }


    //====================================================
    // FADE IN
    //====================================================

    IEnumerator FadeIn(
        Image image,
        float duration)
    {
        if (image == null)
            yield break;


        float t = 0f;

        Color c = image.color;


        while (t < duration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(
                1f,
                0f,
                t / duration
            );

            image.color = c;

            yield return null;
        }


        c.a = 0f;

        image.color = c;
    }
}