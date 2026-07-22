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
        nameInput.text = "";
        gameObject.SetActive(true);

        nameInput.ActivateInputField();
    }

    void ConfirmName()
    {
        BabyName = nameInput.text.Trim();

        if (BabyName == "")
            return;

        PlayerPrefs.SetString("BabyName", BabyName);
        PlayerPrefs.Save();

        gameObject.SetActive(false);

        OnNameConfirmed?.Invoke(BabyName);
    }
}   