#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;

public class InventoryCommon
{
    public string objectType;
    public int maxStackSize;
    public float basePrice = 1f;
    public string category;

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        objectType = reader_parent.GetAttribute("objectType");
        maxStackSize = int.Parse(reader_parent.GetAttribute("maxStackSize") ?? "50");
        basePrice = float.Parse(reader_parent.GetAttribute("basePrice") ?? "1");
        category = reader_parent.GetAttribute("category");
    }
}
