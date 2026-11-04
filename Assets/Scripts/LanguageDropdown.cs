using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    private const string PrefKey = "SelectedLocaleIndex";

    private IEnumerator Start()
    {
        // Wait until the Localization system is ready
        yield return LocalizationSettings.InitializationOperation;

        int saved = PlayerPrefs.GetInt(PrefKey, 0);
        saved = Mathf.Clamp(saved, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);

        dropdown.SetValueWithoutNotify(saved);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[saved];

        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt(PrefKey, index);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
    }
}