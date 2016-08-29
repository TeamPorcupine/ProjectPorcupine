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

public class FurniturePrototypes : XmlPrototypes<Furniture>
{
    public FurniturePrototypes() : base("Furniture.xml", "Furnitures", "Furniture")
    {
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    protected override void LoadPrototype(XmlTextReader reader)
    {
        Furniture furn = new Furniture();
        try
        {
            furn.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, furn.ObjectType);
        }

        SetPrototype(furn.ObjectType, furn);
    }
}
