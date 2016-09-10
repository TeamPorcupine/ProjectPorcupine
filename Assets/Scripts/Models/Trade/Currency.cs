#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Xml;

public class Currency
{
    private float balance;

    public Action<Currency> BalanceChanged { get; set; }

    public string Name { get; set; }

    public string ShortName { get; set; }

    public float Balance
    {
        get
        {
            return balance;
        }

        set
        {
            if (balance == value)
            {
                return;
            }

            balance = value;

            if (BalanceChanged != null)
            {
                BalanceChanged(this);
            }
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Name", Name.ToString());
        writer.WriteAttributeString("ShortName", ShortName.ToString());
        writer.WriteAttributeString("Balance", Balance.ToString());
    }
}