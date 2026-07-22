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
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Navigation Scene Names")]
    public string storySceneName = "Chapters";
    public string quizSceneName = "Chapters";
    public string homeSceneName = "TitleScene";

    private bool isOpen = false;
    private Coroutine transitionCoroutine;
    private Vector2 closedPosition;
    private Vector2 openedPosition;
    private SceneFade fadeManager;

    private void Start()
    {
        // Find existing SceneFade manager in the scene for consistent transition animations
        fadeManager = Object.FindFirstObjectByType<SceneFade>();

        if (hamburgerButton == null && transform.parent != null)
        {
            var burgerTransform = transform.parent.Find("HamburgerButton");
            if (burgerTransform != null)
            {
                hamburgerButton = burgerTransform.gameObject;
            }
        }

        if (menuPanel != null)
        {
            // Calculate opened and closed positions based on panel width
            float panelWidth = menuPanel.rect.width;
            openedPosition = new Vector2(0f, menuPanel.anchoredPosition.y);
            closedPosition = new Vector2(-panelWidth, menuPanel.anchoredPosition.y);
            
            // Set initial state to closed
            menuPanel.anchoredPosition = closedPosition;
        }

        if (scrimCanvasGroup != null)
        {
            scrimCanvasGroup.alpha = 0f;
            scrimCanvasGroup.blocksRaycasts = false;
        }

        // --- AUTOMATIC CLICK EVENT WIRING ---
        // This ensures the listeners are never lost when saved in scene/prefabs
        
        // 1. Hamburger Button
        if (hamburgerButton != null)
        {
            var burgerBtn = hamburgerButton.GetComponent<UnityEngine.UI.Button>();
            if (burgerBtn != null)
            {
                burgerBtn.onClick.RemoveAllListeners();
                burgerBtn.onClick.AddListener(ToggleMenu);
            }
        }

        // 2. Scrim background
        if (scrimCanvasGroup != null)
        {
            var scrimBtn = scrimCanvasGroup.GetComponent<UnityEngine.UI.Button>();
            if (scrimBtn != null)
            {
                scrimBtn.onClick.RemoveAllListeners();
                scrimBtn.onClick.AddListener(CloseMenu);
            }
        }

        // 3. Menu buttons inside MenuPanel
        if (menuPanel != null)
        {
            var storyBtn = menuPanel.Find("ButtonContainer/StoryButton")?.GetComponent<UnityEngine.UI.Button>();
            if (storyBtn != null)
            {
                storyBtn.onClick.RemoveAllListeners();
                storyBtn.onClick.AddListener(OnStoryClicked);
            }

            var quizBtn = menuPanel.Find("ButtonContainer/QuizButton")?.GetComponent<UnityEngine.UI.Button>();
            if (quizBtn != null)
            {
                quizBtn.onClick.RemoveAllListeners();
                quizBtn.onClick.AddListener(OnQuizClicked);
            }

            var settingsBtn = menuPanel.Find("ButtonContainer/SettingsButton")?.GetComponent<UnityEngine.UI.Button>();
            if (settingsBtn != null)
            {
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(OnSettingsClicked);
            }

            var homeBtn = menuPanel.Find("ButtonContainer/HomeButton")?.GetComponent<UnityEngine.UI.Button>();
            if (homeBtn != null)
            {
                homeBtn.onClick.RemoveAllListeners();
                homeBtn.onClick.AddListener(OnHomeClicked);
            }
        }

        // Ensure hamburger button is visible initially when menu is closed
        if (hamburgerButton != null)
        {
            hamburgerButton.SetActive(true);
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateTransition(true));
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateTransition(false));
    }

    private IEnumerator AnimateTransition(bool open)
    {
        float elapsed = 0f;
        Vector2 startPos = menuPanel != null ? menuPanel.anchoredPosition : Vector2.zero;
        Vector2 targetPos = open ? openedPosition : closedPosition;

        float startAlpha = scrimCanvasGroup != null ? scrimCanvasGroup.alpha : 0f;
        float targetAlpha = open ? 1f : 0f;

        if (scrimCanvasGroup != null && open)
        {
            scrimCanvasGroup.blocksRaycasts = true;
        }

        // Hide hamburger button immediately when starting to open
        // Show hamburger button immediately when starting to close
        if (hamburgerButton != null)
        {
            hamburgerButton.SetActive(!open);
        }

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveT = easingCurve.Evaluate(t);

            if (menuPanel != null)
            {
                menuPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveT);
            }

            if (scrimCanvasGroup != null)
            {
                scrimCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveT);
            }

            yield return null;
        }

        if (menuPanel != null)
        {
            menuPanel.anchoredPosition = targetPos;
        }

        if (scrimCanvasGroup != null)
        {
            scrimCanvasGroup.alpha = targetAlpha;
            scrimCanvasGroup.blocksRaycasts = open;
        }

        // Double-check the final state of the hamburger button
        if (hamburgerButton != null)
        {
            hamburgerButton.SetActive(!open);
        }

        transitionCoroutine = null;
    }

    // Button Event Handlers
    public void OnStoryClicked()
    {
        CloseMenu();
        LoadSceneWithName(storySceneName);
    }

    public void OnQuizClicked()
    {
        CloseMenu();
        LoadSceneWithName(quizSceneName);
    }

    public void OnSettingsClicked()
    {
        CloseMenu();
        
        // Find SettingsController in the scene and open the settings panel
        SettingsController settingsController = Object.FindFirstObjectByType<SettingsController>();
        if (settingsController != null)
        {
            settingsController.OpenSettings();
        }
        else
        {
            Debug.LogWarning("SettingsController not found in the scene.");
        }
    }

    public void OnHomeClicked()
    {
        CloseMenu();
        LoadSceneWithName(homeSceneName);
    }

    private void LoadSceneWithName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // Don't reload if we are already in the target scene
        if (SceneManager.GetActiveScene().name == name)
        {
            Debug.Log($"Already in scene: {name}");
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