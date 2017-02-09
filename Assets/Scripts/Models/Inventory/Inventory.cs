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
using System.Linq;
using System.Xml;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea).
[MoonSharpUserData]
[System.Diagnostics.DebuggerDisplay("Inventory {ObjectType} {StackSize}/{MaxStackSize}")]
public class Inventory : ISelectable, IContextActionProvider, IPrototypable
{
    private const float ClaimDuration = 120; // in Seconds

    private int stackSize = 1;
    private List<InventoryClaim> claims;

    public Inventory()
    {
        claims = new List<InventoryClaim>();
    }

    public Inventory(string type, int stackSize, int maxStackSize = 50)
    {
        Type = type;
        ImportPrototypeSettings(maxStackSize, 1f, "inv_cat_none");
        StackSize = stackSize;
        claims = new List<InventoryClaim>();
    }

    private Inventory(Inventory other)
    {
        Type = other.Type;
        MaxStackSize = other.MaxStackSize;
        BasePrice = other.BasePrice;
        Category = other.Category;
        StackSize = other.StackSize;
        Locked = other.Locked;
        LocalizationName = other.LocalizationName;
        LocalizationDescription = other.LocalizationDescription;
        claims = new List<InventoryClaim>();
    }

    private Inventory(string type, int maxStackSize, float basePrice, string category, string localizationName, string localizationDesc)
    {
        Type = type;
        MaxStackSize = maxStackSize;
        BasePrice = basePrice;
        Category = category;
        LocalizationName = localizationName;
        LocalizationDescription = localizationDesc;
    }

    public event Action<Inventory> StackSizeChanged;

    public string Type { get; private set; }

    public int MaxStackSize { get; set; }

    public float BasePrice { get; set; }

    public string Category { get; private set; }

    public string LocalizationName { get; private set; }

    public string LocalizationDescription { get; private set; }

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

    public int AvailableInventory
    {
        get
        {
            float requestTime = TimeManager.Instance.GameTime;
            return this.stackSize - claims.Where(claim => (requestTime - claim.time) < ClaimDuration).Sum(claim => claim.amount);
        }
    }

    public bool IsSelected { get; set; }

    /// <summary>
    /// Creates an Inventory to be used as a prototype. Still needs to be added to the PrototypeMap.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="type">Prototype's Type.</param>
    /// <param name="maxStackSize">Prototype's Max stack size.</param>
    /// <param name="basePrice">Prototype's Base price.</param>
    /// <param name="category">Prototype's Category.</param>
    public static Inventory CreatePrototype(string type, int maxStackSize, float basePrice, string category, string localizationName, string localizationDesc)
    {
        return new Inventory(type, maxStackSize, basePrice, category, localizationName, localizationDesc);
    }

    public Inventory Clone()
    {
        return new Inventory(this);
    }

    public void Claim(Character character, int amount)
    {
        float requestTime = TimeManager.Instance.GameTime;
        List<InventoryClaim> validClaims = claims.Where(claim => (requestTime - claim.time) < ClaimDuration).ToList();
        int availableInventory = this.stackSize - validClaims.Sum(claim => claim.amount);
        if (availableInventory >= amount)
        {
            validClaims.Add(new InventoryClaim(requestTime, character, amount));
        }

        // Set claims to validClaims to keep claims from filling up with old claims
        claims = validClaims;
    }

    public void ReleaseClaim(Character character)
    {
        bool noneAvailable = AvailableInventory == 0;
        claims.RemoveAll(claim => claim.character == character);
        if (noneAvailable && AvailableInventory > 0)
        {
            World.Current.jobQueue.ReevaluateWaitingQueue(this);
        }
    }

    public bool CanClaim()
    {
        float requestTime = TimeManager.Instance.GameTime;
        List<InventoryClaim> validClaims = claims.Where(claim => (requestTime - claim.time) < ClaimDuration).ToList();
        int availableInventory = this.stackSize - validClaims.Sum(claim => claim.amount);

        // Set claims to validClaims to keep claims from filling up with old claims
        claims = validClaims;
        return availableInventory > 0;
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
        yield return string.Format("Available Amount: {0}", AvailableInventory);
        yield return string.Format("Category: {0}", BasePrice);
        yield return string.Format("BasePrice: {0:N2}", BasePrice);
    }

    public object ToJSon()
    {
        JObject inventoryJson = new JObject();
        if (Tile != null)
        {
            inventoryJson.Add("X", Tile.X);
            inventoryJson.Add("Y", Tile.Y);
            inventoryJson.Add("Z", Tile.Z);
        }

        inventoryJson.Add("Type", Type);
        inventoryJson.Add("MaxStackSize", MaxStackSize);
        inventoryJson.Add("StackSize", StackSize);
        inventoryJson.Add("BasePrice", BasePrice);
        inventoryJson.Add("Category", Category);
        inventoryJson.Add("Locked", Locked);
        inventoryJson.Add("LocalizationName", LocalizationName);
        inventoryJson.Add("LocalizationDesc", LocalizationDescription);

        return inventoryJson;
    }

    public void FromJson(JToken inventoryToken)
    {
        Type = (string)inventoryToken["Type"];
        MaxStackSize = (int)inventoryToken["MaxStackSize"];
        StackSize = (int)inventoryToken["StackSize"];
        BasePrice = (float)inventoryToken["BasePrice"];
        Category = (string)inventoryToken["Category"];
        Locked = (bool)inventoryToken["Locked"];
        LocalizationName = (string)inventoryToken["LocalizationName"];
        LocalizationDescription = (string)inventoryToken["LocalizationDesc"];
    }

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            LocalizationKey = "Sample Item Context action",
            RequireCharacterSelected = true,
            Action = (cm, c) => UnityDebugger.Debugger.Log("Inventory", "Sample menu action")
        };

        if (PrototypeManager.Furniture.Has(this.Type))
        {
            yield return new ContextMenuAction
            {
                LocalizationKey = "install_order",
                RequireCharacterSelected = false,
                Action = (cm, c) => BuildModeController.Instance.SetMode_BuildFurniture(Type, true)
            };
        }
    }

    public bool CanBePickedUp(bool canTakeFromStockpile)
    {
        // You can't pick up stuff that isn't on a tile or if it's locked
        if (Tile == null || Locked || !CanClaim())
        {
            return false;
        }

        return Tile.Furniture == null || canTakeFromStockpile == true || Tile.Furniture.HasTypeTag("Storage") == false;
    }

    public override string ToString()
    {
        return string.Format("{0} [{1}/{2}]", Type, StackSize, MaxStackSize);
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        Type = reader_parent.GetAttribute("type");
        MaxStackSize = int.Parse(reader_parent.GetAttribute("maxStackSize") ?? "50");
        BasePrice = float.Parse(reader_parent.GetAttribute("basePrice") ?? "1");
        Category = reader_parent.GetAttribute("category");
        LocalizationName = reader_parent.GetAttribute("localizationName");
        LocalizationDescription = reader_parent.GetAttribute("localizationDesc");
    }

    private void ImportPrototypeSettings(int defaulMaxStackSize, float defaultBasePrice, string defaultCategory)
    {
        if (PrototypeManager.Inventory.Has(Type))
        {
            Inventory prototype = PrototypeManager.Inventory.Get(Type);
            MaxStackSize = prototype.MaxStackSize;
            BasePrice = prototype.BasePrice;
            Category = prototype.Category;
            LocalizationName = prototype.LocalizationName;
            LocalizationDescription = prototype.LocalizationDescription;
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

    public struct InventoryClaim
    {
        public float time;
        public Character character;
        public int amount;

        public InventoryClaim(float time, Character character, int amount)
        {
            this.time = time;
            this.character = character;
            this.amount = amount;
        }
    }
}
