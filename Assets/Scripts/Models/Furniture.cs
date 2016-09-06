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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectable, IContextActionProvider
{
    // Prevent construction too close to the world's edge
    private const int MinEdgeDistance = 5;

    // Base cost of pathfinding over this furniture, movement cost will modify the effective value
    private float pathfindingWeight = 1f;

    // Additional cost of pathfinding over this furniture, will be added to pathfindingWeight * MovementCost
    private float pathfindingModifier = 0f;

    // If the job causes some kind of object to be spawned, where will it appear?
    private Vector2 jobSpawnSpotOffset = Vector2.zero;

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
    /// Custom parameter for this particular piece of furniture.  We are
    /// using a custom Parameter class because later, custom LUA function will be
    /// able to use whatever parameters the user/modder would like, and contain strings or floats.
    /// Basically, the LUA code will bind to this Parameter.
    /// </summary>
    private Parameter furnParameters;

    private List<Job> jobs;

    private List<Job> pausedJobs;

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private Func<Tile, bool> funcPositionValidation;

    private HashSet<TileType> tileTypeBuildPermissions;

    private bool isOperating;

    /// TODO: Implement object rotation
    /// <summary>
    /// Initializes a new instance of the <see cref="Furniture"/> class.
    /// </summary>
    public Furniture()
    {
        Tint = Color.white;
        JobSpotOffset = Vector2.zero;
        VerticalDoor = false;
        EventActions = new EventActions();

        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        furnParameters = new Parameter();
        jobs = new List<Job>();
        typeTags = new HashSet<string>();
        funcPositionValidation = DefaultIsValidPosition;
        tileTypeBuildPermissions = new HashSet<TileType>();
        Height = 1;
        Width = 1;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
    private Furniture(Furniture other)
    {
        ObjectType = other.ObjectType;
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

        JobSpotOffset = other.JobSpotOffset;
        jobSpawnSpotOffset = other.jobSpawnSpotOffset;

        furnParameters = new Parameter(other.furnParameters);
        jobs = new List<Job>();
        pausedJobs = new List<Job>();

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

        tileTypeBuildPermissions = other.tileTypeBuildPermissions;

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
    public float PathfindingModifier
    {
        get { return pathfindingModifier; }
        set { pathfindingModifier = value; }
    }

    /// <summary>
    /// Gets or sets the Furniture's pathfinding weight which is multiplied into the Tile's final PathfindingCost.
    /// </summary>
    /// <value>The pathfinding weight for the tiles the furniture currently occupies.</value>
    public float PathfindingWeight
    {
        get { return pathfindingWeight; }
        set { pathfindingWeight = value; }
    }
    
    /// <summary>
    /// Gets the tint used to change the color of the furniture.
    /// </summary>
    /// <value>The Color of the furniture.</value>
    public Color Tint { get; private set; }

    /// <summary>
    /// Gets the spot where the Character will stand when he is using the furniture. This is relative to the bottom
    /// left tile of the sprite. This can be outside of the actual furniture.
    /// </summary>
    /// <value>The spot where the Character will stand when he uses the furniture.</value>
    public Vector2 JobSpotOffset { get; private set; }

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
    public string ObjectType { get; private set; }

    /// <summary>
    /// Gets the name of the furniture. The name is the object type by default.
    /// </summary>
    /// <value>The name of the furniture.</value>
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(name))
            {
                return ObjectType;
            }

            return name;
        }

        private set
        {
            name = value;
        }
    }

    /// <summary>
    /// Gets a list of furniture ObjectType this furniture can be replaced with.
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
    public bool LinksToNeighbour { get; private set; }

    /// <summary>
    /// Gets the type of dragging that is used to build multiples of this furniture. 
    /// e.g walls.
    /// </summary>
    public string DragType { get; private set; }

    /// <summary>
    /// Gets. or sets the parameters that is tied to the furniture.
    /// </summary>
    public Parameter Parameters
    {
        get
        {
            return furnParameters;
        }

        private set
        {
            furnParameters = value;
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

        if (tile.PlaceFurniture(obj) == false)
        {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        if (obj.LinksToNeighbour)
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

        // Call LUA install scripts
        obj.EventActions.Trigger("OnInstall", obj);

        // Update thermalDiffusifity using coefficient
        float thermalDiffusivity = Temperature.defaultThermalDiffusivity;
        if (obj.furnParameters.ContainsKey("thermal_diffusivity"))
        {
            thermalDiffusivity = obj.furnParameters["thermal_diffusivity"].ToFloat();
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
            if (JobCount() > 0)
            {
                PauseJobs();
            }

            return;
        }

        if (pausedJobs.Count > 0)
        {
            ResumeJobs();
        }

        // TODO: some weird thing happens
        if (EventActions != null)
        {
            // updateActions(this, deltaTime);
            EventActions.Trigger("OnUpdate", this, deltaTime);
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

        //// FurnitureActions.CallFunctionsWithFurniture( isEnterableActions.ToArray(), this );

        DynValue ret = LuaUtilities.CallFunction(isEnterableAction, this);

        return (Enterability)ret.Number;
    }

    /// <summary>
    /// Check if the furniture has a function to determine the sprite name and calls that function.
    /// </summary>
    /// <returns>Name of the sprite.</returns>
    public string GetSpriteName()
    {
        if (string.IsNullOrEmpty(getSpriteNameAction))
        {
            return ObjectType;
        }

        DynValue ret = LuaUtilities.CallFunction(getSpriteNameAction, this);
        return ret.String;
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
        writer.WriteAttributeString("objectType", ObjectType);

        // Let the Parameters handle their own xml
        furnParameters.WriteXml(writer);
    }

    /// <summary>
    /// Reads the prototype furniture from XML.
    /// </summary>
    /// <param name="readerParent">The XML reader to read from.</param>
    public void ReadXmlPrototype(XmlReader readerParent)
    {
        ObjectType = readerParent.GetAttribute("objectType");

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
                    LinksToNeighbour = reader.ReadContentAsBoolean();
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
                    float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                    List<Inventory> invs = new List<Inventory>();

                    XmlReader inventoryReader = reader.ReadSubtree();

                    while (inventoryReader.Read())
                    {
                        if (inventoryReader.Name == "Inventory")
                        {
                            // Found an inventory requirement, so add it to the list!
                            invs.Add(new Inventory(
                                    inventoryReader.GetAttribute("objectType"),
                                    int.Parse(inventoryReader.GetAttribute("amount")),
                                    0));
                        }
                    }

                    Job j = new Job(
                                null,
                                ObjectType,
                                FurnitureActions.JobComplete_FurnitureBuilding,
                                jobTime,
                                invs.ToArray(),
                                Job.JobPriority.High);
                    j.JobDescription = "job_build_" + ObjectType + "_desc";
                    PrototypeManager.FurnitureJob.Set(ObjectType, j);
                    break;

                case "CanBeBuiltOn":
                    TileType tileType = TileType.GetTileType(reader.GetAttribute("tileType"));
                    tileTypeBuildPermissions.Add(tileType);
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
                        RequiereCharacterSelected = bool.Parse(reader.GetAttribute("RequiereCharacterSelected")),
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
                    JobSpotOffset = new Vector2(
                        int.Parse(reader.GetAttribute("X")),
                        int.Parse(reader.GetAttribute("Y")));
                    break;
                case "JobSpawnSpotOffset":
                    jobSpawnSpotOffset = new Vector2(
                        int.Parse(reader.GetAttribute("X")),
                        int.Parse(reader.GetAttribute("Y")));
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
        // X, Y, and objectType have already been set, and we should already
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
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.
        furnParameters = Parameter.ReadXml(reader);
    }

    /// <summary>
    /// Gets the furniture's Parameter structure from a string key.
    /// </summary>
    /// <returns>The Parameter value..</returns>
    public Parameter GetParameters()
    {
        return furnParameters;
    }

    /// <summary>
    /// How many jobs are linked to this furniture.
    /// </summary>
    /// <returns>The number of jobs linked to this furniture.</returns>
    public int JobCount()
    {
        return jobs.Count;
    }

    /// <summary>
    /// Link a job to the current furniture.
    /// </summary>
    /// <param name="job">The job that you want to link to the furniture.</param>
    public void AddJob(Job job)
    {
        job.furniture = this;
        jobs.Add(job);
        job.OnJobStopped += OnJobStopped;
        World.Current.jobQueue.Enqueue(job);
    }

    /// <summary>
    /// Cancel all the jobs linked to this furniture.
    /// </summary>
    public void CancelJobs()
    {
        Job[] jobsArray = jobs.ToArray();
        foreach (Job job in jobsArray)
        {
            job.CancelJob();
        }
    }

    /// TODO: Refactor this when the new job system is implemented
    public void ResumeJobs()
    {
        Job[] jobsArray = pausedJobs.ToArray();
        foreach (Job job in jobsArray)
        {
            AddJob(job);
            pausedJobs.Remove(job);
        }
    }

    /// TODO: Refactor this when the new job system is implemented
    public void PauseJobs()
    {
        Job[] jobsArray = jobs.ToArray();
        foreach (Job job in jobsArray)
        {
            pausedJobs.Add(job);
            job.CancelJob();
        }
    }

    public bool IsStockpile()
    {
        return HasTypeTag("Storage");
    }

    /// <summary>
    /// Accepts for storage.
    /// </summary>
    /// <returns>A list of Inventory which the Furniture accepts for storage.</returns>
    public Inventory[] AcceptsForStorage()
    {
        if (IsStockpile() == false)
        {
            Debug.ULogChannel("Stockpile_messages", "Someone is asking a non-stockpile to store stuff!?");
            return null;
        }

        // TODO: read this from furniture params
        Dictionary<string, Inventory> invsDict = new Dictionary<string, Inventory>();
        foreach (string objectType in PrototypeManager.Inventory.Keys)
        {
            invsDict[objectType] = new Inventory(objectType, PrototypeManager.Inventory.Get(objectType).maxStackSize, 0);
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
        bool linksToNeighbour = false;
        if (Tile.Furniture != null)
        {
            Furniture furniture = Tile.Furniture;
            fwidth = furniture.Width;
            fheight = furniture.Height;
            linksToNeighbour = furniture.LinksToNeighbour;
            furniture.CancelJobs();
        }

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);

        // Update thermalDiffusifity to default value
        World.Current.temperature.SetThermalDiffusivity(Tile.X, Tile.Y, Temperature.defaultThermalDiffusivity);

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
            Room.DoRoomFloodFill(Tile);
        }

        ////World.current.InvalidateTileGraph();

        if (World.Current.tileGraph != null)
        {
            World.Current.tileGraph.RegenerateGraphAtTile(Tile);
        }

        // We should inform our neighbours that they have just lost a
        // neighbour regardless of objectType.  
        // Just trigger their OnChangedCallback. 
        if (linksToNeighbour == true)
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
    /// Gets the tile that is used to do a job.
    /// </summary>
    /// <returns>Tile that is used for jobs.</returns>
    public Tile GetJobSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)JobSpotOffset.x, Tile.Y + (int)JobSpotOffset.y, Tile.Z);
    }

    /// <summary>
    /// Gets the tile that is used to spawn new objects (i.e. Inventory, Character).
    /// </summary>
    /// <returns>Tile that is used to spawn objects (i.e. Inventory, Character).</returns>
    public Tile GetSpawnSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)jobSpawnSpotOffset.x, Tile.Y + (int)jobSpawnSpotOffset.y, Tile.Z);
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
    /// Returns the HitPoints of the current furniture NOT IMPLEMENTED.
    /// </summary>
    /// <returns>String with the HitPoints of the furniture.</returns>
    public string GetHitPointString()
    {
        return "18/18"; // TODO: Add a hitpoint system to...well...everything
    }

    /// <summary>
    /// Returns the description of the job linked to the furniture. NOT INMPLEMENTED.
    /// </summary>
    /// <returns>Job description of the job linked to the furniture.</returns>
    public string GetJobDescription()
    {
        return string.Empty;
    }

    /// <summary>
    /// Gets the Context Menu Actions.
    /// </summary>
    /// <param name="contextMenu">The context menu to check for actions.</param>
    /// <returns>Context menu actions.</returns>
    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Deconstruct " + Name,
            RequireCharacterSelected = false,
            Action = (ca, c) => Deconstruct()
        };
        if (jobs.Count > 0)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                if (!jobs[i].IsBeingWorked)
                {
                    yield return new ContextMenuAction
                    {
                        Text = "Prioritize " + Name,
                        RequireCharacterSelected = true,
                        Action = (ca, c) =>
                        {
                            c.PrioritizeJob(jobs[0]);
                        }
                    };
                }
            }
        }

        foreach (ContextMenuLuaAction contextMenuLuaAction in contextMenuLuaActions)
        {
            if (!contextMenuLuaAction.DevModeOnly ||
                Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
            {
                yield return new ContextMenuAction
                {
                    Text = contextMenuLuaAction.Text,
                    RequireCharacterSelected = contextMenuLuaAction.RequiereCharacterSelected,
                    Action = (cma, c) => InvokeContextMenuLuaAction(contextMenuLuaAction.LuaFunction, c)
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
                Tile t2 = World.Current.GetTileAt(x_off, y_off, tile.Z);

                // Check to see if there is furniture which is replaceable
                bool isReplaceable = false;

                if (t2.Furniture != null)
                {
                    // Furniture can be replaced, if its typeTags share elements with ReplaceableFurniture
                    isReplaceable = t2.Furniture.typeTags.Overlaps(ReplaceableFurniture);
                }

                // Make sure tile is FLOOR
                if (t2.Type != TileType.Floor && tileTypeBuildPermissions.Contains(t2.Type) == false)
                {
                    return false;
                }

                // Make sure tile doesn't already have furniture
                if (t2.Furniture != null && isReplaceable == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void RemoveJob(Job j)
    {
        j.OnJobStopped -= OnJobStopped;
        jobs.Remove(j);
        j.furniture = null;
    }

    private void ClearJobs()
    {
        Job[] jobsArray = jobs.ToArray();
        foreach (Job j in jobsArray)
        {
            RemoveJob(j);
        }
    }

    private void InvokeContextMenuLuaAction(string luaFunction, Character character)
    {
        LuaUtilities.CallFunction(luaFunction, this, character);
    }

    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Furniture furn)
    {
        if (Changed != null)
        {
            Changed(furn);
        }
    }

    private void OnJobStopped(Job j)
    {
        RemoveJob(j);
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
}
