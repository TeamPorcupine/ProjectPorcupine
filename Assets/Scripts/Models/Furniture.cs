//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;


// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa)

[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectable
{
    /// <summary>
    /// Custom parameter for this particular piece of furniture.  We are
    /// using a dictionary because later, custom LUA function will be
    /// able to use whatever parameters the user/modder would like.
    /// Basically, the LUA code will bind to this dictionary.
    /// </summary>
    protected Dictionary<string, float> _furnParameters;

    /// <summary>
    /// These actions are called every update. They get passed the furniture
    /// they belong to, plus a deltaTime.
    /// </summary>
    //protected Action<Furniture, float> updateActions;
    protected List<string> _updateActions;

    //public Func<Furniture, ENTERABILITY> IsEnterable;
    protected string _isEnterableAction;

    protected List<string> _replaceableFurniture = new List<string>();

    private List<Job> _jobs;

    // If this furniture gets worked by a person,
    // where is the correct spot for them to stand,
    // relative to the bottom-left tile of the sprite.
    // NOTE: This could even be something outside of the actual
    // furniture tile itself!  (In fact, this will probably be common).
    public Vector2 jobSpotOffset = Vector2.zero;

    // If the job causes some kind of object to be spawned, where will it appear?
    public Vector2 jobSpawnSpotOffset = Vector2.zero;

    public void Update(float deltaTime)
    {
        if (_updateActions != null)
        {
            //updateActions(this, deltaTime);

            if (powerValue > 0 && isPowerGenerator == false)
            {
                if (World.Current.PowerSystem.RequestPower(this) == false)
                {
                    World.Current.PowerSystem.RegisterPowerConsumer(this);
                    return;
                }
            }

            FurnitureActions.CallFunctionsWithFurniture(_updateActions.ToArray(), this, deltaTime);
        }
    }

    public Enterability IsEnterable()
    {
        if (_isEnterableAction == null || _isEnterableAction.Length == 0)
        {
            return Enterability.Yes;
        }

        //FurnitureActions.CallFunctionsWithFurniture( isEnterableActions.ToArray(), this );

        DynValue ret = FurnitureActions.CallFunction(_isEnterableAction, this);

        return (Enterability)ret.Number;

    }

    // This is true if the Furniture produces power
    public bool isPowerGenerator;
    // If it is a generator this is the amount of power it produces otherwise this is the amount it consumes.
    public float powerValue;

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile Tile { get; protected set; }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string ObjectType { get; protected set; }

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private string _baseType;

    private string _name = null;

    public string Name {
        get {
            if (_name == null || _name.Length == 0)
            {
                return ObjectType;
            }
            return _name;
        }
        set {
            _name = value;
        }
    }

    private string Description = "";

    public List<string> ReplaceableFurniture {
        get {
            return _replaceableFurniture;
        }
    }

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float MovementCost { get; protected set; }

    public bool RoomEnclosure { get; protected set; }

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }

    public int Height { get; protected set; }

    public string LocalizationCode { get; protected set; }
    public string UnlocalizedDescription { get; protected set; }

    public Color tint = Color.white;

    public bool LinksToNeighbour { get; protected set; }

    public event Action<Furniture> OnChanged;
    public event Action<Furniture> OnRemoved;

    private Func<Tile, bool> _funcPositionValidation;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Empty constructor is used for serialization
    public Furniture()
    {
        _updateActions = new List<string>();
        _furnParameters = new Dictionary<string, float>();
        _jobs = new List<Job>();
        this._funcPositionValidation = this.DEFAULT__IsValidPosition;
        this.Height = 1;
        this.Width = 1;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
    protected Furniture(Furniture other)
    {
        this.ObjectType = other.ObjectType;
        this.Name = other.Name;
        this._baseType = other._baseType;
        this.Description = other.Description;
        this.MovementCost = other.MovementCost;
        this.RoomEnclosure = other.RoomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.LinksToNeighbour = other.LinksToNeighbour;

        this.jobSpotOffset = other.jobSpotOffset;
        this.jobSpawnSpotOffset = other.jobSpawnSpotOffset;

        this._furnParameters = new Dictionary<string, float>(other._furnParameters);
        _jobs = new List<Job>();

        if (other._updateActions != null)
            this._updateActions = new List<string>(other._updateActions);

        this._isEnterableAction = other._isEnterableAction;

        this.isPowerGenerator = other.isPowerGenerator;
        this.powerValue = other.powerValue;

        if (isPowerGenerator == true)
        {
            World.Current.PowerSystem.RegisterPowerSupply(this);
        }
        else if (powerValue > 0)
        {
            World.Current.PowerSystem.RegisterPowerConsumer(this);
        }

        if (other._funcPositionValidation != null)
            this._funcPositionValidation = (Func<Tile, bool>)other._funcPositionValidation.Clone();

        this.LocalizationCode = other.LocalizationCode;
        this.UnlocalizedDescription = other.UnlocalizedDescription;
    }

    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    public virtual Furniture Clone()
    {
        return new Furniture(this);
    }

    public static Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto._funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
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

            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = World.Current.GetTileAt(x, y + 1);
            if (t != null && t.Furniture != null && t.Furniture.OnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                // We have a Northern Neighbour with the same object type as us, so
                // tell it that it has changed by firing is callback.
                t.Furniture.OnChanged(t.Furniture);
            }
            t = World.Current.GetTileAt(x + 1, y);
            if (t != null && t.Furniture != null && t.Furniture.OnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.OnChanged(t.Furniture);
            }
            t = World.Current.GetTileAt(x, y - 1);
            if (t != null && t.Furniture != null && t.Furniture.OnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.OnChanged(t.Furniture);
            }
            t = World.Current.GetTileAt(x - 1, y);
            if (t != null && t.Furniture != null && t.Furniture.OnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.OnChanged(t.Furniture);
            }

        }

        return obj;
    }

    public bool IsValidPosition(Tile t)
    {
        return _funcPositionValidation(t);
    }

    // FIXME: These functions should never be called directly,
    // so they probably shouldn't be public functions of Furniture
    // This will be replaced by validation checks fed to use from
    // LUA files that will be customizable for each piece of furniture.
    // For example, a door might specific that it needs two walls to
    // connect to.
    protected bool DEFAULT__IsValidPosition(Tile t)
    {
        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + Height); y_off++)
            {
                Tile t2 = World.Current.GetTileAt(x_off, y_off);


                // Check to see if there is furniture which is replaceable
                bool isReplaceable = false;

                if (t2.Furniture != null)
                {
                    for (int i = 0; i < ReplaceableFurniture.Count; i++)
                    {
                        if (t2.Furniture._baseType == ReplaceableFurniture[i])
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

    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Furniture furn)
    {
        if (OnChanged != null)
        {
            OnChanged(furn);
        }
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
        //writer.WriteAttributeString( "movementCost", movementCost.ToString() );

        foreach (string k in _furnParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", _furnParameters[k].ToString());
            writer.WriteEndElement();
        }

    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        //Debug.Log("ReadXmlPrototype");

        ObjectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();


        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "BaseType":
                    reader.Read();
                    _baseType = reader.ReadContentAsString();
                    break;
                case "Description":
                    reader.Read();
                    Description = reader.ReadContentAsString();
                    break;
                case "MovementCost":
                    reader.Read();
                    MovementCost = reader.ReadContentAsFloat();
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
                    _replaceableFurniture.Add(reader.GetAttribute("baseType").ToString());
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
                                    0
                                ));
                        }
                    }

                    Job j = new Job(null,
                        ObjectType,
                        FurnitureActions.JobComplete_FurnitureBuilding, jobTime,
                        invs.ToArray()
                    );

                    World.Current.SetFurnitureJobPrototype(j, this);

                    break;
                case "OnUpdate":

                    string functionName = reader.GetAttribute("FunctionName");
                    RegisterUpdateAction(functionName);

                    break;
                case "IsEnterable":

                    _isEnterableAction = reader.GetAttribute("FunctionName");

                    break;

                case "JobSpotOffset":
                    jobSpotOffset = new Vector2(
                        int.Parse(reader.GetAttribute("X")),
                        int.Parse(reader.GetAttribute("Y"))
                    );

                    break;
                case "JobSpawnSpotOffset":
                    jobSpawnSpotOffset = new Vector2(
                        int.Parse(reader.GetAttribute("X")),
                        int.Parse(reader.GetAttribute("Y"))
                    );

                    break;

                case "PowerGenerator":
                    isPowerGenerator = true;
                    powerValue = float.Parse(reader.GetAttribute("supply"));
                    break;
                case "Power":
                    reader.Read();
                    powerValue = reader.ReadContentAsFloat();
                    break;

                case "Params":
                    ReadXmlParams(reader);	// Read in the Param tag
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
        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                _furnParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }

    /// <summary>
    /// Gets the custom furniture parameter from a string key.
    /// </summary>
    /// <returns>The parameter value (float).</returns>
    /// <param name="key">Key string.</param>
    /// <param name="default_value">Default value.</param>
    public float GetParameter(string key, float default_value)
    {
        if (_furnParameters.ContainsKey(key) == false)
        {
            return default_value;
        }

        return _furnParameters[key];
    }

    public float GetParameter(string key)
    {
        return GetParameter(key, 0);
    }


    public void SetParameter(string key, float value)
    {
        _furnParameters[key] = value;
    }

    public void ChangeParameter(string key, float value)
    {
        if (_furnParameters.ContainsKey(key) == false)
        {
            _furnParameters[key] = value;
        }

        _furnParameters[key] += value;
    }

    /// <summary>
    /// Registers a function that will be called every Update.
    /// (Later this implementation might change a bit as we support LUA.)
    /// </summary>
    public void RegisterUpdateAction(string luaFunctionName)
    {
        _updateActions.Add(luaFunctionName);
    }

    public void UnregisterUpdateAction(string luaFunctionName)
    {
        _updateActions.Remove(luaFunctionName);
    }

    public int JobCount()
    {
        return _jobs.Count;
    }

    public void AddJob(Job j)
    {
        j.Furniture = this;
        _jobs.Add(j);
        j.JobStopped += OnJobStopped;
        World.Current.JobQueue.Enqueue(j);
    }

    void OnJobStopped(Job j)
    {
        RemoveJob(j);
    }

    protected void RemoveJob(Job j)
    {
        j.JobStopped -= OnJobStopped;
        _jobs.Remove(j);
        j.Furniture = null;
    }

    protected void ClearJobs()
    {
        Job[] jobs_array = _jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            RemoveJob(j);
        }
    }

    public void CancelJobs()
    {
        Job[] jobs_array = _jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            j.CancelJob();
        }
    }

    public bool IsStockpile()
    {
        return ObjectType == "Stockpile";
    }

    public void Deconstruct()
    {
        Debug.Log("Deconstruct");

        Tile.UnplaceFurniture();

        if (OnRemoved != null)
            OnRemoved(this);

        // Do we need to recalculate our rooms?
        if (RoomEnclosure)
        {
            Room.DoRoomFloodFill(this.Tile);
        }

        //World.current.InvalidateTileGraph();
        if (World.Current.TileGraph != null)
        {
            World.Current.TileGraph.RegenerateGraphAtTile(Tile);
        }

        // At this point, no DATA structures should be pointing to us, so we
        // should get garbage-collected.

    }

    public Tile GetJobSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)jobSpotOffset.x, Tile.Y + (int)jobSpotOffset.y);
    }

    public Tile GetSpawnSpotTile()
    {
        return World.Current.GetTileAt(Tile.X + (int)jobSpawnSpotOffset.x, Tile.Y + (int)jobSpawnSpotOffset.y);
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return LocalizationCode;//this.Name;
    }

    public string GetDescription()
    {
        return UnlocalizedDescription;
    }

    public string GetHitPointString()
    {
        return "18/18";	// TODO: Add a hitpoint system to...well...everything
    }

    #endregion
}