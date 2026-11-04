using UnityEngine;
using TMPro;

public class DialogueBoxAutoSize : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Width Settings")]
    [SerializeField] private float minWidth = 1000f;
    [SerializeField] private float maxWidth = 1500f;
    [SerializeField] private float paddingX = 50f;

    [Header("Height Settings")]
    [SerializeField] private float minHeight = 120f;
    [SerializeField] private float maxHeight = 500f;
    [SerializeField] private float paddingY = 40f;

    /// <summary>
    /// Resizes the dialogue box based on the COMPLETE dialogue.
    /// Call this BEFORE starting the typewriter.
    /// </summary>
    public void ResizeForText(string fullText)
    {
        if (dialogueContainer == null || dialogueText == null)
            return;

        if (string.IsNullOrEmpty(fullText))
            return;

        // ------------------------------------------------
        // STEP 1: Temporarily use the COMPLETE dialogue
        // ------------------------------------------------

        dialogueText.text = fullText;

        dialogueText.ForceMeshUpdate();

        // ------------------------------------------------
        // STEP 2: Find how wide the text naturally wants
        // to be WITHOUT restricting it to the minimum width
        // ------------------------------------------------

        Vector2 naturalSize = dialogueText.GetPreferredValues(
            fullText,
            Mathf.Infinity,
            Mathf.Infinity
        );

        float requiredWidth = naturalSize.x + paddingX;

        // ------------------------------------------------
        // STEP 3: Width stays between MIN and MAX
        // ------------------------------------------------

        float finalWidth = Mathf.Clamp(
            requiredWidth,
            minWidth,
            maxWidth
        );

        // ------------------------------------------------
        // STEP 4: Now calculate how much HEIGHT the text
        // needs using the final width
        // ------------------------------------------------

        float availableTextWidth = finalWidth - paddingX;

        Vector2 wrappedTextSize = dialogueText.GetPreferredValues(
            fullText,
            availableTextWidth,
            Mathf.Infinity
        );

        float requiredHeight = wrappedTextSize.y + paddingY;

        // ------------------------------------------------
        // STEP 5: Height stays between MIN and MAX
        // ------------------------------------------------

        float finalHeight = Mathf.Clamp(
            requiredHeight,
            minHeight,
            maxHeight
        );

        // ------------------------------------------------
        // STEP 6: Apply final size to the dialogue bubble
        // ------------------------------------------------

        dialogueContainer.sizeDelta = new Vector2(
            finalWidth,
            finalHeight
        );
    }

    /// <summary>
    /// Manually resize using whatever text is currently
    /// inside the TMP component.
    /// </summary>
    public void ResizeBubble()
    {
        if (dialogueText == null)
            return;

        ResizeForText(dialogueText.text);
    }
}