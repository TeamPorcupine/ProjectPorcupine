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

public class Trader
{
    public string Name;
    public float CurrencyBalance;
    public float SaleMarginMultiplier;
    public List<Inventory> Stock;

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        Name = reader_parent.GetAttribute("name");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "currencyBalance":
                    reader.Read();
                    CurrencyBalance = reader.ReadContentAsInt();
                    break;
                case "saleMarginMultiplier":
                    reader.Read();
                    SaleMarginMultiplier = reader.ReadContentAsFloat();
                    break;
                case "stock":
                    Stock=new List<Inventory>();
                    XmlReader invs_reader = reader.ReadSubtree();

                    while (invs_reader.Read())
                    {
                        if (invs_reader.Name == "Inventory")
                        {
                            // Found an inventory requirement, so add it to the list!
                            Stock.Add(new Inventory(
                                    invs_reader.GetAttribute("objectType"),
                                    int.Parse(invs_reader.GetAttribute("amount")),
                                    0));
                        }
                    }
                    break;
            }
        }
    }
}