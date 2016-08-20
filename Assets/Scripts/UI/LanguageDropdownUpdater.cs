#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
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

		// Set scroll sensitivity based on the save-item count
		dropdown.template.GetComponent<ScrollRect> ().scrollSensitivity = dropdown.options.Count / 3;
    }

    public void SelectLanguage(int lang)
    {
        string[] languages = LocalizationTable.GetLanguages();
        LocalizationTable.currentLanguage = languages[lang];
        PlayerPrefs.SetString("CurrentLanguage", languages[lang]);
    }
}
