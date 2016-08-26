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
    public float CurrencyBalance;
    public float SaleMarginMultiplier;
    public List<Inventory> Stock;

    public static Trader FromPlayer()
    {
        Trader t = new Trader();
        t.Name = "Player";
        t.SaleMarginMultiplier = 0.8f;
        t.Stock = new List<Inventory>();

        List<List<Inventory>> worldInventories =
            World.current.inventoryManager.inventories.Values.Select(i => i.ToList()).ToList();
        
        foreach (List<Inventory> worldInventory in worldInventories)
        {
            foreach (Inventory inventory in worldInventory)
            {
                if (inventory.tile != null && 
                    inventory.tile.Furniture != null &&
                    inventory.tile.Furniture.objectType == "Stockpile")
                {
                    t.Stock.Add(inventory);
                }
            }
        }
        
        return t;
    }
}