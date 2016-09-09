#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class UtilityBuildMenu : MonoBehaviour
{
    public static UtilityBuildMenu instance;
    public GameObject buildutilityButtonPrefab;

    private List<GameObject> buildMenu;
    private string lastLanguage;
    private bool showAllutility;

    public void RebuildMenuButtons(bool showAllutility = false)
    {
        foreach (GameObject gameObject in buildMenu)
        {
            Destroy(gameObject);
        }

        this.showAllutility = showAllutility;

        GenerateMenuButtons();
    }

    private void Start()
    {
        instance = this;
        showAllutility = Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
        GenerateMenuButtons();        
    }

    private void Update()
    {
        if (lastLanguage != LocalizationTable.currentLanguage)
        {
            lastLanguage = LocalizationTable.currentLanguage;

            TextLocalizer[] localizers = GetComponentsInChildren<TextLocalizer>();

            for (int i = 0; i < localizers.Length; i++)
            {
                localizers[i].UpdateText(LocalizationTable.GetLocalization(PrototypeManager.Utility.Get(i).GetName()));
            }
        }
    }

    private void GenerateMenuButtons()
    {
        BuildModeController bmc = WorldController.Instance.buildModeController;

        buildMenu = new List<GameObject>();

        // For each utility prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string utilityKey in PrototypeManager.Utility.Keys)
        {
            if (PrototypeManager.Utility.Get(utilityKey).HasTypeTag("Non-buildable") && showAllutility == false)
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buildutilityButtonPrefab);
            gameObject.transform.SetParent(this.transform);
            buildMenu.Add(gameObject);

            Utility proto = PrototypeManager.Utility.Get(utilityKey);
            string objectId = utilityKey;

            gameObject.name = "Button - Build " + objectId;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(proto.LocalizationCode) };

            Button button = gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate
            {
                bmc.SetMode_BuildUtility(objectId);
                this.gameObject.SetActive(false);
            });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string utility = utilityKey;
            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(PrototypeManager.Utility.Get(utility).LocalizationCode) };
            };
        }

        lastLanguage = LocalizationTable.currentLanguage;
    }
}