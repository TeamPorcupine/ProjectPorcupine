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
    private Trade _trade;

    public Text TraderNameText;
    public Text PlayerCurrencyBalanceText;
    public Text TraderCurrencyBalanceText;
    public Text TradeCurrencyBalanceText;
    public Transform TradeItemListPanel;

    public GameObject TradeItemPrefab;

    public void SetupTrade(Trade trade)
    {
        _trade = trade;

        ClearInterface();
        BuildInterface();
    }

    private void ClearInterface()
    {
        var childrens = TradeItemListPanel.Cast<Transform>().ToList();
        foreach (var child in childrens)
        {
            Destroy(child.gameObject);
        }
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
                new Inventory("Steel Plate",50,10){ basePrice = 3f},
                new Inventory("Raw Iron",100,90){ basePrice = 0.2f},
            }
        };

        Trader mockTrader = new Trader
        {
            CurrencyBalance = 1500,
            Name = "Trader",
            SaleMarginMultiplier = 1.23f,
            Stock = new List<Inventory>
            {
                new Inventory("Steel Plate",50,40){ basePrice = 3f},
                new Inventory("Steel Plate",50,40){ basePrice = 3f},
                new Inventory("Oxygen Bottle",10,10){ basePrice = 50f},
            }
        };
        SetupTrade(new Trade(mockPlayer,mockTrader));
    }

    private void BuildInterface()
    {
        TraderNameText.text = _trade.Trader.Name;
        BuildInterfaceHeader();

        foreach (var tradeItem in _trade.TradeItems)
        {
            var go = (GameObject)Instantiate(TradeItemPrefab);
            go.transform.SetParent(TradeItemListPanel);

            var tradeItemBehaviour = go.GetComponent<DialogBoxTradeItem>();
            tradeItemBehaviour.OnTradeAmountChangedEvent += item => BuildInterfaceHeader();
            tradeItemBehaviour.SetupTradeItem(tradeItem);
        }
    }

    private void BuildInterfaceHeader()
    {
        float tradeAmount = _trade.TradeCurrencyBalanceForPlayer;
        PlayerCurrencyBalanceText.text = (_trade.Player.CurrencyBalance + tradeAmount).ToString();
        TraderCurrencyBalanceText.text = (_trade.Trader.CurrencyBalance - tradeAmount).ToString();
        TradeCurrencyBalanceText.text = tradeAmount.ToString();
    }

    public void CancelTrade()
    {
        _trade = null;
        ClearInterface();
        CloseDialog();
    }

    public void AcceptTrade()
    {
        if (_trade.IsValid())
        {
            _trade.Accept();
            _trade = null;
            ClearInterface();
            CloseDialog();
        }
    }
}