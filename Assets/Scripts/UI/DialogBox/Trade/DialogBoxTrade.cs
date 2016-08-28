﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
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

    public Action OnTradeCompleted;
    public Action OnTradeCancelled;

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
            Currency = new Currency
            {
                Balance = 1000f,
                Name = "Test Currency",
                ShortName = "TC"
            },
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
            Currency = new Currency
            {
                Balance = 1000f,
                Name = "Test Currency",
                ShortName = "TC"
            },
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

    public void CancelTrade()
    {
        trade = null;
        ClearInterface();
        CloseDialog();
        if (OnTradeCompleted != null)
        {
            OnTradeCancelled();
        }
    }

    public void AcceptTrade()
    {
        if (trade.IsValid())
        {
            trade = null;
            ClearInterface();
            CloseDialog();
            if (OnTradeCompleted != null)
            {
                OnTradeCompleted();
            }
        }
    }

    private void ClearInterface()
    {
        var childrens = TradeItemListPanel.Cast<Transform>().ToList();
        foreach (Transform child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildInterface()
    {
        TraderNameText.text = trade.Trader.Name;
        BuildInterfaceHeader();

        foreach (TradeItem tradeItem in trade.TradeItems)
        {
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/TradeItemPrefab"), TradeItemListPanel);

            var tradeItemBehaviour = go.GetComponent<DialogBoxTradeItem>();
            tradeItemBehaviour.OnTradeAmountChangedEvent += item => BuildInterfaceHeader();
            tradeItemBehaviour.SetupTradeItem(tradeItem);
        }
    }

    private void BuildInterfaceHeader()
    {
        float tradeAmount = trade.TradeCurrencyBalanceForPlayer;
        PlayerCurrencyBalanceText.text = string.Format("{0} {1}", Math.Round(trade.Player.Currency.Balance + tradeAmount, 2), trade.Player.Currency.ShortName);
        TraderCurrencyBalanceText.text = string.Format("{0} {1}", Math.Round(trade.Trader.Currency.Balance - tradeAmount, 2), trade.Trader.Currency.ShortName);
        TradeCurrencyBalanceText.text = tradeAmount.ToString();
    }
}