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

public class InventorySpriteController : BaseSpriteController<Inventory>
{
    private GameObject inventoryUIPrefab;

    // Use this for initialization
    public InventorySpriteController(World world, GameObject inventoryUI) : base(world, "Inventory")
    {
        inventoryUIPrefab = inventoryUI;

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.OnInventoryCreated += OnCreated;

        // Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in world.inventoryManager.inventories[objectType])
            {
                OnCreated(inv);
            }
        }
    }

    public override void RemoveAll()
    {
        world.OnInventoryCreated -= OnCreated;

        foreach (string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in world.inventoryManager.inventories[objectType])
            {
                inv.OnInventoryChanged -= OnChanged;
            }
        }

        base.RemoveAll();
    }

    protected override void OnCreated(Inventory inv)
    {
        // FIXME: Does not consider multi-tile objects nor rotated objects
        // This creates a new GameObject and adds it to our scene.
        GameObject inv_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(inv, inv_go);

        inv_go.name = inv.objectType;

        // Only create a Game Object if inventory was created on tile, anything else will handle its own game object
        if (inv.tile != null)
        {
            inv_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
        }

        inv_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.current.GetSprite("Inventory", inv.objectType);
        if (sr.sprite == null)
        {
            Debug.ULogErrorChannel("InventorySpriteController", "No sprite for: " + inv.objectType);
        }

        sr.sortingLayerName = "Inventory";

        if (inv.maxStackSize > 1)
        {
            // This is a stackable object, so let's add a InventoryUI component
            // (Which is text that shows the current stackSize.)
            GameObject ui_go = GameObject.Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            ui_go.GetComponentInChildren<Text>().text = inv.StackSize.ToString();
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inv.OnInventoryChanged += OnChanged;
    }

    protected override void OnChanged(Inventory inv)
    {
        // Make sure the furniture's graphics are correct.
        if (objectGameObjectMap.ContainsKey(inv) == false)
        {
            Debug.ULogErrorChannel("InventorySpriteController", "OnCharacterChanged -- trying to change visuals for inventory not in our map.");
            return;
        }
            
        GameObject inv_go = objectGameObjectMap[inv];
        if (inv.StackSize > 0)
        {
            Text text = inv_go.GetComponentInChildren<Text>();

            // FIXME: If maxStackSize changed to/from 1, then we either need to create or destroy the text
            if (text != null)
            {
                text.text = inv.StackSize.ToString();
            }
        }
        else
        {
            // This stack has gone to zero, so remove the sprite!
            OnRemoved(inv);
        }
    }

    protected override void OnRemoved(Inventory inv)
    {
        inv.OnInventoryChanged -= OnChanged;
        GameObject inv_go = objectGameObjectMap[inv];
        objectGameObjectMap.Remove(inv);
        GameObject.Destroy(inv_go);
    }
}
