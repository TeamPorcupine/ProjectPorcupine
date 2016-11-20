using ProjectPorcupine.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class OrderMenu : MonoBehaviour
{

    private const string LocalizationDeconstruct = "deconstruct_furniture";
    private const string LocalizationMine = "mine_furniture";

    private List<GameObject> taskItems;


    private bool showAllFurniture;

    private MenuLeft menuLeft;

    public void RebuildMenuButtons(bool showAllFurniture = false)
    {       
        foreach (GameObject gameObject in taskItems)
        {
            Destroy(gameObject);
        }

        this.showAllFurniture = showAllFurniture;

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
        title.text = "Orders";
        
        //title.text = "menu_orders";
        //title.gameObject.AddComponent<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization("menu_orders") };
        // TODO: localization, this for some reason breaks the game:
        //GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization("menu_orders") };
        //title.text = LocalizationTable.GetLocalization("menu_orders");

        menuLeft = this.transform.GetComponentInParent<MenuLeft>();

        this.transform.FindChild("Close Button").GetComponent<Button>().onClick.AddListener(delegate
        {
            menuLeft.CloseMenu();
        });

        RenderDeconstructButton();
        RenderMineButton();

        InputField filterField = GetComponentInChildren<InputField>();
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
