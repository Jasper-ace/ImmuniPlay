using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform bubbleContainer;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Bubble Width")]
    [SerializeField] private float minWidth = 300f;
    [SerializeField] private float maxWidth = 600f;

    [Header("Bubble Height")]
    [SerializeField] private float minHeight = 80f;
    [SerializeField] private float maxHeight = 500f;

    [Header("Player Message Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private float startScale = 0.85f;
    [SerializeField] private float startYOffset = -20f;

    private LayoutElement layoutElement;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();

        if (layoutElement == null)
        {
            layoutElement =
                gameObject.AddComponent<LayoutElement>();
        }
    }

    public void Setup(string message)
    {
        if (bubbleContainer == null)
        {
            Debug.LogWarning(
                "[ChatBubble] Bubble Container is not assigned."
            );

            return;
        }

        if (messageText == null)
        {
            Debug.LogWarning(
                "[ChatBubble] Message Text is not assigned."
            );

            return;
        }

        messageText.text = message;

        messageText.enableWordWrapping = true;
        messageText.overflowMode =
            TextOverflowModes.Overflow;

        messageText.ForceMeshUpdate();

        ResizeBubble();
    }

    private void ResizeBubble()
    {
        string text = messageText.text;

        if (string.IsNullOrEmpty(text))
            return;

        // ------------------------------------------------
        // GET THE ACTUAL PADDING FROM THE TEXT RECT
        // ------------------------------------------------

        RectTransform textRect =
            messageText.rectTransform;

        float paddingLeft =
            textRect.offsetMin.x;

        float paddingRight =
            -textRect.offsetMax.x;

        float paddingTop =
            -textRect.offsetMax.y;

        float paddingBottom =
            textRect.offsetMin.y;

        float horizontalPadding =
            paddingLeft + paddingRight;

        float verticalPadding =
            paddingTop + paddingBottom;

        // ------------------------------------------------
        // 1. GET NATURAL TEXT WIDTH
        // ------------------------------------------------

        Vector2 naturalTextSize =
            messageText.GetPreferredValues(
                text,
                Mathf.Infinity,
                Mathf.Infinity
            );

        float requiredBubbleWidth =
            naturalTextSize.x +
            horizontalPadding;

        // ------------------------------------------------
        // 2. CLAMP BETWEEN MIN AND MAX
        // ------------------------------------------------

        float finalWidth =
            Mathf.Clamp(
                requiredBubbleWidth,
                minWidth,
                maxWidth
            );

        // ------------------------------------------------
        // 3. CALCULATE AVAILABLE TEXT WIDTH
        // ------------------------------------------------

        float availableTextWidth =
            finalWidth -
            horizontalPadding;

        // ------------------------------------------------
        // 4. GET TEXT HEIGHT AFTER WRAPPING
        // ------------------------------------------------

        Vector2 wrappedTextSize =
            messageText.GetPreferredValues(
                text,
                availableTextWidth,
                Mathf.Infinity
            );

        float requiredBubbleHeight =
            wrappedTextSize.y +
            verticalPadding;

        // ------------------------------------------------
        // 5. CLAMP HEIGHT
        // ------------------------------------------------

        float finalHeight =
            Mathf.Clamp(
                requiredBubbleHeight,
                minHeight,
                maxHeight
            );

        // ------------------------------------------------
        // 6. APPLY BUBBLE SIZE
        // ------------------------------------------------

        bubbleContainer.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            finalWidth
        );

        bubbleContainer.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            finalHeight
        );

        // ------------------------------------------------
        // 7. TELL UNITY THE PREFERRED SIZE
        // ------------------------------------------------

        layoutElement.minWidth =
            finalWidth;

        layoutElement.preferredWidth =
            finalWidth;

        layoutElement.flexibleWidth = 0f;

        layoutElement.minHeight =
            finalHeight;

        layoutElement.preferredHeight =
            finalHeight;

        layoutElement.flexibleHeight = 0f;

        // ------------------------------------------------
        // 8. FORCE LAYOUT UPDATE
        // ------------------------------------------------

        messageText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            bubbleContainer
        );

        RectTransform row =
            bubbleContainer.parent
            as RectTransform;

        if (row != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                row
            );
        }

        Debug.Log(
            "[ChatBubble] " +
            "Natural Text Width: " +
            naturalTextSize.x +
            " | Bubble Width: " +
            finalWidth +
            " | Text Height: " +
            wrappedTextSize.y +
            " | Bubble Height: " +
            finalHeight
        );
    }

    public void PlayPlayerAnimation()
    {
        StartCoroutine(PlayerMessageAnimation());
    }

    private IEnumerator PlayerMessageAnimation()
    {
        RectTransform rect =
            bubbleContainer;

        Vector3 originalScale =
            rect.localScale;

        Vector2 originalPosition =
            rect.anchoredPosition;

        // Starting state
        rect.localScale =
            originalScale * startScale;

        rect.anchoredPosition =
            originalPosition +
            new Vector2(0f, startYOffset);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / animationDuration
                );

            // Smooth easing
            float eased =
                1f - Mathf.Pow(1f - t, 3f);

            rect.localScale =
                Vector3.Lerp(
                    originalScale * startScale,
                    originalScale,
                    eased
                );

            rect.anchoredPosition =
                Vector2.Lerp(
                    originalPosition +
                    new Vector2(0f, startYOffset),
                    originalPosition,
                    eased
                );

            yield return null;
        }

        // Make absolutely sure it ends at the correct position
        rect.localScale =
            originalScale;

        rect.anchoredPosition =
            originalPosition;
    }
}