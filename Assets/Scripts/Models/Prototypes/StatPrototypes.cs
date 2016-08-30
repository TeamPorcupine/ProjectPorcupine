using System;
using System.Xml;

public class StatPrototypes : XmlPrototypes<Stat>
{
    public StatPrototypes() : base("Stats.xml", "Stats", "Stat")
    {
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    protected override void LoadPrototype(XmlTextReader reader)
    {
        Stat stat = new Stat();
        try
        {
            stat.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, stat.statType);
        }

        SetPrototype(stat.statType, stat);
    }
}
