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

public class Wallet
{
    public Dictionary<string, Currency> Currencies;
    
    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        XmlReader reader = reader_parent.ReadSubtree();
        Currencies = new Dictionary<string, Currency>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Currency":
                    Currency c = new Currency
                    {
                        Name = reader.GetAttribute("Name"),
                        ShortName = reader.GetAttribute("ShortName"),
                        Balance = float.Parse(reader.GetAttribute("StartingBalance"))
                    };
                    Currencies.Add(c.Name, c);
                    break;
            }
        }
    }
}