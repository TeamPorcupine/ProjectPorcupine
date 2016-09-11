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

public class ConstructionMenu : MonoBehaviour
{
    private const string LocalizationDeconstruct = "deconstruct_furniture";

    private List<GameObject> furnitureItems;
    private List<GameObject> tileItems;
    private List<GameObject> taskItems;

    private string lastLanguage;

    private bool showAllFurniture;

    private MenuLeft menuLeft;

    public void RebuildMenuButtons(bool showAllFurniture = false)
    {
        foreach (GameObject gameObject in furnitureItems)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in tileItems)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in taskItems)
        {
            Destroy(gameObject);
        }

        this.showAllFurniture = showAllFurniture;

        RenderDeconstructButton();
        RenderTileButtons();
        RenderFurnitureButtons();
    }

    private void Start()
    {
        menuLeft = this.transform.GetComponentInParent<MenuLeft>();

        this.transform.FindChild("Close Button").GetComponent<Button>().onClick.AddListener(delegate
        {
            menuLeft.CloseMenu();
        });

        RenderDeconstructButton();
        RenderTileButtons();
        RenderFurnitureButtons();

        lastLanguage = LocalizationTable.currentLanguage;
    }

    private void RenderFurnitureButtons()
    {
        furnitureItems = new List<GameObject>();

        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string furnitureKey in PrototypeManager.Furniture.Keys)
        {
            if (PrototypeManager.Furniture.Get(furnitureKey).HasTypeTag("Non-buildable") && showAllFurniture == false)
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
            gameObject.transform.SetParent(contentTransform);
            furnitureItems.Add(gameObject);

            Furniture proto = PrototypeManager.Furniture.Get(furnitureKey);
            string objectId = furnitureKey;

            gameObject.name = "Button - Build " + objectId;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(proto.LocalizationCode) };

            Button button = gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate
            {
                buildModeController.SetMode_BuildFurniture(objectId);
                menuLeft.CloseMenu();
            });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string furniture = furnitureKey;
            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(PrototypeManager.Furniture.Get(furniture).LocalizationCode) };
            };
        }
    }

    private void RenderTileButtons()
    {
        tileItems = new List<GameObject>();

        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        TileType[] tileTypes = tileTypes = TileType.LoadedTileTypes;

        foreach (TileType item in tileTypes)
        {
            TileType tileType = item;
            string key = tileType.Type;

            GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
            gameObject.transform.SetParent(contentTransform);
            tileItems.Add(gameObject);

            gameObject.name = "Button - Build Tile " + key;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(key) };

            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(delegate
            {
                buildModeController.SetModeBuildTile(tileType);
            });

            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(key) };
            };
        }
    }

    private void RenderDeconstructButton()
    {
        taskItems = new List<GameObject>();

        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
        gameObject.transform.SetParent(contentTransform);
        taskItems.Add(gameObject);

        gameObject.name = "Button - Deconstruct";

        gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LocalizationDeconstruct) };

        Button button = gameObject.GetComponent<Button>();

        button.onClick.AddListener(delegate
        {
            buildModeController.SetMode_Deconstruct();
        });

        LocalizationTable.CBLocalizationFilesChanged += delegate
        {
            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LocalizationDeconstruct) };
        };
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
