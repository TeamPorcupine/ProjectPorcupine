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


public class NeedPrototypes : Prototypes<Need>
{

    public NeedPrototypes()
    {
        prototypes = new Dictionary<string, Need>();
        fileName = "Need.xml";
        listTag = "Needs";
        elementTag = "Need";

        LoadPrototypesFromFile();
    }



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
