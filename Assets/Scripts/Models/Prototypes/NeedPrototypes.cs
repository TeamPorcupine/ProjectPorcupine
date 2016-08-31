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

public class NeedPrototypes : XmlPrototypes<Need>
{
    public NeedPrototypes() : base("Need.xml", "Needs", "Need")
    {
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    protected override void LoadPrototype(XmlTextReader reader)
    {
        Need need = new Need();
        try
        {
            need.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, need.needType);
        }

        SetPrototype(need.needType, need);
    }
}
