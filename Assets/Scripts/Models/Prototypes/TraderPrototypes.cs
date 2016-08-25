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
using System.Xml;


public class TraderPrototypes : Prototypes<TraderPrototype>
{

    public TraderPrototypes()
    {
        prototypes = new Dictionary<string, TraderPrototype>();
        fileName = "Trader.xml";
        listTag = "Traders";
        elementTag = "Trader";

        LoadPrototypesFromFile();
    }



    protected override void LoadPrototype(XmlTextReader reader)
    {
        TraderPrototype trader = new TraderPrototype();
        try
        {
            trader.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, trader.ObjectType);
        }

        SetPrototype(trader.ObjectType, trader);
    }
}
