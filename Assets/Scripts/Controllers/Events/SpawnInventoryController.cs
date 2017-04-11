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

[MoonSharp.Interpreter.MoonSharpUserData]
public class SpawnInventoryController
{
    private GameObject spawnUI;

    public SpawnInventoryController()
    {
        CreateSpawnUI();
        CreateInventoryEntries();
    }

    public string InventoryToBuild { get; protected set; }

    public int AmountToCreate { get; protected set; }

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

        Inventory inventoryChange = new Inventory(InventoryToBuild, AmountToCreate);

        // You can't spawn on occupied tiles
        if (t.Furniture != null)
        {
            return;
        }

        if (t.Inventory == null || t.Inventory.Type == InventoryToBuild)
        {
            World.Current.InventoryManager.PlaceInventory(t, inventoryChange);
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

        spawnUI.AddComponent<Image>();

        VerticalLayoutGroup vlg = spawnUI.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 0;

        ContentSizeFitter csf = spawnUI.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.MinSize;
        csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    private void CreateInventoryEntries()
    {
        foreach (Inventory inventory in PrototypeManager.Inventory.Values.OrderByDescending(inv => inv.Category))
        {
            GameObject inventorySlot_go = new GameObject();
            inventorySlot_go.name = "Slot - " + inventory.Type;
            inventorySlot_go.layer = LayerMask.NameToLayer("UI");

            inventorySlot_go.transform.SetParent(spawnUI.transform);

            HorizontalLayoutGroup hlg = inventorySlot_go.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 2;

            inventorySlot_go.AddComponent<Image>();

            string localName = inventory.LocalizationName;

            GameObject textComponent = CreateTextComponent(inventorySlot_go, localName, TextAnchor.MiddleLeft);
            TextLocalizer textLocalizer = textComponent.AddComponent<TextLocalizer>();
            textLocalizer.formatValues = new string[0];
            CreateButtonComponents(inventorySlot_go, inventory, new int[] { 1, 20, 50 });

            LayoutElement layoutElement = inventorySlot_go.AddComponent<LayoutElement>();
            layoutElement.minWidth = 160;
            layoutElement.minHeight = 20;
        }
    }

    private void CreateButtonComponents(GameObject go, Inventory inventory, int[] amounts)
    {
        foreach (int amount in amounts)
        {
            GameObject button_go = new GameObject();
            button_go.name = "Button";
            button_go.layer = LayerMask.NameToLayer("UI");

            button_go.AddComponent<Image>();

            RectTransform rectTransform = button_go.GetComponent<RectTransform>();
            rectTransform.SetParent(go.transform);

            Button button = button_go.AddComponent<Button>();
            CreateTextComponent(button_go, amount.ToString(), TextAnchor.MiddleCenter);

            LayoutElement layoutElement = button_go.AddComponent<LayoutElement>();
            layoutElement.minWidth = 20;
            layoutElement.minHeight = 20;

            int localAmount = amount;

            button.onClick.AddListener(
                () => OnButtonClick(inventory.Type, localAmount));
        }
    }

    private GameObject CreateTextComponent(GameObject go, string invName, TextAnchor textAnchor)
    {
        GameObject text_go = new GameObject();
        text_go.name = "Text";
        text_go.layer = LayerMask.NameToLayer("UI");

        RectTransform rectTransform = text_go.AddComponent<RectTransform>();
        rectTransform.SetParent(go.transform);
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;

        Text text = text_go.AddComponent<Text>();
        text.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        text.alignment = textAnchor;
        text.color = Color.black;
        text.text = invName;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        LayoutElement layoutElement = text_go.AddComponent<LayoutElement>();
        layoutElement.minWidth = 100;
        layoutElement.minHeight = 20;

        return text_go;
    }

    private void OnButtonClick(string invName, int amount)
    {
        InventoryToBuild = invName;
        AmountToCreate = amount;
        WorldController.Instance.mouseController.StartSpawnMode();
    }
}