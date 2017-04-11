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
using Animation;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Newtonsoft.Json.Linq;
using ProjectPorcupine.Buildable.Components;
using ProjectPorcupine.Jobs;
using ProjectPorcupine.Localization;
using ProjectPorcupine.OrderActions;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Furniture : ISelectable, IPrototypable, IContextActionProvider, IBuildable, IUpdatable
{
    #region Private Variables
    private string isEnterableAction;

    /// <summary>
    /// This action is called to get the progress info based on the furniture parameters.
    /// </summary>
    private string getProgressInfoNameAction;

    private List<string> replaceableFurniture = new List<string>();

    /// <summary>
    /// These context menu lua action are used to build the context menu of the furniture.
    /// </summary>
    private List<ContextMenuLuaAction> contextMenuLuaActions;

    private HashSet<BuildableComponent> components;

    private Dictionary<string, OrderAction> orderActions;

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private HashSet<string> tileTypeBuildPermissions;

    private bool isOperating;

    // Need to hold the health value.
    private HealthSystem health;

    // Did we have power in the last update?
    private bool prevUpdatePowerOn;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Furniture"/> class.
    /// This constructor is used to create prototypes and should never be used ouside the Prototype Manager.
    /// </summary>
    public Furniture()
    {
        Tint = Color.white;
        VerticalDoor = false;
        EventActions = new EventActions();

        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        Parameters = new Parameter();
        Jobs = new BuildableJobs(this);
        typeTags = new HashSet<string>();
        tileTypeBuildPermissions = new HashSet<string>();
        PathfindingWeight = 1f;
        PathfindingModifier = 0f;
        Height = 1;
        Width = 1;
        CanRotate = false;
        Rotation = 0f;
        DragType = "single";
        LinksToNeighbour = string.Empty;
        components = new HashSet<BuildableComponent>();
        orderActions = new Dictionary<string, OrderAction>();
    }

    /// <summary>
    /// Copy Constructor -- don't call this directly, unless we never
    /// do ANY sub-classing. Instead use Clone(), which is more virtual.
    /// </summary>
    private Furniture(Furniture other)
    {
        Type = other.Type;
        typeTags = new HashSet<string>(other.typeTags);
        MovementCost = other.MovementCost;
        PathfindingModifier = other.PathfindingModifier;
        PathfindingWeight = other.PathfindingWeight;
        RoomEnclosure = other.RoomEnclosure;
        Width = other.Width;
        Height = other.Height;
        CanRotate = other.CanRotate;
        Rotation = other.Rotation;
        Tint = other.Tint;
        LinksToNeighbour = other.LinksToNeighbour;
        health = other.health;

        Parameters = new Parameter(other.Parameters);
        Jobs = new BuildableJobs(this, other.Jobs);

        // add cloned components
        components = new HashSet<BuildableComponent>();
        foreach (BuildableComponent component in other.components)
        {
            components.Add(component.Clone());
        }

        // add cloned order actions
        orderActions = new Dictionary<string, OrderAction>();
        foreach (var orderAction in other.orderActions)
        {
            orderActions.Add(orderAction.Key, orderAction.Value.Clone());
        }

        if (other.Animation != null)
        {
            Animation = other.Animation.Clone();
        }

        if (other.EventActions != null)
        {
            EventActions = other.EventActions.Clone();
        }

        if (other.contextMenuLuaActions != null)
        {
            contextMenuLuaActions = new List<ContextMenuLuaAction>(other.contextMenuLuaActions);
        }

        if (other.ReplaceableFurniture != null)
        {
            replaceableFurniture = other.ReplaceableFurniture;
        }

        isEnterableAction = other.isEnterableAction;

        getProgressInfoNameAction = other.getProgressInfoNameAction;

        tileTypeBuildPermissions = new HashSet<string>(other.tileTypeBuildPermissions);

        RequiresSlowUpdate = EventActions.HasEvent("OnUpdate") || components.Any(c => c.RequiresSlowUpdate);

        RequiresFastUpdate = EventActions.HasEvent("OnFastUpdate") || components.Any(c => c.RequiresFastUpdate);

        LocalizationCode = other.LocalizationCode;
        UnlocalizedDescription = other.UnlocalizedDescription;

        // force true as default, to trigger OnIsOperatingChange (to sync the furniture icons after initialization)
        IsOperating = true;
    }
    #endregion

    #region Accessors
    /// <summary>
    /// This event will trigger when the furniture has been changed.
    /// This is means that any change (parameters, job state etc) to the furniture will trigger this.
    /// </summary>
    public event Action<Furniture> Changed;

    /// <summary>
    /// This event will trigger when the furniture has been removed.
    /// </summary>
    public event Action<Furniture> Removed;

    /// <summary>
    /// This event will trigger if <see cref="IsOperating"/> has been changed.
    /// </summary>
    public event Action<Furniture> IsOperatingChanged;

    /// <summary>
    /// Gets or sets the Furniture's <see cref="PathfindingModifier"/> which is added into the Tile's final PathfindingCost.
    /// </summary>
    /// <value>The modifier used in pathfinding.</value>
    public float PathfindingModifier { get; set; }

    /// <summary>
    /// Gets or sets the Furniture's pathfinding weight which is multiplied into the Tile's final PathfindingCost.
    /// </summary>
    /// <value>The pathfinding weight for the tiles the furniture currently occupies.</value>
    public float PathfindingWeight { get; set; }

    /// <summary>
    /// Gets the tint used to change the color of the furniture.
    /// </summary>
    /// <value>The Color of the furniture.</value>
    public Color Tint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the door is Vertical or not.
    /// Should be false if the furniture is not a door.
    /// This field will most likely be moved to another class.
    /// </summary>
    /// <value>Whether the door is Vertical or not.</value>
    public bool VerticalDoor { get; set; }

    /// <summary>
    /// Checks whether furniturehas some custom progress info.
    /// </summary>
    public bool HasCustomProgressReport
    {
        get
        {
            return !string.IsNullOrEmpty(getProgressInfoNameAction);
        }
    }

    /// <summary>
    /// Gets the EventAction for the current furniture.
    /// These actions are called when an event is called. They get passed the furniture
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    /// <value>The event actions that is called on update.</value>
    public EventActions EventActions { get; private set; }

    public Bounds Bounds
    {
        get
        {
            return new Bounds(
                new Vector3(Tile.X - 0.5f + (Width / 2), Tile.Y - 0.5f + (Height / 2), 0),
                new Vector3(Width, Height));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the furniture is operating or not.
    /// </summary>
    /// <value>Whether the furniture is operating or not.</value>
    public bool IsOperating
    {
        get
        {
            return isOperating;
        }

        private set
        {
            if (isOperating == value)
            {
                return;
            }

            isOperating = value;
            OnIsOperatingChanged(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the furniture is selected by the player or not.
    /// </summary>
    /// <value>Whether the furniture is selected or not.</value>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets the BASE tile of the furniture. (Large objects can span over multiple tiles).
    /// This should be RENAMED (possibly to BaseTile).
    /// </summary>
    /// <value>The BASE tile of the furniture.</value>
    public Tile Tile { get; private set; }

    /// <summary>
    /// Gets the string that defines the type of object the furniture is. This gets queried by the visual system to
    /// know what sprite to render for this furniture.
    /// </summary>
    /// <value>The type of the furniture.</value>
    public string Type { get; private set; }

    /// <summary>
    /// Gets a list of furniture Type this furniture can be replaced with.
    /// This should most likely not be a list of strings.
    /// </summary>
    /// <value>A list of furniture that this furniture can be replaced with.</value>
    public List<string> ReplaceableFurniture
    {
        get { return replaceableFurniture; }
    }

    /// <summary>
    /// Gets the movement cost multiplier that this furniture has. This can be a float value from 0 to any positive number.
    /// The movement cost acts as a multiplier: e.g. 1 is default, 2 is twice as slow.
    /// Tile types and environmental effects will be combined with this value (additive).
    /// If this value is '0' then the furniture is impassable.
    /// </summary>
    /// <value>The movement cost multiplier the furniture has.</value>
    public float MovementCost { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the furniture can close a room (e.g. act as a wall).
    /// </summary>
    public bool RoomEnclosure { get; private set; }

    /// <summary>
    /// Gets the width of the furniture.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the furniture.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// If true player is allowed to rotate the furniture.
    /// </summary>
    public bool CanRotate { get; private set; }

    /// <summary>
    /// Gets/Set the rotation of the furniture.
    /// </summary>
    public float Rotation { get; private set; }

    /// <summary>
    /// Gets the code used for Localization of the furniture.
    /// </summary>
    public string LocalizationCode { get; private set; }

    /// <summary>
    /// Gets the description of the furniture. This is used by localization.
    /// </summary>
    public string UnlocalizedDescription { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this furniture is next to any furniture of the same type.
    /// This is used to check what sprite to use if furniture is next to each other.
    /// </summary>
    public string LinksToNeighbour { get; private set; }

    /// <summary>
    /// Gets the type of dragging that is used to build multiples of this furniture.
    /// e.g walls.
    /// </summary>
    public string DragType { get; private set; }

    /// <summary>
    /// Represents name of the sprite shown in menus.
    /// </summary>
    public string DefaultSpriteName { get; set; }

    /// <summary>
    /// Actual sprite name (can be null).
    /// </summary>
    public string SpriteName { get; set; }

    /// <summary>
    /// Sprite name for overlay.
    /// </summary>
    public string OverlaySpriteName { get; set; }

    /// <summary>
    /// Gets or sets the furniture animation.
    /// </summary>
    public FurnitureAnimation Animation { get; set; }

    /// <summary>
    /// Gets or sets the parameters that is tied to the furniture.
    /// </summary>
    public Parameter Parameters { get; set; }

    /// <summary>
    /// Gets a component that handles the jobs linked to the furniture.
    /// </summary>
    public BuildableJobs Jobs { get; private set; }

    /// <summary>
    /// This flag is set if the furniture is tasked to be destroyed.
    /// </summary>
    public bool IsBeingDestroyed { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this instance has components.
    /// </summary>
    /// <value><c>true</c> if this instance has components; otherwise, <c>false</c>.</value>
    public bool HasComponents
    {
        get
        {
            return components != null || components.Count != 0;
        }
    }

    public bool RequiresFastUpdate { get; set; }

    public bool RequiresSlowUpdate { get; set; }

    /// <summary>
    /// Flag with furniture requirements (used for showing icon overlay, e.g. No power, ... ).
    /// </summary>
    public BuildableComponent.Requirements Requirements { get; protected set; }

    /// <summary>
    /// Gets the Health of this object.
    /// </summary>
    public HealthSystem Health
    {
        get
        {
            if (health == null)
            {
                health = new HealthSystem(-1f, true, false, false, false);
            }

            return health;
        }
    }

    #endregion

    /// <summary>
    /// Used to place furniture in a certain position.
    /// </summary>
    /// <param name="proto">The prototype furniture to place.</param>
    /// <param name="tile">The base tile to place the furniture on, The tile will be the bottom left corner of the furniture (to check).</param>
    /// <returns>Furniture object.</returns>
    public static Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.IsValidPosition(tile) == false)
        {
            UnityDebugger.Debugger.LogWarning("Furniture", "PlaceInstance :: Position Validity Function returned FALSE. " + proto.Type + " " + tile.X + ", " + tile.Y + ", " + tile.Z);
            return null;
        }

        // We know our placement destination is valid.
        Furniture furnObj = proto.Clone();
        furnObj.Tile = tile;

        if (tile.PlaceFurniture(furnObj) == false)
        {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        // need to update reference to furniture and call Initialize (so components can place hooks on events there)
        foreach (BuildableComponent component in furnObj.components)
        {
            component.Initialize(furnObj);
        }

        if (furnObj.LinksToNeighbour != string.Empty)
        {
            // This type of furniture links itself to its neighbours,
            // so we should inform our neighbours that they have a new
            // buddy.  Just trigger their OnChangedCallback.
            int x = tile.X;
            int y = tile.Y;

            for (int xpos = x - 1; xpos < x + proto.Width + 1; xpos++)
            {
                for (int ypos = y - 1; ypos < y + proto.Height + 1; ypos++)
                {
                    Tile tileAt = World.Current.GetTileAt(xpos, ypos, tile.Z);
                    if (tileAt != null && tileAt.Furniture != null && tileAt.Furniture.Changed != null)
                    {
                        tileAt.Furniture.Changed(tileAt.Furniture);
                    }
                }
            }
        }

        // Let our workspot tile know it is reserved for us
        World.Current.ReserveTileAsWorkSpot(furnObj);

        // Call LUA install scripts
        furnObj.EventActions.Trigger("OnInstall", furnObj);

        return furnObj;
    }

    #region Update and Animation
    /// <summary>
    /// This function is called to update the furniture animation in lua.
    /// This will be called every frame and should be used carefully.
    /// </summary>
    /// <param name="deltaTime">The time since the last update was called.</param>
    public void EveryFrameUpdate(float deltaTime)
    {
        if (EventActions != null)
        {
            EventActions.Trigger("OnFastUpdate", this, deltaTime);
        }

        foreach (BuildableComponent component in components)
        {
            component.EveryFrameUpdate(deltaTime);
        }
    }

    /// <summary>
    /// This function is called to update the furniture. This will also trigger EventsActions.
    /// This checks if the furniture is a PowerConsumer, and if it does not have power it cancels its job.
    /// </summary>
    /// <param name="deltaTime">The time since the last update was called.</param>
    public void FixedFrequencyUpdate(float deltaTime)
    {
        // requirements from components (gas, ...)
        bool canFunction = true;
        BuildableComponent.Requirements newRequirements = BuildableComponent.Requirements.None;
        foreach (BuildableComponent component in components)
        {
            bool componentCanFunction = component.CanFunction();
            canFunction &= componentCanFunction;

            // if it can't function, collect all stuff it needs (power, gas, ...) for icon signalization
            if (!componentCanFunction)
            {
                newRequirements |= component.Needs;
            }
        }

        // requirements were changed, force update of status icons
        if (Requirements != newRequirements)
        {
            Requirements = newRequirements;
            OnIsOperatingChanged(this);
        }

        IsOperating = canFunction;

        if (canFunction == false)
        {
            if (prevUpdatePowerOn)
            {
                EventActions.Trigger("OnPowerOff", this, deltaTime);
            }

            Jobs.PauseAll();
            prevUpdatePowerOn = false;
            return;
        }

        prevUpdatePowerOn = true;
        Jobs.ResumeAll();

        if (EventActions != null)
        {
            EventActions.Trigger("OnUpdate", this, deltaTime);
        }

        foreach (BuildableComponent component in components)
        {
            component.FixedFrequencyUpdate(deltaTime);
        }

        if (Animation != null)
        {
            Animation.Update(deltaTime);
        }
    }

    /// <summary>
    /// Set the animation state. Will only have an effect if stateName is different from current animation stateName.
    /// </summary>
    public void SetAnimationState(string stateName)
    {
        if (Animation == null)
        {
            return;
        }

        Animation.SetState(stateName);
    }

    /// <summary>
    /// Set the animation frame depending on a value. The currentvalue percent of the maxvalue will determine which frame is shown.
    /// </summary>
    public void SetAnimationProgressValue(float currentValue, float maxValue)
    {
        if (Animation == null)
        {
            return;
        }

        if (maxValue == 0)
        {
            UnityDebugger.Debugger.LogError("SetAnimationProgressValue maxValue is zero");
        }

        float percent = Mathf.Clamp01(currentValue / maxValue);
        Animation.SetProgressValue(percent);
    }
    #endregion

    #region Get Status
    /// <summary>
    /// Whether this furniture is an exit for a room.
    /// </summary>
    /// <returns>True if furniture is an exit.</returns>
    public bool IsExit()
    {
        if (RoomEnclosure && MovementCost > 0f)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the furniture can be Entered.
    /// </summary>
    /// <returns>Enterability state Yes if furniture can be entered, Soon if it can be entered after a bit and No
    /// if it cannot be entered.</returns>
    public Enterability IsEnterable()
    {
        if (string.IsNullOrEmpty(isEnterableAction))
        {
            return Enterability.Yes;
        }

        DynValue ret = FunctionsManager.Furniture.Call(isEnterableAction, this);
        return (Enterability)ret.Number;
    }

    public string GetDefaultSpriteName()
    {
        if (!string.IsNullOrEmpty(DefaultSpriteName))
        {
            return DefaultSpriteName;
        }

        // Else return default Type string
        return Type;
    }

    /// <summary>
    /// Check if the furniture has a function to determine the sprite name and calls that function.
    /// </summary>
    /// <param name="explicitSpriteUsed">Out: true if explicit sprite was used, false if default type was used.</param>
    /// <returns>Name of the sprite.</returns>
    public string GetSpriteName(out bool explicitSpriteUsed)
    {
        explicitSpriteUsed = true;
        if (!string.IsNullOrEmpty(SpriteName))
        {
            return SpriteName;
        }

        // Try to get spritename from animation
        if (Animation != null)
        {
            return Animation.GetSpriteName();
        }

        // Else return default Type string
        explicitSpriteUsed = false;
        return Type;
    }

    public string GetOverlaySpriteName()
    {
        return OverlaySpriteName;
    }
    #endregion

    #region Read Prototype
    /// <summary>
    /// Reads the prototype furniture from XML.
    /// </summary>
    /// <param name="readerParent">The XML reader to read from.</param>
    public void ReadXmlPrototype(XmlReader readerParent)
    {
        Type = readerParent.GetAttribute("type");

        XmlReader reader = readerParent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "TypeTag":
                    reader.Read();
                    typeTags.Add(reader.ReadContentAsString());
                    break;
                case "MovementCost":
                    reader.Read();
                    MovementCost = reader.ReadContentAsFloat();
                    break;
                case "PathfindingModifier":
                    reader.Read();
                    PathfindingModifier = reader.ReadContentAsFloat();
                    break;
                case "PathfindingWeight":
                    reader.Read();
                    PathfindingWeight = reader.ReadContentAsFloat();
                    break;
                case "Width":
                    reader.Read();
                    Width = reader.ReadContentAsInt();
                    break;
                case "Height":
                    reader.Read();
                    Height = reader.ReadContentAsInt();
                    break;
                case "Health":
                    reader.Read();
                    health = new HealthSystem(reader.ReadContentAsFloat(), false, true, false, false);
                    break;
                case "LinksToNeighbours":
                    reader.Read();
                    LinksToNeighbour = reader.ReadContentAsString();
                    break;
                case "EnclosesRooms":
                    reader.Read();
                    RoomEnclosure = reader.ReadContentAsBoolean();
                    break;
                case "CanReplaceFurniture":
                    replaceableFurniture.Add(reader.GetAttribute("typeTag").ToString());
                    break;
                case "CanRotate":
                    reader.Read();
                    CanRotate = reader.ReadContentAsBoolean();
                    break;
                case "DragType":
                    reader.Read();
                    DragType = reader.ReadContentAsString();
                    break;
                case "CanBeBuiltOn":
                    tileTypeBuildPermissions.Add(reader.GetAttribute("tileType"));
                    break;
                case "Animations":
                    XmlReader animationReader = reader.ReadSubtree();
                    ReadAnimationXml(animationReader);
                    break;
                case "Action":
                    XmlReader subtree = reader.ReadSubtree();
                    EventActions.ReadXml(subtree);
                    subtree.Close();
                    break;
                case "ContextMenuAction":
                    contextMenuLuaActions.Add(new ContextMenuLuaAction
                    {
                        LuaFunction = reader.GetAttribute("FunctionName"),
                        LocalizationKey = reader.GetAttribute("LocalizationKey"),
                        RequireCharacterSelected = bool.Parse(reader.GetAttribute("RequireCharacterSelected")),
                        DevModeOnly = bool.Parse(reader.GetAttribute("DevModeOnly") ?? "false")
                    });
                    break;
                case "IsEnterable":
                    isEnterableAction = reader.GetAttribute("FunctionName");
                    break;
                case "GetProgressInfo":
                    getProgressInfoNameAction = reader.GetAttribute("functionName");
                    break;
                case "JobWorkSpotOffset":
                    Jobs.ReadWorkSpotOffset(reader);
                    break;
                case "JobInputSpotOffset":
                    Jobs.ReadInputSpotOffset(reader);
                    break;
                case "JobOutputSpotOffset":
                    Jobs.ReadOutputSpotOffset(reader);
                    break;
                case "Params":
                    ReadXmlParams(reader);  // Read in the Param tag
                    break;
                case "LocalizationCode":
                    reader.Read();
                    LocalizationCode = reader.ReadContentAsString();
                    break;
                case "UnlocalizedDescription":
                    reader.Read();
                    UnlocalizedDescription = reader.ReadContentAsString();
                    break;
                case "Component":
                    BuildableComponent component = BuildableComponent.Deserialize(reader);
                    if (component != null)
                    {
                        component.InitializePrototype(this);
                        components.Add(component);
                    }

                    break;
                case "OrderAction":
                    OrderAction orderAction = OrderAction.Deserialize(reader);
                    if (orderAction != null)
                    {
                        orderActions[orderAction.Type] = orderAction;
                    }

                    break;
            }
        }

        if (orderActions.ContainsKey("Uninstall"))
        {
            Inventory asInventory = Inventory.CreatePrototype(Type, 1, 0f, "crated_furniture", LocalizationCode, UnlocalizedDescription);
            PrototypeManager.Inventory.Add(asInventory);
        }
    }

    /// <summary>
    /// Reads the XML for parameters that this furniture has and assign it to the furniture.
    /// </summary>
    /// <param name="reader">The reader to read the parameters from.</param>
    public void ReadXmlParams(XmlReader reader)
    {
        Parameters = Parameter.ReadXml(reader);
    }
    #endregion

    public object ToJSon()
    {
        JObject furnitureJSon = new JObject();
        furnitureJSon.Add("X", Tile.X);
        furnitureJSon.Add("Y", Tile.Y);
        furnitureJSon.Add("Z", Tile.Z);
        furnitureJSon.Add("Type", Type);
        furnitureJSon.Add("Rotation", Rotation);
        if (Parameters.HasContents())
        {
            furnitureJSon.Add("Parameters", Parameters.ToJson());
        }

        return furnitureJSon;
    }

    public void FromJson(JToken furnitureToken)
    {
        JObject furnitureJObject = (JObject)furnitureToken;

        // Everything else has already been set by FurnitureManager, we just need our parameters
        if (furnitureJObject.Children().Contains("Parameters"))
        {
            Parameters.FromJson(furnitureJObject["Parameters"]);
        }
    }

    /// <summary>
    /// Accepts for storage.
    /// </summary>
    /// <returns>A list of RequestedItem which the Furniture accepts for storage.</returns>
    public RequestedItem[] AcceptsForStorage()
    {
        if (HasTypeTag("Storage") == false)
        {
            UnityDebugger.Debugger.Log("Stockpile_messages", "Someone is asking a non-stockpile to store stuff!?");
            return null;
        }

        // TODO: read this from furniture params
        Dictionary<string, RequestedItem> itemsDict = new Dictionary<string, RequestedItem>();
        foreach (Inventory inventoryProto in PrototypeManager.Inventory.Values)
        {
            itemsDict[inventoryProto.Type] = new RequestedItem(inventoryProto.Type, 1, inventoryProto.MaxStackSize);
        }

        return itemsDict.Values.ToArray();
    }

    public void SetUninstallJob()
    {
        if (SettingsKeyHolder.DeveloperMode)
        {
            Uninstall();
            return;
        }

        Jobs.CancelAll();

        Uninstall uninstallOrder = GetOrderAction<Uninstall>();
        if (uninstallOrder != null)
        {
            Job job = uninstallOrder.CreateJob(Tile, Type);
            job.OnJobCompleted += (inJob) => Uninstall();
            World.Current.jobQueue.Enqueue(job);
        }
    }

    /// <summary>
    /// Deconstructs the furniture.
    /// </summary>
    public void Uninstall()
    {
        int x = Tile.X;
        int y = Tile.Y;
        int fwidth = 1;
        int fheight = 1;
        string linksToNeighbour = string.Empty;
        if (Tile.Furniture != null)
        {
            Furniture furniture = Tile.Furniture;
            fwidth = furniture.Width;
            fheight = furniture.Height;
            linksToNeighbour = furniture.LinksToNeighbour;
            furniture.Jobs.CancelAll();
        }

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);

        // Let our workspot tile know it is no longer reserved for us
        World.Current.UnreserveTileAsWorkSpot(this);

        Tile.UnplaceFurniture();

        Deconstruct deconstructOrder = GetOrderAction<Deconstruct>();
        if (deconstructOrder != null)
        {
            World.Current.InventoryManager.PlaceInventoryAround(Tile, new Inventory(Type, 1));
        }

        if (Removed != null)
        {
            Removed(this);
        }

        // Do we need to recalculate our rooms?
        if (RoomEnclosure)
        {
            World.Current.RoomManager.DoRoomFloodFill(Tile, false);
        }

        ////World.current.InvalidateTileGraph();

        if (World.Current.tileGraph != null)
        {
            World.Current.tileGraph.RegenerateGraphAtTile(Tile);
        }

        // We should inform our neighbours that they have just lost a
        // neighbour regardless of type.  
        // Just trigger their OnChangedCallback. 
        if (linksToNeighbour != string.Empty)
        {
            for (int xpos = x - 1; xpos < x + fwidth + 1; xpos++)
            {
                for (int ypos = y - 1; ypos < y + fheight + 1; ypos++)
                {
                    Tile t = World.Current.GetTileAt(xpos, ypos, Tile.Z);
                    if (t != null && t.Furniture != null && t.Furniture.Changed != null)
                    {
                        t.Furniture.Changed(t.Furniture);
                    }
                }
            }
        }

        // At this point, no DATA structures should be pointing to us, so we
        // should get garbage-collected.
    }

    #region Deconstruct
    /// <summary>
    /// Sets the furniture to be deconstructed.
    /// </summary>
    public void SetDeconstructJob()
    {
        if (SettingsKeyHolder.DeveloperMode)
        {
            Deconstruct();
            return;
        }

        if (IsBeingDestroyed)
        {
            return; // Already being destroyed, don't do anything more
        }

        IsBeingDestroyed = true;
        Jobs.CancelAll();

        Deconstruct deconstructOrder = GetOrderAction<Deconstruct>();
        if (deconstructOrder != null)
        {
            Job job = deconstructOrder.CreateJob(Tile, Type);
            job.OnJobCompleted += (inJob) => Deconstruct();
            World.Current.jobQueue.Enqueue(job);
        }
    }

    /// <summary>
    /// Deconstructs the furniture.
    /// </summary>
    public void Deconstruct()
    {
        int x = Tile.X;
        int y = Tile.Y;
        int fwidth = 1;
        int fheight = 1;
        string linksToNeighbour = string.Empty;
        if (Tile.Furniture != null)
        {
            Furniture furniture = Tile.Furniture;
            fwidth = furniture.Width;
            fheight = furniture.Height;
            linksToNeighbour = furniture.LinksToNeighbour;
            furniture.Jobs.CancelAll();
        }

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);

        // Let our workspot tile know it is no longer reserved for us
        World.Current.UnreserveTileAsWorkSpot(this);

        Tile.UnplaceFurniture();

        Deconstruct deconstructOrder = GetOrderAction<Deconstruct>();
        if (deconstructOrder != null)
        {
            foreach (OrderAction.InventoryInfo inv in deconstructOrder.Inventory)
            {
                World.Current.InventoryManager.PlaceInventoryAround(Tile, new Inventory(inv.Type, inv.Amount));
            }
        }

        if (Removed != null)
        {
            Removed(this);
        }

        // Do we need to recalculate our rooms?
        if (RoomEnclosure)
        {
            World.Current.RoomManager.DoRoomFloodFill(Tile, false);
        }

        ////World.current.InvalidateTileGraph();

        if (World.Current.tileGraph != null)
        {
            World.Current.tileGraph.RegenerateGraphAtTile(Tile);
        }

        // We should inform our neighbours that they have just lost a
        // neighbour regardless of type.  
        // Just trigger their OnChangedCallback. 
        if (linksToNeighbour != string.Empty)
        {
            for (int xpos = x - 1; xpos < x + fwidth + 1; xpos++)
            {
                for (int ypos = y - 1; ypos < y + fheight + 1; ypos++)
                {
                    Tile t = World.Current.GetTileAt(xpos, ypos, Tile.Z);
                    if (t != null && t.Furniture != null && t.Furniture.Changed != null)
                    {
                        t.Furniture.Changed(t.Furniture);
                    }
                }
            }
        }

        // At this point, no DATA structures should be pointing to us, so we
        // should get garbage-collected.
    }
    #endregion

    #region Get Description Information
    /// <summary>
    /// Checks whether the furniture has a certain tag.
    /// </summary>
    /// <param name="typeTag">Tag to check for.</param>
    /// <returns>True if furniture has specified tag.</returns>
    public bool HasTypeTag(string typeTag)
    {
        return typeTags.Contains(typeTag);
    }

    /// <summary>
    /// Gets the type tags.
    /// </summary>
    /// <returns>The type tags.</returns>
    public string[] GetTypeTags()
    {
        return typeTags.ToArray();
    }

    /// <summary>
    /// Returns LocalizationCode name for the furniture.
    /// </summary>
    /// <returns>LocalizationCode for the name of the furniture.</returns>
    public string GetName()
    {
        return LocalizationCode; // this.Name;
    }

    /// <summary>
    /// Returns the UnlocalizedDescription of the furniture.
    /// </summary>
    /// <returns>Description of the furniture.</returns>
    public string GetDescription()
    {
        return UnlocalizedDescription;
    }

    /// <summary>
    /// Returns the description of the job linked to the furniture. NOT INMPLEMENTED.
    /// </summary>
    /// <returns>Job description of the job linked to the furniture.</returns>
    public string GetJobDescription()
    {
        return string.Empty;
    }

    public string GetProgressInfo()
    {
        if (string.IsNullOrEmpty(getProgressInfoNameAction))
        {
            return string.Empty;
        }
        else
        {
            DynValue ret = FunctionsManager.Furniture.Call(getProgressInfoNameAction, this);
            return ret.String;
        }
    }

    public IEnumerable<string> GetAdditionalInfo()
    {
        // try to get some info from components
        foreach (BuildableComponent component in components)
        {
            IEnumerable<string> desc = component.GetDescription();
            if (desc != null)
            {
                foreach (string inf in desc)
                {
                    yield return inf;
                }
            }
        }

        if (health != null)
        {
            yield return health.TextForSelectionPanel();
        }

        yield return GetProgressInfo();
    }

    public IPluggable GetPluggable(HashSet<string> utilityTags)
    {
        if (components != null)
        {
            foreach (BuildableComponent component in components)
            {
                IPluggable pluggable = component as IPluggable;
                if (pluggable != null && utilityTags.Contains(pluggable.UtilityType))
                {
                    return pluggable;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets component if present or null.
    /// </summary>
    /// <typeparam name="T">Type of component.</typeparam>
    /// <param name="componentName">Type of the component, e.g. PowerConnection, WorkShop.</param>
    /// <returns>Component or null.</returns>
    public T GetComponent<T>(string componentName) where T : BuildableComponent
    {
        if (components != null)
        {
            foreach (BuildableComponent component in components)
            {
                if (component.Type.Equals(componentName))
                {
                    return (T)component;
                }
            }
        }

        return null;
    }

    public BuildableComponent.Requirements GetPossibleRequirements()
    {
        BuildableComponent.Requirements requires = BuildableComponent.Requirements.None;

        foreach (BuildableComponent component in components)
        {
            requires |= component.Needs;
        }

        return requires;
    }

    public T GetOrderAction<T>() where T : OrderAction
    {
        OrderAction orderAction;
        if (orderActions.TryGetValue(typeof(T).Name, out orderAction))
        {
            return (T)orderAction;
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Context Menu
    /// <summary>
    /// Gets the Context Menu Actions.
    /// </summary>
    /// <param name="contextMenu">The context menu to check for actions.</param>
    /// <returns>Context menu actions.</returns>
    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        if (SettingsKeyHolder.DeveloperMode || HasTypeTag("Non-deconstructible") == false)
        {
            yield return new ContextMenuAction
            {
                LocalizationKey = LocalizationTable.GetLocalization("deconstruct_furniture", LocalizationCode),
                RequireCharacterSelected = false,
                Action = (ca, c) => SetDeconstructJob()
            };
        }

        if (PrototypeManager.Inventory.Has(this.Type))
        {
            yield return new ContextMenuAction
            {
                LocalizationKey = LocalizationTable.GetLocalization("uninstall_furniture", LocalizationCode),
                RequireCharacterSelected = false,
                Action = (ca, c) => SetUninstallJob()
            };
        }

        for (int i = 0; i < Jobs.Count; i++)
        {
            if (!Jobs[i].IsBeingWorked)
            {
                yield return new ContextMenuAction
                {
                    LocalizationKey = LocalizationTable.GetLocalization("prioritize_furniture", LocalizationCode),
                    RequireCharacterSelected = true,
                    Action = (ca, c) => c.PrioritizeJob(Jobs[0])
                };
            }
        }

        // check for context menus of components
        foreach (BuildableComponent component in components)
        {
            List<ContextMenuAction> componentContextMenu = component.GetContextMenu();
            if (componentContextMenu != null)
            {
                foreach (ContextMenuAction compContextMenuAction in componentContextMenu)
                {
                    yield return compContextMenuAction;
                }
            }
        }

        foreach (ContextMenuLuaAction contextMenuLuaAction in contextMenuLuaActions)
        {
            if (!contextMenuLuaAction.DevModeOnly ||
                SettingsKeyHolder.DeveloperMode)
            {
                // TODO The Action could be done via a lambda, but it always uses the same space of memory, thus if 2 actions are performed, the same action will be produced for each.
                yield return new ContextMenuAction
                {
                    LocalizationKey = contextMenuLuaAction.LocalizationKey,
                    RequireCharacterSelected = contextMenuLuaAction.RequireCharacterSelected,
                    Action = InvokeContextMenuLuaAction,
                    Parameter = contextMenuLuaAction.LuaFunction    // Note that this is only in place because of the problem with the previous statement.
                };
            }
        }
    }
    #endregion

    // <summary>
    // Set rotation on a furniture. It will swap height and width.
    // </summary>
    // <param name="rotation">The z rotation.</param>
    public void SetRotation(float rotation)
    {
        if (Math.Abs(Rotation - rotation) == 90 || Math.Abs(Rotation - rotation) == 270)
        {
            int tmp = Height;
            Height = Width;
            Width = tmp;
        }

        Rotation = rotation;
    }

    public Tile[] GetAllTiles()
    {
        Tile[] tiles = new Tile[Height * Width];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x + (y * Width)] = World.Current.GetTileAt(Tile.X + x, Tile.Y + y, Tile.Z);
            }
        }

        return tiles;
    }

    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    public Furniture Clone()
    {
        return new Furniture(this);
    }

    /// <summary>
    /// Check if the position of the furniture is valid or not.
    /// This is called when placing the furniture.
    /// TODO : Add some LUA special requierments.
    /// </summary>
    /// <param name="t">The base tile.</param>
    /// <returns>True if the tile is valid for the placement of the furniture.</returns>
    public bool IsValidPosition(Tile tile)
    {
        if (HasTypeTag("OutdoorOnly"))
        {
            if (tile.Room == null || !tile.Room.IsOutsideRoom())
            {
                return false;
            }
        }

        for (int x_off = tile.X; x_off < tile.X + Width; x_off++)
        {
            for (int y_off = tile.Y; y_off < tile.Y + Height; y_off++)
            {
                Tile tile2 = World.Current.GetTileAt(x_off, y_off, tile.Z);

                // Check to see if there is furniture which is replaceable
                bool isReplaceable = false;

                if (tile2.Furniture != null)
                {
                    // Furniture can be replaced, if its typeTags share elements with ReplaceableFurniture
                    isReplaceable = tile2.Furniture.typeTags.Overlaps(ReplaceableFurniture);
                }

                if (!HasTypeTag("DoesntNeedFloor"))
                {
                    // Make sure tile is FLOOR
                    if (tile2.Type != TileType.Floor && tileTypeBuildPermissions.Contains(tile2.Type.Type) == false)
                    {
                        return false;
                    }
                }

                // Make sure tile doesn't already have furniture
                if (tile2.Furniture != null && isReplaceable == false)
                {
                    return false;
                }

                // Make sure we're not building on another furniture's workspot
                if (tile2.IsReservedWorkSpot())
                {
                    return false;
                }
            }
        }

        return true;
    }

    #region Private Context Menu
    private void InvokeContextMenuLuaAction(ContextMenuAction action, Character character)
    {
        FunctionsManager.Furniture.Call(action.Parameter, this, character);
    }
    #endregion

    #region OnChanges
    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Furniture furn)
    {
        if (Changed != null)
        {
            Changed(furn);
        }
    }

    private void OnIsOperatingChanged(Furniture furniture)
    {
        Action<Furniture> handler = IsOperatingChanged;
        if (handler != null)
        {
            handler(furniture);
        }
    }

    private void OnNewThresholdReached(Connection connection)
    {
        UpdateOnChanged(this);
    }

    #endregion

    /// <summary>
    /// Reads and creates FurnitureAnimation from the prototype xml.
    /// </summary>
    private void ReadAnimationXml(XmlReader animationReader)
    {
        Animation = new FurnitureAnimation();
        while (animationReader.Read())
        {
            if (animationReader.Name == "Animation")
            {
                string state = animationReader.GetAttribute("state");
                float fps = 1;
                float.TryParse(animationReader.GetAttribute("fps"), out fps);
                bool looping = true;
                bool.TryParse(animationReader.GetAttribute("looping"), out looping);
                bool valueBased = false;
                bool.TryParse(animationReader.GetAttribute("valuebased"), out valueBased);

                // read frames
                XmlReader frameReader = animationReader.ReadSubtree();
                List<string> framesSpriteNames = new List<string>();
                while (frameReader.Read())
                {
                    if (frameReader.Name == "Frame")
                    {
                        framesSpriteNames.Add(frameReader.GetAttribute("name"));
                    }
                }

                Animation.AddAnimation(state, framesSpriteNames, fps, looping, valueBased);
            }
        }
    }
}
