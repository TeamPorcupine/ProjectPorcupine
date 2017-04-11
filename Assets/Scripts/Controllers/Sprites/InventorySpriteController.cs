#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventorySpriteController : BaseSpriteController<Inventory>
{
    private GameObject inventoryUIPrefab;

    // Use this for initialization
    public InventorySpriteController(World world, GameObject inventoryUI) : base(world, "Inventory")
    {
        inventoryUIPrefab = inventoryUI;

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.InventoryManager.InventoryCreated += OnCreated;

        // Check for pre-existing inventory, which won't do the callback.
        foreach (Inventory inventory in world.InventoryManager.Inventories.SelectMany(pair => pair.Value))
        {
            OnCreated(inventory);
        }
    }

    public static SpriteRenderer SetSprite(GameObject inventoryGO, Inventory inventory, string sortingLayerName = "Inventory")
    {
        SpriteRenderer sr = inventoryGO.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = inventoryGO.AddComponent<SpriteRenderer>();
        }

        sr.sortingLayerName = sortingLayerName;

        if (inventory.Category != "crated_furniture")
        {
            sr.sprite = SpriteManager.GetSprite("Inventory", inventory.Type);
            if (sr.sprite == null)
            {
                UnityDebugger.Debugger.LogError("InventorySpriteController", "No sprite for: " + inventory.Type);
            }
        }
        else
        {
            sr.sprite = SpriteManager.GetSprite("Inventory", "crate");

            if (sr.sprite == null)
            {
                UnityDebugger.Debugger.LogError("InventorySpriteController", "No sprite for: Crate");
            }

            GameObject labelGO;
            if (inventoryGO.transform.childCount < 1)
            {
                // There should only ever be exactly one child of a crate, if it already has it then everything else should be correct
                labelGO = new GameObject();
                labelGO.transform.SetParent(inventoryGO.transform, false);
                float scale = .5f;
                Furniture furniture = PrototypeManager.Furniture.Get(inventory.Type);
                scale *= 1f / Mathf.Max(furniture.Height, furniture.Width);
                labelGO.transform.localScale = new Vector3(scale, scale, scale);
                SpriteRenderer labelSR = labelGO.AddComponent<SpriteRenderer>();
                labelSR.sprite = SpriteManager.GetSprite("Furniture", furniture.GetDefaultSpriteName());

                Color labelSpriteColor = labelSR.color;
                labelSpriteColor.a = .5f;
                labelSR.color = labelSpriteColor;
                labelSR.sortingLayerName = sortingLayerName;
                labelSR.sortingOrder = sr.sortingOrder + 1;
            }
        }

        return sr;
    }

    public override void RemoveAll()
    {
        world.InventoryManager.InventoryCreated -= OnCreated;
        foreach (Inventory inventory in world.InventoryManager.Inventories.SelectMany(pair => pair.Value))
        {
            inventory.StackSizeChanged -= OnChanged;
        }

        base.RemoveAll();
    }

    protected override void OnCreated(Inventory inventory)
    {
        // This creates a new GameObject and adds it to our scene.
        GameObject inventoryGameObject = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(inventory, inventoryGameObject);

        inventoryGameObject.name = inventory.Type;

        // Only create a Game Object if inventory was created on tile, anything else will handle its own game object
        if (inventory.Tile != null)
        {
            inventoryGameObject.transform.position = new Vector3(inventory.Tile.X, inventory.Tile.Y, inventory.Tile.Z);
        }

        inventoryGameObject.transform.SetParent(objectParent.transform, true);

        SetSprite(inventoryGameObject, inventory);

        if (inventory.MaxStackSize > 1)
        {
            // This is a stackable object, so let's add a InventoryUI component
            // (Which is text that shows the current stackSize.)
            GameObject uiGameObject = GameObject.Instantiate(inventoryUIPrefab);
            uiGameObject.transform.SetParent(inventoryGameObject.transform);
            uiGameObject.transform.localPosition = Vector3.zero;
            uiGameObject.GetComponentInChildren<Text>().text = inventory.StackSize.ToString();
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inventory.StackSizeChanged += OnChanged;
    }

    protected override void OnChanged(Inventory inventory)
    {
        // Make sure the furniture's graphics are correct.
        if (objectGameObjectMap.ContainsKey(inventory) == false)
        {
            UnityDebugger.Debugger.LogError("InventorySpriteController", "OnCharacterChanged -- trying to change visuals for inventory not in our map.");
            return;
        }

        GameObject inventoryGameObject = objectGameObjectMap[inventory];
        if (inventory.StackSize > 0)
        {
            Text text = inventoryGameObject.GetComponentInChildren<Text>();

            // FIXME: If maxStackSize changed to/from 1, then we either need to create or destroy the text
            if (text != null)
            {
                text.text = inventory.StackSize.ToString();
            }
        }
        else
        {
            // This stack has gone to zero, so remove the sprite!
            OnRemoved(inventory);
        }
    }

    protected override void OnRemoved(Inventory inventory)
    {
        inventory.StackSizeChanged -= OnChanged;
        GameObject inventoryGameObject = objectGameObjectMap[inventory];
        objectGameObjectMap.Remove(inventory);
        GameObject.Destroy(inventoryGameObject);
    }
}
