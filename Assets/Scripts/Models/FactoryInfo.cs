using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class FactoryInfo
{
    [Serializable]
    public class Item
    {
        [XmlAttribute("objectType")]
        public string ObjectType { get; set; }
        [XmlAttribute("amount")]
        public int Amount { get; set; }
        //[XmlAttribute("relativeSlotPos")]
        //public IntVector2 RelativeSlotPos { get; set; }
        [XmlAttribute("slotPosX")]
        public int SlotPosX { get; set; }
        [XmlAttribute("slotPosY")]
        public int SlotPosY { get; set; }
    }

    [Serializable]
    public class ProductionChain
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("processingTime")]
        public float ProcessingTime { get; set; }

        public List<Item> Input { get; set; }
        public List<Item> Output { get; set; }
    }

    [Serializable]
    public class IntVector2
    {
        [XmlAttribute("x")]
        public int X { get; set; }
        [XmlAttribute("y")]
        public int Y { get; set; }

        public IntVector2()
        { }

        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2 AsVector2()
        {
            return new Vector2(X, Y);
        }
    }

    public List<ProductionChain> PossibleProductions { get; set; }
    // TODO: future - possibility to use InputSlot as a stockpile, need some flag for that here (useful for smelters)

    //[XmlElement(ElementName = "InputSlotPosition")]
    //public List<IntVector2> InputSlots { get; set; }
    //[XmlElement(ElementName = "OutputSlotPosition")]
    //public List<IntVector2> OutputSlots { get; set; }
}

public class TileObjectTypeAmount
{
    public Tile Tile { get; set; }
    public bool IsEmpty { get; set; }
    public string ObjectType { get; set; }
    public int Amount { get; set; }
}

public class FactoryContextMenu
{
    public string Text { get; set; }
    public Action Function { get; set; }
}



