using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities.GalacticTradeNode;
using Assets.Scripts.Utilities.GalacticTradeNode.Models;
using UnityEngine;

public class DialogBoxGalacticMarket : global::DialogBox
{
    public GameObject galacticOrderPrefab;
    public GameObject orderPanel;

    public override void ShowDialog()
    {
        base.ShowDialog();
        RefreshOrders();
    }

    public void RefreshOrders()
    {
        var remoteOrders = GetGalacticMarketOrders();

        foreach (var child in orderPanel.transform.Cast<Transform>().ToList())
            Destroy(child.gameObject);

        foreach (var remoteOrder in remoteOrders)
        {
            var go = Instantiate(galacticOrderPrefab);
            go.transform.SetParent(orderPanel.transform);

            var gmo = go.GetComponent<GalacticMarketOrder>();
            gmo.Order = remoteOrder;
        }
    }

    private List<Order> GetGalacticMarketOrders()
    {
        var context = new GalacticMarketContext();
        return context.GetRunningOrders();
    }
}