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

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        ////Debug.Log("ReadXmlPrototype");

        objectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "maxStackSize":
                    reader.Read();
                    maxStackSize = reader.ReadContentAsInt();
                    break;
            }
        }
    }
}

