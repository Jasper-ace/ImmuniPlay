using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageManager : MonoBehaviour
{
    [Header("Chat UI")]
    public Transform content;
    public GameObject chatBubblePrefab;
    public ScrollRect chatScrollRect;

    [Header("Message Colors")]
    public Color playerMessageColor = Color.white;
    public Color npcMessageColor = Color.white;

    [Header("Typing Indicator")]
    public GameObject typingIndicatorPrefab;

    [Tooltip("How long the typing indicator stays visible before the NPC message appears.")]
    [Min(0f)]
    public float typingIndicatorDuration = 0.5f;

    [Header("Message Sounds")]
    public AudioClip playerMessagePopSound;
    [Range(0f, 1f)] public float playerMessagePopVolume = 1f;
    public AudioClip npcMessagePopSound;
    [Range(0f, 1f)] public float npcMessagePopVolume = 1f;

    [Header("Message Timing")]
    [Tooltip("Delay between each player/NPC message.")]
    [Min(0f)]
    public float messageDelay = 0.5f;

    [Header("Conversation")]
    public ChatExchange[] exchanges;

    [Header("Controls")]
    public Button chatButton;

    [Header("Myth / Fact")]
    public MythFactManager mythFactManager;

    private int currentExchangeIndex = 0;

    private bool isProcessingExchange = false;

    private void Start()
    {
        if (chatButton != null)
        {
            chatButton.onClick.AddListener(
                ShowNextExchange
            );
        }
    }

    // =========================================================
    // CHAT BUTTON
    // =========================================================

    public void ShowNextExchange()
    {
        if (isProcessingExchange)
        {
            return;
        }

        StartCoroutine(
            ShowNextExchangeCoroutine()
        );
    }

    // =========================================================
    // SHOW EXCHANGE
    // =========================================================

    private IEnumerator ShowNextExchangeCoroutine()
    {
        // -----------------------------------------------------
        // CHECK IF CONVERSATION IS FINISHED
        // -----------------------------------------------------

        if (exchanges == null ||
            currentExchangeIndex >= exchanges.Length)
        {
            EndConversation();
            yield break;
        }

        isProcessingExchange = true;

        // -----------------------------------------------------
        // HIDE CHAT BUTTON
        // -----------------------------------------------------

        if (chatButton != null)
        {
            chatButton.gameObject.SetActive(false);
        }

        ChatExchange exchange =
            exchanges[currentExchangeIndex];

        Debug.Log(
            "[ChatMessageManager] Starting Exchange " +
            currentExchangeIndex
        );

        // =====================================================
        // PLAYER MESSAGES
        // =====================================================

        if (exchange.playerMessages != null)
        {
            for (
                int i = 0;
                i < exchange.playerMessages.Length;
                i++
            )
            {
                ChatMessage message =
                    exchange.playerMessages[i];

                if (message == null)
                {
                    continue;
                }

                // Show player message
                CreateChatBubble(
                    message.message,
                    false
                );

                PlayMessagePopSound(false);

                yield return StartCoroutine(
                    ScrollToBottom()
                );

                Debug.Log(
                    "[ChatMessageManager] Player: " +
                    message.message
                );

                // Wait before next message
                if (i <
                    exchange.playerMessages.Length - 1)
                {
                    yield return new WaitForSeconds(
                        messageDelay
                    );
                }
            }
        }

        // =====================================================
        // TYPING INDICATOR
        // =====================================================

        if (exchange.npcMessages != null &&
            exchange.npcMessages.Length > 0)
        {
            GameObject typingRow =
                CreateTypingIndicator();

            if (typingRow != null)
            {
                Debug.Log(
                    "[ChatMessageManager] " +
                    "NPC typing..."
                );

                // Wait while typing indicator is visible
                yield return new WaitForSeconds(
                    typingIndicatorDuration
                );

                // Remove typing indicator
                Destroy(typingRow);

                Canvas.ForceUpdateCanvases();

                if (content != null)
                {
                    RectTransform contentRect =
                        content as RectTransform;

                    if (contentRect != null)
                    {
                        LayoutRebuilder
                            .ForceRebuildLayoutImmediate(
                                contentRect
                            );
                    }
                }

                Canvas.ForceUpdateCanvases();
            }
        }

        // =====================================================
        // NPC MESSAGES
        // =====================================================

        if (exchange.npcMessages != null)
        {
            for (
                int i = 0;
                i < exchange.npcMessages.Length;
                i++
            )
            {
                ChatMessage message =
                    exchange.npcMessages[i];

                if (message == null)
                {
                    continue;
                }

                // -------------------------------------------------
                // SHOW TYPING INDICATOR BEFORE EVERY NPC MESSAGE
                // -------------------------------------------------

                GameObject typingRow =
                    CreateTypingIndicator();

                if (typingRow != null)
                {
                    Debug.Log(
                        "[ChatMessageManager] " +
                        "NPC typing..."
                    );

                    // Wait while typing indicator is visible
                    yield return new WaitForSeconds(
                        typingIndicatorDuration
                    );

                    // Remove typing indicator
                    Destroy(typingRow);

                    Canvas.ForceUpdateCanvases();

                    RectTransform contentRect =
                        content as RectTransform;

                    if (contentRect != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(
                            contentRect
                        );
                    }

                    Canvas.ForceUpdateCanvases();
                }

                // -------------------------------------------------
                // SHOW NPC MESSAGE
                // -------------------------------------------------

                CreateChatBubble(
                    message.message,
                    true
                );

                PlayMessagePopSound(true);

                yield return StartCoroutine(
                    ScrollToBottom()
                );

                Debug.Log(
                    "[ChatMessageManager] NPC: " +
                    message.message
                );

                // Wait before the next NPC typing indicator
                if (i <
                    exchange.npcMessages.Length - 1)
                {
                    yield return new WaitForSeconds(
                        messageDelay
                    );
                }
            }
        }

        // =====================================================
        // FINISH EXCHANGE
        // =====================================================

        currentExchangeIndex++;
        isProcessingExchange = false;

        if (currentExchangeIndex >= exchanges.Length)
        {
            // Last chat is finished
            if (chatButton != null)
            {
                chatButton.gameObject.SetActive(false);
            }

            // Start Myth / Fact mini-game
            if (mythFactManager != null)
            {
                mythFactManager.StartMiniGame();
            }
        }
        else
        {
            // More chat exchanges remain
            if (chatButton != null)
            {
                chatButton.gameObject.SetActive(true);
            }
        }
    }

    private void PlayMessagePopSound(bool isNPC)
    {
        AudioClip clip = isNPC ? npcMessagePopSound : playerMessagePopSound;
        float volume = isNPC ? npcMessagePopVolume : playerMessagePopVolume;

        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume);
        }
    }

    // =========================================================
    // CREATE CHAT BUBBLE
    // =========================================================

    public void CreateChatBubble(
        string message,
        bool isNPC
    )
    {
        if (content == null)
        {
            Debug.LogWarning(
                "[ChatMessageManager] " +
                "Content is not assigned."
            );

            return;
        }

        if (chatBubblePrefab == null)
        {
            Debug.LogWarning(
                "[ChatMessageManager] " +
                "Chat Bubble Prefab is not assigned."
            );

            return;
        }

        // -----------------------------------------------------
        // CREATE MESSAGE ROW
        // -----------------------------------------------------

        GameObject messageRow =
            new GameObject("MessageRow");

        RectTransform rowRect =
            messageRow.AddComponent<RectTransform>();

        rowRect.anchorMin =
            new Vector2(0f, 1f);

        rowRect.anchorMax =
            new Vector2(1f, 1f);

        rowRect.pivot =
            new Vector2(0.5f, 1f);

        rowRect.localScale =
            Vector3.one;

        messageRow.transform.SetParent(
            content,
            false
        );

        // -----------------------------------------------------
        // HORIZONTAL LAYOUT
        // -----------------------------------------------------

        HorizontalLayoutGroup horizontal =
            messageRow.AddComponent<HorizontalLayoutGroup>();

        horizontal.padding =
            new RectOffset(
                20,
                20,
                0,
                0
            );

        horizontal.spacing = 0f;

        horizontal.childControlWidth = false;
        horizontal.childControlHeight = true;

        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;

        // NPC = LEFT
        // PLAYER = RIGHT

        if (isNPC)
        {
            horizontal.childAlignment =
                TextAnchor.UpperLeft;
        }
        else
        {
            horizontal.childAlignment =
                TextAnchor.UpperRight;
        }

        // -----------------------------------------------------
        // CONTENT SIZE FITTER
        // -----------------------------------------------------

        ContentSizeFitter rowFitter =
            messageRow.AddComponent<ContentSizeFitter>();

        rowFitter.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;

        rowFitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // -----------------------------------------------------
        // CREATE BUBBLE
        // -----------------------------------------------------

        GameObject newBubble =
            Instantiate(
                chatBubblePrefab,
                messageRow.transform
            );

        newBubble.name =
            "Chat Bubble";

        Image bubbleBackground =
            newBubble.transform
                .Find("Background")
                .GetComponent<Image>();

        if (bubbleBackground != null)
        {
            bubbleBackground.color =
                isNPC
                    ? npcMessageColor
                    : playerMessageColor;
        }

        ChatBubble bubble =
            newBubble.GetComponent<ChatBubble>();

        if (bubble != null)
        {
            bubble.Setup(message);
        }
        else
        {
            Debug.LogWarning(
                "[ChatMessageManager] " +
                "Chat Bubble prefab does not have " +
                "a ChatBubble component."
            );

            Destroy(messageRow);

            return;
        }

        // -----------------------------------------------------
        // FORCE LAYOUT UPDATE
        // -----------------------------------------------------

        Canvas.ForceUpdateCanvases();

        RectTransform bubbleRect =
            newBubble.GetComponent<RectTransform>();

        if (bubbleRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                bubbleRect
            );
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rowRect
        );

        RectTransform contentRect =
            content as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }

        Canvas.ForceUpdateCanvases();

        // -----------------------------------------------------
        // PLAYER ANIMATION
        // -----------------------------------------------------

        if (!isNPC)
        {
            bubble.PlayPlayerAnimation();
        }
    }

    // =========================================================
    // CREATE TYPING INDICATOR
    // =========================================================

    public GameObject CreateTypingIndicator()
    {
        if (content == null)
        {
            Debug.LogWarning(
                "[ChatMessageManager] " +
                "Content is not assigned."
            );

            return null;
        }

        if (typingIndicatorPrefab == null)
        {
            Debug.LogWarning(
                "[ChatMessageManager] " +
                "Typing Indicator Prefab is not assigned."
            );

            return null;
        }

        // -----------------------------------------------------
        // CREATE ROW
        // -----------------------------------------------------

        GameObject typingRow =
            new GameObject(
                "TypingIndicatorRow"
            );

        RectTransform rowRect =
            typingRow.AddComponent<RectTransform>();

        rowRect.anchorMin =
            new Vector2(0f, 1f);

        rowRect.anchorMax =
            new Vector2(1f, 1f);

        rowRect.pivot =
            new Vector2(0.5f, 1f);

        rowRect.localScale =
            Vector3.one;

        typingRow.transform.SetParent(
            content,
            false
        );

        // -----------------------------------------------------
        // HORIZONTAL LAYOUT
        // -----------------------------------------------------

        HorizontalLayoutGroup horizontal =
            typingRow.AddComponent<HorizontalLayoutGroup>();

        horizontal.padding =
            new RectOffset(
                20,
                20,
                0,
                0
            );

        horizontal.spacing = 0f;

        // NPC side
        horizontal.childAlignment =
            TextAnchor.UpperLeft;

        horizontal.childControlWidth = false;
        horizontal.childControlHeight = true;

        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;

        // -----------------------------------------------------
        // CONTENT SIZE FITTER
        // -----------------------------------------------------

        ContentSizeFitter rowFitter =
            typingRow.AddComponent<ContentSizeFitter>();

        rowFitter.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;

        rowFitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // -----------------------------------------------------
        // CREATE TYPING INDICATOR
        // -----------------------------------------------------

        GameObject indicator =
            Instantiate(
                typingIndicatorPrefab,
                typingRow.transform
            );

        indicator.name =
            "Typing Indicator";

        // -----------------------------------------------------
        // FORCE LAYOUT UPDATE
        // -----------------------------------------------------

        Canvas.ForceUpdateCanvases();

        RectTransform indicatorRect =
            indicator.GetComponent<RectTransform>();

        if (indicatorRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                indicatorRect
            );
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rowRect
        );

        RectTransform contentRect =
            content as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }

        Canvas.ForceUpdateCanvases();

        return typingRow;
    }

    // =========================================================
    // END CONVERSATION
    // =========================================================

    private void EndConversation()
    {
        Debug.Log(
            "[ChatMessageManager] " +
            "Conversation finished."
        );

        isProcessingExchange = false;

        if (chatButton != null)
        {
            chatButton.gameObject.SetActive(false);
        }
    }

    public IEnumerator ScrollToBottom()
    {
        // Let the layout system update first
        Canvas.ForceUpdateCanvases();

        RectTransform contentRect =
            content as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }

        // Wait one frame so the ScrollRect gets the
        // updated Content height
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}