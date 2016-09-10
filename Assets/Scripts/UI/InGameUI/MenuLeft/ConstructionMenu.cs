#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionMenu : MonoBehaviour
{
    const string LOCALIZATION_DECONSTRUCT = "deconstruct_furniture";

    private string lastLanguage;

    private void Start()
    {
        this.transform.FindChild("Close Button").GetComponent<Button>().onClick.AddListener(delegate
        {
            this.transform.GetComponentInParent<MenuLeft>().CloseMenu();
        });

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        RenderDeconstructButton(buildModeController);
        RenderTileButtons(buildModeController);
        RenderFurnitureButtons(buildModeController);

        lastLanguage = LocalizationTable.currentLanguage;
    }

    private void RenderFurnitureButtons(BuildModeController buildModeController)
    {
        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        foreach (string key in World.Current.furniturePrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buttonPrefab);
            go.transform.SetParent(contentTransform);
            go.name = "Button - Build " + key;

            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.Current.furniturePrototypes[key].LocalizationCode) };

            Button button = go.GetComponent<Button>();

            string objectId = key;
            button.onClick.AddListener(delegate
            {
                buildModeController.SetMode_BuildFurniture(objectId);
            });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string furnitureKey = key;
            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.Current.furniturePrototypes[furnitureKey].LocalizationCode) };
            };
        }
    }

    private void RenderTileButtons(BuildModeController buildModeController)
    {
        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        TileType[] tileTypes = TileType.GetTileTypes();

        foreach (TileType item in tileTypes)
        {
            TileType tileType = item;
            string key = tileType.Type;
            if (tileType == TileType.Empty)
            {
                key = "remove";
            }

            GameObject go = (GameObject)Instantiate(buttonPrefab);
            go.transform.SetParent(contentTransform);
            go.name = "Button - Build Tile " + key;

            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(key) };


            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(delegate
            {
                Debug.ULog(tileType.Type);
                buildModeController.SetModeBuildTile(tileType);
            });

            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(key) };
            };
        }
    }

    private void RenderDeconstructButton(BuildModeController buildModeController)
    {
        Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        GameObject go = (GameObject)Instantiate(buttonPrefab);
        go.transform.SetParent(contentTransform);
        go.name = "Button - Deconstruct";

        go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LOCALIZATION_DECONSTRUCT) };

        Button button = go.GetComponent<Button>();

        button.onClick.AddListener(delegate
        {
            buildModeController.SetMode_Deconstruct();
        });

        LocalizationTable.CBLocalizationFilesChanged += delegate
        {
            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LOCALIZATION_DECONSTRUCT) };
        };
    }

    //private void Update()
    //{
    //    if (lastLanguage != LocalizationTable.currentLanguage)
    //    {
    //        lastLanguage = LocalizationTable.currentLanguage;

    //        TextLocalizer[] localizers = GetComponentsInChildren<TextLocalizer>();

    //        for (int i = 0; i < localizers.Length; i++)
    //        {
    //            localizers[i].UpdateText(LocalizationTable.GetLocalization(World.Current.furniturePrototypes.ElementAt(i).Value.GetName()));
    //        }
    //    }
    //}
}
