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

public class Trade
{
    public List<TradeItem> TradeItems;

    public Trader Player;
    public Trader Trader;

    public float TradeCurrencyBalanceForPlayer
    {
        get { return TradeItems.Sum(i => i.TradeCurrencyBalanceForPlayer); }
    }

    public Trade(Trader player, Trader trader)
    {
        Player = player;
        Trader = trader;

        var totalStock = new List<Inventory>();
        totalStock.AddRange(player.Stock);
        totalStock.AddRange(trader.Stock);
        TradeItems = totalStock.GroupBy(s => s.objectType).Select(g => new TradeItem
        {
            ObjectType = g.Key,
            BaseItemPrice = g.First().basePrice,
            PlayerStock = player.Stock.Where(s => s.objectType == g.Key).Sum(s => s.stackSize),
            TraderStock = trader.Stock.Where(s => s.objectType == g.Key).Sum(s => s.stackSize),
            TradeAmount = 0,
            PlayerSellItemPrice = g.First().basePrice*player.SaleMarginMultiplier,
            TraderSellItemPrice = g.First().basePrice*trader.SaleMarginMultiplier
        }).ToList();
    }

    public void Accept()
    {
        //TODO
    }

    public bool IsValid()
    {
        return true; //TODO
    }
}