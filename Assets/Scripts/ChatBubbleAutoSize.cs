using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ChatBubbleAutoSize : MonoBehaviour
{
    [Header("References")]
    public RectTransform bubbleRect;   // The "Bubble Mother" rect (background image)
    public TMP_Text bubbleText;        // The TMP child
    public Image bubbleImage;          // The background Image (to read native size)

    [Header("Box Sizing")]
    public float paddingX = 80f;       // Left+right padding combined
    public float paddingY = 100f;       // Top+bottom padding combined
    public float maxWidth = 500f;      // Cap before wrapping/font-shrink kicks in

    [Header("Font Auto-Scaling")]
    public bool enableFontScaling = true;
    public float maxFontSize = 36f;
    public float minFontSize = 30f;

    private Vector2 defaultSize;
    private string lastText;
    private bool initialized = false;

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
        Resize();
    }

    void Init()
    {
        if (bubbleRect == null) bubbleRect = GetComponent<RectTransform>();
        if (bubbleImage == null) bubbleImage = GetComponent<Image>();

        if (bubbleImage != null && bubbleImage.sprite != null)
        {
            defaultSize = new Vector2(
                bubbleImage.sprite.rect.width,
                bubbleImage.sprite.rect.height
            );
        }
        else if (!initialized)
        {
            defaultSize = bubbleRect.sizeDelta;
        }

        // Make TMP stretch to fill the bubble, with padding as inset offsets
        if (bubbleText != null)
        {
            RectTransform tr = bubbleText.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 0.5f);
            ApplyPadding(tr);

            if (enableFontScaling)
            {
                bubbleText.enableAutoSizing = true;
                bubbleText.fontSizeMax = maxFontSize;
                bubbleText.fontSizeMin = minFontSize;
            }
        }

        initialized = true;
    }

    void ApplyPadding(RectTransform tr)
    {
        // offsetMin = bottom-left inset, offsetMax = top-right inset (negative)
        tr.offsetMin = new Vector2(paddingX / 2f, paddingY / 2f);
        tr.offsetMax = new Vector2(-paddingX / 2f, -paddingY / 2f);
    }

    void Update()
    {
        if (bubbleText != null && bubbleText.text != lastText)
        {
            Resize();
            lastText = bubbleText.text;
        }
    }

    public void SetText(string newText)
    {
        bubbleText.text = newText;
        Resize();
    }

    public void Resize()
    {
        if (bubbleText == null || bubbleRect == null) return;
        if (defaultSize == Vector2.zero) Init();

        if (enableFontScaling)
        {
            bubbleText.enableAutoSizing = false;
            bubbleText.fontSize = maxFontSize;
        }

        bubbleText.ForceMeshUpdate();

        Vector2 unwrapped = bubbleText.GetPreferredValues(bubbleText.text, Mathf.Infinity, 0);
        float targetWidth = Mathf.Min(unwrapped.x + paddingX, maxWidth);
        targetWidth = Mathf.Max(targetWidth, defaultSize.x);

        Vector2 wrapped = bubbleText.GetPreferredValues(bubbleText.text, targetWidth - paddingX, 0);
        float targetHeight = Mathf.Max(wrapped.y + paddingY, defaultSize.y);

        // Resize the bubble background
        bubbleRect.sizeDelta = new Vector2(targetWidth, targetHeight);

        // TMP just re-applies padding offsets — it stretches automatically to the new bubble size
        ApplyPadding(bubbleText.rectTransform);

        if (enableFontScaling)
        {
            bubbleText.enableAutoSizing = true;
            bubbleText.fontSizeMax = maxFontSize;
            bubbleText.fontSizeMin = minFontSize;
        }
    }
}