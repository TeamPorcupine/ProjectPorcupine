using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpawnInventoryController {
    public static bool isEnabled = true;

    public string InventoryToBuild { get; protected set; }

    private GameObject spawnUI;

    public SpawnInventoryController()
    {
        CreateSpawnUI();
        CreateInventoryButtons();
    }

    private void CreateSpawnUI()
    {
        spawnUI = new GameObject();
        spawnUI.name = "Spawn Inventory UI";
        spawnUI.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        spawnUI.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = spawnUI.AddComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0,0);

        Image image = spawnUI.AddComponent<Image>();

        VerticalLayoutGroup vlg = spawnUI.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 0;

        ContentSizeFitter csf = spawnUI.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.MinSize;
        csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    private void CreateInventoryButtons()
    {
        foreach(string iName in World.current.inventoryPrototypes.Keys)
        {
            GameObject inventoryButton_go = new GameObject();
            inventoryButton_go.name = "Button - " + iName;
            inventoryButton_go.transform.SetParent(spawnUI.transform);

            Image image = inventoryButton_go.AddComponent<Image>();

            Button button = inventoryButton_go.AddComponent<Button>();
            string localName = iName;
            button.onClick.AddListener(
                () => { InventoryToBuild = localName; }
            );

            GameObject text_go = CreateTextComponent(inventoryButton_go, iName);

            LayoutElement layoutElement = inventoryButton_go.AddComponent<LayoutElement>();
            layoutElement.minWidth = 120;
            layoutElement.minHeight = 20;
        }
    }

    private GameObject CreateTextComponent(GameObject go, string iName) {
        GameObject text_go = new GameObject();
        text_go.name = "Text";

        RectTransform rectTransform = text_go.AddComponent<RectTransform>();
        rectTransform.SetParent(go.transform);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Text text = text_go.AddComponent<Text>();
        text.text = iName;

        return text_go;
    }
}