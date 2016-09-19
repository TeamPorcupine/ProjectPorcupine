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
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectable, IPrototypable, IContextActionProvider, IBuildable
{
    // Prevent construction too close to the world's edge
    private const int MinEdgeDistance = 5;

    private string isEnterableAction;

    /// <summary>
    /// This action is called to get the sprite name based on the furniture parameters.
    /// </summary>
    private string getSpriteNameAction;

    private List<string> replaceableFurniture = new List<string>();

    /// <summary>
    /// These context menu lua action are used to build the context menu of the furniture.
    /// </summary>
    private List<ContextMenuLuaAction> contextMenuLuaActions;

    /// <summary>
    /// Workshop reference if furniture is consumer/producer (not null). 
    /// </summary>
    private FurnitureWorkshop workshop;

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private Func<Tile, bool> funcPositionValidation;

    private HashSet<string> tileTypeBuildPermissions;

    private bool isOperating;

    // did we have power in the last update?
    private bool prevUpdatePowerOn;

    /// TODO: Implement object rotation
    /// <summary>
    /// Initializes a new instance of the <see cref="Furniture"/> class.
    /// </summary>
    public Furniture()
    {
        Tint = Color.white;
        VerticalDoor = false;
        EventActions = new EventActions();
        
        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        Parameters = new Parameter();
        Jobs = new FurnitureJobs(this);
        typeTags = new HashSet<string>();
        funcPositionValidation = DefaultIsValidPosition;
        tileTypeBuildPermissions = new HashSet<string>();
        PathfindingWeight = 1f;
        PathfindingModifier = 0f;
        Height = 1;
        Width = 1;
        DragType = "single";
        LinksToNeighbour = string.Empty;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
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
        Tint = other.Tint;
        LinksToNeighbour = other.LinksToNeighbour;

        Parameters = new Parameter(other.Parameters);
        Jobs = new FurnitureJobs(this, other);
        workshop = other.workshop; // don't need to clone here, as all are prototype things (not changing)

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

        if (other.PowerConnection != null)
        {
            PowerConnection = other.PowerConnection.Clone() as Connection;
            World.Current.PowerNetwork.PlugIn(PowerConnection);
            PowerConnection.NewThresholdReached += OnNewThresholdReached;
        }

        if (other.funcPositionValidation != null)
        {
            funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
        }

        tileTypeBuildPermissions = new HashSet<string>(other.tileTypeBuildPermissions);

        LocalizationCode = other.LocalizationCode;
        UnlocalizedDescription = other.UnlocalizedDescription;
    }

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
    public Color Tint { get; private set; }

    /// <summary>
    /// Returns whether this instance is workshop or not.
    /// </summary>
    /// <value><c>true</c> if this instance is workshop; otherwise, <c>false</c>.</value>
    public bool IsWorkshop
    {
        get { return workshop != null; }
    }

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
        get
        {
            return replaceableFurniture;
        }
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
    public FurnitureJobs Jobs { get; private set; }

    /// <summary>
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
    /// Used to place furniture in a certain position.
    /// </summary>
    /// <param name="proto">The prototype furniture to place.</param>
    /// <param name="tile">The base tile to place the furniture on, The tile will be the bottom left corner of the furniture (to check).</param>
    /// <returns>Furniture object.</returns>
    public static Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.ULogErrorChannel("Furniture", "PlaceInstance -- Position Validity Function returned FALSE. " + proto.Name + " " + tile.X + ", " + tile.Y + ", " + tile.Z);
            return null;
        }

        // We know our placement destination is valid.
        Furniture obj = proto.Clone();
        obj.Tile = tile;        
        if (obj.IsWorkshop)
        {
            // need to update reference to furniture for workshop (is there a nicer way?)
            obj.workshop.SetParentFurniture(obj);
        }

        if (tile.PlaceFurniture(obj) == false)
        {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }
        
        if (obj.LinksToNeighbour != string.Empty)
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
        World.Current.ReserveTileAsWorkSpot(obj);

        // Call LUA install scripts
        obj.EventActions.Trigger("OnInstall", obj);

        // Update thermalDiffusifity using coefficient
        float thermalDiffusivity = Temperature.defaultThermalDiffusivity;
        if (obj.Parameters.ContainsKey("thermal_diffusivity"))
        {
            thermalDiffusivity = obj.Parameters["thermal_diffusivity"].ToFloat();
        }

        World.Current.temperature.SetThermalDiffusivity(tile.X, tile.Y, thermalDiffusivity);

        return obj;
    }

    /// <summary>
    /// This function is called to update the furniture. This will also trigger EventsActions.
    /// This checks if the furniture is a PowerConsumer, and if it does not have power it cancels its job.
    /// </summary>
    /// <param name="deltaTime">The time since the last update was called.</param>
    public void Update(float deltaTime)
    {
        if (PowerConnection != null && PowerConnection.IsPowerConsumer && HasPower() == false)
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

        if (IsWorkshop)
        {
            workshop.Update(deltaTime);
        }

        if (Animation != null)
        {
            Animation.Update(deltaTime);
        }
    }
    
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

    /// <summary>
    /// Check if the position of the furniture is valid or not.
    /// This is called when placing the furniture.
    /// </summary>
    /// <param name="t">The base tile.</param>
    /// <returns>True if the tile is valid for the placement of the furniture.</returns>
    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    /// <summary>
    /// Whether the furniture has power or not.
    /// </summary>
    /// <returns>True if the furniture has power.</returns>
    public bool HasPower()
    {
        IsOperating = PowerConnection == null || World.Current.PowerNetwork.HasPower(PowerConnection);
        return IsOperating;
    }

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

        // Let the Parameters handle their own xml
        Parameters.WriteXml(writer);
    }

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
                case "DragType":
                    reader.Read();
                    DragType = reader.ReadContentAsString();
                    break;
                case "BuildingJob":
                    ReadXmlBuildingJob(reader);
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
                case "JobSpotOffset":
                    Jobs.ReadWorkSpotOffset(reader);
                    break;
                case "JobSpawnSpotOffset":
                    Jobs.ReadSpawnSpotOffset(reader);
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

                case "Workshop":                   
                    workshop = FurnitureWorkshop.Deserialize(reader);
                    workshop.SetParentFurniture(this);
                    workshop.Initialize();
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
        // X, Y, and type have already been set, and we should already
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
        // X, Y, and type have already been set, and we should already
        // be assigned to a tile.  So just read extra data.
        Parameters = Parameter.ReadXml(reader);
    }

    /// <summary>
    /// Reads the XML building job.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    public void ReadXmlBuildingJob(XmlReader reader)
    {
        float jobTime = float.Parse(reader.GetAttribute("jobTime"));
        List<Inventory> invs = new List<Inventory>();
        XmlReader inventoryReader = reader.ReadSubtree();

        while (inventoryReader.Read())
        {
            if (inventoryReader.Name == "Inventory")
            {
                // Found an inventory requirement, so add it to the list!
                invs.Add(new Inventory(
                    inventoryReader.GetAttribute("type"),
                    0,
                    int.Parse(inventoryReader.GetAttribute("amount"))));
            }
        }

        Job job = new Job(
            null,
            Type,
            FunctionsManager.JobComplete_FurnitureBuilding,
            jobTime,
            invs.ToArray(),
            Job.JobPriority.High);
        job.JobDescription = "job_build_" + Type + "_desc";

        PrototypeManager.FurnitureJob.Set(job);
    }

    /// <summary>
    /// Accepts for storage.
    /// </summary>
    /// <returns>A list of Inventory which the Furniture accepts for storage.</returns>
    public Inventory[] AcceptsForStorage()
    {
        if (HasTypeTag("Storage") == false)
        {
            Debug.ULogChannel("Stockpile_messages", "Someone is asking a non-stockpile to store stuff!?");
            return null;
        }

        // TODO: read this from furniture params
        Dictionary<string, Inventory> invsDict = new Dictionary<string, Inventory>();
        foreach (string type in PrototypeManager.Inventory.Keys)
        {
            invsDict[type] = new Inventory(type, 0);
        }

        Inventory[] invs = new Inventory[invsDict.Count];
        invsDict.Values.CopyTo(invs, 0);
        return invs;
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
        World.Current.temperature.SetThermalDiffusivity(Tile.X, Tile.Y, Temperature.defaultThermalDiffusivity);

        // Let our workspot tile know it is no longer reserved for us
        World.Current.UnreserveTileAsWorkSpot(this);

        Tile.UnplaceFurniture();

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

    public IEnumerable<string> GetAdditionalInfo()
    {
        if (IsWorkshop)
        {
            yield return workshop.GetDescription();
        }

        yield return string.Format("Hitpoint 18 / 18");

        if (PowerConnection != null)
        {
            bool hasPower = HasPower();
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
                Action = (ca, c) => Deconstruct()
            };
        }

        for (int i = 0; i < Jobs.Count(); i++)
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

        // context menu if it's workshop and has multiple production chains
        if (IsWorkshop && workshop.WorkshopMenuActions != null)
        {
            foreach (WorkshopContextMenu factoryContextMenuAction in workshop.WorkshopMenuActions)
            {
                yield return CreateWorkshopContextMenuItem(factoryContextMenuAction);
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
    
    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    public Furniture Clone()
    {
        return new Furniture(this);
    }

    // FIXME: These functions should never be called directly,
    // so they probably shouldn't be public functions of Furniture
    // This will be replaced by validation checks fed to use from
    // LUA files that will be customizable for each piece of furniture.
    // For example, a door might specific that it needs two walls to
    // connect to.
    private bool DefaultIsValidPosition(Tile tile)
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

    private ContextMenuAction CreateWorkshopContextMenuItem(WorkshopContextMenu factoryContextMenuAction)
    {
        return new ContextMenuAction
        {
            Text = factoryContextMenuAction.ProductionChainName, // TODO: localization here
            RequireCharacterSelected = false,
            Action = (cma, c) => InvokeContextMenuAction(factoryContextMenuAction.Function, factoryContextMenuAction.ProductionChainName)
        };
    }

    private void InvokeContextMenuAction(Action<Furniture, string> function, string arg)
    {
        function(this, arg);
    }

    private void InvokeContextMenuLuaAction(ContextMenuAction action, Character character)
    {
        FunctionsManager.Furniture.Call(action.Parameter, this, character);
    }

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
