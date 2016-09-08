#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class InventoryManager
{
    private static readonly string InventoryManagerLogChanel = "InventoryManager";

    public InventoryManager()
    {
        Inventories = new Dictionary<string, List<Inventory>>();
    }

    public Dictionary<string, List<Inventory>> Inventories { get; private set; }

    public bool PlaceInventory(Tile tile, Inventory inventory)
    {
        bool tileWasEmpty = tile.Inventory == null;

        if (tile.PlaceInventory(inventory) == false)
        {
            // The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

        CleanupInventory(inventory);

        // We may also created a new stack on the tile, if the startTile was previously empty.
        if (tileWasEmpty)
        {
            if (Inventories.ContainsKey(tile.Inventory.ObjectType) == false)
            {
                Inventories[tile.Inventory.ObjectType] = new List<Inventory>();
            }

            Inventories[tile.Inventory.ObjectType].Add(tile.Inventory);

            World.Current.OnInventoryCreatedCallback(tile.Inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inventory)
    {
        if (job.inventoryRequirements.ContainsKey(inventory.ObjectType) == false)
        {
            Debug.ULogErrorChannel(InventoryManagerLogChanel, "Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        job.inventoryRequirements[inventory.ObjectType].StackSize += inventory.StackSize;

        if (job.inventoryRequirements[inventory.ObjectType].MaxStackSize < job.inventoryRequirements[inventory.ObjectType].StackSize)
        {
            inventory.StackSize = job.inventoryRequirements[inventory.ObjectType].StackSize - job.inventoryRequirements[inventory.ObjectType].MaxStackSize;
            job.inventoryRequirements[inventory.ObjectType].StackSize = job.inventoryRequirements[inventory.ObjectType].MaxStackSize;
        }
        else
        {
            inventory.StackSize = 0;
        }

        CleanupInventory(inventory);

        return true;
    }

    public bool PlaceInventory(Character character, Inventory sourceInventory, int amount = -1)
    {
        amount = amount < 0 ? sourceInventory.StackSize : Math.Min(amount, sourceInventory.StackSize);

        if (character.inventory == null)
        {
            character.inventory = sourceInventory.Clone();
            character.inventory.StackSize = 0;
            Inventories[character.inventory.ObjectType].Add(character.inventory);
        }
        else if (character.inventory.ObjectType != sourceInventory.ObjectType)
        {
            Debug.ULogErrorChannel(InventoryManagerLogChanel, "Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.inventory.StackSize += amount;

        if (character.inventory.MaxStackSize < character.inventory.StackSize)
        {
            sourceInventory.StackSize = character.inventory.StackSize - character.inventory.MaxStackSize;
            character.inventory.StackSize = character.inventory.MaxStackSize;
        }
        else
        {
            sourceInventory.StackSize -= amount;
        }

        CleanupInventory(sourceInventory);

        return true;
    }

    /// <summary>
    /// Gets <see cref="Inventory"/> closest to <see cref="startTile"/>.
    /// </summary>
    /// <returns>The closest inventory of type.</returns>
    public Inventory GetClosestInventoryOfType(string objectType, Tile startTile, int desiredAmount, bool canTakeFromStockpile)
    {
        Path_AStar path = GetPathToClosestInventoryOfType(objectType, startTile, desiredAmount, canTakeFromStockpile);
        return path.EndTile().Inventory;
    }

    public bool HasInventoryOfType(string objectType)
    {
        return Inventories.ContainsKey(objectType) && Inventories[objectType].Count != 0;
    }

    public bool RemoveInventoryOfType(string objectType, int quantity, bool onlyFromStockpiles)
    {
        if (!HasInventoryOfType(objectType))
        {
            return quantity == 0;
        }

        foreach (Inventory inventory in Inventories[objectType].ToList())
        {
            if (onlyFromStockpiles)
            {
                if (inventory.Tile == null || 
                    inventory.Tile.Furniture == null ||
                    inventory.Tile.Furniture.ObjectType != "Stockpile")
                {
                    continue;
                }
            }

            if (quantity <= 0)
            {
                break;
            }

            int removedFromStack = Math.Min(inventory.StackSize, quantity);
            quantity -= removedFromStack;
            inventory.StackSize -= removedFromStack;
            CleanupInventory(inventory);
        }

        return quantity == 0;
    }

    public Path_AStar GetPathToClosestInventoryOfType(string objectType, Tile t, int desiredAmount, bool canTakeFromStockpile)
    {
        HasInventoryOfType(objectType);

        // We can also avoid going through the Astar construction if we know
        // that all available inventories are stockpiles and we are not allowed
        // to touch those
        if (!canTakeFromStockpile && Inventories[objectType].TrueForAll(i => i.Tile != null && i.Tile.Furniture != null && i.Tile.Furniture.IsStockpile()))
        {
            return null;
        }

        // We shouldn't search if all inventories are locked.
        if (Inventories[objectType].TrueForAll(i => i.Tile != null && i.Tile.Furniture != null && i.Tile.Inventory != null && i.Tile.Inventory.Locked))
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
        Path_AStar path = new Path_AStar(World.Current, t, null, objectType, desiredAmount, canTakeFromStockpile);
        return path;
    }

    private void CleanupInventory(Inventory inventory)
    {
        if (inventory.StackSize != 0)
        {
            return;
        }

        if (Inventories.ContainsKey(inventory.ObjectType))
        {
            Inventories[inventory.ObjectType].Remove(inventory);
        }

        if (inventory.Tile != null)
        {
            inventory.Tile.Inventory = null;
            inventory.Tile = null;
        }
    }
}
