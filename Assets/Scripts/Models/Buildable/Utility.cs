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
using ProjectPorcupine.Jobs;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and utility (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Utility : IXmlSerializable, ISelectable, IPrototypable, IContextActionProvider, IBuildable
{
    // Prevent construction too close to the world's edge
    private const int MinEdgeDistance = 5;

    /// <summary>
    /// This action is called to get the sprite name based on the utility parameters.
    /// </summary>
    private string getSpriteNameAction;

    /// <summary>
    /// These context menu lua action are used to build the context menu of the utility.
    /// </summary>
    private List<ContextMenuLuaAction> contextMenuLuaActions;

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private HashSet<string> tileTypeBuildPermissions;

    private List<Inventory> deconstructInventory;

    /// TODO: Implement object rotation
    /// <summary>
    /// Initializes a new instance of the <see cref="Utility"/> class.
    /// </summary>
    public Utility()
    {
        Tint = new Color(1f, 1f, 1f, .25f);
        EventActions = new EventActions();

        contextMenuLuaActions = new List<ContextMenuLuaAction>();
        Parameters = new Parameter();
        Jobs = new BuildableJobs(this);
        typeTags = new HashSet<string>();
        tileTypeBuildPermissions = new HashSet<string>();
    }

    /// <summary>
    /// Copy Constructor -- don't call this directly, unless we never
    /// do ANY sub-classing. Instead use Clone(), which is more virtual.
    /// </summary>
    /// <param name="other"><see cref="Utility"/> being cloned.</param>
    private Utility(Utility other)
    {
        Type = other.Type;
        Name = other.Name;
        typeTags = new HashSet<string>(other.typeTags);
        description = other.description;
        Tint = other.Tint;
        deconstructInventory = other.deconstructInventory;

        Parameters = new Parameter(other.Parameters);
        Jobs = new BuildableJobs(this, other.Jobs);

        if (other.EventActions != null)
        {
            EventActions = other.EventActions.Clone();
        }

        if (other.contextMenuLuaActions != null)
        {
            contextMenuLuaActions = new List<ContextMenuLuaAction>(other.contextMenuLuaActions);
        }

        getSpriteNameAction = other.getSpriteNameAction;

        tileTypeBuildPermissions = new HashSet<string>(other.tileTypeBuildPermissions);

        LocalizationCode = other.LocalizationCode;
        UnlocalizedDescription = other.UnlocalizedDescription;
    }

    /// <summary>
    /// This event will trigger when the utility has been changed.
    /// This is means that any change (parameters, job state etc) to the utility will trigger this.
    /// </summary>
    public event Action<Utility> Changed;

    /// <summary>
    /// This event will trigger when the utility has been removed.
    /// </summary>
    public event Action<Utility> Removed;

    /// <summary>
    /// Gets the width of the utility.
    /// </summary>
    public int Width
    {
        get { return 1; }
    }

    /// <summary>
    /// Gets the height of the utility.
    /// </summary>
    public int Height
    {
        get { return 1; }
    }

    /// <summary>
    /// Gets the tint used to change the color of the utility.
    /// </summary>
    /// <value>The Color of the utility.</value>
    public Color Tint { get; private set; }

    /// <summary>
    /// Gets the EventAction for the current utility.
    /// These actions are called when an event is called. They get passed the utility
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    /// <value>The event actions that is called on update.</value>
    public EventActions EventActions { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the utility is selected by the player or not.
    /// </summary>
    /// <value>Whether the utility is selected or not.</value>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets the BASE tile of the utility. (Large objects can span over multiple tiles).
    /// This should be RENAMED (possibly to BaseTile).
    /// </summary>
    /// <value>The BASE tile of the utility.</value>
    public Tile Tile { get; private set; }

    /// <summary>
    /// Gets the string that defines the type of object the utility is. This gets queried by the visual system to
    /// know what sprite to render for this utility.
    /// </summary>
    /// <value>The type of the utility.</value>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the name of the utility. The name is the object type by default.
    /// </summary>
    /// <value>The name of the utility.</value>
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
    /// Gets the code used for Localization of the utility.
    /// </summary>
    public string LocalizationCode { get; private set; }

    /// <summary>
    /// Gets the description of the utility. This is used by localization.
    /// </summary>
    public string UnlocalizedDescription { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this utility is next to any utility of the same type.
    /// This is used to check what sprite to use if utility is next to each other.
    /// </summary>
    public bool LinksToNeighbour 
    { 
        get { return true; }
    }

    /// <summary>
    /// Gets the type of dragging that is used to build multiples of this utility.
    /// e.g walls.
    /// </summary>
    public string DragType 
    { 
        get { return "path"; }
    }

    /// <summary>
    /// Gets or sets the parameters that is tied to the utility.
    /// </summary>
    public Parameter Parameters { get; private set; }

    /// <summary>
    /// Gets a component that handles the jobs linked to the furniture.
    /// </summary>
    public BuildableJobs Jobs { get; private set; }

    /// <summary>
    /// Details if this is tasked for destruction.
    /// </summary>
    public bool IsBeingDestroyed { get; protected set; }

    /// <summary>
    /// Used to place utility in a certain position.
    /// </summary>
    /// <param name="proto">The prototype utility to place.</param>
    /// <param name="tile">The base tile to place the utility on, The tile will be the bottom left corner of the utility (to check).</param>
    /// <returns>Utility object.</returns>
    public static Utility PlaceInstance(Utility proto, Tile tile)
    {
        if (proto.IsValidPosition(tile) == false)
        {
            Debug.ULogErrorChannel("Utility", "PlaceInstance -- Position Validity Function returned FALSE. " + proto.Name + " " + tile.X + ", " + tile.Y + ", " + tile.Z);
            return null;
        }

        // We know our placement destination is valid.
        Utility obj = proto.Clone();
        obj.Tile = tile;

        if (tile.PlaceUtility(obj) == false)
        {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        // This type of utility links itself to its neighbours,
        // so we should inform our neighbours that they have a new
        // buddy.  Just trigger their OnChangedCallback.
        int x = tile.X;
        int y = tile.Y;

        for (int xpos = x - 1; xpos < x + 2; xpos++)
        {
            for (int ypos = y - 1; ypos < y + 2; ypos++)
            {
                Tile tileAt = World.Current.GetTileAt(xpos, ypos, tile.Z);
                if (tileAt != null && tileAt.Utilities != null)
                {
                    foreach (Utility utility in tileAt.Utilities.Values)
                    {
                        if (utility.Changed != null)
                        {
                            utility.Changed(utility);
                        }
                    }
                }
            }
        }

        // Call LUA install scripts
        obj.EventActions.Trigger("OnInstall", obj);
        return obj;
    }

    /// <summary>
    /// This function is called to update the utility. This will also trigger EventsActions.
    /// This checks if the utility is a PowerConsumer, and if it does not have power it cancels its job.
    /// </summary>
    /// <param name="deltaTime">The time since the last update was called.</param>
    public void Update(float deltaTime)
    {
        if (EventActions != null)
        {
            // updateActions(this, deltaTime);
            EventActions.Trigger("OnUpdate", this, deltaTime);
        }
    }

    /// <summary>
    /// Check if the utility has a function to determine the sprite name and calls that function.
    /// </summary>
    /// <returns>Name of the sprite.</returns>
    public string GetSpriteName()
    {
        if (string.IsNullOrEmpty(getSpriteNameAction))
        {
            return Type;
        }

        DynValue ret = FunctionsManager.Utility.Call(getSpriteNameAction, this);
        return ret.String;
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
    /// Writes the utility to XML.
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
    /// Reads the prototype utility from XML.
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
                case "BuildingJob":
                    ReadXmlBuildingJob(reader);
                    break;
                case "DeconstructJob":
                    ReadXmlDeconstructJob(reader);
                    break;
                case "CanBeBuiltOn":
                    tileTypeBuildPermissions.Add(reader.GetAttribute("tileType"));
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
                case "GetSpriteName":
                    getSpriteNameAction = reader.GetAttribute("FunctionName");
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
    /// This is used to load utility from a save file.
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
    /// Reads the XML for parameters that this utility has and assign it to the utility.
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
            (theJob) => World.Current.UtilityManager.ConstructJobCompleted(theJob),
            jobTime,
            items.ToArray(),
            Job.JobPriority.High);
        job.JobDescription = "job_build_" + Type + "_desc";
        PrototypeManager.UtilityConstructJob.Set(job);
    }

    /// <summary>
    /// Sets up a job to deconstruct the utility.
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

        Job job = PrototypeManager.UtilityDeconstructJob.Get(Type).Clone();
        job.tile = Tile;
        job.OnJobCompleted += (inJob) => Deconstruct();

        World.Current.jobQueue.Enqueue(job);
    }

    /// <summary>
    /// Deconstructs the utility.
    /// </summary>
    public void Deconstruct()
    {
        int x = Tile.X;
        int y = Tile.Y;
        if (Tile.Utilities != null)
        {
            Jobs.CancelAll();
        }

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);
        Tile.UnplaceUtility(this);

        if (Removed != null)
        {
            Removed(this);
        }

        if (deconstructInventory != null)
        {
            foreach (Inventory inv in deconstructInventory)
            {
                inv.MaxStackSize = PrototypeManager.Inventory.Get(inv.Type).maxStackSize;
                World.Current.InventoryManager.PlaceInventoryAround(Tile, inv.Clone());
            }
        }

        // We should inform our neighbours that they have just lost a
        // neighbour regardless of type.
        // Just trigger their OnChangedCallback.
        for (int xpos = x - 1; xpos < x + 2; xpos++)
        {
            for (int ypos = y - 1; ypos < y + 2; ypos++)
            {
                Tile tileAt = World.Current.GetTileAt(xpos, ypos, Tile.Z);
                if (tileAt != null && tileAt.Utilities != null)
                {
                    foreach (Utility neighborUtility in tileAt.Utilities.Values)
                    {
                        if (neighborUtility.Changed != null)
                        {
                            neighborUtility.Changed(neighborUtility);
                        }
                    }
                }
            }
        }

        // At this point, no DATA structures should be pointing to us, so we
        // should get garbage-collected.
    }

    /// <summary>
    /// Checks whether the utility has a certain tag.
    /// </summary>
    /// <param name="typeTag">Tag to check for.</param>
    /// <returns>True if utility has specified tag.</returns>
    public bool HasTypeTag(string typeTag)
    {
        return typeTags.Contains(typeTag);
    }

    /// <summary>
    /// Returns LocalizationCode name for the utility.
    /// </summary>
    /// <returns>LocalizationCode for the name of the utility.</returns>
    public string GetName()
    {
        return LocalizationCode; // this.Name;
    }

    /// <summary>
    /// Returns the UnlocalizedDescription of the utility.
    /// </summary>
    /// <returns>Description of the utility.</returns>
    public string GetDescription()
    {
        return UnlocalizedDescription;
    }

    public IEnumerable<string> GetAdditionalInfo()
    {
        yield return string.Empty;
    }

    /// <summary>
    /// Returns the description of the job linked to the utility. NOT INMPLEMENTED.
    /// </summary>
    /// <returns>Job description of the job linked to the utility.</returns>
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
            Action = (contextMenuAction, character) => SetDeconstructJob()
        };
        if (Jobs.Count > 0)
        {
            for (int i = 0; i < Jobs.Count; i++)
            {
                if (!Jobs[i].IsBeingWorked)
                {
                    yield return new ContextMenuAction
                    {
                        Text = "Prioritize " + Name,
                        RequireCharacterSelected = true,
                        Action = (contextMenuAcion, character) => character.PrioritizeJob(Jobs[0])
                    };
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

    /// <summary>
    /// Make a copy of the current utility.  Sub-classes should
    /// override this Clone() if a different (sub-classed) copy
    /// constructor should be run.
    /// </summary>
    /// <returns>A clone of the utility.</returns>
    public Utility Clone()
    {
        return new Utility(this);
    }

    /// <summary>
    /// Check if the position of the utility is valid or not.
    /// This is called when placing the utility.
    /// TODO : Add some LUA special requierments.
    /// </summary>
    /// <param name="tile">The base tile.</param>
    /// <returns>True if the tile is valid for the placement of the utility.</returns>
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

        // Make sure tile is FLOOR
        if (tile.Type != TileType.Floor && tileTypeBuildPermissions.Contains(tile.Type.Type) == false)
        {
            return false;
        }

        return true;
    }

    private void ReadXmlDeconstructJob(XmlReader reader)
    {
        float jobTime = float.Parse(reader.GetAttribute("jobTime"));

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
        PrototypeManager.UtilityDeconstructJob.Set(job);
    }

    private void InvokeContextMenuLuaAction(ContextMenuAction action, Character character)
    {
        FunctionsManager.Utility.Call(action.Parameter, this, character);
    }

    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Utility util)
    {
        if (Changed != null)
        {
            Changed(util);
        }
    }
}
