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
using UnityEngine;
using ProjectPorcupine.Pathfinding;

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

    public bool PlaceInventory(Job job, Character character)
    {
        Inventory sourceInventory = character.inventory;

        // Check that it's wanted by the job
        if (job.RequestedItems.ContainsKey(sourceInventory.Type) == false)
        {
            Debug.ULogErrorChannel(InventoryManagerLogChanel, "Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        // Check that there is a target to transfer to
        if (job.HeldInventory.ContainsKey(sourceInventory.Type) == false)
        {
            job.HeldInventory[sourceInventory.Type] = new Inventory(sourceInventory.Type, 0, sourceInventory.MaxStackSize);
        }

        Inventory targetInventory = job.HeldInventory[sourceInventory.Type];
        int transferAmount = Mathf.Min(targetInventory.MaxStackSize - targetInventory.StackSize, sourceInventory.StackSize);

        sourceInventory.StackSize -= transferAmount;
        targetInventory.StackSize += transferAmount;

        CleanupInventory(character);

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
    public Inventory GetClosestInventoryOfType(string objectType, Tile tile, bool canTakeFromStockpile)
    {
        List<Tile> path = GetPathToClosestInventoryOfType(objectType, tile, canTakeFromStockpile);
        return path != null ? path.Last().Inventory : null;
    }

    public static bool InventoryCanBePickedUp(Inventory inventory, bool canTakeFromStockpile)
    {
        // You can't pick up stuff that isn't on a tile or if it's locked
        if (inventory == null || inventory.Tile == null || inventory.Locked)
        {
            return false;
        }

        Furniture furniture = inventory.Tile.Furniture;
        return furniture == null || canTakeFromStockpile == true || furniture.HasTypeTag("Storage") == false;
    }

    public bool RemoveInventoryOfType(string type, int quantity, bool onlyFromStockpiles)
    {
        if (!HasInventoryOfType(type, true))
        {
            return quantity == 0;
        }

        foreach (Inventory inventory in Inventories[type].ToList())
        {
            if (onlyFromStockpiles)
            {
                if (inventory.Tile == null ||
                    inventory.Tile.Furniture == null ||
                    inventory.Tile.Furniture.Type != "Stockpile" ||
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

    public bool HasInventoryOfType(string type, bool canTakeFromStockpile)
    {
        if (Inventories.ContainsKey(type) == false || Inventories[type].Count == 0)
        {
            return false;
        }

        return Inventories[type].Find(inventory => InventoryCanBePickedUp(inventory, canTakeFromStockpile)) != null;
    }

    public bool HasInventoryOfType(string[] objectTypes, bool canTakeFromStockpile)
    {
        // Test that we have records for any of the types
        List<string> filteredTypes = objectTypes
            .ToList()
            .FindAll(type => Inventories.ContainsKey(type) && Inventories[type].Count > 0);

        if (filteredTypes.Count == 0)
        {
            return false;
        }

        foreach (string objectType in filteredTypes)
        {
            if (Inventories[objectType].Find(inventory => InventoryCanBePickedUp(inventory, canTakeFromStockpile)) != null)
            {
                return true;
            }
        }

        return false;
    }

    public List<Tile> GetPathToClosestInventoryOfType(string type, Tile tile, bool canTakeFromStockpile)
    {
        if (HasInventoryOfType(type, canTakeFromStockpile) == false)
        {
            return null;
        }

        // We know the objects are out there, now find the closest.
        return Pathfinder.FindPathToInventory(tile, type, canTakeFromStockpile);
    }

    public List<Tile> GetPathToClosestInventoryOfType(string[] objectTypes, Tile tile, bool canTakeFromStockpile)
    {
        if (HasInventoryOfType(objectTypes, canTakeFromStockpile) == false)
        {
            return null;
        }

        // We know the objects are out there, now find the closest.
        return Pathfinder.FindPathToInventory(tile, objectTypes, canTakeFromStockpile);
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

    private void CleanupInventory(Character character)
    {
        CleanupInventory(character.inventory);

        if (character.inventory.StackSize == 0)
        {
            character.inventory = null;
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
