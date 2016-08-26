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
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectable, IContextActionProvider, IPowerRelated
{
    private float powerValue;
    /// <summary>
    /// Custom parameter for this particular piece of furniture.  We are
    /// using a custom Parameter class because later, custom LUA function will be
    /// able to use whatever parameters the user/modder would like, and contain strings or floats.
    /// Basically, the LUA code will bind to this Parameter.
    /// </summary>
    protected Parameter furnParameters;

    /// <summary>
    /// These actions are called when Trigger is called. They get passed the furniture
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    public EventAction eventActions { set; get; }
    
    /// <summary>
    /// These context menu lua action are used to build the context menu of the furniture
    /// </summary>
    protected List<ContextMenuLuaAction> contextMenuLuaActions; 
    
    protected string isEnterableAction;

    /// <summary>
    /// This action is called to get the sprite name based on the furniture parameters
    /// </summary>
    protected string getSpriteNameAction;

    protected List<string> replaceableFurniture = new List<string>();

    private List<Job> jobs;

    // If this furniture gets worked by a person,
    // where is the correct spot for them to stand,
    // relative to the bottom-left tile of the sprite.
    // NOTE: This could even be something outside of the actual
    // furniture tile itself!  (In fact, this will probably be common).
    public Vector2 jobSpotOffset = Vector2.zero;

    // If the job causes some kind of object to be spawned, where will it appear?
    public Vector2 jobSpawnSpotOffset = Vector2.zero;

    public event Action<IPowerRelated> PowerValueChanged;

    public float PowerValue
    {
        get
        {
            return powerValue;
        }

        set
        {
            if (powerValue.AreEqual(value)) return;
            powerValue = value;
            InvokePowerValueChanged(this);
        }
    }

    public bool IsPowerConsumer
    {
        get
        {
            return PowerValue < 0.0f;
        }
    }

    //TODO: public PowerRelated PowerRelated { get; private set; }

    public bool IsSelected { get; set; }

    public void Update(float deltaTime)
    {
        // TODO: some weird thing happens
        if (eventActions != null)
        {

            // updateActions(this, deltaTime);
            eventActions.Trigger("OnUpdate", this, deltaTime);
        }
    }

    // Flag for Lua to check if this is a vertical or horizontal door and display the correct sprite.
    public bool verticalDoor = false;

    public ENTERABILITY IsEnterable()
    {
        if (isEnterableAction == null || isEnterableAction.Length == 0)
        {
            return ENTERABILITY.Yes;
        }

        //// FurnitureActions.CallFunctionsWithFurniture( isEnterableActions.ToArray(), this );

        DynValue ret = LuaUtilities.CallFunction(isEnterableAction, this);

        return (ENTERABILITY)ret.Number;
    }

    public string GetSpriteName()
    {
        if (getSpriteNameAction == null || getSpriteNameAction.Length == 0)
        {
            return objectType;
        }

        DynValue ret = LuaUtilities.CallFunction(getSpriteNameAction, this);
        return ret.String;
    }

    // If this furniture generates power then powerValue will be positive, if it consumer power then it will be negative

    private void InvokePowerValueChanged(IPowerRelated powerRelated)
    {
        Action<IPowerRelated> handler = PowerValueChanged;
        if (handler != null)
        {
            handler(powerRelated);
        }
    }

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile tile
    {
        get;
        protected set;
    }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string objectType
    {
        get;
        protected set;
    }

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string _Name = null;

    public string Name
    {
        get
        {
            if (_Name == null || _Name.Length == 0)
            {
                return objectType;
            }

            return _Name;
        }

        set
        {
            _Name = value;
        }
    }

    private string Description = "";

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
    public float movementCost { get; protected set; }

    public bool roomEnclosure { get; protected set; }

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }

    public int Height { get; protected set; }

    public string localizationCode { get; protected set; }

    public string unlocalizedDescription { get; protected set; }

    public Color tint = Color.white;

    public bool linksToNeighbour
    {
        get;
        protected set;
    }

    public string dragType
    {
        get;
        protected set;
    }

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

    public event Action<Furniture> cbOnChanged;

    public event Action<Furniture> cbOnRemoved;

    private Func<Tile, bool> funcPositionValidation;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Empty constructor is used for serialization
    public Furniture()
    {
        eventActions = new EventAction();
        
        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        furnParameters = new Parameter("furnParameters");
        jobs = new List<Job>();
        typeTags = new HashSet<string>();
        this.funcPositionValidation = this.DEFAULT__IsValidPosition;
        this.Height = 1;
        this.Width = 1;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
    protected Furniture(Furniture other)
    {
        this.objectType = other.objectType;
        this.Name = other.Name;
        this.typeTags = new HashSet<string>(other.typeTags);
        this.Description = other.Description;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;

        this.jobSpotOffset = other.jobSpotOffset;
        this.jobSpawnSpotOffset = other.jobSpawnSpotOffset;

        this.furnParameters = new Parameter(other.furnParameters);
        jobs = new List<Job>();

        if (other.eventActions != null)
        {
            this.eventActions = other.eventActions.Clone();
        }

        if (other.contextMenuLuaActions != null)
        {
            this.contextMenuLuaActions = new List<ContextMenuLuaAction>(other.contextMenuLuaActions);
        }

        this.isEnterableAction = other.isEnterableAction;
        this.getSpriteNameAction = other.getSpriteNameAction;

        this.powerValue = other.powerValue;

        if (!powerValue.IsZero())
        {
            World.current.powerSystem.AddToPowerGrid(this);
        }

        if (other.funcPositionValidation != null)
        {
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
        }

        this.localizationCode = other.localizationCode;
        this.unlocalizedDescription = other.unlocalizedDescription;
    }

    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    public virtual Furniture Clone()
    {
        return new Furniture(this);
    }

    static public Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
            return null;
        }

        // We know our placement destination is valid.
        Furniture obj = proto.Clone();
        obj.tile = tile;

        // FIXME: This assumes we are 1x1!
        if (tile.PlaceFurniture(obj) == false)
        {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        if (obj.linksToNeighbour)
        {
            // This type of furniture links itself to its neighbours,
            // so we should inform our neighbours that they have a new
            // buddy.  Just trigger their OnChangedCallback.

            Tile t;
            int x = tile.X;
            int y = tile.Y;

            for (int xpos = x - 1; xpos < (x + proto.Width + 1); xpos++)
            {
                for (int ypos = y - 1; ypos < (y + proto.Height + 1); ypos++)
                {
                    t = World.current.GetTileAt(xpos, ypos);
                    if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null)
                    {
                        t.Furniture.cbOnChanged(t.Furniture);
                    }
                }
            }
        }

        // Call LUA install scripts
        obj.eventActions.Trigger("OnInstall", obj);

        // Update thermalDiffusifity using coefficient
        float thermalDiffusivity = Temperature.defaultThermalDiffusivity;
        if(obj.furnParameters.ContainsKey("thermal_diffusivity"))
        {
            thermalDiffusivity = obj.furnParameters["thermal_diffusivity"].ToFloat();
        }

        World.current.temperature.SetThermalDiffusivity(tile.X, tile.Y, thermalDiffusivity);

        return obj;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    // FIXME: These functions should never be called directly,
    // so they probably shouldn't be public functions of Furniture
    // This will be replaced by validation checks fed to use from
    // LUA files that will be customizable for each piece of furniture.
    // For example, a door might specific that it needs two walls to
    // connect to.
    protected bool DEFAULT__IsValidPosition(Tile t)
    {
        // Prevent construction too close to the world's edge
        const int minEdgeDistance = 5;
        bool tooCloseToEdge = t.X < minEdgeDistance || t.Y < minEdgeDistance ||
            (World.current.Width - t.X) <= minEdgeDistance ||
            (World.current.Height - t.Y) <= minEdgeDistance;

        if (tooCloseToEdge)
        {
            return false;
        }

        if (HasTypeTag("OutdoorOnly"))
        {
            if (t.Room == null || !t.Room.IsOutsideRoom())
                return false;
        }

        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + Height); y_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, y_off);

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
                if (t2.Type != TileType.Floor)
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

    public bool HasPower()
    {
        if (World.current.powerSystem.RequestPower(this))
        {
            return true;
        }

        return World.current.powerSystem.AddToPowerGrid(this);
    }

    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Furniture furn)
    {
        if (cbOnChanged != null)
        {
            cbOnChanged(furn);
        }
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        // Let the Parameters handle their own xml
        furnParameters.WriteXml(writer);
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        ////Debug.Log("ReadXmlPrototype");

        objectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();

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
                Description = reader.ReadContentAsString();
                break;
            case "MovementCost":
                reader.Read();
                movementCost = reader.ReadContentAsFloat();
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
                linksToNeighbour = reader.ReadContentAsBoolean();
                break;
            case "EnclosesRooms":
                reader.Read();
                roomEnclosure = reader.ReadContentAsBoolean();
                break;
            case "CanReplaceFurniture":
                replaceableFurniture.Add(reader.GetAttribute("typeTag").ToString());
                break;
            case "DragType":
                reader.Read();
                dragType = reader.ReadContentAsString();
                break;
            case "BuildingJob":
                float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                List<Inventory> invs = new List<Inventory>();

                XmlReader invs_reader = reader.ReadSubtree();

                while (invs_reader.Read())
                {
                    if (invs_reader.Name == "Inventory")
                    {
                        // Found an inventory requirement, so add it to the list!
                        invs.Add(new Inventory(
                            invs_reader.GetAttribute("objectType"),
                            int.Parse(invs_reader.GetAttribute("amount")),
                            0));
                    }
                }

                Job j = new Job(
                    null,
                    objectType,
                    FurnitureActions.JobComplete_FurnitureBuilding,
                    jobTime,
                    invs.ToArray(),
                    Job.JobPriority.High );
                j.JobDescription = "job_build_" + objectType + "_desc";
                World.current.SetFurnitureJobPrototype(j, this);
                break;

            case "Action":
                XmlReader subtree = reader.ReadSubtree();
                eventActions.ReadXml(subtree);
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
                jobSpotOffset = new Vector2(
                    int.Parse(reader.GetAttribute("X")),
                    int.Parse(reader.GetAttribute("Y")));
                break;
            case "JobSpawnSpotOffset":
                jobSpawnSpotOffset = new Vector2(
                    int.Parse(reader.GetAttribute("X")),
                    int.Parse(reader.GetAttribute("Y")));
                break;

            case "Power":
                reader.Read();
                powerValue = reader.ReadContentAsFloat();
                //TODO: PowerRelated = new PowerRelated();
                //TODO: PowerRelated.ReadPrototype(reader);
                break;

            case "Params":
                ReadXmlParams(reader);  // Read in the Param tag
                break;

            case "LocalizationCode":
                reader.Read();
                localizationCode = reader.ReadContentAsString();
                break;

            case "UnlocalizedDescription":
                reader.Read();
                unlocalizedDescription = reader.ReadContentAsString();
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
    /// <param name="key">Key string.</param>
    public Parameter GetParameters() {
        return furnParameters;
    }

    public int JobCount()
    {
        return jobs.Count;
    }

    public void AddJob(Job j)
    {
        j.furniture = this;
        jobs.Add(j);
        j.cbJobStopped += OnJobStopped;
        World.current.jobQueue.Enqueue(j);
    }

    void OnJobStopped(Job j)
    {
        RemoveJob(j);
    }

    protected void RemoveJob(Job j)
    {
        j.cbJobStopped -= OnJobStopped;
        jobs.Remove(j);
        j.furniture = null;
    }

    protected void ClearJobs()
    {
        Job[] jobs_array = jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            RemoveJob(j);
        }
    }

    public void CancelJobs()
    {
        Job[] jobs_array = jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            j.CancelJob();
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
        foreach (string objectType in World.current.inventoryPrototypes.Keys)
        {
            invsDict[objectType] = new Inventory(objectType, World.current.inventoryPrototypes[objectType].maxStackSize, 0);
        }

        Inventory[] invs = new Inventory[invsDict.Count];
        invsDict.Values.CopyTo(invs, 0);
        return invs;
    }


    public void Deconstruct()
    {
        Debug.Log("Deconstruct");
        int x = tile.X;
        int y = tile.Y;
        int fwidth = 1;
        int fheight = 1;
        bool linksToNeighbour = false;
        if (tile.Furniture != null)
        {
            Furniture f = tile.Furniture;
            fwidth = f.Width;
            fheight = f.Height;
            linksToNeighbour = f.linksToNeighbour;
            f.CancelJobs();
        }

        // We call lua to decostruct
        eventActions.Trigger("OnUninstall", this);

        // Update thermalDiffusifity to default value
        World.current.temperature.SetThermalDiffusivity(tile.X, tile.Y,
            Temperature.defaultThermalDiffusivity);

        tile.UnplaceFurniture();

        if (cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }
        // Do we need to recalculate our rooms?
        if (roomEnclosure)
        {
            Room.DoRoomFloodFill(this.tile);
        }

        ////World.current.InvalidateTileGraph();

        if (World.current.tileGraph != null)
        {
            World.current.tileGraph.RegenerateGraphAtTile(tile);
        }

        // We should inform our neighbours that they have just lost a
        // neighbour regardless of objectType.  
        // Just trigger their OnChangedCallback. 
        if (linksToNeighbour == true)
        {
            for (int xpos = x - 1; xpos < (x + fwidth + 1); xpos++)
            {
                for (int ypos = y - 1; ypos < (y + fheight + 1); ypos++)
                {
                    Tile t = World.current.GetTileAt(xpos, ypos);
                    if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null)
                    {
                        t.Furniture.cbOnChanged(t.Furniture);
                    }
                }
            }
        }

        // At this point, no DATA structures should be pointing to us, so we
        // should get garbage-collected.
    }

    public Tile GetJobSpotTile()
    {
        return World.current.GetTileAt(tile.X + (int)jobSpotOffset.x, tile.Y + (int)jobSpotOffset.y);
    }

    public Tile GetSpawnSpotTile()
    {
        return World.current.GetTileAt(tile.X + (int)jobSpawnSpotOffset.x, tile.Y + (int)jobSpawnSpotOffset.y);
    }

    // Returns true if furniture has typeTag, though simple, the intent is to separate the interaction with
    //  the Furniture's typeTags from the implementation.
    public bool HasTypeTag(string typeTag)
    {
        return typeTags.Contains(typeTag);
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return localizationCode; // this.Name;
    }

    public string GetDescription()
    {
        return unlocalizedDescription;
    }

    public string GetHitPointString()
    {
        return "18/18"; // TODO: Add a hitpoint system to...well...everything
    }

    public string GetJobDescription()
    {
        return "";
    }
    #endregion

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Deconstruct "+Name,
            RequiereCharacterSelected = false,
            Action = (ca, c) => Deconstruct()
        };

        foreach (var contextMenuLuaAction in contextMenuLuaActions)
        {
            yield return new ContextMenuAction
            {
                Text = contextMenuLuaAction.Text,
                RequiereCharacterSelected = contextMenuLuaAction.RequiereCharacterSelected,
                Action = (cma, c) => InvokeContextMenuLuaAction(contextMenuLuaAction.LuaFunction, c)
            };
        }
    }

    private void InvokeContextMenuLuaAction(string luaFunction, Character character)
    {
        LuaUtilities.CallFunction(luaFunction, this, character);
    }
}
