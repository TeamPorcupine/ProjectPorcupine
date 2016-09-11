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

public class StatPrototypes : XmlPrototypes<Stat>
{
    public StatPrototypes() : base("Stats.xml", "Stats", "Stat")
    {
    }

    protected override void LoadPrototype(XmlTextReader reader)
    {
        Stat stat = new Stat();
        try
        {
            stat.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, stat.Type);
        }

        Set(stat.Type, stat);
    }
}
