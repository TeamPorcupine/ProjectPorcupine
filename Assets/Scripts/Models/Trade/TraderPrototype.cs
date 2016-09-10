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
using System.Xml;
using UnityEngine;

public class TraderPrototype
{
    private float rarity;

    public string ObjectType { get; set; }

    public List<string> PotentialNames { get; set; }

    public float MinCurrencyBalance { get; set; }

    public float MaxCurrencyBalance { get; set; }

    public string CurrencyName { get; set; }

    public float MinSaleMarginMultiplier { get; set; }

    public float MaxSaleMarginMultiplier { get; set; }

    public List<TraderPotentialInventory> PotentialStock { get; set; }

    /// <summary>
    /// Value from 0 to 1, higher value represent higher availability of the trade resource.
    /// </summary>
    public float Rarity
    {
        get
        {
            return rarity;
        }

        set
        {
            rarity = Mathf.Clamp(value, 0f, 1f);
        }
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        ObjectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "potentialNames":
                    PotentialNames = new List<string>();
                    XmlReader namesReader = reader.ReadSubtree();

                    while (namesReader.Read())
                    {
                        if (namesReader.Name == "name")
                        {
                            PotentialNames.Add(namesReader.ReadElementContentAsString());
                        }
                    }

                    break;
                case "currencyName":
                    reader.Read();
                    CurrencyName = reader.ReadContentAsString();
                    break;
                case "minCurrencyBalance":
                    reader.Read();
                    MinCurrencyBalance = reader.ReadContentAsInt();
                    break;
                case "maxCurrencyBalance":
                    reader.Read();
                    MaxCurrencyBalance = reader.ReadContentAsInt();
                    break;
                case "minSaleMarginMultiplier":
                    reader.Read();
                    MinSaleMarginMultiplier = reader.ReadContentAsFloat();
                    break;
                case "maxSaleMarginMultiplier":
                    reader.Read();
                    MaxSaleMarginMultiplier = reader.ReadContentAsFloat();
                    break;
                
                case "potentialStock":
                    PotentialStock = new List<TraderPotentialInventory>();
                    XmlReader invs_reader = reader.ReadSubtree();

                    while (invs_reader.Read())
                    {
                        if (invs_reader.Name == "Inventory")
                        {
                            // Found an inventory requirement, so add it to the list!
                            PotentialStock.Add(new TraderPotentialInventory
                            {
                                ObjectType = invs_reader.GetAttribute("objectType"),
                                ObjectCategory = invs_reader.GetAttribute("objectCategory"),
                                MinQuantity = int.Parse(invs_reader.GetAttribute("minQuantity")),
                                MaxQuantity = int.Parse(invs_reader.GetAttribute("maxQuantity")),
                                Rarity = float.Parse(invs_reader.GetAttribute("rarity"))
                            });
                        }
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Create a random Trader out of this TraderPrototype.
    /// </summary>
    public Trader CreateTrader()
    {
        Trader t = new Trader
        {
            Currency = new Currency
            {
                Name = CurrencyName,
                Balance = Random.Range(MinCurrencyBalance, MaxCurrencyBalance),   
                ShortName = World.Current.Wallet.Currencies[CurrencyName].ShortName
            },
            Name = PotentialNames[Random.Range(0, PotentialNames.Count - 1)],
            SaleMarginMultiplier = Random.Range(MinSaleMarginMultiplier, MaxSaleMarginMultiplier),
            Stock = new List<Inventory>()
        };

        foreach (TraderPotentialInventory potentialStock in PotentialStock)
        {
            bool itemIsInStock = Random.Range(0f, 1f) > potentialStock.Rarity;

            if (itemIsInStock)
            {
                if (!string.IsNullOrEmpty(potentialStock.ObjectType))
                {
                    Inventory inventory = new Inventory(
                        potentialStock.ObjectType,
                        Random.Range(potentialStock.MinQuantity, potentialStock.MaxQuantity));

                    t.Stock.Add(inventory);
                }
                else if (!string.IsNullOrEmpty(potentialStock.ObjectCategory))
                {
                    List<InventoryCommon> potentialObjects = GetInventoryCommonWithCategory(potentialStock.ObjectCategory);

                    foreach (InventoryCommon potentialObject in potentialObjects)
                    {
                        Inventory inventory = new Inventory(
                            potentialObject.type,
                            Random.Range(potentialStock.MinQuantity, potentialStock.MaxQuantity));

                        t.Stock.Add(inventory);
                    }
                }
            }
        }

        return t;
    }

    private List<InventoryCommon> GetInventoryCommonWithCategory(string category)
    {
        return PrototypeManager.Inventory.Values.Where(i => i.category == category).ToList();
    }
}
