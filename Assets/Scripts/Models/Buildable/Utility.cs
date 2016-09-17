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
using Animation;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ProjectPorcupine.PowerNetwork;
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

    private List<Job> jobs;

    private List<Job> pausedJobs;

    // This is the generic type of object this is, allowing things to interact with it based on it's generic type
    private HashSet<string> typeTags;

    private string name = null;

    private string description = string.Empty;

    private Func<Tile, bool> funcPositionValidation;

    private HashSet<string> tileTypeBuildPermissions;

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
        jobs = new List<Job>();
        typeTags = new HashSet<string>();
        funcPositionValidation = DefaultIsValidPosition;
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

        Parameters = new Parameter(other.Parameters);
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

        getSpriteNameAction = other.getSpriteNameAction;

        if (other.funcPositionValidation != null)
        {
            funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
        }

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
            if (string.IsNullOrEmpty(name))
            {
                return Type;
            }

            return name;
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
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the type of dragging that is used to build multiples of this utility. 
    /// e.g walls.
    /// </summary>
    public string DragType 
    { 
        get
        {
            return "path";
        }
    }

    /// <summary>
    /// Gets or sets the parameters that is tied to the utility.
    /// </summary>
    public Parameter Parameters { get; private set; }

    /// <summary>
    /// Used to place utility in a certain position.
    /// </summary>
    /// <param name="proto">The prototype utility to place.</param>
    /// <param name="tile">The base tile to place the utility on, The tile will be the bottom left corner of the utility (to check).</param>
    /// <returns>Utility object.</returns>
    public static Utility PlaceInstance(Utility proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
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
        if (pausedJobs.Count > 0)
        {
            ResumeJobs();
        }
            
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
    /// Check if the position of the utility is valid or not.
    /// This is called when placing the utility.
    /// </summary>
    /// <param name="tile">The base tile.</param>
    /// <returns>True if the tile is valid for the placement of the utility.</returns>
    public bool IsValidPosition(Tile tile)
    {
        return funcPositionValidation(tile);
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
                                    int.Parse(inventoryReader.GetAttribute("amount")),
                                    0));
                        }
                    }

                    Job job = new Job(
                                null,
                                Type,
                                FunctionsManager.JobComplete_UtilityBuilding,
                                jobTime,
                                invs.ToArray(),
                                Job.JobPriority.High);
                    job.JobDescription = "job_build_" + Type + "_desc";
                    PrototypeManager.UtilityJob.Set(job);
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
                        RequireCharacterSelected = bool.Parse(reader.GetAttribute("RequiereCharacterSelected")),
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
    /// How many jobs are linked to this utility.
    /// </summary>
    /// <returns>The number of jobs linked to this utility.</returns>
    public int JobCount()
    {
        return jobs.Count;
    }

    /// <summary>
    /// Link a job to the current utility.
    /// </summary>
    /// <param name="job">The job that you want to link to the utility.</param>
    public void AddJob(Job job)
    {
        job.buildable = this;
        jobs.Add(job);
        job.OnJobStopped += OnJobStopped;
        World.Current.jobQueue.Enqueue(job);
    }

    /// <summary>
    /// Cancel all the jobs linked to this utility.
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

    /// <summary>
    /// Deconstructs the utility.
    /// </summary>
    public void Deconstruct(Utility utility)
    {
        int x = Tile.X;
        int y = Tile.Y;
        if (Tile.Utilities != null)
        {
            utility.CancelJobs();
        }

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);
        Tile.UnplaceUtility();

        if (Removed != null)
        {
            Removed(this);
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
                            neighborUtility.Changed(utility);
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
            Action = (contextMenuAction, character) => Deconstruct(this)
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
                        Action = (contextMenuAcion, character) =>
                        {
                            character.PrioritizeJob(jobs[0]);
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
                    RequireCharacterSelected = contextMenuLuaAction.RequireCharacterSelected,
                    Action = (cma, c) => InvokeContextMenuLuaAction(contextMenuLuaAction.LuaFunction, c)
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

    // FIXME: These functions should never be called directly,
    // so they probably shouldn't be public functions of Utility
    // This will be replaced by validation checks fed to use from
    // LUA files that will be customizable for each piece of utility.
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

        // Make sure tile is FLOOR
        if (tile.Type != TileType.Floor && tileTypeBuildPermissions.Contains(tile.Type.Type) == false)
        {
            return false;
        }

        return true;
    }

    private void RemoveJob(Job job)
    {
        job.OnJobStopped -= OnJobStopped;
        jobs.Remove(job);
        job.buildable = null;
    }

    private void ClearJobs()
    {
        Job[] jobsArray = jobs.ToArray();
        foreach (Job job in jobsArray)
        {
            RemoveJob(job);
        }
    }

    private void InvokeContextMenuLuaAction(string luaFunction, Character character)
    {
        FunctionsManager.Utility.Call(luaFunction, this, character);
    }

    [MoonSharpVisible(true)]
    private void UpdateOnChanged(Utility util)
    {
        if (Changed != null)
        {
            Changed(util);
        }
    }

    private void OnJobStopped(Job job)
    {
        RemoveJob(job);
    }
}
