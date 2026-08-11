using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnlineConversation : MonoBehaviour
{
    public enum Speaker
    {
        Char1,
        Char2,
        Char3,
        Char4
    }

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;

        public Speaker speaker;
    }

    [Header("Dialogue")]
    public Dialogue[] dialogues;

    [Header("Char1")]
    public GameObject char1Bubble;
    public TextMeshProUGUI char1TMP;

    [Header("Char2")]
    public GameObject char2Bubble;
    public TextMeshProUGUI char2TMP;

    [Header("Char3")]
    public GameObject char3Bubble;
    public TextMeshProUGUI char3TMP;

    [Header("Char4")]
    public GameObject char4Bubble;
    public TextMeshProUGUI char4TMP;

    // Optional: assign a ScrollRect if your bubbles live inside one and you want to auto-scroll to the newest message.
    public ScrollRect scrollRect;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Slide-Up Animation")]
    public float slideUpDuration = 0.6f;
    public float slideDistance = 80f;

    [Header("Next Scene (Optional)")]
    public string nextSceneName = ""; // Leave empty to use nextParent instead

    [Header("Next Parent (Fallback)")]
    public GameObject nextParent;

    [Header("Continue Button (Optional)")]
    // If assigned, this button appears after the last dialogue line finishes typing,
    // and the player must click it (instead of tapping the screen) to move on.
    // Leave empty to keep the old behavior of just clicking to continue.
    public GameObject continueButton;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // Track the currently-typing bubble's text component so FinishTyping can complete it.
    private TextMeshProUGUI currentTMP;

    void Start()
    {
        // Bubbles start hidden; each one is revealed (and stays visible) the first time
        // its character speaks, so nothing disappears afterward.
        if (char1Bubble != null) char1Bubble.SetActive(false);
        if (char2Bubble != null) char2Bubble.SetActive(false);
        if (char3Bubble != null) char3Bubble.SetActive(false);
        if (char4Bubble != null) char4Bubble.SetActive(false);

        if (dialogues.Length > 0)
            ShowDialogue();

        if (continueButton != null)
        {
            continueButton.SetActive(false);

            Button btn = continueButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(EndConversation);
            }
            else
            {
                Debug.LogWarning("continueButton has no Button component on it.");
            }
        }
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (isTyping)
        {
            FinishTyping();
            return;
        }

        bool onLastDialogue = currentIndex == dialogues.Length - 1;

        // If a continue button is assigned, the last line waits for the button press
        // instead of advancing (and ending) on a plain screen click.
        if (onLastDialogue && continueButton != null)
        {
            return;
        }

        NextDialogue();
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogues.Length)
        {
            EndConversation();
            return;
        }

        ShowDialogue();
    }

    void EndConversation()
    {
        // If a scene name is provided, load it
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        // Otherwise, activate the next parent GameObject if assigned
        else if (nextParent != null)
        {
            nextParent.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    void GetBubbleAndTMP(Speaker speaker, out GameObject bubble, out TextMeshProUGUI tmp)
    {
        switch (speaker)
        {
            case Speaker.Char1:
                bubble = char1Bubble;
                tmp = char1TMP;
                break;
            case Speaker.Char2:
                bubble = char2Bubble;
                tmp = char2TMP;
                break;
            case Speaker.Char3:
                bubble = char3Bubble;
                tmp = char3TMP;
                break;
            case Speaker.Char4:
                bubble = char4Bubble;
                tmp = char4TMP;
                break;
            default:
                bubble = null;
                tmp = null;
                break;
        }
    }

    void ShowDialogue()
    {
        Dialogue dialogue = dialogues[currentIndex];

        GetBubbleAndTMP(dialogue.speaker, out GameObject bubble, out TextMeshProUGUI tmp);

        if (bubble == null || tmp == null)
        {
            Debug.LogWarning($"No bubble/TMP assigned for {dialogue.speaker}");
            return;
        }

        // Note: previous bubbles are NOT hidden here, so earlier messages stay on screen.
        bubble.SetActive(true);
        tmp.text = "";
        currentTMP = tmp;

        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        if (bubbleRect != null)
        {
            StartCoroutine(SlideUpBubble(bubbleRect));
        }

        typingCoroutine = StartCoroutine(TypeText(tmp, dialogue.text));
    }

    IEnumerator TypeText(TextMeshProUGUI target, string text)
    {
        isTyping = true;

        target.text = "";

        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        ScrollToBottom();
        ShowContinueButtonIfLastDialogue();
    }

    IEnumerator SlideUpBubble(RectTransform rect)
    {
        CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();

        // Create CanvasGroup if it doesn't exist
        if (canvasGroup == null)
        {
            canvasGroup = rect.gameObject.AddComponent<CanvasGroup>();
        }

        // Store initial position
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + slideDistance);

        // Set starting state (below its final spot, invisible)
        rect.anchoredPosition = startPos - new Vector2(0, slideDistance);
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < slideUpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideUpDuration;

            // Ease-out animation (smooth deceleration)
            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);

            rect.anchoredPosition = Vector2.Lerp(startPos - new Vector2(0, slideDistance), endPos, easeProgress);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeProgress);

            yield return null;
        }

        // Ensure final state
        rect.anchoredPosition = endPos;
        canvasGroup.alpha = 1f;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        Dialogue dialogue = dialogues[currentIndex];

        if (currentTMP != null)
        {
            currentTMP.text = dialogue.text;
        }

        isTyping = false;

        ScrollToBottom();
        ShowContinueButtonIfLastDialogue();
    }

    void ShowContinueButtonIfLastDialogue()
    {
        if (continueButton == null)
            return;

        bool isLastDialogue = currentIndex == dialogues.Length - 1;
        if (isLastDialogue)
        {
            continueButton.SetActive(true);
        }
    }

    void ScrollToBottom()
    {
        if (scrollRect == null)
            return;

        // Wait a frame so layout groups can rebuild before snapping the scroll position.
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}