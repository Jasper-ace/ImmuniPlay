using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SideMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform menuPanel;
    public CanvasGroup scrimCanvasGroup;
    public GameObject hamburgerButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.4f;
    public AnimationCurve easingCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Navigation Scene Names")]
    public string storySceneName = "Chapters";
    public string homeSceneName = "TitleScene";

    [Header("Quiz Settings")]
    [Tooltip("Must match the chapter JSON/save name, such as Chapter1, Chapter2, etc.")]
    public string chapterFileName = "Chapter1";

    [Tooltip("Scene to load after the quiz is completed.")]
    public string nextChapterScene = "Scene13";

    [Tooltip("Lock icon shown when the quiz has not been attempted.")]
    public GameObject quizLockIcon;

    // There is only ONE shared quiz scene.
    private const string QUIZ_SCENE_NAME = "ChapterQuiz";

    private bool isOpen = false;
    private Coroutine transitionCoroutine;

    private Vector2 closedPosition;
    private Vector2 openedPosition;

    private SceneFade fadeManager;

    private void Start()
    {
        fadeManager =
            Object.FindFirstObjectByType<SceneFade>();

        // ---------------------------------------------------------
        // Find Hamburger Button
        // ---------------------------------------------------------

        if (hamburgerButton == null && transform.parent != null)
        {
            var burgerTransform =
                transform.parent.Find("HamburgerButton");

            if (burgerTransform != null)
            {
                hamburgerButton =
                    burgerTransform.gameObject;
            }
        }

        // ---------------------------------------------------------
        // Menu Positions
        // ---------------------------------------------------------

        if (menuPanel != null)
        {
            float panelWidth =
                menuPanel.rect.width;

            openedPosition =
                new Vector2(
                    0f,
                    menuPanel.anchoredPosition.y
                );

            closedPosition =
                new Vector2(
                    -panelWidth,
                    menuPanel.anchoredPosition.y
                );

            menuPanel.anchoredPosition =
                closedPosition;
        }

        // ---------------------------------------------------------
        // Scrim
        // ---------------------------------------------------------

        if (scrimCanvasGroup != null)
        {
            scrimCanvasGroup.alpha = 0f;
            scrimCanvasGroup.blocksRaycasts = false;
        }

        // ---------------------------------------------------------
        // Hamburger Button
        // ---------------------------------------------------------

        if (hamburgerButton != null)
        {
            var burgerBtn =
                hamburgerButton.GetComponent<Button>();

            if (burgerBtn != null)
            {
                burgerBtn.onClick.RemoveAllListeners();
                burgerBtn.onClick.AddListener(ToggleMenu);
            }
        }

        // ---------------------------------------------------------
        // Scrim Button
        // ---------------------------------------------------------

        if (scrimCanvasGroup != null)
        {
            var scrimBtn =
                scrimCanvasGroup.GetComponent<Button>();

            if (scrimBtn != null)
            {
                scrimBtn.onClick.RemoveAllListeners();
                scrimBtn.onClick.AddListener(CloseMenu);
            }
        }

        // ---------------------------------------------------------
        // Story Button
        // ---------------------------------------------------------

        if (menuPanel != null)
        {
            var storyBtn =
                menuPanel
                    .Find("ButtonContainer/StoryButton")
                    ?.GetComponent<Button>();

            if (storyBtn != null)
            {
                storyBtn.onClick.RemoveAllListeners();
                storyBtn.onClick.AddListener(
                    OnStoryClicked
                );
            }

            // -----------------------------------------------------
            // Quiz Button
            // -----------------------------------------------------

            var quizBtn =
                menuPanel
                    .Find("ButtonContainer/QuizButton")
                    ?.GetComponent<Button>();

            if (quizBtn != null)
            {
                quizBtn.onClick.RemoveAllListeners();
                quizBtn.onClick.AddListener(
                    OnQuizClicked
                );

                // Update quiz lock state
                UpdateQuizButtonState(quizBtn);
            }

            // -----------------------------------------------------
            // Settings Button
            // -----------------------------------------------------

            var settingsBtn =
                menuPanel
                    .Find("ButtonContainer/SettingsButton")
                    ?.GetComponent<Button>();

            if (settingsBtn != null)
            {
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(
                    OnSettingsClicked
                );
            }

            // -----------------------------------------------------
            // Home Button
            // -----------------------------------------------------

            var homeBtn =
                menuPanel
                    .Find("ButtonContainer/HomeButton")
                    ?.GetComponent<Button>();

            if (homeBtn != null)
            {
                homeBtn.onClick.RemoveAllListeners();
                homeBtn.onClick.AddListener(
                    OnHomeClicked
                );
            }
        }

        // ---------------------------------------------------------
        // Hamburger Visible
        // ---------------------------------------------------------

        if (hamburgerButton != null)
        {
            hamburgerButton.SetActive(true);
        }
    }

    // =============================================================
    // QUIZ LOCK SYSTEM
    // =============================================================

    private void UpdateQuizButtonState(Button quizButton)
    {
        if (quizButton == null)
            return;

        // Safety check
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[SideMenu] SaveManager not found. " +
                "Quiz button will remain disabled."
            );

            quizButton.interactable = false;

            if (quizLockIcon != null)
                quizLockIcon.SetActive(true);

            return;
        }

        if (string.IsNullOrEmpty(chapterFileName))
        {
            Debug.LogWarning(
                "[SideMenu] Chapter File Name is empty."
            );

            quizButton.interactable = false;

            if (quizLockIcon != null)
                quizLockIcon.SetActive(true);

            return;
        }

        // Check the SaveManager
        bool quizAttempted =
            SaveManager.Instance.IsQuizAttempted(
                chapterFileName
            );

        // Enable / disable button
        quizButton.interactable = quizAttempted;

        // Show / hide lock
        if (quizLockIcon != null)
        {
            quizLockIcon.SetActive(!quizAttempted);
        }

        Debug.Log(
            $"[SideMenu] Quiz Button State\n" +
            $"Chapter: {chapterFileName}\n" +
            $"Quiz Attempted: {quizAttempted}\n" +
            $"Button Enabled: {quizButton.interactable}\n" +
            $"Lock Visible: {!quizAttempted}"
        );
    }

    // =============================================================
    // MENU
    // =============================================================

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSideMenuAudioPaused(true);

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine =
            StartCoroutine(
                AnimateTransition(true)
            );
    }

    public void CloseMenu()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSideMenuAudioPaused(false);

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine =
            StartCoroutine(
                AnimateTransition(false)
            );
    }

    private IEnumerator AnimateTransition(bool open)
    {
        float elapsed = 0f;

        Vector2 startPos =
            menuPanel != null
                ? menuPanel.anchoredPosition
                : Vector2.zero;

        Vector2 targetPos =
            open
                ? openedPosition
                : closedPosition;

        float startAlpha =
            scrimCanvasGroup != null
                ? scrimCanvasGroup.alpha
                : 0f;

        float targetAlpha =
            open
                ? 1f
                : 0f;

        if (scrimCanvasGroup != null && open)
            scrimCanvasGroup.blocksRaycasts = true;

        if (hamburgerButton != null)
            hamburgerButton.SetActive(!open);

        while (elapsed < animationDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / animationDuration
                );

            float curveT =
                easingCurve.Evaluate(t);

            if (menuPanel != null)
            {
                menuPanel.anchoredPosition =
                    Vector2.Lerp(
                        startPos,
                        targetPos,
                        curveT
                    );
            }

            if (scrimCanvasGroup != null)
            {
                scrimCanvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        targetAlpha,
                        curveT
                    );
            }

            yield return null;
        }

        if (menuPanel != null)
            menuPanel.anchoredPosition = targetPos;

        if (scrimCanvasGroup != null)
        {
            scrimCanvasGroup.alpha = targetAlpha;
            scrimCanvasGroup.blocksRaycasts = open;
        }

        if (hamburgerButton != null)
            hamburgerButton.SetActive(!open);

        transitionCoroutine = null;
    }

    // =============================================================
    // STORY
    // =============================================================

    public void OnStoryClicked()
    {
        CloseMenu();
        LoadSceneWithName(storySceneName);
    }

    // =============================================================
    // QUIZ
    // =============================================================

    public void OnQuizClicked()
    {
        // Double safety check
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[SideMenu] SaveManager instance not found."
            );
            return;
        }

        if (string.IsNullOrEmpty(chapterFileName))
        {
            Debug.LogError(
                "[SideMenu] Chapter File Name is empty."
            );
            return;
        }

        // Check if this chapter's quiz was attempted
        bool quizAttempted =
            SaveManager.Instance.IsQuizAttempted(
                chapterFileName
            );

        if (!quizAttempted)
        {
            Debug.Log(
                "[SideMenu] Quiz is locked for "
                + chapterFileName
            );

            return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAllSounds();

        CloseMenu();

        if (string.IsNullOrEmpty(nextChapterScene))
        {
            Debug.LogWarning(
                "[SideMenu] Next Chapter Scene is empty."
            );
        }

        // Send data to the shared ChapterQuiz scene
        QuizSceneData.chapterFileName =
            chapterFileName;

        QuizSceneData.nextChapterScene =
            nextChapterScene;

        Debug.Log(
            $"[SideMenu] Opening Quiz\n" +
            $"Chapter: {chapterFileName}\n" +
            $"Next Chapter: {nextChapterScene}"
        );

        LoadSceneWithName(
            QUIZ_SCENE_NAME
        );
    }

    // =============================================================
    // SETTINGS
    // =============================================================

    public void OnSettingsClicked()
    {
        SettingsController settingsController =
            Object.FindFirstObjectByType<SettingsController>();

        if (settingsController != null)
        {
            // Open settings first. Its AudioPauseWhileEnabled component
            // requests a settings pause before the menu releases its pause.
            settingsController.OpenSettings();
        }
        else
        {
            Debug.LogWarning(
                "[SideMenu] SettingsController not found in the scene."
            );
        }

        CloseMenu();
    }

    // =============================================================
    // HOME
    // =============================================================

    public void OnHomeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAllSounds();

        CloseMenu();
        LoadSceneWithName(homeSceneName);
    }

    // =============================================================
    // SCENE LOADING
    // =============================================================

    private void LoadSceneWithName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning(
                "[SideMenu] Scene name is empty."
            );

            return;
        }

        if (SceneManager.GetActiveScene().name == name)
        {
            Debug.Log(
                $"[SideMenu] Already in scene: {name}"
            );

            return;
        }

        if (fadeManager != null)
        {
            fadeManager.FadeToScene(name);
        }
        else
        {
            SceneManager.LoadScene(name);
        }
    }
}