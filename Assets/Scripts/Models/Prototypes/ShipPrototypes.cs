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

public class ShipPrototypes : XmlPrototypes<Ship>
{
    public ShipPrototypes() : base("Ships.xml", "Ships", "Ship")
    {
    }

    protected override void LoadPrototype(XmlTextReader reader)
    {
        Ship ship = new Ship();
        try
        {
            ship.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, ship.ShipType);
        }

        SetPrototype(ship.ShipType, ship);
    }
}