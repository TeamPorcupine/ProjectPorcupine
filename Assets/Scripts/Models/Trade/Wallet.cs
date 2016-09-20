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
    private Dictionary<string, Currency> currencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="Wallet"/> class.
    /// </summary>
    public Wallet()
    {
        currencies = new Dictionary<string, Currency>(); 
    }

    /// <summary>
    /// Gets the <see cref="Currency"/> with the specified name.
    /// </summary>
    /// <param name="name">The currency name.</param>
    public Currency this[string name]
    {
        get
        {
            if (currencies.ContainsKey(name))
            {
                return currencies[name];
            }

            return null;
        }
    } 

    /// <summary>
    /// Adds a currency with the given name and balance.
    /// </summary>
    /// <param name="name">The currency name.</param>
    /// <param name="balance">The starting balance.</param>
    public void AddCurrency(string name, float balance)
    {
        if (PrototypeManager.Currency.Has(name))
        {
            Currency currency = PrototypeManager.Currency.Get(name).Clone();
            currency.Balance = balance;
            currencies[currency.Name] = currency;
        }
    }

    /// <summary>
    /// Writes the Wallet to the Xml.
    /// </summary>
    /// <param name="writer">The Xml writer.</param>
    public void WriteXml(XmlWriter writer)
    {
        foreach (Currency currency in currencies.Values)
        {
            writer.WriteStartElement("Currency");
            currency.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
