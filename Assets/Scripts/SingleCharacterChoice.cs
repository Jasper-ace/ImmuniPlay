using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SingleCharacterChoice : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;
    }

    [Header("Dialogue")]
    public Dialogue[] dialogues;

    [Header("Character")]
    public GameObject charBubble;
    public TextMeshProUGUI charTMP;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Slide-Up Animation")]
    public float slideUpDuration = 0.6f;
    public float slideDistance = 80f;

    [Header("Choice Button 1")]
    public GameObject choiceButton1;      // Needs a Button component
    public GameObject nextParent1;        // Activated when button 1 is clicked

    [Header("Choice Button 2")]
    public GameObject choiceButton2;      // Needs a Button component
    public GameObject nextParent2;        // Activated when button 2 is clicked

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (charBubble != null)
            charBubble.SetActive(false);

        if (choiceButton1 != null)
        {
            choiceButton1.SetActive(false);

            Button btn1 = choiceButton1.GetComponent<Button>();
            if (btn1 != null)
                btn1.onClick.AddListener(() => ChooseOption(nextParent1));
            else
                Debug.LogWarning("choiceButton1 has no Button component on it.");
        }

        if (choiceButton2 != null)
        {
            choiceButton2.SetActive(false);

            Button btn2 = choiceButton2.GetComponent<Button>();
            if (btn2 != null)
                btn2.onClick.AddListener(() => ChooseOption(nextParent2));
            else
                Debug.LogWarning("choiceButton2 has no Button component on it.");
        }

        if (dialogues.Length > 0)
            ShowDialogue();
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

        // Once the last line is showing, screen taps do nothing —
        // the player must pick one of the two buttons to continue.
        if (onLastDialogue)
            return;

        NextDialogue();
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogues.Length)
        {
            // Shouldn't normally be reached since Update() stops advancing
            // on the last line, but guard against it just in case.
            currentIndex = dialogues.Length - 1;
            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (charBubble == null || charTMP == null)
        {
            Debug.LogWarning("charBubble or charTMP is not assigned.");
            return;
        }

        Dialogue dialogue = dialogues[currentIndex];

        charBubble.SetActive(true);
        charTMP.text = "";

        RectTransform bubbleRect = charBubble.GetComponent<RectTransform>();
        if (bubbleRect != null)
        {
            StartCoroutine(SlideUpBubble(bubbleRect));
        }

        typingCoroutine = StartCoroutine(TypeText(charTMP, dialogue.text));
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

        ShowChoiceButtonsIfLastDialogue();
    }

    IEnumerator SlideUpBubble(RectTransform rect)
    {
        CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = rect.gameObject.AddComponent<CanvasGroup>();
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + slideDistance);

        rect.anchoredPosition = startPos - new Vector2(0, slideDistance);
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < slideUpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideUpDuration;

            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);

            rect.anchoredPosition = Vector2.Lerp(startPos - new Vector2(0, slideDistance), endPos, easeProgress);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeProgress);

            yield return null;
        }

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

        if (charTMP != null)
        {
            charTMP.text = dialogue.text;
        }

        isTyping = false;

        ShowChoiceButtonsIfLastDialogue();
    }

    void ShowChoiceButtonsIfLastDialogue()
    {
        bool isLastDialogue = currentIndex == dialogues.Length - 1;
        if (!isLastDialogue)
            return;

        if (choiceButton1 != null)
            choiceButton1.SetActive(true);

        if (choiceButton2 != null)
            choiceButton2.SetActive(true);
    }

    void ChooseOption(GameObject parentToActivate)
    {
        if (parentToActivate != null)
        {
            parentToActivate.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}