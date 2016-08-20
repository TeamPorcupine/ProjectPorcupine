#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour
{

    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.cbInventoryCreated += OnInventoryCreated;

        // Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in world.inventoryManager.inventories[objectType])
            {
                OnInventoryCreated(inv);
            }
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    public void OnInventoryCreated(Inventory inv)
    {
        //Logger.Log("OnInventoryCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject inv_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        inventoryGameObjectMap.Add(inv, inv_go);

        inv_go.name = inv.objectType;
        inv_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
        inv_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.current.GetSprite("Inventory", inv.objectType);
        if (sr.sprite == null)
        {
            Logger.LogError("No sprite for: " + inv.objectType);
        }
        sr.sortingLayerName = "Inventory";

        if (inv.maxStackSize > 1)
        {
            // This is a stackable object, so let's add a InventoryUI component
            // (Which is text that shows the current stackSize.)

            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            ui_go.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inv.cbInventoryChanged += OnInventoryChanged;

    }

    void OnInventoryChanged(Inventory inv)
    {

        //Logger.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (inventoryGameObjectMap.ContainsKey(inv) == false)
        {
            Logger.LogError("OnCharacterChanged -- trying to change visuals for inventory not in our map.");
            return;
        }

        GameObject inv_go = inventoryGameObjectMap[inv];
        if (inv.stackSize > 0)
        {
            Text text = inv_go.GetComponentInChildren<Text>();
            // FIXME: If maxStackSize changed to/from 1, then we either need to create or destroy the text
            if (text != null)
            {
                text.text = inv.stackSize.ToString();
            }
        }
        else
        {
            // This stack has gone to zero, so remove the sprite!
            Destroy(inv_go);
            inventoryGameObjectMap.Remove(inv);
            inv.cbInventoryChanged -= OnInventoryChanged;
        }

    }


	
}
