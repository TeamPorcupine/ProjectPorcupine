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

public class InventoryPrototypes : XmlPrototypes<Inventory>
{
    public InventoryPrototypes() : base("Inventory.xml", "Inventories", "Inventory")
    {
    }

    protected override void LoadPrototype(XmlTextReader reader)
    {
        Inventory inventory = new Inventory();
        try
        {
            inventory.ReadXmlFromPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, inventory.Type);
        }

        Set(inventory.Type, inventory);
    }
}
