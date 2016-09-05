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
public enum RequestLevel {
    	
    	Desired = 1,
    	Needed = 2,
    	Desperate = 3
    	
}

public class Trader
{
    public string Name;
    public Currency Currency;
    public float SaleMarginMultiplier;
    public List<Inventory> Stock;
    public List<TraderPotentialInventory> possibleStock;
    public Dictionary<TraderPotentialInventory, RequestLevel> requests;
    public float requestChanceModifier = 0.2f;
    
    
    
    // Function allows to request items which have a 
    // higher chance to be brought the next time this
    // trader comes
    public void RequestItems (Dictionary <TraderPotentialInventory, RequestLevel> requests) {
    	
    	this.requests = (Dictionary<TraderPotentialInventory, RequestLevel>)this.requests.Union (requests);
    	
    }
    
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
                if (inventory.tile != null && 
                    inventory.tile.Furniture != null &&
                    inventory.tile.Furniture.ObjectType == "Stockpile")
                {
                    t.Stock.Add(inventory);
                }
            }
        }
        
        return t;
    }
    
    
    
    public void RefreshInventory () {
    	
    	List<TraderPotentialInventory> stockExceptRequests = possibleStock.Except (requests.Keys).ToList();
    	
    	// Maybe change different attributes (sales margins, possible stocks, traded currencies, etc.)
    	// based on a relationship / time element?
    	foreach (TraderPotentialInventory potentialStock in stockExceptRequests)
        {
            bool itemIsInStock = Random.Range(0f, 1f) > potentialStock.Rarity;

            if (itemIsInStock)
            	AddItemToStock (potentialStock);
            
        }
    	// TODO make requests cost more based on how much you want them
    	foreach (KeyValuePair<TraderPotentialInventory, RequestLevel> requestAndLevel in requests) {
    		
    		bool itemIsInStock = Random.Range(0f, 1f) + requestChanceModifier * (int)(requestAndLevel.Value) > requestAndLevel.Key.Rarity;
    		
    		if (itemIsInStock) {
    			AddItemToStock (requestAndLevel.Key);
    			requests.Remove (requestAndLevel.Key);
    		}
    		
    		
    	}
    	
    	
    }
    void AddItemToStock (TraderPotentialInventory inventory) {
    	
    	Stock.Add (new Inventory {
    	           	objectType = inventory.ObjectType,
    	           	StackSize = Random.Range (inventory.MinQuantity, inventory.MaxQuantity)
    	           });
    	
    	
    }
}
