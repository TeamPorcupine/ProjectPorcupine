#region License
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
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogBoxTrade : DialogBox
{
    public Text TraderNameText;
    public Text PlayerCurrencyBalanceText;
    public Text TraderCurrencyBalanceText;
    public Text TradeCurrencyBalanceText;
    public Text title;
    public Transform TradeItemListPanel;
    public Transform TradeHeaderPanel;

    public GameObject TradeItemPrefab;

    public Action TradeCompleted;
    public Action TradeCancelled;

    public Button AcceptButton;

    private Trade trade;
    private List<Transform> requests;

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
                new Inventory("Steel Plate", 50, 10) { BasePrice = 3f },
                new Inventory("Raw Iron", 100, 90) { BasePrice = 0.2f },
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
                new Inventory("Steel Plate", 50, 40) { BasePrice = 3f },
                new Inventory("Steel Plate", 50, 40) { BasePrice = 3f },
                new Inventory("Oxygen Bottle", 10, 10) { BasePrice = 50f },
            },
            possibleStock = new List<TraderPotentialInventory>
            {
                new TraderPotentialInventory() { ObjectType = "Steel Plate" }
            }
        };
        SetupTrade(new Trade(mockPlayer, mockTrader));
    }

    public void CancelTrade()
    {
        trade = null;
        ClearInterface();
        CloseDialog();
        if (TradeCompleted != null)
        {
            TradeCancelled();
        }
    }

    public void AcceptTrade()
    {
        if (trade.IsValid())
        {
            if (TradeCompleted != null)
            {
                TradeCompleted();
            }
        }
        else
        {
            return;
        }

        StartRequests();
    }

    public void FinalizeRequest()
    {
        Dictionary<TraderPotentialInventory, RequestLevel> requestAndLevel = new Dictionary<TraderPotentialInventory, RequestLevel>();
        foreach (Transform requestTransform in requests)
        {
            DialogBoxTradeRequest request = requestTransform.GetComponent<DialogBoxTradeRequest>();
            if (request.requestLevel == RequestLevel.None)
            {
                continue;
            }

            requestAndLevel.Add(request.request, request.requestLevel);
        }

        trade.Trader.RequestItems(requestAndLevel);
        trade = null;
        ClearInterface();
        CloseDialog();
    }

    private void StartRequests()
    {
        TradeHeaderPanel.gameObject.SetActive(false);
        title.text = "Requests";
        requests = new List<Transform>();
        ClearInterface();
        BuildRequestInterface();
        AcceptButton.onClick.RemoveAllListeners();
        AcceptButton.onClick.AddListener(new UnityAction(FinalizeRequest));
    }

    private void ClearInterface()
    {
        List<Transform> childrens = TradeItemListPanel.Cast<Transform>().ToList();
        foreach (Transform child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildRequestInterface()
    {
        foreach (TraderPotentialInventory possible in trade.Trader.possibleStock)
        {
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/TradeItemRequestPrefab"), TradeItemListPanel);
            DialogBoxTradeRequest tradeItemBehaviour = go.GetComponent<DialogBoxTradeRequest>();
            tradeItemBehaviour.itemName.text = possible.ObjectType;
            tradeItemBehaviour.request = possible;
            requests.Add(tradeItemBehaviour.transform);
        }
    }

    private void BuildInterface()
    {
        TraderNameText.text = trade.Trader.Name;
        BuildInterfaceHeader();

        foreach (TradeItem tradeItem in trade.TradeItems)
        {
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/TradeItemPrefab"), TradeItemListPanel);

            DialogBoxTradeItem tradeItemBehaviour = go.GetComponent<DialogBoxTradeItem>();
            tradeItemBehaviour.OnTradeAmountChangedEvent += item => BuildInterfaceHeader();
            tradeItemBehaviour.SetupTradeItem(tradeItem);
        }
    }

    private void BuildInterfaceHeader()
    {
        float tradeAmount = trade.TradeCurrencyBalanceForPlayer;
        PlayerCurrencyBalanceText.text = string.Format(
            "{0:N2} {1}", 
            trade.Player.Currency.Balance + trade.TradeCurrencyBalanceForPlayer, 
            trade.Player.Currency.ShortName);
        TraderCurrencyBalanceText.text = string.Format(
            "{0:N2} {1}", 
            trade.Trader.Currency.Balance - trade.TradeCurrencyBalanceForPlayer, 
            trade.Trader.Currency.ShortName);
        TradeCurrencyBalanceText.text = tradeAmount.ToString("N2");

        AcceptButton.interactable = trade.IsValid();
    }
}