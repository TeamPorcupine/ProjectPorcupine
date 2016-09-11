#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxTradeItem : MonoBehaviour
{
    public Text ItemNameText;
    public Text PlayerStockText;
    public Text PlayerSellItemPriceText;
    public Text TraderStockText;
    public Text TraderSellItemPriceText;
    public InputField TradeAmountText;

    private TradeItem item;

    public event Action<TradeItem> OnTradeAmountChangedEvent;

    public void SetupTradeItem(TradeItem item)
    {
        this.item = item;
        BindInterface();
    }

    public void SetTradeAmount()
    {
        item.TradeAmount = int.Parse(TradeAmountText.text);
    }

    public void OnTradeAmountChanged()
    {
        BindInterface();
        if (OnTradeAmountChangedEvent != null)
        {
            OnTradeAmountChangedEvent(this.item);
        }
    }

    public void PlayerBuyOneMore()
    {
        item.TradeAmount++;
        OnTradeAmountChanged();
    }

    public void TraderBuyOneMore()
    {
        item.TradeAmount--;
        OnTradeAmountChanged();
    }

    public void PlayerBuyAll()
    {
        item.TradeAmount = item.TraderStock;
        OnTradeAmountChanged();
    }

    public void TraderBuyAll()
    {
        item.TradeAmount = -item.PlayerStock;
        OnTradeAmountChanged();
    }

    private void BindInterface()
    {
        ItemNameText.text = item.Type;
        PlayerStockText.text = (item.PlayerStock + item.TradeAmount).ToString();
        PlayerSellItemPriceText.text = item.PlayerSellItemPrice.ToString("N2");
        TraderStockText.text = (item.TraderStock - item.TradeAmount).ToString();
        TraderSellItemPriceText.text = item.TraderSellItemPrice.ToString("N2");
        TradeAmountText.text = item.TradeAmount.ToString();
    }
}