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

    /// <summary>
    /// Create a trade instance between two trader
    /// Creating a trade will scan the inventory of both trader and assign a trade price to each inventory
    /// A trade is a simple pivot on 2 Stock in a format easier to display to a human.
    /// </summary>
    public Trade(Trader player, Trader trader)
    {
        Player = player;
        Trader = trader;

        List<Inventory> totalStock = new List<Inventory>();
        totalStock.AddRange(player.Stock);
        totalStock.AddRange(trader.Stock);
        TradeItems = totalStock.GroupBy(s => s.Type).Select(g => new TradeItem
        {
            Type = g.Key,
            BaseItemPrice = g.First().BasePrice,
            PlayerStock = player.Stock.Where(s => s.Type == g.Key).Sum(s => s.StackSize),
            TraderStock = trader.Stock.Where(s => s.Type == g.Key).Sum(s => s.StackSize),
            TradeAmount = 0,
            PlayerSellItemPrice = g.First().BasePrice * player.SaleMarginMultiplier,
            TraderSellItemPrice = g.First().BasePrice * trader.SaleMarginMultiplier
        }).ToList();
    }

    /// <summary>
    /// Current value of the deal for the player currency in the trade currency
    /// If negative, the player has to give money to complete the trade
    /// If positive, the player win money at the end of the trade.
    /// </summary>
    public float TradeCurrencyBalanceForPlayer
    {
        get
        {
            return TradeItems.Sum(i => i.TradeCurrencyBalanceForPlayer);
        }
    }

    /// <summary>
    /// Check if both trader in the trade have enough currency to complete the deal.
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        return TradeCurrencyBalanceForPlayer < 0
            ? Player.Currency.Balance >= -TradeCurrencyBalanceForPlayer
            : Trader.Currency.Balance >= TradeCurrencyBalanceForPlayer;
    }
}
