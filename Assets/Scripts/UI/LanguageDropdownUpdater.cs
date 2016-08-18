using UnityEngine;
using UnityEngine.UI;
using ProjectPorcupine.Localization;

public class LanguageDropdownUpdater : MonoBehaviour
{
    void Start()
    {
        Dropdown dropdown = GetComponent<Dropdown>();

        string[] languages = LocalizationTable.GetLanguages();

        foreach (string lang in languages)
        {
            dropdown.options.Add(new Dropdown.OptionData(lang));
        }

        for (int i = 0; i < languages.Length; i++)
        {
            if (languages[i] == LocalizationTable.currentLanguage)
            {
                //This tbh quite stupid looking code is necessary due to a Unity (optimization?, bug(?)).
                dropdown.value = i + 1;
                dropdown.value = i;
            }
        }
    }

    public void SelectLanguage(int lang)
    {
        string[] languages = LocalizationTable.GetLanguages();
        LocalizationTable.currentLanguage = languages[lang];
        PlayerPrefs.SetString("CurrentLanguage", languages[lang]);
    }
}
