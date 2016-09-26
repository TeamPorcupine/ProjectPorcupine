#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Xml;

public class Currency
{
    public string Name;
    public string ShortName;
    public float Balance;
    
    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Name", Name.ToString());
        writer.WriteAttributeString("ShortName", ShortName.ToString());
        writer.WriteAttributeString("Balance", Balance.ToString());
    }
}