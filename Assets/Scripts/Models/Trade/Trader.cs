#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RequestLevel
{
        None = 0,
        Desired = 1,
        Needed = 2,
        Desperate = 3
}

public class Trader
{
    public List<TraderPotentialInventory> possibleStock;

    public Dictionary<TraderPotentialInventory, RequestLevel> requests;

    public float requestChanceModifier = 0.2f;

    public string Name { get; set; }

    public Currency Currency { get; set; }

    public float SaleMarginMultiplier { get; set; }

    public List<Inventory> Stock { get; set; }

    /// <summary>
    /// Create a Trader from the current player
    /// This method will scan every stockpile build and add the found inventory to the stock
    /// It will also assign a 0.8f sale margin multiplayer to the Trader.
    /// </summary>
    public static Trader FromPlayer(Currency currency)
    {
        Trader t = new Trader
        {
            Name = "Player",
            SaleMarginMultiplier = 0.8f,
            Stock = new List<Inventory>(),
            Currency = currency
        };

        List<List<Inventory>> worldInventories =
            WorldController.Instance.World.inventoryManager.inventories.Values.Select(i => i.ToList()).ToList();
        
        foreach (List<Inventory> worldInventory in worldInventories)
        {
            foreach (Inventory inventory in worldInventory)
            {
                if (inventory.Tile != null && 
                    inventory.Tile.Furniture != null &&
                    inventory.Tile.Furniture.ObjectType == "Stockpile")
                {
                    t.Stock.Add(inventory);
                }
            }
        }
        
        return t;
    }

    // <summary>
    // Function allows to request items which have a higher 
    // chance to be brought the next time this trader comes
    // </summary>
    public void RequestItems(Dictionary<TraderPotentialInventory, RequestLevel> requests)
    {
        if (this.requests == null)
        {
            this.requests = new Dictionary<TraderPotentialInventory, RequestLevel>();
        }
        
        this.requests.Union(requests);
    }

    public void RefreshInventory()
    {
        List<TraderPotentialInventory> stockExceptRequests = possibleStock.Except(requests.Keys).ToList();

        // Maybe change different attributes(sales margins, possible stocks, traded currencies, etc.)
        // based on a relationship / time element?
        foreach (TraderPotentialInventory potentialStock in stockExceptRequests)
        {
            bool itemIsInStock = Random.Range(0f, 1f) > potentialStock.Rarity;

            if (itemIsInStock)
            {
                AddItemToStock(potentialStock);
            }
        }

        // TODO make requests cost more based on how much you want them
        foreach (KeyValuePair<TraderPotentialInventory, RequestLevel> requestAndLevel in requests)
        {
            bool itemIsInStock = Random.Range(0f, 1f) + (requestChanceModifier * (int)requestAndLevel.Value) > requestAndLevel.Key.Rarity;

            if (itemIsInStock)
            {
                AddItemToStock(requestAndLevel.Key);
                requests.Remove(requestAndLevel.Key);
            }
        }
    }
    
    private void AddItemToStock(TraderPotentialInventory inventory)
    {
        if (!string.IsNullOrEmpty(inventory.ObjectType))
        {
            Inventory newInventory = new Inventory(
                inventory.ObjectType,
                Random.Range(inventory.MinQuantity, inventory.MaxQuantity));

            Stock.Add(newInventory);
        }
        else if (!string.IsNullOrEmpty(inventory.ObjectCategory))
        {
            List<InventoryCommon> potentialObjects = GetInventoryCommonWithCategory(inventory.ObjectCategory);

            foreach (InventoryCommon potentialObject in potentialObjects)
            {
                Inventory newInventory = new Inventory(
                    potentialObject.objectType,
                    Random.Range(inventory.MinQuantity, inventory.MaxQuantity));

                Stock.Add(newInventory);
            }
        }
    }

    private List<InventoryCommon> GetInventoryCommonWithCategory(string category)
    {
        return PrototypeManager.Inventory.Values.Where(i => i.category == category).ToList();
    }
}