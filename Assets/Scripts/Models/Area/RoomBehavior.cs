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
using ProjectPorcupine.Localization;

namespace ProjectPorcupine.Rooms
{
    /// <summary>
    /// Room Behaviors are functions added to specific rooms, such as an airlock, a dining room, or an abattoir.
    /// </summary>
    [MoonSharpUserData]
    public class RoomBehavior : ISelectable, IPrototypable, IContextActionProvider
    {
        /// <summary>
        /// These context menu lua action are used to build the context menu of the room behavior.
        /// </summary>
        private List<ContextMenuLuaAction> contextMenuLuaActions;

        // This is the generic type of object this is, allowing things to interact with it based on it's generic type
        private HashSet<string> typeTags;

        private List<FurnitureRequirement> requiredFurniture;

        private int requiredSize = 0;

        private string name = null;

        private string description = string.Empty;

        private Func<Room, bool> funcRoomValidation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomBehavior"/> class.
        /// </summary>
        public RoomBehavior()
        {
            EventActions = new EventActions();

            contextMenuLuaActions = new List<ContextMenuLuaAction>();
            Parameters = new Parameter();
            typeTags = new HashSet<string>();
            funcRoomValidation = DefaultIsValidRoom;
            requiredFurniture = new List<FurnitureRequirement>();
            ControlledFurniture = new Dictionary<string, List<Furniture>>();
        }

        /// <summary>
        /// Copy Constructor -- don't call this directly, unless we never
        /// do ANY sub-classing. Instead use Clone(), which is more virtual.
        /// </summary>
        /// <param name="other"><see cref="RoomBehavior"/> being cloned.</param>
        private RoomBehavior(RoomBehavior other)
        {
            Type = other.Type;
            Name = other.Name;
            typeTags = new HashSet<string>(other.typeTags);
            description = other.description;

            Parameters = new Parameter(other.Parameters);

            if (other.EventActions != null)
            {
                EventActions = other.EventActions.Clone();
            }

            if (other.contextMenuLuaActions != null)
            {
                contextMenuLuaActions = new List<ContextMenuLuaAction>(other.contextMenuLuaActions);
            }

            if (other.funcRoomValidation != null)
            {
                funcRoomValidation = (Func<Room, bool>)other.funcRoomValidation.Clone();
            }

            if (other.requiredFurniture != null)
            {
                requiredFurniture = new List<FurnitureRequirement>(other.requiredFurniture);
            }

            if (other.ControlledFurniture != null)
            {
                ControlledFurniture = new Dictionary<string, List<Furniture>>(other.ControlledFurniture);
            }

            LocalizationCode = other.LocalizationCode;
            UnlocalizedDescription = other.UnlocalizedDescription;
        }

        /// <summary>
        /// This event will trigger when the RoomBehavior has been changed.
        /// This is means that any change (parameters, job state etc) to the RoomBehavior will trigger this.
        /// </summary>
        public event Action<RoomBehavior> Changed;

        /// <summary>
        /// This event will trigger when the RoomBehavior has been removed.
        /// </summary>
        public event Action<RoomBehavior> Removed;

        /// <summary>
        /// Gets the EventAction for the current RoomBehavior.
        /// These actions are called when an event is called. They get passed the RoomBehavior
        /// they belong to, plus a deltaTime (which defaults to 0).
        /// </summary>
        /// <value>The event actions that is called on update.</value>
        public EventActions EventActions { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RoomBehavior is selected by the player or not.
        /// </summary>
        /// <value>Whether the room behavior is selected or not.</value>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets the string that defines the type of object the room behavior is. This gets queried by the visual system to 
        /// know what sprite to render for this RoomBehavior.
        /// </summary>
        /// <value>The type of the RoomBehavior.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the room this Behavior is attached to.
        /// </summary>
        public Room Room { get; private set; }

        /// <summary>
        /// Gets the name of the RoomBehavior. The name is the object type by default.
        /// </summary>
        /// <value>The name of the RoomBehavior.</value>
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
        /// Gets the code used for Localization of the room behavior.
        /// </summary>
        public string LocalizationCode { get; private set; }

        /// <summary>
        /// Gets the description of the room behavior. This is used by localization.
        /// </summary>
        public string UnlocalizedDescription { get; private set; }

        /// <summary>
        /// Gets or sets the parameters that is tied to the room behavior.
        /// </summary>
        public Parameter Parameters { get; private set; }

        public Dictionary<string, List<Furniture>> ControlledFurniture { get; private set; }

        /// <summary>
        /// This function is called to update the room behavior. This will also trigger EventsActions.
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
        /// Check if the room is valid for the room behavior.
        /// This is called when assigning the room behavior.
        /// </summary>
        /// <param name = "room">The room to be validated.</param>
        /// <returns>True if the room is valid for the the room behavior.</returns>
        public bool IsValidRoom(Room room)
        {
            return funcRoomValidation(room);
        }

        /// <summary>
        /// Reads the prototype room behavior from XML.
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
                    case "Requirements":
                        ReadXmlRequirements(reader);
                        break;
                    case "Optional":
                        ReadXmlRequirements(reader, true);
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
        /// This is used to load room behavior from a save file.
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
        /// Reads the XML for parameters that this room behavior has and assign it to the room behavior.
        /// </summary>
        /// <param name="reader">The reader to read the parameters from.</param>
        public void ReadXmlParams(XmlReader reader)
        {
            // X, Y, and type have already been set, and we should already
            // be assigned to a tile.  So just read extra data.
            Parameters = Parameter.ReadXml(reader);
        }

        /// <summary>
        /// Deconstructs the room behavior.
        /// </summary>
        public void Deconstruct(RoomBehavior roomBehavior)
        {
            // We call lua to decostruct
            EventActions.Trigger("OnUninstall", this);
            Room.UndesignateRoomBehavior(roomBehavior);

            if (Removed != null)
            {
                Removed(this);
            }

            // At this point, no DATA structures should be pointing to us, so we
            // should get garbage-collected.
        }

        /// <summary>
        /// Checks whether the room behavior has a certain tag.
        /// </summary>
        /// <param name="typeTag">Tag to check for.</param>
        /// <returns>True if room behavior has specified tag.</returns>
        public bool HasTypeTag(string typeTag)
        {
            return typeTags.Contains(typeTag);
        }

        /// <summary>
        /// Returns LocalizationCode name for the room behavior.
        /// </summary>
        /// <returns>LocalizationCode for the name of the room behavior.</returns>
        public string GetName()
        {
            return LocalizationCode; // this.Name;
        }

        /// <summary>
        /// Returns the UnlocalizedDescription of the room behavior.
        /// </summary>
        /// <returns>Description of the room behavior.</returns>
        public string GetDescription()
        {
            return UnlocalizedDescription;
        }

        public IEnumerable<string> GetAdditionalInfo()
        {
            yield return string.Empty;
        }

        /// <summary>
        /// Returns the description of the job linked to the room behavior. NOT INMPLEMENTED.
        /// </summary>
        /// <returns>Job description of the job linked to the room behavior.</returns>
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
                LocalizationKey = LocalizationTable.GetLocalization("deconstruct", LocalizationCode),
                RequireCharacterSelected = false,
                Action = (contextMenuAction, character) => Deconstruct(this)
            };

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
        /// Make a copy of the current room behavior.  Sub-classes should
        /// override this Clone() if a different (sub-classed) copy
        /// constructor should be run.
        /// </summary>
        /// <returns>A clone of the room behavior.</returns>
        public RoomBehavior Clone()
        {
            return new RoomBehavior(this);
        }

        public void Control(Room room)
        {
            this.Room = room;
            List<Tile> innerTiles = room.GetInnerTiles();
            List<Tile> borderTiles = room.GetBorderingTiles();

            List<Tile> allTiles = innerTiles.Union(borderTiles).ToList();

            foreach (FurnitureRequirement requirement in requiredFurniture)
            {
                string furnitureKey = requirement.type ?? requirement.typeTag;
                ControlledFurniture.Add(furnitureKey, new List<Furniture>());
                foreach (Tile tile in allTiles.FindAll(tile => (tile.Furniture != null && (tile.Furniture.Type == requirement.type || tile.Furniture.HasTypeTag(requirement.typeTag)))))
                {
                    ControlledFurniture[furnitureKey].Add(tile.Furniture);
                }
            }

            EventActions.Trigger("OnControl", this);
        }

        public JObject ToJson()
        {
            JObject behaviorJson = new JObject();
            behaviorJson.Add("Room", Room.ID);
            behaviorJson.Add("Behavior", Type);
            return behaviorJson;
        }

        [MoonSharpVisible(true)]
        private void CallEventAction(string actionName, params object[] parameters)
        {
            EventActions.Trigger(actionName, this, parameters);
        }

        private bool DefaultIsValidRoom(Room room)
        {
            if (room.TileCount < requiredSize)
            {
                return false;
            }

            List<Tile> innerTiles = room.GetInnerTiles();
            List<Tile> borderTiles = room.GetBorderingTiles();

            List<Tile> allTiles = innerTiles.Union(borderTiles).ToList();

            foreach (FurnitureRequirement requirement in requiredFurniture)
            {
                if (allTiles.Count(tile => (tile.Furniture != null && (tile.Furniture.Type == requirement.type || tile.Furniture.HasTypeTag(requirement.typeTag)))) < requirement.count)
                {
                    return false;
                }
            }

            return true;
        }

        private void InvokeContextMenuLuaAction(ContextMenuAction action, Character character)
        {
            FunctionsManager.RoomBehavior.Call(action.Parameter, this, character);
        }

        [MoonSharpVisible(true)]
        private void UpdateOnChanged(RoomBehavior util)
        {
            if (Changed != null)
            {
                Changed(util);
            }
        }

        private void ReadXmlRequirements(XmlReader readerParent, bool isOptional = false)
        {
            XmlReader reader = readerParent.ReadSubtree();
            reader.Read();

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Furniture":
                        // Furniture must have either Type or TypeTag, try both, check for null later
                        string type = reader.GetAttribute("type");
                        string typeTag = reader.GetAttribute("typeTag");
                        int count = 0;
                        if (!isOptional)
                        {
                            int.TryParse(reader.GetAttribute("count"), out count);
                        }

                        requiredFurniture.Add(new FurnitureRequirement(type, typeTag, count));
                        break;
                    case "Size":
                        if (!isOptional)
                        {
                            int.TryParse(reader.GetAttribute("tiles"), out requiredSize);
                        }

                        break;
                }
            }
        }

        private struct FurnitureRequirement
        {
            public string type, typeTag;
            public int count;

            public FurnitureRequirement(string type, string typeTag, int count)
            {
                this.type = type;
                this.typeTag = typeTag;
                this.count = count;
            }
        }
    }
}
