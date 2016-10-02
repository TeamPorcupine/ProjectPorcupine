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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea).
[MoonSharpUserData]
[System.Diagnostics.DebuggerDisplay("Inventory {ObjectType} {StackSize}/{MaxStackSize}")]
public class Inventory : IXmlSerializable, ISelectable, IContextActionProvider
{
    private int stackSize = 1;

    public Inventory()
    {
    }

    public Inventory(string type, int stackSize, int maxStackSize = 50)
    {
        Type = type;
        ImportPrototypeSettings(maxStackSize, 1f, "inv_cat_none");
        StackSize = stackSize;
    }

    private Inventory(Inventory other)
    {
        Type = other.Type;
        MaxStackSize = other.MaxStackSize;
        BasePrice = other.BasePrice;
        Category = other.Category;
        StackSize = other.StackSize;
        Locked = other.Locked;
    }

    public event Action<Inventory> StackSizeChanged;

    public string Type { get; private set; }

    public int MaxStackSize { get; set; }

    public float BasePrice { get; set; }

    public string Category { get; private set; }

    public Tile Tile { get; set; }

    // Should this inventory be allowed to be picked up for completing a job?
    public bool Locked { get; set; }

    public int StackSize
    {
        get
        {
            return stackSize;
        }

        set
        {
            if (stackSize == value)
            {
                return;
            }

            stackSize = value;
            InvokeStackSizeChanged(this);
        }
    }

    public bool IsSelected { get; set; }

    public Inventory Clone()
    {
        return new Inventory(this);
    }

    public string GetName()
    {
        return Type;
    }

    public string GetDescription()
    {
        return string.Format("StackSize: {0}\nCategory: {1}\nBasePrice: {2:N2}", StackSize, Category, BasePrice);
    }
    
    public string GetJobDescription()
    {
        return string.Empty;
    }

    public bool CanAccept(Inventory inv)
    {
        return inv.Type == Type && inv.StackSize + stackSize <= MaxStackSize;
    }

    public IEnumerable<string> GetAdditionalInfo()
    {
        // Does inventory have hitpoints? How does it get destroyed? Maybe it's just a percentage chance based on damage.
        yield return string.Format("StackSize: {0}", stackSize);
        yield return string.Format("Category: {0}", BasePrice);
        yield return string.Format("BasePrice: {0:N2}", BasePrice);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // If we reach this point through inventories we definitely have a tile
        // If we don't have a tile, that means we're writing a character's inventory
        if (Tile != null)
        {
            writer.WriteAttributeString("X", Tile.X.ToString());
            writer.WriteAttributeString("Y", Tile.Y.ToString());
            writer.WriteAttributeString("Z", Tile.Z.ToString());
        }

        writer.WriteAttributeString("type", Type);
        writer.WriteAttributeString("maxStackSize", MaxStackSize.ToString());
        writer.WriteAttributeString("stackSize", StackSize.ToString());
        writer.WriteAttributeString("basePrice", BasePrice.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("category", Category);
        writer.WriteAttributeString("locked", Locked.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
    }

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Sample Item Context action",
            RequireCharacterSelected = true,
            Action = (cm, c) => Debug.ULogChannel("Inventory", "Sample menu action")
        };
    }

    public bool CanBePickedUp(bool canTakeFromStockpile)
    {
        // You can't pick up stuff that isn't on a tile or if it's locked
        if (Tile == null || Locked)
        {
            return false;
        }

        return Tile.Furniture == null || canTakeFromStockpile == true || Tile.Furniture.HasTypeTag("Storage") == false;
    }

    public override string ToString()
    {
        return string.Format("{0} [{1}/{2}]", Type, StackSize, MaxStackSize);
    }

    private void ImportPrototypeSettings(int defaulMaxStackSize, float defaultBasePrice, string defaultCategory)
    {
        if (PrototypeManager.Inventory.Has(Type))
        {
            InventoryCommon prototype = PrototypeManager.Inventory.Get(Type);
            MaxStackSize = prototype.maxStackSize;
            BasePrice = prototype.basePrice;
            Category = prototype.category;
        }
        else
        {
            MaxStackSize = defaulMaxStackSize;
            BasePrice = defaultBasePrice;
            Category = defaultCategory;
        }
    }

    private void InvokeStackSizeChanged(Inventory inventory)
    {
        Action<Inventory> handler = StackSizeChanged;
        if (handler != null)
        {
            handler(inventory);
        }
    }
}
