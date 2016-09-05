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

public class Trader
{
    public string Name;
    public Currency Currency;
    public float SaleMarginMultiplier;
    public List<Inventory> Stock;

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
}
