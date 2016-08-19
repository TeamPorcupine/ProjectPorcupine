//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class InventoryManager
{

    // This is a list of all "live" inventories.
    // Later on this will likely be organized by rooms instead
    // of a single master list. (Or in addition to.)
    public Dictionary<string, List<Inventory>> Inventories { get; set; }

    public InventoryManager()
    {
        Inventories = new Dictionary<string, List<Inventory>>();
    }

    private void CleanupInventory(Inventory inv)
    {
        if (inv.StackSize == 0)
        {
            if (Inventories.ContainsKey(inv.objectType))
            {
                Inventories[inv.objectType].Remove(inv);
            }
            if (inv.Tile != null)
            {
                inv.Tile.Inventory = null;
                inv.Tile = null;
            }
            if (inv.Character != null)
            {
                inv.Character.Inventory = null;
                inv.Character = null;
            }
        }

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
            if (Inventories.ContainsKey(tile.Inventory.objectType) == false)
            {
                Inventories[tile.Inventory.objectType] = new List<Inventory>();
            }

            Inventories[tile.Inventory.objectType].Add(tile.Inventory);

            World.Current.OnInventoryCreated(tile.Inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inv)
    {
        if (job.InventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            Debug.LogError("Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        job.InventoryRequirements[inv.objectType].StackSize += inv.StackSize;

        if (job.InventoryRequirements[inv.objectType].maxStackSize < job.InventoryRequirements[inv.objectType].StackSize)
        {
            inv.StackSize = job.InventoryRequirements[inv.objectType].StackSize - job.InventoryRequirements[inv.objectType].maxStackSize;
            job.InventoryRequirements[inv.objectType].StackSize = job.InventoryRequirements[inv.objectType].maxStackSize;
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

        if (character.Inventory == null)
        {
            character.Inventory = sourceInventory.Clone();
            character.Inventory.StackSize = 0;
            Inventories[character.Inventory.objectType].Add(character.Inventory);
        }
        else if (character.Inventory.objectType != sourceInventory.objectType)
        {
            Debug.LogError("Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.Inventory.StackSize += amount;

        if (character.Inventory.maxStackSize < character.Inventory.StackSize)
        {
            sourceInventory.StackSize = character.Inventory.StackSize - character.Inventory.maxStackSize;
            character.Inventory.StackSize = character.Inventory.maxStackSize;
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
    /// <param name="tile">T.</param>
    /// <param name="desiredAmount">Desired amount. If no stack has enough, it instead returns the largest</param>
    public Inventory GetClosestInventoryOfType(string objectType, Tile tile, int desiredAmount, bool canTakeFromStockpile)
    {
        Path_AStar path = GetPathToClosestInventoryOfType(objectType, tile, desiredAmount, canTakeFromStockpile);
        return path.EndTile().Inventory;
    }

    public Path_AStar GetPathToClosestInventoryOfType(string objectType, Tile tile, int desiredAmount, bool canTakeFromStockpile)
    {
        // If the inventories doesn't contain the objectType, we know that no
        // stacks of this type exists and can return.
        if (Inventories.ContainsKey(objectType) == false)
        {
            return null;
        }

        // We know that there is a list for objectType, we still need to test if
        // the list contains anything
        if (Inventories[objectType].Count == 0)
        {
            return null;
        }

        // We can also avoid going through the Astar construction if we know
        // that all available inventories are stockpiles and we are not allowed
        // to touch those
        if (!canTakeFromStockpile && Inventories[objectType].TrueForAll(i => i.Tile != null && i.Tile.Furniture != null && i.Tile.Furniture.IsStockpile()))
        {
            return null;
        }

        // Test that there is at least one stack on the floor, otherwise the
        // search below might cause a full map search for nothing.
        if (Inventories[objectType].Find(i => i.Tile != null) == null)
        {
            return null;
        }

        // We know the objects are out there, now find the closest.
        Path_AStar path = new Path_AStar(World.Current, tile, null, objectType, desiredAmount, canTakeFromStockpile);
        return path;
    }
}
