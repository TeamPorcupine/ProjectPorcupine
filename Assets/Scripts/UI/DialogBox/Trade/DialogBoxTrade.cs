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
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxTrade : DialogBox
{
    public Text TraderNameText;
    public Text PlayerCurrencyBalanceText;
    public Text TraderCurrencyBalanceText;
    public Text TradeCurrencyBalanceText;
    public Transform TradeItemListPanel;
    public GameObject TradeItemPrefab;

    private Trade trade;

    public void SetupTrade(Trade trade)
    {
        this.trade = trade;

        ClearInterface();
        BuildInterface();
    }

    public void DoTradingTestWithMockTraders()
    {
        Trader mockPlayer = new Trader
        {
            CurrencyBalance = 500,
            Name = "Player",
            SaleMarginMultiplier = 1f,
            Stock = new List<Inventory>
            {
                new Inventory("Steel Plate", 50, 10) { basePrice = 3f },
                new Inventory("Raw Iron", 100, 90) { basePrice = 0.2f },
            }
        };

        Trader mockTrader = new Trader
        {
            CurrencyBalance = 1500,
            Name = "Trader",
            SaleMarginMultiplier = 1.23f,
            Stock = new List<Inventory>
            {
                new Inventory("Steel Plate", 50, 40) { basePrice = 3f },
                new Inventory("Steel Plate", 50, 40) { basePrice = 3f },
                new Inventory("Oxygen Bottle", 10, 10) { basePrice = 50f },
            }
        };
        SetupTrade(new Trade(mockPlayer, mockTrader));
    }

    public void CancelTrade()
    {
        trade = null;
        ClearInterface();
        CloseDialog();
    }

    public void AcceptTrade()
    {
        if (trade.IsValid())
        {
            trade.Accept();
            trade = null;
            ClearInterface();
            CloseDialog();
        }
    }

    private void ClearInterface()
    {
        var childrens = TradeItemListPanel.Cast<Transform>().ToList();
        foreach (var child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildInterfaceHeader()
    {
        float tradeAmount = trade.TradeCurrencyBalanceForPlayer;
        PlayerCurrencyBalanceText.text = (trade.Player.CurrencyBalance + tradeAmount).ToString();
        TraderCurrencyBalanceText.text = (trade.Trader.CurrencyBalance - tradeAmount).ToString();
        TradeCurrencyBalanceText.text = tradeAmount.ToString();
    }

    private void BuildInterface()
    {
        TraderNameText.text = trade.Trader.Name;
        BuildInterfaceHeader();

        foreach (var tradeItem in trade.TradeItems)
        {
            var go = (GameObject)Instantiate(TradeItemPrefab);
            go.transform.SetParent(TradeItemListPanel);

            var tradeItemBehaviour = go.GetComponent<DialogBoxTradeItem>();
            tradeItemBehaviour.OnTradeAmountChangedEvent += item => BuildInterfaceHeader();
            tradeItemBehaviour.SetupTradeItem(tradeItem);
        }
    }
}