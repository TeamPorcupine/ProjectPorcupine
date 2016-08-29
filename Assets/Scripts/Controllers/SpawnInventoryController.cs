#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnInventoryController
{
    private GameObject spawnUI;

    public SpawnInventoryController()
    {
        CreateSpawnUI();
        CreateInventoryButtons();
    }

    public string InventoryToBuild { get; protected set; }

    public void HideUI() 
    {
        spawnUI.SetActive(false);
    }

    public void ShowUI() 
    {
        spawnUI.SetActive(true);
    }

    public void SetUIVisibility(bool visibility)
    {
        spawnUI.SetActive(visibility);
    }

    public void SpawnInventory(Tile t)
    {
        // If the user clicks outside the game area t may be null.
        if (t == null)
        {
            return;
        }

        Inventory inventoryChange = new Inventory(InventoryToBuild, 1);

        // You can't spawn on occupied tiles
        if (t.Furniture != null)
        {
            return; 
        }

        if (t.Inventory == null || t.Inventory.objectType == InventoryToBuild)
        {
            World.Current.inventoryManager.PlaceInventory(t, inventoryChange);
        }
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
        rectTransform.anchoredPosition = new Vector2(0, 0);

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
        foreach (string invName in PrototypeManager.Inventory.Keys)
        {
            GameObject inventoryButton_go = new GameObject();
            inventoryButton_go.name = "Button - " + invName;
            inventoryButton_go.layer = LayerMask.NameToLayer("UI");

            inventoryButton_go.transform.SetParent(spawnUI.transform);

            Image image = inventoryButton_go.AddComponent<Image>();

            Button button = inventoryButton_go.AddComponent<Button>();
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = Color.white;
            colorBlock.pressedColor = Color.blue;
            button.colors = colorBlock;
            string localName = invName;
            button.onClick.AddListener(
                () => { OnButtonClick(localName); });

            GameObject text_go = CreateTextComponent(inventoryButton_go, invName);

            LayoutElement layoutElement = inventoryButton_go.AddComponent<LayoutElement>();
            layoutElement.minWidth = 120;
            layoutElement.minHeight = 20;
        }
    }

    private GameObject CreateTextComponent(GameObject go, string invName)
    {
        GameObject text_go = new GameObject();
        text_go.name = "Text";
        text_go.layer = LayerMask.NameToLayer("UI");

        RectTransform rectTransform = text_go.AddComponent<RectTransform>();
        rectTransform.SetParent(go.transform);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Text text = text_go.AddComponent<Text>();
        text.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.black;
        text.text = invName;

        return text_go;
    }

    private void OnButtonClick(string invName)
    {
        InventoryToBuild = invName;
        WorldController.Instance.mouseController.StartSpawnMode();
    }
}