using System;
using System.Linq;
using Assets.Scripts.Utilities.GalacticTradeNode;
using Assets.Scripts.Utilities.GalacticTradeNode.Models;
using UnityEngine;
using UnityEngine.UI;

public class GalacticMarketOrder : MonoBehaviour
{
    public Order Order;

    public Text OrderDescriptionText;
    public Text ButtonText;

    private void Start()
    {
        OrderDescriptionText.text = Order.TraderName + " " + Enum.GetName(typeof (OrderDirection), Order.Direction) +
                                    " " + Order.CurrentQuantity +
                                    " * " + Order.ItemName + " @ " + Order.SingleItemPrice +
                                    " each (" + Order.SingleItemPrice*Order.CurrentQuantity + ")";

        ButtonText.text = Order.Direction == OrderDirection.Sell ? "Buy 10" : "Sell 10";
    }

    public void DoTrade()
    {
        Debug.Log("Trading with " + Order);
        var context = new GalacticMarketContext();

        //TODO implement much more realistic logic
        Order.CurrentQuantity -= 10;
        if (Order.CurrentQuantity <= 0)
        {
            Order.IsRunningOrder = false;
            Destroy(gameObject);
        }

        context.Orders.Save(Order);

        switch (Order.Direction)
        {
            case OrderDirection.Buy:
                CreareHaulJobToFurniture(Order.ItemName, 10);
                break;
            case OrderDirection.Sell:
                CreateInventoryOnGround(Order.ItemName, 10);
                break;
        }
    }
    
    private void CreateInventoryOnGround(string itemName, int i)
    {
        var terminal = World.current.furnitures.FirstOrDefault(f => f.Name == "Galactic Market Terminal");
        if (terminal != null)
        {
            World.current.inventoryManager.PlaceInventory(terminal.GetSpawnSpotTile(),
                new Inventory(itemName, 50, i));
        }
    }

    private void CreareHaulJobToFurniture(string itemName, int quantity)
    {
        //FIXME: i'm not sure how to create a haul job to the furniture
        var terminal = World.current.furnitures.FirstOrDefault(f => f.Name == "Galactic Market Terminal");
        if (terminal != null)
        {
            Job j = new Job(terminal.GetJobSpotTile(), TileType.Floor, (cj) =>
            {
                if (cj.HasAllMaterial())
                {
                }
            }, 10, new[] { new Inventory(itemName, quantity, 5) }, false);
            j.canTakeFromStockpile = true;
        }
    }
}