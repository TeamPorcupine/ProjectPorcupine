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
    
    public void CallTradeShipTest(Furniture landingPad)
    {
        // Currently not using any logic to select a trader
        TraderPrototype prototype = PrototypeManager.Trader.Get(Random.Range(0, PrototypeManager.Trader.Count - 1));
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
        spriteRenderer.sprite = SpriteManager.current.GetSprite("Trader", "BasicHaulShip");
        spriteRenderer.sortingLayerName = "TradeShip";
    }

    public void ShowTradeDialogBox(TraderShipController tradeShip)
    {
        DialogBoxManager dbm = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();

        Trader playerTrader = Trader.FromPlayer(World.Current.Wallet.Currencies[tradeShip.Trader.Currency.Name]);
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

    private void TrasfertTradedItems(Trade trade, Vector3 tradingCoordinates)
    {
        trade.Player.Currency.Balance += trade.TradeCurrencyBalanceForPlayer;

        foreach (TradeItem tradeItem in trade.TradeItems)
        {
            if (tradeItem.TradeAmount > 0)
            {
                Tile tile = WorldController.Instance.World.GetFirstTileWithNoInventoryAround(6, (int)tradingCoordinates.x, (int)tradingCoordinates.y, (int)tradingCoordinates.z);
                Inventory inv = new Inventory(tradeItem.ObjectType, tradeItem.TradeAmount, tradeItem.TradeAmount);
                WorldController.Instance.World.inventoryManager.PlaceInventory(tile, inv);
            }
            else if (tradeItem.TradeAmount < 0)
            {
                WorldController.Instance.World.inventoryManager.QuickRemove(tradeItem.ObjectType, -tradeItem.TradeAmount, true);
            }
        }
    }

    private void EvaluateTraderVisit(ScheduledEvent scheduledEvent)
    {
        Furniture landingPad = FindRandomLandingPadWithouTrader();

        if (landingPad != null)
        {
            CallTradeShipTest(landingPad);
        }
    }

    private Furniture FindRandomLandingPadWithouTrader()
    {
        List<Furniture> landingPads = World.Current.furnitures.Where(f => f.HasTypeTag("LandingPad")).ToList();

        if (landingPads.Any())
        {
            return landingPads[Random.Range(0, landingPads.Count - 1)];
        }

        return null;
    }
}