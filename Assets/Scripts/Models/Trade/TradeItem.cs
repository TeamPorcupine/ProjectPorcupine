#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

public class TradeItem
{
    private int tradeAmount;

    public string Type { get; set; }

    public float BaseItemPrice { get; set; }

    public float PlayerSellItemPrice { get; set; }

    public float TraderSellItemPrice { get; set; }

    public int PlayerStock { get; set; }

    public int TraderStock { get; set; }

    public int TradeAmount
    {
        get
        {
            return tradeAmount;
        }

        set
        {
            tradeAmount = value < 0
                ? Math.Max(value, -PlayerStock)
                : Math.Min(value, TraderStock);
        }
    }

    public float TradeCurrencyBalanceForPlayer
    {
        get
        {
            return TradeAmount < 0
                ? -TradeAmount * PlayerSellItemPrice
                : -TradeAmount * TraderSellItemPrice;
        }
    }
}
