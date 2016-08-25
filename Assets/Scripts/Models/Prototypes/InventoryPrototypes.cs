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


public class InventoryPrototypes : Prototypes<InventoryCommon>
{

    public InventoryPrototypes()
    {
        prototypes = new Dictionary<string, InventoryCommon>();
        fileName = "Inventory.xml";
        listTag = "Inventories";
        elementTag = "Inventory";

        LoadPrototypesFromFile();
    }



    protected override void LoadPrototype(XmlTextReader reader)
    {
        InventoryCommon inv = new InventoryCommon();
        try
        {
            inv.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, inv.objectType);
        }

        SetPrototype(inv.objectType, inv);
    }
}
