using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MythFactManager : MonoBehaviour
{
    [Header("Myth / Fact Separator")]
    public GameObject mythFactSeparatorPrefab;

    [Header("Chat")]
    public ChatMessageManager chatMessageManager;

    [Header("Answer Buttons")]
    public Button mythButton;
    public Button factButton;
    public Button nextButton;

    [Header("Typing Indicator")]
    public GameObject typingIndicatorPrefab;

    [Tooltip("How long the typing indicator stays visible.")]
    [Min(0f)]
    public float typingIndicatorDuration = 0.5f;

    [Header("Message Timing")]
    [Tooltip("Delay between chat messages.")]
    [Min(0f)]
    public float messageDelay = 0.5f;

    [Header("Message Pop Sounds")]
    public AudioClip playerMessagePopSound;
    [Range(0f, 1f)]
    public float playerMessagePopVolume = 1f;

    public AudioClip npcMessagePopSound;
    [Range(0f, 1f)]
    public float npcMessagePopVolume = 1f;

    [Header("JSON")]
    public string jsonFileName = "Chapter3MythFact";

    [Header("Mini-Game Settings")]
    [Min(1)]
    public int statementsToShow = 5;

    [Header("Scene Transition")]
    public string nextSceneName;

    private MythFactData mythFactData;

    private int currentStatementIndex = 0;

    private List<int> randomizedStatementIndices =
        new List<int>();

    public void StartMiniGame()
    {
        Debug.Log(
            "[MythFactManager] Mini-game started."
        );

        LoadJSON();

        if (mythFactData == null ||
            mythFactData.statements == null ||
            mythFactData.statements.Length == 0)
        {
            Debug.LogError(
                "[MythFactManager] No statements found."
            );

            return;
        }

        currentStatementIndex = 0;

        CreateRandomizedStatementList();

        CreateMythFactSeparator();

        // Hide buttons while Statement 1 is typing
        if (mythButton != null)
            mythButton.gameObject.SetActive(false);

        if (factButton != null)
            factButton.gameObject.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        // Show Statement 1 with typing indicator
        StartCoroutine(
            ShowStatementWithTyping()
        );
    }

    private void LoadJSON()
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>(
                "MythFact/" + jsonFileName
            );

        if (jsonFile == null)
        {
            Debug.LogError(
                "[MythFactManager] Could not find JSON: " +
                "MythFact/" + jsonFileName
            );

            return;
        }

        mythFactData =
            JsonUtility.FromJson<MythFactData>(
                jsonFile.text
            );

        if (mythFactData == null ||
            mythFactData.statements == null)
        {
            Debug.LogError(
                "[MythFactManager] Invalid JSON data."
            );

            return;
        }

        Debug.Log(
            "[MythFactManager] Loaded " +
            mythFactData.statements.Length +
            " statements."
        );
    }

    private void CreateRandomizedStatementList()
    {
        randomizedStatementIndices.Clear();

        if (mythFactData == null ||
            mythFactData.statements == null)
        {
            return;
        }

        // Add all available statement indices
        for (
            int i = 0;
            i < mythFactData.statements.Length;
            i++
        )
        {
            randomizedStatementIndices.Add(i);
        }

        // Shuffle statements only
        // The Myth/Fact answer buttons are NOT randomized.
        for (
            int i = randomizedStatementIndices.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(0, i + 1);

            int temp =
                randomizedStatementIndices[i];

            randomizedStatementIndices[i] =
                randomizedStatementIndices[randomIndex];

            randomizedStatementIndices[randomIndex] =
                temp;
        }

        // Prevent showing more statements than exist
        statementsToShow =
            Mathf.Clamp(
                statementsToShow,
                1,
                randomizedStatementIndices.Count
            );

        Debug.Log(
            "[MythFactManager] Will show " +
            statementsToShow +
            " randomized statements."
        );
    }

    private void ShowCurrentStatement()
    {
        if (mythFactData == null ||
            mythFactData.statements == null ||
            randomizedStatementIndices == null ||
            currentStatementIndex >= statementsToShow)
        {
            return;
        }

        // Get the randomized JSON index
        int randomizedIndex =
            randomizedStatementIndices[
                currentStatementIndex
            ];

        MythFactStatement statement =
            mythFactData.statements[
                randomizedIndex
            ];

        if (chatMessageManager != null)
        {
            chatMessageManager.CreateChatBubble(
                "Statement " +
                (currentStatementIndex + 1) +
                ": " +
                statement.statement,
                true
            );
        }
    }

    private IEnumerator ShowStatementWithTyping()
    {
        GameObject typingRow = null;

        if (chatMessageManager != null)
        {
            typingRow =
                chatMessageManager.CreateTypingIndicator();
        }

        if (typingRow != null)
        {
            yield return new WaitForSeconds(
                typingIndicatorDuration
            );

            Destroy(typingRow);

            Canvas.ForceUpdateCanvases();
        }

        // Show statement
        ShowCurrentStatement();

        // Auto-scroll
        if (chatMessageManager != null)
        {
            yield return StartCoroutine(
                chatMessageManager.ScrollToBottom()
            );
        }

        // Show answer buttons
        if (mythButton != null)
            mythButton.gameObject.SetActive(true);

        if (factButton != null)
            factButton.gameObject.SetActive(true);
    }

    private void SubmitAnswer(string playerAnswer)
    {
        if (mythFactData == null ||
            mythFactData.statements == null ||
            randomizedStatementIndices == null ||
            currentStatementIndex >= statementsToShow)
        {
            return;
        }

        // Get the actual randomized statement
        int randomizedIndex =
            randomizedStatementIndices[
                currentStatementIndex
            ];

        MythFactStatement statement =
            mythFactData.statements[
                randomizedIndex
            ];

        // Check the answer belonging to THIS statement
        bool isCorrect =
            playerAnswer.Equals(
                statement.correctAnswer,
                System.StringComparison.OrdinalIgnoreCase
            );

        // Hide answer buttons
        if (mythButton != null)
            mythButton.gameObject.SetActive(false);

        if (factButton != null)
            factButton.gameObject.SetActive(false);

        // Show player's answer
        if (chatMessageManager != null)
        {
            chatMessageManager.CreateChatBubble(
                playerAnswer,
                false
            );

            PlayMessagePopSound(false);

            StartCoroutine(
                chatMessageManager.ScrollToBottom()
            );
        }

        StartCoroutine(
            ShowAnswerResult(
                statement,
                isCorrect
            )
        );
    }

    private IEnumerator ShowAnswerResult(
        MythFactStatement statement,
        bool isCorrect
    )
    {
        // Delay after player's answer
        yield return new WaitForSeconds(
            messageDelay
        );

        // NPC typing indicator
        GameObject typingRow = null;

        if (chatMessageManager != null)
        {
            typingRow =
                chatMessageManager.CreateTypingIndicator();
        }

        if (typingRow != null)
        {
            yield return new WaitForSeconds(
                typingIndicatorDuration
            );

            Destroy(typingRow);

            Canvas.ForceUpdateCanvases();
        }

        // Result message
        string resultMessage;

        if (isCorrect)
        {
            resultMessage =
                "Yes, that is a " +
                statement.correctAnswer.ToLower() +
                ".";
        }
        else
        {
            resultMessage =
                "No, that is a " +
                statement.correctAnswer.ToLower() +
                ".";
        }

        // Show result
        if (chatMessageManager != null)
        {
            chatMessageManager.CreateChatBubble(
                resultMessage,
                true
            );

            PlayMessagePopSound(true);

            yield return StartCoroutine(
                chatMessageManager.ScrollToBottom()
            );
        }

        // Delay before explanation
        yield return new WaitForSeconds(
            messageDelay
        );

        // Typing indicator before explanation
        typingRow = null;

        if (chatMessageManager != null)
        {
            typingRow =
                chatMessageManager.CreateTypingIndicator();
        }

        if (typingRow != null)
        {
            yield return new WaitForSeconds(
                typingIndicatorDuration
            );

            Destroy(typingRow);

            Canvas.ForceUpdateCanvases();
        }

        // Explanation
        if (chatMessageManager != null)
        {
            chatMessageManager.CreateChatBubble(
                statement.explanation,
                true
            );

            PlayMessagePopSound(true);

            yield return StartCoroutine(
                chatMessageManager.ScrollToBottom()
            );
        }

        // Show Next button
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator ShowNextStatement()
    {
        currentStatementIndex++;

        // Hide Next button
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        // Check if we've reached the selected number
        if (currentStatementIndex >= statementsToShow)
        {
            Debug.Log(
                "[MythFactManager] Myth/Fact mini-game completed."
            );

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(
                    nextSceneName
                );
            }
            else
            {
                Debug.LogWarning(
                    "[MythFactManager] Next Scene Name is empty."
                );
            }

            yield break;
        }

        // Show typing indicator
        GameObject typingRow = null;

        if (chatMessageManager != null)
        {
            typingRow =
                chatMessageManager.CreateTypingIndicator();
        }

        // Wait for typing animation
        if (typingRow != null)
        {
            yield return new WaitForSeconds(
                typingIndicatorDuration
            );

            Destroy(typingRow);

            Canvas.ForceUpdateCanvases();
        }

        // Show next randomized NPC statement
        ShowCurrentStatement();

        PlayMessagePopSound(true);

        // Auto-scroll
        if (chatMessageManager != null)
        {
            yield return StartCoroutine(
                chatMessageManager.ScrollToBottom()
            );
        }

        // Show answer buttons
        if (mythButton != null)
        {
            mythButton.gameObject.SetActive(true);
        }

        if (factButton != null)
        {
            factButton.gameObject.SetActive(true);
        }
    }

    private void PlayMessagePopSound(bool isNPC)
    {
        AudioClip clip = isNPC
            ? npcMessagePopSound
            : playerMessagePopSound;

        float volume = isNPC
            ? npcMessagePopVolume
            : playerMessagePopVolume;

        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume);
        }
    }

    private void Awake()
    {
        if (mythButton != null)
        {
            mythButton.onClick.AddListener(
                () => SubmitAnswer("Myth")
            );
        }

        if (factButton != null)
        {
            factButton.onClick.AddListener(
                () => SubmitAnswer("Fact")
            );
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(
                () => StartCoroutine(
                    ShowNextStatement()
                )
            );
        }
    }

    private void CreateMythFactSeparator()
    {
        if (mythFactSeparatorPrefab == null ||
            chatMessageManager == null)
        {
            return;
        }

        Instantiate(
            mythFactSeparatorPrefab,
            chatMessageManager.content
        );
    }
}