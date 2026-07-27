using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BabyNamePanel : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Button confirmButton;

    public string BabyName { get; private set; }

    public delegate void NameConfirmed(string babyName);
    public event NameConfirmed OnNameConfirmed;

    void Awake()
    {
        gameObject.SetActive(false);

        confirmButton.onClick.AddListener(ConfirmName);
    }

    public void Open()
    {
        // Pre-fill with previously saved name (if any)
        string saved = SaveManager.Instance != null ? SaveManager.Instance.GetBabyName() : "";
        nameInput.text = saved;

        gameObject.SetActive(true);

        nameInput.ActivateInputField();
    }

    void ConfirmName()
    {
        BabyName = nameInput.text.Trim();

        if (BabyName == "")
            return;

        // Save to JSON via SaveManager (auto-saves to disk)
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetBabyName(BabyName);

        gameObject.SetActive(false);

        OnNameConfirmed?.Invoke(BabyName);
    }
}
