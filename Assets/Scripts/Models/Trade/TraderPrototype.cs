#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class TraderPrototype
{
    public string ObjectType;
    public List<string> PotentialNames;
    public float MinCurrencyBalance;
    public float MaxCurrencyBalance;
    public float MinSaleMarginMultiplier;
    public float MaxSaleMarginMultiplier;
    public List<TraderPotentialInventory> PotentialStock;

    [Range(0, 1)]
    public float Rarity;

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
                    XmlReader names_reader = reader.ReadSubtree();

                    while (names_reader.Read())
                    {
                        if (names_reader.Name == "name")
                        {
                            PotentialNames.Add(names_reader.Value);
                        }
                    }
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

    public Trader CreateTrader()
    {
        Trader t = new Trader
        {
            CurrencyBalance = Random.Range(MinCurrencyBalance, MaxCurrencyBalance),
            Name = PotentialNames[Random.Range(PotentialNames.Count, PotentialNames.Count - 1)],
            SaleMarginMultiplier = Random.Range(MinSaleMarginMultiplier, MaxSaleMarginMultiplier)
        };

        foreach (var potentialStock in PotentialStock)
        {
            var itemIsInStock = Random.Range(0f, 1f) > potentialStock.Rarity;

            if (itemIsInStock)
            {
                t.Stock.Add(new Inventory
                {
                    objectType = potentialStock.ObjectType,
                    stackSize = Random.Range(potentialStock.MinQuantity,potentialStock.MaxQuantity)
                });
            }
        }

        return t;
    }
}