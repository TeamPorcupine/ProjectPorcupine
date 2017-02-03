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
using MoonSharp.Interpreter.Interop;
using Newtonsoft.Json.Linq;
using ProjectPorcupine.Buildable.Components;
using ProjectPorcupine.OrderActions;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

/// <summary>
/// InstalledObjects are things like walls, doors, and utility (e.g. a sofa).
/// </summary>
[MoonSharpUserData]
public class Utility : ISelectable, IPrototypable, IContextActionProvider, IBuildable
{
    private bool gridUpdatedThisFrame = false;

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

    private HashSet<string> tileTypeBuildPermissions;

    private Dictionary<string, OrderAction> orderActions;

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
        orderActions = new Dictionary<string, OrderAction>();
    }

    /// <summary>
    /// Copy Constructor -- don't call this directly, unless we never
    /// do ANY sub-classing. Instead use Clone(), which is more virtual.
    /// </summary>
    /// <param name="other"><see cref="Utility"/> being cloned.</param>
    private Utility(Utility other)
    {
        Type = other.Type;
        typeTags = new HashSet<string>(other.typeTags);
        Tint = other.Tint;

        // add cloned order actions
        orderActions = new Dictionary<string, OrderAction>();
        foreach (var orderAction in other.orderActions)
        {
            orderActions.Add(orderAction.Key, orderAction.Value.Clone());
        }

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
    /// Gets or sets the grid used by this utility.
    /// </summary>
    /// <value>The grid used by this utility.</value>
    public Grid Grid { get; set; }

    /// <summary>
    /// Used to place utility in a certain position.
    /// </summary>
    /// <param name="proto">The prototype utility to place.</param>
    /// <param name="tile">The base tile to place the utility on, The tile will be the bottom left corner of the utility (to check).</param>
    /// <param name="skipGridUpdate">If true, the grid won't be updated from neighboring Utilities, UpdateGrid must be called on at least one
    /// utility connected to this utility for them to network properly.</param>
    /// <returns>Utility object.</returns>
    public static Utility PlaceInstance(Utility proto, Tile tile, bool skipGridUpdate = false)
    {
        if (proto.IsValidPosition(tile) == false)
        {
            UnityDebugger.Debugger.LogError("Utility", "PlaceInstance -- Position Validity Function returned FALSE. " + proto.GetName() + " " + tile.X + ", " + tile.Y + ", " + tile.Z);
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

        // All utilities link to neighbors of the same type,
        // so we should inform our neighbours that they have a new
        // buddy.  Just trigger their OnChangedCallback.
        foreach (Tile neighbor in obj.Tile.GetNeighbours())
        {
            if (neighbor.Utilities != null && neighbor.Utilities.ContainsKey(obj.Type))
            {
                Utility utility = neighbor.Utilities[obj.Type];
                if (utility.Changed != null)
                {
                    utility.Changed(utility);
                }
            }
        }

        if (!skipGridUpdate)
        {
            obj.UpdateGrid(obj);
        }
        else
        {
            // If we're skipping the update, we need a temporary grid for furniture in the same tile to connect to.
            obj.Grid = new Grid();
            World.Current.PowerNetwork.RegisterGrid(obj.Grid);
            obj.SeekConnection();
        }

        // Call LUA install scripts
        obj.EventActions.Trigger("OnInstall", obj);

        return obj;
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
                case "TypeTag":
                    reader.Read();
                    typeTags.Add(reader.ReadContentAsString());
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
                        LocalizationKey = reader.GetAttribute("Text"),
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
                case "OrderAction":
                    OrderAction orderAction = OrderAction.Deserialize(reader);
                    if (orderAction != null)
                    {
                        orderActions[orderAction.Type] = orderAction;
                    }

                    break;
            }
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
    /// Sets up a job to deconstruct the utility.
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
    /// Deconstructs the utility.
    /// </summary>
    public void Deconstruct()
    {
        if (Tile.Utilities != null)
        {
            Jobs.CancelAll();
        }

        // Just unregister our grid, it will get reregistered if there are any other utilities on this grid
        World.Current.PowerNetwork.RemoveGrid(Grid);

        // We call lua to decostruct
        EventActions.Trigger("OnUninstall", this);
        Tile.UnplaceUtility(this);

        if (Removed != null)
        {
            Removed(this);
        }

        Deconstruct deconstructOrder = GetOrderAction<Deconstruct>();
        if (deconstructOrder != null)
        {
            foreach (OrderAction.InventoryInfo inv in deconstructOrder.Inventory)
            {
                World.Current.InventoryManager.PlaceInventoryAround(Tile, new Inventory(inv.Type, inv.Amount));
            }
        }

        // We should inform our neighbours that they have just lost a neighbour.
        // Just trigger their OnChangedCallback.
        foreach (Tile neighbor in Tile.GetNeighbours())
        {
            if (neighbor.Utilities != null && neighbor.Utilities.ContainsKey(this.Type))
            {
                Utility neighborUtility = neighbor.Utilities[this.Type];
                if (neighborUtility.Changed != null)
                {
                    neighborUtility.Changed(neighborUtility);
                }

                if (neighborUtility.Grid == this.Grid)
                {
                    neighborUtility.Grid = new Grid();
                }

                neighborUtility.UpdateGrid(neighborUtility);
                neighborUtility.Grid.Split();
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
    /// Gets the type tags.
    /// </summary>
    /// <returns>The type tags.</returns>
    public string[] GetTypeTags()
    {
        return typeTags.ToArray();
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
            LocalizationKey = "Deconstruct " + GetName(),
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
                        LocalizationKey = "Prioritize " + GetName(),
                        RequireCharacterSelected = true,
                        Action = (contextMenuAcion, character) => character.PrioritizeJob(Jobs[0])
                    };
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

    /// <summary>
    /// Updates the grids of this utility sharing the grids along the network of connected utilities.
    /// </summary>
    /// <param name="utilityToUpdate">Utility to update.</param>
    /// <param name="newGrid">If not null this will force neighboring utilities to use the specified Instance of Grid.</param>
    public void UpdateGrid(Utility utilityToUpdate, Grid newGrid = null)
    {
        if (gridUpdatedThisFrame)
        {
            return;
        }

        gridUpdatedThisFrame = true;
        TimeManager.Instance.RunNextFrame(() => gridUpdatedThisFrame = false);
        Grid oldGrid = utilityToUpdate.Grid;

        World.Current.PowerNetwork.RemoveGrid(utilityToUpdate.Grid);

        if (newGrid == null)
        {
            foreach (Tile neighborTile in utilityToUpdate.Tile.GetNeighbours())
            {
                if (neighborTile != null && neighborTile.Utilities != null && neighborTile.Utilities.ContainsKey(this.Type))
                {
                    Utility utility = neighborTile.Utilities[this.Type];

                    if (utility.Grid != null && utilityToUpdate.Grid == null)
                    {
                        utilityToUpdate.Grid = utility.Grid;
                    }
                }
            }

            if (utilityToUpdate.Grid == null)
            {
                utilityToUpdate.Grid = new Grid();
            }
        }
        else
        {
            utilityToUpdate.Grid = newGrid;
        }

        if (utilityToUpdate.Grid != oldGrid)
        {
            World.Current.PowerNetwork.UnregisterGrid(oldGrid);
        }

        if (oldGrid != null && newGrid != null)
        {
            newGrid.Merge(oldGrid);
        }

        World.Current.PowerNetwork.RegisterGrid(utilityToUpdate.Grid);

        foreach (Tile neighborTile in utilityToUpdate.Tile.GetNeighbours())
        {
            if (neighborTile != null && neighborTile.Utilities != null)
            {
                if (neighborTile != null && neighborTile.Utilities != null && neighborTile.Utilities.ContainsKey(this.Type))
                {
                    Utility utility = neighborTile.Utilities[this.Type];
                    utility.UpdateGrid(utility, utilityToUpdate.Grid);
                }
            }
        }

        SeekConnection();
    }

    public object ToJSon()
    {
        JObject utilityJSon = new JObject();
        utilityJSon.Add("X", Tile.X);
        utilityJSon.Add("Y", Tile.Y);
        utilityJSon.Add("Z", Tile.Z);
        utilityJSon.Add("Type", Type);
        if (Parameters.HasContents())
        {
            utilityJSon.Add("Parameters", Parameters.ToJson());
        }

        return utilityJSon;
    }

    public void FromJson(JToken utilityToken)
    {
        JObject utilityJObject = (JObject)utilityToken;

        // Everything else has already been set by FurnitureManager, we just need our parameters
        if (utilityJObject.Children().Contains("Parameters"))
        {
            Parameters.FromJson(utilityJObject["Parameters"]);
        }
    }

    private void SeekConnection()
    {
        // try to reconnect furniture if present and compatible     
        if (Tile != null && Tile.Furniture != null)
        {
            IPluggable pluggableComponent = Tile.Furniture.GetPluggable(typeTags);
            if (pluggableComponent != null)
            {
                // plug in
                pluggableComponent.Reconnect();
            }
        }
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
