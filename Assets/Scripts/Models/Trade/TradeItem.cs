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