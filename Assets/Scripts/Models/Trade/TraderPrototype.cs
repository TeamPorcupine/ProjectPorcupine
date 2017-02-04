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
using Animation;
using UnityEngine;

public class TraderPrototype : IPrototypable
{
    private float rarity;

    public string Type { get; set; }

    public List<string> PotentialNames { get; set; }

    public float MinCurrencyBalance { get; set; }

    public float MaxCurrencyBalance { get; set; }

    public string CurrencyName { get; set; }

    public float MinSaleMarginMultiplier { get; set; }

    public float MaxSaleMarginMultiplier { get; set; }

    public List<TraderPotentialInventory> PotentialStock { get; set; }

    public SpritenameAnimation AnimationIdle { get; set; }

    public SpritenameAnimation AnimationFlying { get; set; }

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
        Type = reader_parent.GetAttribute("type");

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
                                Type = invs_reader.GetAttribute("type"),
                                Category = invs_reader.GetAttribute("category"),
                                MinQuantity = int.Parse(invs_reader.GetAttribute("minQuantity")),
                                MaxQuantity = int.Parse(invs_reader.GetAttribute("maxQuantity")),
                                Rarity = float.Parse(invs_reader.GetAttribute("rarity"))
                            });
                        }
                    }

                    break;
                case "Animations":
                    XmlReader animationReader = reader.ReadSubtree();
                    ReadAnimationXml(animationReader);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Create a random Trader out of this TraderPrototype.
    /// </summary>
    public Trader CreateTrader()
    {
        Currency curency = PrototypeManager.Currency.Get(CurrencyName).Clone();
        curency.Balance = Random.Range(MinCurrencyBalance, MaxCurrencyBalance);

        Trader t = new Trader
        {
            Currency = curency,
            Name = PotentialNames[Random.Range(0, PotentialNames.Count - 1)],
            SaleMarginMultiplier = Random.Range(MinSaleMarginMultiplier, MaxSaleMarginMultiplier),
            Stock = new List<Inventory>()
        };

        foreach (TraderPotentialInventory potentialStock in PotentialStock)
        {
            bool itemIsInStock = Random.Range(0f, 1f) > potentialStock.Rarity;

            if (itemIsInStock)
            {
                if (!string.IsNullOrEmpty(potentialStock.Type))
                {
                    Inventory inventory = new Inventory(
                        potentialStock.Type,
                        Random.Range(potentialStock.MinQuantity, potentialStock.MaxQuantity));

                    t.Stock.Add(inventory);
                }
                else if (!string.IsNullOrEmpty(potentialStock.Category))
                {
                    List<Inventory> potentialObjects = GetInventoryCommonWithCategory(potentialStock.Category);

                    foreach (Inventory potentialObject in potentialObjects)
                    {
                        Inventory inventory = new Inventory(
                            potentialObject.Type,
                            Random.Range(potentialStock.MinQuantity, potentialStock.MaxQuantity));

                        t.Stock.Add(inventory);
                    }
                }
            }
        }

        return t;
    }

    /// <summary>
    /// Reads and creates Animations from the prototype xml. 
    /// For now, this requires an idle animation and a flying animation state.
    /// </summary>
    private void ReadAnimationXml(XmlReader animationReader)
    {
        while (animationReader.Read())
        {
            if (animationReader.Name == "Animation")
            {
                string state = animationReader.GetAttribute("state");
                float fps = 1;
                float.TryParse(animationReader.GetAttribute("fps"), out fps);
                bool looping = true;
                bool.TryParse(animationReader.GetAttribute("looping"), out looping);
                bool valueBased = false;

                // read frames
                XmlReader frameReader = animationReader.ReadSubtree();
                List<string> framesSpriteNames = new List<string>();
                while (frameReader.Read())
                {
                    if (frameReader.Name == "Frame")
                    {
                        framesSpriteNames.Add(frameReader.GetAttribute("name"));
                    }
                }

                switch (state)
                {
                    case "idle":
                        AnimationIdle = new SpritenameAnimation(state, framesSpriteNames.ToArray(), fps, looping, valueBased);
                        break;
                    case "flying":
                        AnimationFlying = new SpritenameAnimation(state, framesSpriteNames.ToArray(), fps, looping, valueBased);
                        break;
                }
            }
        }
    }

    private List<Inventory> GetInventoryCommonWithCategory(string category)
    {
        return PrototypeManager.Inventory.Values.Where(i => i.Category == category).ToList();
    }
}
