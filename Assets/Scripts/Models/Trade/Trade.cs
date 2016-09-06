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

    public Trade(Trader player, Trader trader)
    {
        Player = player;
        Trader = trader;

        List<Inventory> totalStock = new List<Inventory>();
        totalStock.AddRange(player.Stock);
        totalStock.AddRange(trader.Stock);
        TradeItems = totalStock.GroupBy(s => s.ObjectType).Select(g => new TradeItem
        {
            ObjectType = g.Key,
            BaseItemPrice = g.First().BasePrice,
            PlayerStock = player.Stock.Where(s => s.ObjectType == g.Key).Sum(s => s.StackSize),
            TraderStock = trader.Stock.Where(s => s.ObjectType == g.Key).Sum(s => s.StackSize),
            TradeAmount = 0,
            PlayerSellItemPrice = g.First().BasePrice * player.SaleMarginMultiplier,
            TraderSellItemPrice = g.First().BasePrice * trader.SaleMarginMultiplier
        }).ToList();
    }

    public float TradeCurrencyBalanceForPlayer
    {
        get
        {
            return TradeItems.Sum(i => i.TradeCurrencyBalanceForPlayer);
        }
    }

    public bool IsValid()
    {
        return TradeCurrencyBalanceForPlayer < 0
            ? Player.Currency.Balance >= -TradeCurrencyBalanceForPlayer
            : Trader.Currency.Balance >= TradeCurrencyBalanceForPlayer;
    }
}
