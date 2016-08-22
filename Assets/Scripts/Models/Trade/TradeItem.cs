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
    public string ObjectType;
    public float BaseItemPrice;

    public float PlayerSellItemPrice;
    public float TraderSellItemPrice;

    public int PlayerStock;
    public int TraderStock;
    private int _tradeAmount;

    public int TradeAmount
    {
        get { return _tradeAmount; }
        set
        {
            _tradeAmount = value < 0
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