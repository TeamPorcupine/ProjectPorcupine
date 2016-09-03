using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class Gas
{
    public bool hasValue;
    public string name;

    public Gas(string dataName)
    {
        this.name = dataName;
    }

    public static Dictionary<string, Gas> ReadXml(XmlReader reader)
    {
        Dictionary<string, Gas> gases = new Dictionary<string, Gas>();
        if (reader.ReadToDescendant("Gas"))
        {
            do
            {
                Gas gas = new Gas(reader.GetAttribute("name"));
                string hasValueString = reader.GetAttribute("hasValue");
                if (hasValueString != null)
                {
                    gas.hasValue = reader.GetAttribute("hasValue").Equals("true") ? true : false;
                }
                else
                {
                    gas.hasValue = false;
                }
                gases.Add(gas.name, gas);
            }
            while (reader.ReadToNextSibling("Gas"));
        }
        return gases;
    }

    public override string ToString()
    {
        return string.Format("{0}: Must have value? {1}", this.name, this.hasValue);
    }
}