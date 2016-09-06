#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Linq;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureBuildMenu : MonoBehaviour
{
    public GameObject buildFurnitureButtonPrefab;

    private string lastLanguage;

    // Use this for initialization
    private void Start()
    {
        BuildModeController bmc = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string furnitureKey in PrototypeManager.Furniture.Keys)
        {
            if (PrototypeManager.Furniture.Get(furnitureKey).HasTypeTag("Non-buildable"))
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            gameObject.transform.SetParent(this.transform);

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
}
