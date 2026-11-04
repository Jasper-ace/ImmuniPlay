using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Attach to the "TermsAndConditions" root object.
/// - "I Agree" stays disabled until the checkbox is ticked
///   (and, optionally, until the player scrolled to the bottom).
/// - Agree saves acceptance and closes the popup.
/// - Disagree closes the popup / quits the game.
/// </summary>
public class TermsAndConditionsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;      // TermsAndConditions/Panel (or the root itself)
    [SerializeField] private ScrollRect scrollView;     // Panel/Scroll View
    [SerializeField] private Toggle agreeToggle;        // Bottom/Checkbox/Toggle
    [SerializeField] private Button agreeButton;        // Bottom/Buttons/AgreeBtn
    [SerializeField] private Button disagreeButton;     // Bottom/Buttons/DisagreeBtn

    [Header("Agree Button Visuals")]
    [SerializeField] private GameObject panel1;         // grey (unchecked)
    [SerializeField] private GameObject text1;          // Text (TMP)1 (unchecked)
    [SerializeField] private GameObject panel2;         // red (checked)
    [SerializeField] private GameObject text2;          // Text (TMP)2 (checked)

    [Header("Options")]
    [Tooltip("Checkbox stays locked until the player scrolls to the bottom.")]
    [SerializeField] private bool requireScrollToBottom = true;
    [Tooltip("File name of the save JSON inside Application.persistentDataPath. " +
             "If this file exists, the Terms and Conditions stay disabled.")]
    [SerializeField] private string saveFileName = "save.json";
    [Tooltip("ON: Disagree quits the game. OFF: it only closes the popup.")]
    [SerializeField] private bool quitOnDisagree = false;

    [Header("Events")]
    public UnityEvent onAgreed;      // e.g. load next scene / enable Start button
    public UnityEvent onDisagreed;

    private bool reachedBottom;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
    }

    private void OnEnable()
    {
        // JSON loaded -> Terms and Conditions stay disabled
        if (IsJsonLoaded())
        {
            panelRoot.SetActive(false);
            return;
        }

        // JSON not loaded -> Terms and Conditions are enabled
        panelRoot.SetActive(true);

        // Reset state every time the popup opens
        agreeToggle.isOn = false;
        reachedBottom = !requireScrollToBottom;
        agreeToggle.interactable = reachedBottom;
        agreeButton.interactable = false;
        UpdateAgreeVisuals(false);

        if (scrollView != null)
        {
            scrollView.verticalNormalizedPosition = 1f; // start at top
            scrollView.onValueChanged.AddListener(OnScrolled);
        }

        agreeToggle.onValueChanged.AddListener(OnToggleChanged);
        agreeButton.onClick.AddListener(OnAgreeClicked);
        disagreeButton.onClick.AddListener(OnDisagreeClicked);
    }

    private void OnDisable()
    {
        if (scrollView != null) scrollView.onValueChanged.RemoveListener(OnScrolled);
        agreeToggle.onValueChanged.RemoveListener(OnToggleChanged);
        agreeButton.onClick.RemoveListener(OnAgreeClicked);
        disagreeButton.onClick.RemoveListener(OnDisagreeClicked);
    }

    private void OnScrolled(Vector2 pos)
    {
        // 0 = bottom of the scroll view
        if (!reachedBottom && pos.y <= 0.02f)
        {
            reachedBottom = true;
            agreeToggle.interactable = true;
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        // Grey "I Agree" becomes red/clickable only when checked
        agreeButton.interactable = isOn && reachedBottom;
        UpdateAgreeVisuals(isOn);
    }

    // Unchecked -> Panel1 + Text1 on, Panel2 + Text2 off
    // Checked   -> Panel2 + Text2 on, Panel1 + Text1 off
    private void UpdateAgreeVisuals(bool isChecked)
    {
        if (panel1 != null) panel1.SetActive(!isChecked);
        if (text1 != null) text1.SetActive(!isChecked);
        if (panel2 != null) panel2.SetActive(isChecked);
        if (text2 != null) text2.SetActive(isChecked);
    }

    private void OnAgreeClicked()
    {
        if (!agreeToggle.isOn) return;

        // Writes save.json so the Terms and Conditions don't show again
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetTermsAccepted();

        panelRoot.SetActive(false);
        onAgreed?.Invoke();
    }

    /// <summary>
    /// True if the save JSON exists and isn't empty.
    /// If you already have your own save/load class, replace the body
    /// with something like: return SaveSystem.HasLoadedData;
    /// </summary>
    private bool IsJsonLoaded()
    {
        bool loaded;

        if (SaveManager.Instance != null)
        {
            // Same file SaveManager reads/writes (persistentDataPath/save.json)
            loaded = SaveManager.Instance.HasSave();
        }
        else
        {
            // SaveManager not ready yet -> check the same file directly
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            loaded = File.Exists(path) && new FileInfo(path).Length > 0;
        }

        Debug.Log($"[T&C] JSON save exists = {loaded} " +
                  $"(SaveManager ready: {SaveManager.Instance != null})");
        return loaded;
    }

    private void OnDisagreeClicked()
    {
        onDisagreed?.Invoke();

        if (quitOnDisagree)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            panelRoot.SetActive(false);
        }
    }
}