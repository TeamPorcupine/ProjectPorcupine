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
using System.Xml.Schema;
using System.Xml.Serialization;
using Animation;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ProjectPorcupine.Buildable.Components;
using ProjectPorcupine.Jobs;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectable, IPrototypable, IContextActionProvider, IBuildable
{
    #region Private Variables
    // Prevent construction too close to the world's edge
    private const int MinEdgeDistance = 5;

    private string isEnterableAction;

    /// <summary>
    /// This action is called to get the sprite name based on the furniture parameters.
    /// </summary>
    private string getSpriteNameAction;

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

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private HashSet<string> tileTypeBuildPermissions;

    private bool isOperating;

    private List<Inventory> deconstructInventory;

    // did we have power in the last update?
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
    }

    /// <summary>
    /// Copy Constructor -- don't call this directly, unless we never
    /// do ANY sub-classing. Instead use Clone(), which is more virtual.
    /// </summary>
    private Furniture(Furniture other)
    {
        Type = other.Type;
        Name = other.Name;
        typeTags = new HashSet<string>(other.typeTags);
        description = other.description;
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
        deconstructInventory = other.deconstructInventory;

        Parameters = new Parameter(other.Parameters);
        Jobs = new BuildableJobs(this, other.Jobs);

        // don't need to clone here, as all are prototype things (not changing)
        components = new HashSet<BuildableComponent>(other.components);

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

        isEnterableAction = other.isEnterableAction;
        getSpriteNameAction = other.getSpriteNameAction;
        getProgressInfoNameAction = other.getProgressInfoNameAction;

        if (other.PowerConnection != null)
        {
            PowerConnection = other.PowerConnection.Clone() as Connection;
            PowerConnection.NewThresholdReached += OnNewThresholdReached;
        }

        tileTypeBuildPermissions = new HashSet<string>(other.tileTypeBuildPermissions);

        LocalizationCode = other.LocalizationCode;
        UnlocalizedDescription = other.UnlocalizedDescription;
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
    /// Gets the EventAction for the current furniture.
    /// These actions are called when an event is called. They get passed the furniture
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    /// <value>The event actions that is called on update.</value>
    public EventActions EventActions { get; private set; }
    
    /// <summary>
    /// Gets the Connection that the furniture has to the power system.
    /// </summary>
    /// <value>The Connection of the furniture.</value>
    public Connection PowerConnection { get; private set; }

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
    /// Gets the name of the furniture. The name is the object type by default.
    /// </summary>
    /// <value>The name of the furniture.</value>
    public string Name
    {
        get
        {
            return string.IsNullOrEmpty(name) ? Type : name;
        }

        private set
        {
            name = value;
        }
    }

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

    /// Should we only use the default name? If not, then more complex logic is tested, such as walls.
    /// </summary>
    public bool OnlyUseDefaultSpriteName
    {
        get
        {
            return !string.IsNullOrEmpty(getSpriteNameAction);
        }
    }

    /// <summary>
    /// Whether the furniture has power or not. Always true if power is not applicable to the furniture.
    /// </summary>
    /// <returns>True if the furniture has power or if the furniture doesn't require power to function.</returns>
    public bool DoesntNeedOrHasPower
    {
        get
        {
            return PowerConnection == null || World.Current.PowerNetwork.HasPower(PowerConnection);
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
            Debug.ULogErrorChannel("Furniture", "PlaceInstance -- Position Validity Function returned FALSE. " + proto.Name + " " + tile.X + ", " + tile.Y + ", " + tile.Z);
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

        // plug-in furniture only when it is placed in world
        if (furnObj.PowerConnection != null)
        {
            World.Current.PowerNetwork.PlugIn(furnObj.PowerConnection);
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

        // Update thermalDiffusivity using coefficient
        float thermalDiffusivity = Temperature.defaultThermalDiffusivity;
        if (furnObj.Parameters.ContainsKey("thermal_diffusivity"))
        {
            thermalDiffusivity = furnObj.Parameters["thermal_diffusivity"].ToFloat();
        }

        World.Current.temperature.SetThermalDiffusivity(tile.X, tile.Y, tile.Z, thermalDiffusivity);

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
        foreach (BuildableComponent component in components)
        {
            canFunction &= component.CanFunction();
        }

        IsOperating = DoesntNeedOrHasPower && canFunction;

        if ((PowerConnection != null && PowerConnection.IsPowerConsumer && DoesntNeedOrHasPower == false) ||
            canFunction == false)
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

        // TODO: some weird thing happens
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
            Debug.ULogError("SetAnimationProgressValue maxValue is zero");
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

    /// <summary>
    /// Check if the furniture has a function to determine the sprite name and calls that function.
    /// </summary>
    /// <returns>Name of the sprite.</returns>
    public string GetSpriteName()
    {
        if (!string.IsNullOrEmpty(getSpriteNameAction))
        {
            DynValue ret = FunctionsManager.Furniture.Call(getSpriteNameAction, this);
            return ret.String;
        }

        // Try to get spritename from animation
        if (Animation != null)
        {
            return Animation.GetSpriteName();
        }

        // Else return default Type string
        return Type;
    }
    #endregion

    #region Save Load
    /// <summary>
    /// This does absolutely nothing.
    /// This is required to implement IXmlSerializable.
    /// </summary>
    /// <returns>NULL and NULL.</returns>
    public XmlSchema GetSchema()
    {
        return null;
    }

    /// <summary>
    /// Writes the furniture to XML.
    /// </summary>
    /// <param name="writer">The XML writer to write to.</param>
    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Y", Tile.Y.ToString());
        writer.WriteAttributeString("Z", Tile.Z.ToString());
        writer.WriteAttributeString("type", Type);
        writer.WriteAttributeString("Rotation", Rotation.ToString());

        // Let the Parameters handle their own xml
        Parameters.WriteXml(writer);
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
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "TypeTag":
                    reader.Read();
                    typeTags.Add(reader.ReadContentAsString());
                    break;
                case "Description":
                    reader.Read();
                    description = reader.ReadContentAsString();
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
                case "BuildingJob":
                    ReadXmlBuildingJob(reader);
                    break;
                case "DeconstructJob":
                    ReadXmlDeconstructJob(reader);
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
                        Text = reader.GetAttribute("Text"),
                        RequireCharacterSelected = bool.Parse(reader.GetAttribute("RequireCharacterSelected")),
                        DevModeOnly = bool.Parse(reader.GetAttribute("DevModeOnly") ?? "false")
                    });
                    break;
                case "IsEnterable":
                    isEnterableAction = reader.GetAttribute("FunctionName");
                    break;
                case "GetSpriteName":
                    getSpriteNameAction = reader.GetAttribute("FunctionName");
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
                case "PowerConnection":
                    PowerConnection = new Connection();
                    PowerConnection.ReadPrototype(reader);
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
                        components.Add(component);
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Reads the specified XMLReader (pass it to <see cref="ReadXmlParams(XmlReader)"/>)
    /// This is used to load furniture from a save file.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    public void ReadXml(XmlReader reader)
    {
        // X, Y, type and rotation have already been set, and we should already
        // be assigned to a tile.  So just read extra data if we have any.
        if (!reader.IsEmptyElement)
        {
            ReadXmlParams(reader);
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

    /// <summary>
    /// Reads the XML building job.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    public void ReadXmlBuildingJob(XmlReader reader)
    {
        float jobTime = float.Parse(reader.GetAttribute("jobTime"));
        List<RequestedItem> items = new List<RequestedItem>();
        XmlReader inventoryReader = reader.ReadSubtree();

        while (inventoryReader.Read())
        {
            if (inventoryReader.Name == "Inventory")
            {
                // Found an inventory requirement, so add it to the list!
                int amount = int.Parse(inventoryReader.GetAttribute("amount"));
                items.Add(new RequestedItem(inventoryReader.GetAttribute("type"), amount));
            }
        }

        Job job = new Job(
            null,
            Type,
            (theJob) => World.Current.FurnitureManager.ConstructJobCompleted(theJob),
            jobTime,
            items.ToArray(),
            Job.JobPriority.High);
        job.JobDescription = "job_build_" + Type + "_desc";

        PrototypeManager.FurnitureConstructJob.Set(job);
    }

    /// <summary>
    /// Reads the XML building job.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    public void ReadXmlDeconstructJob(XmlReader reader)
    {
        float jobTime = 0;
        float.TryParse(reader.GetAttribute("jobTime"), out jobTime);
        deconstructInventory = new List<Inventory>();
        XmlReader inventoryReader = reader.ReadSubtree();

        while (inventoryReader.Read())
        {
            if (inventoryReader.Name == "Inventory")
            {
                // Found an inventory requirement, so add it to the list!
                deconstructInventory.Add(new Inventory(
                    inventoryReader.GetAttribute("type"),
                    int.Parse(inventoryReader.GetAttribute("amount"))));
            }
        }

        Job job = new Job(
            null,
            Type,
            null,
            jobTime,
            null,
            Job.JobPriority.High);
        job.JobDescription = "job_deconstruct_" + Type + "_desc";
        job.adjacent = true;

        PrototypeManager.FurnitureDeconstructJob.Set(job);
    }
    #endregion

    /// <summary>
    /// Accepts for storage.
    /// </summary>
    /// <returns>A list of RequestedItem which the Furniture accepts for storage.</returns>
    public RequestedItem[] AcceptsForStorage()
    {
        if (HasTypeTag("Storage") == false)
        {
            Debug.ULogChannel("Stockpile_messages", "Someone is asking a non-stockpile to store stuff!?");
            return null;
        }

        // TODO: read this from furniture params
        Dictionary<string, RequestedItem> itemsDict = new Dictionary<string, RequestedItem>();
        foreach (InventoryCommon inventoryProto in PrototypeManager.Inventory.Values)
        {
            itemsDict[inventoryProto.type] = new RequestedItem(inventoryProto.type, 1, inventoryProto.maxStackSize);
        }

        return itemsDict.Values.ToArray();
    }

    #region Deconstruct
    /// <summary>
    /// Sets the furniture to be deconstructed.
    /// </summary>
    public void SetDeconstructJob()
    {
        if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
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

        Job job = PrototypeManager.FurnitureDeconstructJob.Get(Type).Clone();
        job.tile = Tile;
        job.OnJobCompleted += (inJob) => Deconstruct();

        World.Current.jobQueue.Enqueue(job);
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

        // Update thermalDiffusifity to default value
        World.Current.temperature.SetThermalDiffusivity(Tile.X, Tile.Y, Tile.Z, Temperature.defaultThermalDiffusivity);

        // Let our workspot tile know it is no longer reserved for us
        World.Current.UnreserveTileAsWorkSpot(this);

        Tile.UnplaceFurniture();

        if (deconstructInventory != null)
        {
            foreach (Inventory inv in deconstructInventory)
            {
                inv.MaxStackSize = PrototypeManager.Inventory.Get(inv.Type).maxStackSize;
                World.Current.InventoryManager.PlaceInventoryAround(Tile, inv.Clone());
            }
        }

        if (PowerConnection != null)
        {
            World.Current.PowerNetwork.Unplug(PowerConnection);
            PowerConnection.NewThresholdReached -= OnNewThresholdReached;
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
            string desc = component.GetDescription();
            if (!string.IsNullOrEmpty(desc))
            {
                yield return desc;
            }
        }
        
        yield return string.Format("Hitpoint 18 / 18");

        if (PowerConnection != null)
        {
            bool hasPower = DoesntNeedOrHasPower;
            string powerColor = hasPower ? "green" : "red";

            yield return string.Format("Power Grid: <color={0}>{1}</color>", powerColor, hasPower ? "Online" : "Offline");

            if (PowerConnection.IsPowerConsumer)
            {
                yield return string.Format("Power Input: <color={0}>{1}</color>", powerColor, PowerConnection.InputRate);
            }

            if (PowerConnection.IsPowerProducer)
            {
                yield return string.Format("Power Output: <color={0}>{1}</color>", powerColor, PowerConnection.OutputRate);
            }

            if (PowerConnection.IsPowerAccumulator)
            {
                yield return string.Format("Power Accumulated: {0} / {1}", PowerConnection.AccumulatedPower, PowerConnection.Capacity);
            }
        }

        yield return GetProgressInfo();
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
        if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false) == true || HasTypeTag("Non-deconstructible") == false)
        {
            yield return new ContextMenuAction
            {
                Text = "Deconstruct " + Name,
                RequireCharacterSelected = false,
                Action = (ca, c) => SetDeconstructJob()
            };
        }

        for (int i = 0; i < Jobs.Count; i++)
        {
            if (!Jobs[i].IsBeingWorked)
            {
                yield return new ContextMenuAction
                {
                    Text = "Prioritize " + Name,
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
                Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
            {
                // TODO The Action could be done via a lambda, but it always uses the same space of memory, thus if 2 actions are performed, the same action will be produced for each.
                yield return new ContextMenuAction
                {
                    Text = contextMenuLuaAction.Text,
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
        bool tooCloseToEdge = tile.X < MinEdgeDistance || tile.Y < MinEdgeDistance ||
                              World.Current.Width - tile.X <= MinEdgeDistance ||
                              World.Current.Height - tile.Y <= MinEdgeDistance;

        if (tooCloseToEdge)
        {
            return false;
        }

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

                // Make sure tile is FLOOR
                if (tile2.Type != TileType.Floor && tileTypeBuildPermissions.Contains(tile2.Type.Type) == false)
                {
                    return false;
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
