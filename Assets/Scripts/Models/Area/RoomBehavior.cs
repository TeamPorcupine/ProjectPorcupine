#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Animation;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

namespace ProjectPorcupine.Rooms
{
    /// <summary>
    /// Room Behaviors are functions added to specific rooms, such as an airlock, a dining room, or an abattoir
    /// </summary>
    [MoonSharpUserData]
    public class RoomBehavior : IXmlSerializable, ISelectable, IPrototypable, IContextActionProvider
    {
        public Room Room;
        /// <summary>
        /// These context menu lua action are used to build the context menu of the utility.
        /// </summary>
        private List<ContextMenuLuaAction> contextMenuLuaActions;

        // This is the generic type of object this is, allowing things to interact with it based on it's generic type
        private HashSet<string> typeTags;

        private List<FurnitureRequirement> furnitureRequirements;

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
            furnitureRequirements = new List<FurnitureRequirement>();
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

            if (other.furnitureRequirements != null)
            {
                furnitureRequirements = new List<FurnitureRequirement>(other.furnitureRequirements);
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
        /// <value>Whether the utility is selected or not.</value>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets the string that defines the type of object the utility is. This gets queried by the visual system to 
        /// know what sprite to render for this RoomBehavior.
        /// </summary>
        /// <value>The type of the RoomBehavior.</value>
        public string Type { get; private set; }

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
        /// Gets the code used for Localization of the utility.
        /// </summary>
        public string LocalizationCode { get; private set; }

        /// <summary>
        /// Gets the description of the utility. This is used by localization.
        /// </summary>
        public string UnlocalizedDescription { get; private set; }

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
        public static RoomBehavior PlaceInstance(RoomBehavior proto, Room room)
        {
            if (proto.funcRoomValidation(room) == false)
            {
                Debug.ULogErrorChannel("RoomBehavior", "PlaceInstance -- Position Validity Function returned FALSE. " + proto.Name + " " + room.ID);
                return null;
            }

            // We know our placement destination is valid.
            RoomBehavior obj = proto.Clone();
            obj.Room = room;

            if (room.DesignateRoomBehavior(obj) == false)
            {
                // For some reason, we weren't able to place our object in this tile.
                // (Probably it was already occupied.)

                // Do NOT return our newly instantiated object.
                // (It will be garbage collected.)
                return null;
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
        /// Check if the position of the RoomBehavior is valid or not.
        /// This is called when placing the utility.
        /// </summary>
        /// <param name="tile">The base tile.</param>
        /// <returns>True if the tile is valid for the placement of the utility.</returns>
        public bool IsValidRoom(Room room)
        {
            return funcRoomValidation(room);
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
            writer.WriteAttributeString("Room", Room.ID.ToString());
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
                    case "Requirements":
                        ReadXmlRequirements(reader);
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
        /// Deconstructs the utility.
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
        public RoomBehavior Clone()
        {
            return new RoomBehavior(this);
        }

        private bool DefaultIsValidRoom(Room room)
        {
            if (room.TileCount < requiredSize) 
            {
                Debug.Log("Denied");
                return false;
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

        private void ReadXmlRequirements(XmlReader readerParent)
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
                        string typeTag = reader.GetAttribute("type");
                        int count = 1;
                        int.TryParse(reader.GetAttribute("count"), out count);
                        furnitureRequirements.Add(new FurnitureRequirement(type, typeTag, count));
                        break;
                    case "Size":
                        int.TryParse(reader.GetAttribute("tiles"), out requiredSize);
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