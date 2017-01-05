#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.Localization;
using ProjectPorcupine.Rooms;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionMenu : MonoBehaviour
{
    private const string LocalizationDeconstruct = "deconstruct_furniture";

    private List<GameObject> furnitureItems;
    private List<GameObject> roomBehaviorItems;
    private List<GameObject> utilityItems;
    private List<GameObject> tileItems;

    private bool showAllFurniture;

    private MenuLeft menuLeft;

    public void RebuildMenuButtons(bool showAllFurniture = false)
    {
        foreach (GameObject gameObject in furnitureItems)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in roomBehaviorItems)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in utilityItems)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in tileItems)
        {
            Destroy(gameObject);
        }
        
        this.showAllFurniture = showAllFurniture;
        
        RenderRoomBehaviorButtons();
        RenderTileButtons();
        RenderFurnitureButtons();
        RenderUtilityButtons();
    }

    public void FilterTextChanged(string filterText)
    {
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        List<Transform> childs = contentTransform.Cast<Transform>().ToList();

        foreach (Transform child in childs)
        {
            Text buttonText = child.gameObject.transform.GetComponentInChildren<Text>();

            string buildableName = buttonText.text;

            bool nameMatchFilter = string.IsNullOrEmpty(filterText) || buildableName.ToLower().Contains(filterText.ToLower());

            child.gameObject.SetActive(nameMatchFilter);
        }
    }

    private void Start()
    {
        Text title = GetComponentInChildren<Text>();
        title.text = LocalizationTable.GetLocalization("menu_construction");

        menuLeft = this.transform.GetComponentInParent<MenuLeft>();

        this.transform.FindChild("Close Button").GetComponent<Button>().onClick.AddListener(delegate
        {
            menuLeft.CloseMenu();
        });
        
        RenderRoomBehaviorButtons();
        RenderTileButtons();
        RenderFurnitureButtons();
        RenderUtilityButtons();

        InputField filterField = GetComponentInChildren<InputField>();
        filterField.onValueChanged.AddListener(delegate { FilterTextChanged(filterField.text); });
        KeyboardManager.Instance.RegisterModalInputField(filterField);
    }

    private void RenderFurnitureButtons()
    {
        furnitureItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
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

            Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
            image.sprite = WorldController.Instance.furnitureSpriteController.GetSpriteForFurniture(furnitureKey);
        }
    }

    private void RenderRoomBehaviorButtons()
    {
        roomBehaviorItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string roomBehaviorKey in PrototypeManager.RoomBehavior.Keys)
        {
            if (PrototypeManager.RoomBehavior.Get(roomBehaviorKey).HasTypeTag("Non-buildable") && showAllFurniture == false)
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
            gameObject.transform.SetParent(contentTransform);
            roomBehaviorItems.Add(gameObject);

            RoomBehavior proto = PrototypeManager.RoomBehavior.Get(roomBehaviorKey);
            string objectId = roomBehaviorKey;

            gameObject.name = "Button - Designate " + objectId;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(proto.LocalizationCode) };

            Button button = gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate
                {
                    buildModeController.SetMode_DesignateRoomBehavior(objectId);
                    menuLeft.CloseMenu();
                });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string roomBehavior = roomBehaviorKey;
            LocalizationTable.CBLocalizationFilesChanged += delegate
                {
                    gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(PrototypeManager.RoomBehavior.Get(roomBehavior).LocalizationCode) };
                };

            Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
            image.sprite = SpriteManager.GetSprite("RoomBehavior", roomBehaviorKey);
        }
    }

    private void RenderUtilityButtons()
    {
        utilityItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string utilityKey in PrototypeManager.Utility.Keys)
        {
            if (PrototypeManager.Utility.Get(utilityKey).HasTypeTag("Non-buildable") && showAllFurniture == false)
            {
                continue;
            }

            GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
            gameObject.transform.SetParent(contentTransform);
            furnitureItems.Add(gameObject);

            Utility proto = PrototypeManager.Utility.Get(utilityKey);
            string objectId = utilityKey;

            gameObject.name = "Button - Build " + objectId;

            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(proto.LocalizationCode) };

            Button button = gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate
                {
                    buildModeController.SetMode_BuildUtility(objectId);
                    menuLeft.CloseMenu();
                });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string utility = utilityKey;
            LocalizationTable.CBLocalizationFilesChanged += delegate
                {
                    gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(PrototypeManager.Utility.Get(utility).LocalizationCode) };
                };

            Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
            image.sprite = WorldController.Instance.utilitySpriteController.GetSpriteForUtility(utilityKey);
        }
    }

    private void RenderTileButtons()
    {
        tileItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        foreach (TileType item in PrototypeManager.TileType.Values)
        {
            TileType tileType = item;

            string key = tileType.LocalizationCode;

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

            Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
            image.sprite = SpriteManager.GetSprite("Tile", tileType.Type);
        }
    }
}
