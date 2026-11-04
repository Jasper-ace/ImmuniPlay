
using UnityEngine;
using TMPro;

public class BabyNameSaveButton : MonoBehaviour
{
    public TMP_InputField babyNameInput;

    private void Awake()
    {
        if (babyNameInput != null)
            babyNameInput.onValueChanged.AddListener(OnBabyNameChanged);
    }

    private void OnDestroy()
    {
        if (babyNameInput != null)
            babyNameInput.onValueChanged.RemoveListener(OnBabyNameChanged);
    }

    private void OnBabyNameChanged(string text)
    {
        string uppercaseText = text.ToUpper();

        if (babyNameInput.text != uppercaseText)
        {
            int caretPosition = babyNameInput.caretPosition;
            babyNameInput.SetTextWithoutNotify(uppercaseText);
            babyNameInput.caretPosition = caretPosition;
        }
    }

    public void SaveBabyName()
    {
        if (babyNameInput == null)
        {
            Debug.LogError("Baby Name Input is not assigned.");
            return;
        }

        string enteredName = babyNameInput.text.Trim();

        if (string.IsNullOrEmpty(enteredName))
        {
            Debug.LogWarning("Please enter a baby name.");
            babyNameInput.ActivateInputField();
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance is null.");
            return;
        }

        SaveManager.Instance.SetBabyName(enteredName);

        PlayerPrefs.SetString("BabyName", enteredName);
        PlayerPrefs.Save();

        Debug.Log("Baby name saved: " + enteredName);
    }
}
