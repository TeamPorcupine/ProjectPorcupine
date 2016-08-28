#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class InventoryManager
{
    // This is a list of all "live" inventories.
    // Later on this will likely be organized by rooms instead
    // of a single master list. (Or in addition to.)
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager()
    {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory(Tile tile, Inventory inv)
    {
        bool tileWasEmpty = tile.Inventory == null;

        if (tile.PlaceInventory(inv) == false)
        {
            // The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

        CleanupInventory(inv);

        // We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty)
        {
            if (inventories.ContainsKey(tile.Inventory.objectType) == false)
            {
                inventories[tile.Inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.Inventory.objectType].Add(tile.Inventory);

            World.Current.OnInventoryCreatedCallback(tile.Inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inv)
    {
        if (job.inventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            Debug.ULogErrorChannel("InventoryManager", "Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        job.inventoryRequirements[inv.objectType].StackSize += inv.StackSize;

        if (job.inventoryRequirements[inv.objectType].maxStackSize < job.inventoryRequirements[inv.objectType].StackSize)
        {
            inv.StackSize = job.inventoryRequirements[inv.objectType].StackSize - job.inventoryRequirements[inv.objectType].maxStackSize;
            job.inventoryRequirements[inv.objectType].StackSize = job.inventoryRequirements[inv.objectType].maxStackSize;
        }
        else
        {
            inv.StackSize = 0;
        }

        CleanupInventory(inv);

        return true;
    }

    public bool PlaceInventory(Character character, Inventory sourceInventory, int amount = -1)
    {
        if (amount < 0)
        {
            amount = sourceInventory.StackSize;
        }
        else
        {
            amount = Mathf.Min(amount, sourceInventory.StackSize);
        }

        if (character.inventory == null)
        {
            character.inventory = sourceInventory.Clone();
            character.inventory.StackSize = 0;
            inventories[character.inventory.objectType].Add(character.inventory);
        }
        else if (character.inventory.objectType != sourceInventory.objectType)
        {
            Debug.ULogErrorChannel("InventoryManager", "Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.inventory.StackSize += amount;

        if (character.inventory.maxStackSize < character.inventory.StackSize)
        {
            sourceInventory.StackSize = character.inventory.StackSize - character.inventory.maxStackSize;
            character.inventory.StackSize = character.inventory.maxStackSize;
        }
        else
        {
            sourceInventory.StackSize -= amount;
        }

        CleanupInventory(sourceInventory);

        return true;
    }

    /// <summary>
    /// Gets the type of the closest inventory of.
    /// </summary>
    /// <returns>The closest inventory of type.</returns>
    /// <param name="objectType">Object type.</param>
    /// <param name="t">Tile from, which distance is mesured.</param>
    /// <param name="desiredAmount">Desired amount. If no stack has enough, it instead returns the largest.</param>
    public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredAmount, bool canTakeFromStockpile)
    {
        Path_AStar path = GetPathToClosestInventoryOfType(objectType, t, desiredAmount, canTakeFromStockpile);
        return path.EndTile().Inventory;
    }

    public bool QuickCheck(string objectType)
    {
        // If the inventories doesn't contain the objectType, we know that no
        // stacks of this type exists and can return.
        if (inventories.ContainsKey(objectType) == false)
        {
            return false;
        }

        // We know that there is a list for objectType, we still need to test if
        // the list contains anything
        if (inventories[objectType].Count == 0)
        {
            return false;
        }

        return true;
    }

    public Path_AStar GetPathToClosestInventoryOfType(string objectType, Tile t, int desiredAmount, bool canTakeFromStockpile)
    {
        QuickCheck(objectType);

        // We can also avoid going through the Astar construction if we know
        // that all available inventories are stockpiles and we are not allowed
        // to touch those
        if (!canTakeFromStockpile && inventories[objectType].TrueForAll(i => i.tile != null && i.tile.Furniture != null && i.tile.Furniture.IsStockpile()))
        {
            return null;
        }

        // We shouldn't search if all inventories are locked.
        if (inventories[objectType].TrueForAll(i => i.tile != null && i.tile.Furniture != null && i.tile.Inventory.locked))
        {
            return null;
        }

        // Test that there is at least one stack on the floor, otherwise the
        // search below might cause a full map search for nothing.
        if (inventories[objectType].Find(i => i.tile != null) == null)
        {
            return null;
        }

        // We know the objects are out there, now find the closest.
        Path_AStar path = new Path_AStar(World.Current, t, null, objectType, desiredAmount, canTakeFromStockpile);
        return path;
    }

    private void CleanupInventory(Inventory inv)
    {
        if (inv.StackSize == 0)
        {
            if (inventories.ContainsKey(inv.objectType))
            {
                inventories[inv.objectType].Remove(inv);
            }

            if (inv.tile != null)
            {
                inv.tile.Inventory = null;
                inv.tile = null;
            }

            if (inv.character != null)
            {
                inv.character.inventory = null;
                inv.character = null;
            }
        }
    }
}
