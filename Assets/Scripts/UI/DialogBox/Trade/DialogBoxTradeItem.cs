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
    private TradeItem _item;

    public Text ItemNameText;
    public Text PlayerStockText;
    public Text PlayerSellItemPriceText;
    public Text TraderStockText;
    public Text TraderSellItemPriceText;
    public InputField TradeAmountText;

    public event Action<TradeItem> OnTradeAmountChangedEvent;

    public void SetupTradeItem(TradeItem item)
    {
        _item = item;
        BindInterface();
    }

    private void BindInterface()
    {
        ItemNameText.text = _item.ObjectType;
        PlayerStockText.text = _item.PlayerStock.ToString();
        PlayerSellItemPriceText.text = _item.PlayerSellItemPrice.ToString();
        TraderStockText.text = _item.TraderStock.ToString();
        TraderSellItemPriceText.text = _item.TraderSellItemPrice.ToString();
        TradeAmountText.text = _item.TradeAmount.ToString();
    }

    public void OnTradeAmountChanged()
    {
        BindInterface();
        if (OnTradeAmountChangedEvent != null)
            OnTradeAmountChangedEvent(this._item);
    }

    public void PlayerBuyOneMore()
    {
        _item.TradeAmount++;
        OnTradeAmountChanged();
    }

    public void TraderBuyOneMore()
    {
        _item.TradeAmount--;
        OnTradeAmountChanged();
    }

    public void PlayerBuyAll()
    {
        _item.TradeAmount = _item.TraderStock;
        OnTradeAmountChanged();
    }

    public void TraderBuyAll()
    {
        _item.TradeAmount = -_item.PlayerStock;
        OnTradeAmountChanged();
    }
}