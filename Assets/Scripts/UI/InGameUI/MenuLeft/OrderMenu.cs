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
using UnityEngine;
using UnityEngine.UI;

public class OrderMenu : MonoBehaviour
{
    private const string LocalizationDeconstruct = "deconstruct_furniture";
    private const string LocalizationMine = "mine_furniture";

    private List<GameObject> taskItems;

    private MenuLeft menuLeft;

    public void RebuildMenuButtons()
    {       
        foreach (GameObject gameObject in taskItems)
        {
            Destroy(gameObject);
        }
        
        RenderDeconstructButton();
        RenderMineButton();
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
        title.text = LocalizationTable.GetLocalization("menu_orders");

        menuLeft = this.transform.GetComponentInParent<MenuLeft>();

        this.transform.FindChild("Close Button").GetComponent<Button>().onClick.AddListener(delegate
        {
            menuLeft.CloseMenu();
        });

        RenderDeconstructButton();
        RenderMineButton();

        InputField filterField = GetComponentInChildren<InputField>();
        filterField.onValueChanged.AddListener(delegate { FilterTextChanged(filterField.text); });
        KeyboardManager.Instance.RegisterModalInputField(filterField);
    }    

    private void RenderDeconstructButton()
    {
        taskItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
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

        Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
        image.sprite = SpriteManager.GetSprite("UI", "Deconstruct");
    }

    private void RenderMineButton()
    {
        taskItems = new List<GameObject>();

        UnityEngine.Object buttonPrefab = Resources.Load("UI/MenuLeft/ConstructionMenu/Button");
        Transform contentTransform = this.transform.FindChild("Scroll View").FindChild("Viewport").FindChild("Content");

        BuildModeController buildModeController = WorldController.Instance.buildModeController;

        GameObject gameObject = (GameObject)Instantiate(buttonPrefab);
        gameObject.transform.SetParent(contentTransform);
        taskItems.Add(gameObject);

        gameObject.name = "Button - Mine";

        gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LocalizationMine) };

        Button button = gameObject.GetComponent<Button>();

        button.onClick.AddListener(delegate
        {
            buildModeController.SetMode_Mine();
        });

        LocalizationTable.CBLocalizationFilesChanged += delegate
        {
            gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(LocalizationMine) };
        };

        Image image = gameObject.transform.GetChild(0).GetComponentsInChildren<Image>().First();
        image.sprite = SpriteManager.GetSprite("UI", "Mine");
    }
}
