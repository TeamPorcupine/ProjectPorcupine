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

    public event Action<Inventory> InventoryCreated;

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
            if (Inventories.ContainsKey(tile.Inventory.Type) == false)
            {
                Inventories[tile.Inventory.Type] = new List<Inventory>();
            }

            Inventories[tile.Inventory.Type].Add(tile.Inventory);
            InvokeInventoryCreated(tile.Inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inventory)
    {
        if (job.inventoryRequirements.ContainsKey(inventory.Type) == false)
        {
            Debug.ULogErrorChannel(InventoryManagerLogChanel, "Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        job.inventoryRequirements[inventory.Type].StackSize += inventory.StackSize;

        if (job.inventoryRequirements[inventory.Type].MaxStackSize < job.inventoryRequirements[inventory.Type].StackSize)
        {
            inventory.StackSize = job.inventoryRequirements[inventory.Type].StackSize - job.inventoryRequirements[inventory.Type].MaxStackSize;
            job.inventoryRequirements[inventory.Type].StackSize = job.inventoryRequirements[inventory.Type].MaxStackSize;
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
            Inventories[character.inventory.Type].Add(character.inventory);
        }
        else if (character.inventory.Type != sourceInventory.Type)
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
    public Inventory GetClosestInventoryOfType(string type, Tile startTile, int desiredAmount, bool canTakeFromStockpile)
    {
        Path_AStar path = GetPathToClosestInventoryOfType(type, startTile, desiredAmount, canTakeFromStockpile);
        return path.EndTile().Inventory;
    }

    public bool HasInventoryOfType(string type)
    {
        return Inventories.ContainsKey(type) && Inventories[type].Count != 0;
    }

    public bool RemoveInventoryOfType(string type, int quantity, bool onlyFromStockpiles)
    {
        if (!HasInventoryOfType(type))
        {
            return quantity == 0;
        }

        foreach (Inventory inventory in Inventories[type].ToList())
        {
            if (onlyFromStockpiles)
            {
                if (inventory.Tile == null ||
                    inventory.Tile.Furniture == null ||
                    inventory.Tile.Furniture.ObjectType != "Stockpile" ||
                    inventory.Tile.Furniture.HasTypeTag("Stockpile"))
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

    public Path_AStar GetPathToClosestInventoryOfType(string type, Tile tile, int desiredAmount, bool canTakeFromStockpile)
    {
        HasInventoryOfType(type);

        // We can also avoid going through the A* construction if we know
        // that all available inventories are stockpiles and we are not allowed
        // to touch those
        if (!canTakeFromStockpile && Inventories[type].TrueForAll(i => i.Tile != null && i.Tile.Furniture != null && i.Tile.Furniture.IsStockpile()))
        {
            return null;
        }

        // We shouldn't search if all inventories are locked.
        if (Inventories[type].TrueForAll(i => i.Tile != null && i.Tile.Furniture != null && i.Tile.Inventory != null && i.Tile.Inventory.Locked))
        {
            return null;
        }

        // Test that there is at least one stack on the floor, otherwise the
        // search below might cause a full map search for nothing.
        if (Inventories[type].Find(i => i.Tile != null) == null)
        {
            return null;
        }

        // We know the objects are out there, now find the closest.
        Path_AStar path = new Path_AStar(World.Current, tile, null, type, desiredAmount, canTakeFromStockpile);
        return path;
    }

    private void CleanupInventory(Inventory inventory)
    {
        if (inventory.StackSize != 0)
        {
            return;
        }

        if (Inventories.ContainsKey(inventory.Type))
        {
            Inventories[inventory.Type].Remove(inventory);
        }

        if (inventory.Tile != null)
        {
            inventory.Tile.Inventory = null;
            inventory.Tile = null;
        }
    }

    private void InvokeInventoryCreated(Inventory inventory)
    {
        Action<Inventory> handler = InventoryCreated;
        if (handler != null)
        {
            handler(inventory);
        }
    }
}
