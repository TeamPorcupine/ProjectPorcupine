#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea).
[MoonSharpUserData]
public class Inventory : IXmlSerializable, ISelectable, IContextActionProvider
{
    public string objectType = "Steel Plate";
    public int maxStackSize = 50;
    public float basePrice = 1f;
    public Tile tile;
    public Character character;

    // Should this inventory be allowed to be picked up for completing a job?
    public bool locked = false;

    protected int stackSize = 1;

    public Inventory()
    {
    }

    public Inventory(string objectType, int maxStackSize, int stackSize)
    {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.StackSize = stackSize;
    }

    public Inventory(string objectType, int stackSize)
    {
        this.objectType = objectType;

        if (PrototypeManager.Inventory.HasPrototype(objectType))
        {
            this.maxStackSize = PrototypeManager.Inventory.GetPrototype(objectType).maxStackSize;
        }
        else
        {
            this.maxStackSize = 50;
        }

        this.StackSize = stackSize;
    }

    protected Inventory(Inventory other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        StackSize = other.StackSize;
        locked = other.locked;
    }

    // The function we callback any time our tile's data changes.
    public event Action<Inventory> OnInventoryChanged;

    public int StackSize
    {
        get 
        {
            return stackSize; 
        }

        set
        {
            if (stackSize != value)
            {
                stackSize = value;
                if (OnInventoryChanged != null)
                {
                    OnInventoryChanged(this);
                }
            }
        }
    }

    public bool IsSelected
    {
        get;
        set;
    }

    public static Inventory New(string objectType, int maxStackSize, int stackSize)
    {
        return new Inventory(objectType, maxStackSize, stackSize);
    }

    public static Inventory New(string objectType, int stackSize)
    {
        return new Inventory(objectType, stackSize);
    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }
    
    #region ISelectableInterface implementation

    public string GetName()
    {
        return this.objectType;
    }

    public string GetDescription()
    {
        return "A stack of inventory.";
    }

    public string GetHitPointString()
    {
        return string.Empty;  // Does inventory have hitpoints? How does it get destroyed? Maybe it's just a percentage chance based on damage.
    }

    public string GetJobDescription()
    {
        return string.Empty;
    }
    #endregion

    #region IXmlSerializable implementation

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // If we reach this point through inventories we definitely have a tile
        // If we don't have a tile, that means we're writing a character's inventory
        if (tile != null)
        {
            writer.WriteAttributeString("X", tile.X.ToString());
            writer.WriteAttributeString("Y", tile.Y.ToString());
        }

        writer.WriteAttributeString("objectType", objectType);
        writer.WriteAttributeString("maxStackSize", maxStackSize.ToString());
        writer.WriteAttributeString("stackSize", StackSize.ToString());
        writer.WriteAttributeString("basePrice", basePrice.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
    }

    #endregion

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Sample Item Context action",
            RequireCharacterSelected = true,
            Action = (cm, c) => Debug.ULogChannel("Inventory", "Sample menu action")
        };
    }
}
