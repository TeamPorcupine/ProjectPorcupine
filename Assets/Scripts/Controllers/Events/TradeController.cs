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
using Scheduler;
using UnityEngine;
using Random = UnityEngine.Random;

[MoonSharpUserData]
public class TradeController
{
    public List<TraderShipController> TradeShips;

    private readonly ScheduledEvent traderVisitEvaluationEvent;

    /// <summary>
    /// Instanciate a new TradeController
    /// This will also schedull and event every 5 minutes for the trade controller to evaluate
    /// if a trader will or not visit.
    /// </summary>
    public TradeController()
    {
        TradeShips = new List<TraderShipController>();

        traderVisitEvaluationEvent = new ScheduledEvent(
            "EvaluateTraderVisit",
            EvaluateTraderVisit,
            (int)TimeSpan.FromMinutes(5).TotalSeconds,
            true);
        Scheduler.Scheduler.Current.RegisterEvent(traderVisitEvaluationEvent);
    }
    
    /// <summary>
    /// Summon the visit of a trader to a specific landing pad
    /// The trader will be generated randomly from the trader prototypes.
    /// </summary>
    /// <param name="landingPad"></param>
    public void CallTradeShipTest(Furniture landingPad)
    {
        // Currently not using any logic to select a trader
        TraderPrototype prototype = PrototypeManager.Trader[Random.Range(0, PrototypeManager.Trader.Count - 1)];
        Trader trader = prototype.CreateTrader();

        GameObject go = new GameObject(trader.Name);
        go.transform.parent = WorldController.Instance.transform;
        TraderShipController controller = go.AddComponent<TraderShipController>();
        TradeShips.Add(controller);
        controller.Trader = trader;
        controller.Speed = 5f;
        go.transform.position = new Vector3(-10, 50, 0);
        controller.LandingCoordinates = new Vector3(landingPad.Tile.X + 1, landingPad.Tile.Y + 1, 0);
        controller.LeavingCoordinates = new Vector3(100, 50, 0);
        go.transform.localScale = new Vector3(1, 1, 1);
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteManager.GetSprite("Trader", prototype.AnimationIdle.CurrentFrameName);
        spriteRenderer.sortingLayerName = "TradeShip";
        controller.AnimationFlying = prototype.AnimationFlying.Clone();
        controller.AnimationIdle = prototype.AnimationIdle.Clone();
        controller.Renderer = spriteRenderer;
    }

    /// <summary>
    /// Display the TradeDialogBox and allow the user to trade.
    /// </summary>
    /// <param name="tradeShip"></param>
    public void ShowTradeDialogBox(TraderShipController tradeShip)
    {
        DialogBoxManager dbm = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();

        Trader playerTrader = Trader.FromPlayer(World.Current.Wallet[tradeShip.Trader.Currency.Name]);
        Trade trade = new Trade(playerTrader, tradeShip.Trader);
        dbm.dialogBoxTrade.SetupTrade(trade);
        dbm.dialogBoxTrade.TradeCancelled = () =>
        {
            tradeShip.TradeCompleted = true;
            TradeShips.Remove(tradeShip);
        };
        dbm.dialogBoxTrade.TradeCompleted = () =>
        {
            tradeShip.TradeCompleted = true;
            TrasfertTradedItems(trade, tradeShip.LandingCoordinates);
            TradeShips.Remove(tradeShip);
        };
        dbm.dialogBoxTrade.ShowDialog();
    }

    /// <summary>
    /// Once a trade is completed (and the trade dialog box is close): 
    /// - spawn bougth inventory in a square of 6x6 around the tradingCoordinate (tile of the landing pad)
    /// - delete all sold inventory from stockpiles.
    /// </summary>
    private void TrasfertTradedItems(Trade trade, Vector3 tradingCoordinates)
    {
        trade.Player.Currency.Balance += trade.TradeCurrencyBalanceForPlayer;

        foreach (TradeItem tradeItem in trade.TradeItems)
        {
            if (tradeItem.TradeAmount > 0)
            {
                Tile tile = World.Current.GetTileAt((int)tradingCoordinates.x, (int)tradingCoordinates.y, (int)tradingCoordinates.z);
                Inventory inv = new Inventory(tradeItem.Type, tradeItem.TradeAmount, tradeItem.TradeAmount);
                World.Current.InventoryManager.PlaceInventoryAround(tile, inv, 6);
            }
            else if (tradeItem.TradeAmount < 0)
            {
                World.Current.InventoryManager.RemoveInventoryOfType(tradeItem.Type, -tradeItem.TradeAmount, true);
            }
        }
    }

    /// <summary>
    /// Check if the player base should have the visit of a trader
    /// this method is very simple for now: 
    ///   if the player built a landing pad, then a trader will come.
    /// </summary>
    /// <param name="scheduledEvent"></param>
    private void EvaluateTraderVisit(ScheduledEvent scheduledEvent)
    {
        Furniture landingPad = FindRandomLandingPadWithouTrader();

        if (landingPad != null)
        {
            CallTradeShipTest(landingPad);
        }
    }

    /// <summary>
    /// Search all the built furniture in the world for the one with the tag 'LandingPad'.
    /// </summary>
    /// <returns></returns>
    private Furniture FindRandomLandingPadWithouTrader()
    {
        List<Furniture> landingPads = World.Current.FurnitureManager.Find(f => f.HasTypeTag("LandingPad"));

        if (landingPads.Any())
        {
            return landingPads[Random.Range(0, landingPads.Count - 1)];
        }

        return null;
    }
}