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

public class FurnitureBuildMenu : MonoBehaviour
{
    public static FurnitureBuildMenu instance;
    public GameObject buildFurnitureButtonPrefab;

    private List<GameObject> buildMenu;
    private string lastLanguage;
    private bool showAllFurniture;

    public void RebuildMenuButtons(bool showAllFurniture = false)
    {
        foreach (GameObject gameObject in buildMenu)
        {
            Destroy(gameObject);
        }

        this.showAllFurniture = showAllFurniture;

        GenerateMenuButtons();
    }

    private void Start()
    {
        instance = this;
        showAllFurniture = Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
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
                localizers[i].UpdateText(LocalizationTable.GetLocalization(PrototypeManager.Furniture.Get(i).GetName()));
            }
        }
    }

    private void GenerateMenuButtons()
    {
        BuildModeController bmc = WorldController.Instance.buildModeController;

        buildMenu = new List<GameObject>();

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string furnitureKey in PrototypeManager.Furniture.Keys)
        {
            if (PrototypeManager.Furniture.Get(furnitureKey).HasTypeTag("Non-buildable") && showAllFurniture == false)
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            gameObject.transform.SetParent(this.transform);
            buildMenu.Add(gameObject);

            Furniture proto = PrototypeManager.Furniture.Get(furnitureKey);
            string objectId = furnitureKey;

            gameObject.name = "Button - Build " + objectId;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(proto.LocalizationCode) };

            Button button = gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate
            {
                bmc.SetMode_BuildFurniture(objectId);
                this.gameObject.SetActive(false);
            });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string furniture = furnitureKey;
            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(PrototypeManager.Furniture.Get(furniture).LocalizationCode) };
            };
        }

        lastLanguage = LocalizationTable.currentLanguage;
    }
}