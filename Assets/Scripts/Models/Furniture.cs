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
using Power;
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

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private Func<Tile, bool> funcPositionValidation;

    private HashSet<TileType> tileTypeBuildPermissions;

    private bool isOperating;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Empty constructor is used for serialization
    public Furniture()
    {
        Tint = Color.white;
        JobSpotOffset = Vector2.zero;
        VerticalDoor = false;
        EventActions = new EventAction();

        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        furnParameters = new Parameter("furnParameters");
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

        PowerConnection = other.PowerConnection;

        if (PowerConnection != null)
        {
            World.Current.PowerSystem.PlugIn(PowerConnection);
        }

        if (other.funcPositionValidation != null)
        {
            funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
        }

        tileTypeBuildPermissions = other.tileTypeBuildPermissions;

        LocalizationCode = other.LocalizationCode;
        UnlocalizedDescription = other.UnlocalizedDescription;
    }

    public event Action<Furniture> Changed;

    public event Action<Furniture> Removed;

    public event Action<Furniture> IsOperatingChanged;

    /// <summary>
    /// Gets or sets the Furniture's pathfinding modifier which is added into the Tile's final PathfindingCost.
    /// </summary>
    public float PathfindingModifier
    {
        get { return pathfindingWeight; }
        set { pathfindingWeight = value; }
    }

    /// <summary>
    /// Gets or sets the Furniture's pathfinding weight which is multiplied into the Tile's final PathfindingCost.
    /// </summary>
    public float PathfindingWeight
    {
        get { return pathfindingWeight; }
        set { pathfindingWeight = value; }
    }

    public Color Tint { get; private set; }

    // If this furniture gets worked by a person,
    // where is the correct spot for them to stand,
    // relative to the bottom-left tile of the sprite.
    // NOTE: This could even be something outside of the actual
    // furniture tile itself!  (In fact, this will probably be common).
    public Vector2 JobSpotOffset { get; private set; }

    // Flag for Lua to check if this is a vertical or horizontal door and display the correct sprite.
    public bool VerticalDoor { get; set; }

    /// <summary>
    /// These actions are called when Trigger is called. They get passed the furniture
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    public EventAction EventActions { get; private set; }

    /// <summary>
    /// Connection to power system.
    /// </summary>
    public Connection PowerConnection { get; private set; }

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

    public bool IsSelected { get; set; }

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile Tile { get; private set; }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string ObjectType { get; private set; }

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

    public List<string> ReplaceableFurniture
    {
        get
        {
            return replaceableFurniture;
        }
    }

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float MovementCost { get; private set; }

    public bool RoomEnclosure { get; private set; }

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; private set; }

    public int Height { get; private set; }

    public string LocalizationCode { get; private set; }

    public string UnlocalizedDescription { get; private set; }

    public bool LinksToNeighbour { get; private set; }

    public string DragType { get; private set; }

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

    public static Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.ULogErrorChannel("Furniture", "PlaceInstance -- Position Validity Function returned FALSE.");
            return null;
        }

        // We know our placement destination is valid.
        Furniture obj = proto.Clone();
        obj.Tile = tile;

        // FIXME: This assumes we are 1x1!
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
                    Tile tileAt = World.Current.GetTileAt(xpos, ypos);
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

    public void Update(float deltaTime)
    {
        // TODO: some weird thing happens
        if (EventActions != null)
        {
            // updateActions(this, deltaTime);
            EventActions.Trigger("OnUpdate", this, deltaTime);
        }
    }

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

    public string GetSpriteName()
    {
        if (string.IsNullOrEmpty(getSpriteNameAction))
        {
            return ObjectType;
        }

        DynValue ret = LuaUtilities.CallFunction(getSpriteNameAction, this);
        return ret.String;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    public bool HasPower()
    {
        IsOperating = PowerConnection == null || World.Current.PowerSystem.HasPower(PowerConnection);
        return IsOperating;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Y", Tile.Y.ToString());
        writer.WriteAttributeString("objectType", ObjectType);

        // Let the Parameters handle their own xml
        furnParameters.WriteXml(writer);
    }

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
                    PrototypeManager.FurnitureJob.SetPrototype(ObjectType, j);
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
                    RequiereCharacterSelected = bool.Parse(reader.GetAttribute("RequiereCharacterSelected"))
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

    public void ReadXml(XmlReader reader)
    {
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.
        ReadXmlParams(reader);
    }

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

    public int JobCount()
    {
        return jobs.Count;
    }

    public void AddJob(Job job)
    {
        job.furniture = this;
        jobs.Add(job);
        job.OnJobStopped += OnJobStopped;
        World.Current.jobQueue.Enqueue(job);
    }

    public void CancelJobs()
    {
        Job[] jobsArray = jobs.ToArray();
        foreach (Job job in jobsArray)
        {
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
            invsDict[objectType] = new Inventory(objectType, PrototypeManager.Inventory.GetPrototype(objectType).maxStackSize, 0);
        }

        Inventory[] invs = new Inventory[invsDict.Count];
        invsDict.Values.CopyTo(invs, 0);
        return invs;
    }

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
            World.Current.PowerSystem.Unplug(PowerConnection);
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
                    Tile t = World.Current.GetTileAt(xpos, ypos);
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

    public Tile GetJobSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)JobSpotOffset.x, Tile.Y + (int)JobSpotOffset.y);
    }

    public Tile GetSpawnSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)jobSpawnSpotOffset.x, Tile.Y + (int)jobSpawnSpotOffset.y);
    }

    // Returns true if furniture has typeTag, though simple, the intent is to separate the interaction with
    //  the Furniture's typeTags from the implementation.
    public bool HasTypeTag(string typeTag)
    {
        return typeTags.Contains(typeTag);
    }

    public string GetName()
    {
        return LocalizationCode; // this.Name;
    }

    public string GetDescription()
    {
        return UnlocalizedDescription;
    }

    public string GetHitPointString()
    {
        return "18/18"; // TODO: Add a hitpoint system to...well...everything
    }

    public string GetJobDescription()
    {
        return string.Empty;
    }

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Deconstruct " + Name,
            RequireCharacterSelected = false,
            Action = (ca, c) => Deconstruct()
        };
        if (jobs.Count > 0 && !jobs[0].IsBeingWorked)
        {
            yield return new ContextMenuAction
            {
                Text = "Prioritize " + Name,
                RequireCharacterSelected = true,
                Action = (ca, c) => { c.PrioritizeJob(jobs[0]); }
            };
        }

        foreach (var contextMenuLuaAction in contextMenuLuaActions)
        {
            yield return new ContextMenuAction
            {
                Text = contextMenuLuaAction.Text,
                RequireCharacterSelected = contextMenuLuaAction.RequiereCharacterSelected,
                Action = (cma, c) => InvokeContextMenuLuaAction(contextMenuLuaAction.LuaFunction, c)
            };
        }
    }

    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    private Furniture Clone()
    {
        return new Furniture(this);
    }

    // FIXME: These functions should never be called directly,
    // so they probably shouldn't be public functions of Furniture
    // This will be replaced by validation checks fed to use from
    // LUA files that will be customizable for each piece of furniture.
    // For example, a door might specific that it needs two walls to
    // connect to.
    private bool DefaultIsValidPosition(Tile t)
    {
        bool tooCloseToEdge = t.X < MinEdgeDistance || t.Y < MinEdgeDistance ||
            World.Current.Width - t.X <= MinEdgeDistance ||
            World.Current.Height - t.Y <= MinEdgeDistance;

        if (tooCloseToEdge)
        {
            return false;
        }

        if (HasTypeTag("OutdoorOnly"))
        {
            if (t.Room == null || !t.Room.IsOutsideRoom())
            {
                return false;
            }
        }

        for (int x_off = t.X; x_off < t.X + Width; x_off++)
        {
            for (int y_off = t.Y; y_off < t.Y + Height; y_off++)
            {
                Tile t2 = World.Current.GetTileAt(x_off, y_off);

                // Check to see if there is furniture which is replaceable
                bool isReplaceable = false;

                if (t2.Furniture != null)
                {
                    for (int i = 0; i < ReplaceableFurniture.Count; i++)
                    {
                        if (t2.Furniture.HasTypeTag(ReplaceableFurniture[i]))
                        {
                            isReplaceable = true;
                        }
                    }
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
}
