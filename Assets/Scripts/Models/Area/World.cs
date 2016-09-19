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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using ProjectPorcupine.PowerNetwork;
using ProjectPorcupine.Rooms;
using UnityEngine;

[MoonSharpUserData]
public class World : IXmlSerializable
{
    // TODO: Should this be also saved with the world data?
    // If so - beginner task!
    public readonly string GameVersion = "Someone_will_come_up_with_a_proper_naming_scheme_later";

    public List<Furniture> furnitures;
    public List<Utility> utilities;
    public InventoryManager inventoryManager;
    public Material skybox;

    public CameraData cameraData;

    // Store all temperature information
    public Temperature temperature;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;
    public Path_RoomGraph roomGraph;

    public Wallet Wallet;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;
    public JobQueue jobWaitingQueue;

    // A three-dimensional array to hold our tile data.
    private Tile[,,] tiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="width">Width in tiles.</param>
    /// <param name="height">Height in tiles.</param>
    /// <param name="depth">Depth in amount.</param>
    public World(int width, int height, int depth)
    {
        // Creates an empty world.
        SetupWorld(width, height, depth);
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        WorldGenerator.Generate(this, seed);
        Debug.ULogChannel("World", "Generated World");

        // Make one character
        Debug.Log("Generated World");

        // Adding air to enclosed rooms
        foreach (Room room in this.RoomManager)
        {
            if (room.ID > 0)
            {
                room.ChangeGas("O2", 0.2f * room.TileCount);
                room.ChangeGas("N2", 0.8f * room.TileCount);
            }
        }

        // Make one character.
        CharacterManager.Create(GetTileAt(Width / 2, Height / 2, 0));

        TestRoomGraphGeneration(this);
    }

    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    public World()
    {
    }

    /// <summary>
    /// Releases the TimeManager events when <see cref="World"/> is reclaimed by garbage collection.
    /// </summary>
    ~World()
    {
        TimeManager.Instance.EveryFrameUnpaused -= TickEveryFrame;
        TimeManager.Instance.FixedFrequencyUnpaused -= TickFixedFrequency;
    }

    public event Action<Furniture> OnFurnitureCreated;

    public event Action<Utility> OnUtilityCreated;

    public event Action<Tile> OnTileChanged;

    public static World Current { get; protected set; }

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    // The tile depth of the world
    public int Depth { get; protected set; }

    /// <summary>
    /// Gets or sets the character manager.
    /// </summary>
    /// <value>The character manager.</value>
    public CharacterManager CharacterManager { get; protected set; }

    public PowerNetwork PowerNetwork { get; private set; }

    public RoomManager RoomManager { get; private set; }

    public int CountFurnitureType(string type)
    {
        int count = furnitures.Count(f => f.Type == type);
        return count;
    }

    public void AddEventListeners()
    {
        TimeManager.Instance.EveryFrameUnpaused += TickEveryFrame;
        TimeManager.Instance.FixedFrequencyUnpaused += TickFixedFrequency;
    }

    public void TickEveryFrame(float deltaTime)
    {
        CharacterManager.Update(deltaTime);
    }

    public void TickFixedFrequency(float deltaTime)
    {
        // Update Furniture
        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

        // Progress temperature modelling
        temperature.Update();
        PowerNetwork.Update(deltaTime);
    }

    /// <summary>
    /// Gets the tile data at x and y.
    /// </summary>
    /// <returns>The <see cref="Tile"/> or null if called with invalid arguments.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int y, int z)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0 || z >= Depth || z < 0)
        {
            return null;
        }

        return tiles[x, y, z];
    }

    // Currently Hardcoded to have center tile at highest layer, to play nice with pathfinding.
    public Tile GetCenterTile()
    {
        return GetTileAt(Width / 2, Height / 2, 0);
    }

    public Tile GetFirstCenterTileWithNoInventory(int maxOffset)
    {
        return GetFirstTileWithNoInventoryAround(maxOffset, Width / 2, Height / 2, 0);
    }

    public Tile GetFirstTileWithNoInventoryAround(int maxOffset, int centerX, int centerY, int centerZ)
    {
        for (int offset = 0; offset <= maxOffset; offset++)
        {
            int offsetX = 0;
            int offsetY = 0;
            Tile tile;

            // searching top & bottom line of the square
            for (offsetX = -offset; offsetX <= offset; offsetX++)
            {
                offsetY = offset;
                tile = GetTileAt(centerX + offsetX, centerY + offsetY, centerZ);
                if (tile.Inventory == null)
                {
                    return tile;
                }

                offsetY = -offset;
                tile = GetTileAt(centerX + offsetX, centerY + offsetY, centerZ);
                if (tile.Inventory == null)
                {
                    return tile;
                }
            }

            // searching left & right line of the square
            for (offsetY = -offset; offsetY <= offset; offsetY++)
            {
                offsetX = offset;
                tile = GetTileAt(centerX + offsetX, centerY + offsetY, centerZ);
                if (tile.Inventory == null)
                {
                    return tile;
                }

                offsetX = -offset;
                tile = GetTileAt(centerX + offsetX, centerY + offsetY, centerZ);
                if (tile.Inventory == null)
                {
                    return tile;
                }
            }
        }

        return null;
    }

    public Furniture PlaceFurniture(string type, Tile t, bool doRoomFloodFill = true)
    {
        if (PrototypeManager.Furniture.Has(type) == false)
        {
            Debug.ULogErrorChannel("World", "furniturePrototypes doesn't contain a proto for key: " + type);
            return null;
        }

        Furniture furn = PrototypeManager.Furniture.Get(type);

        return PlaceFurniture(furn, t, doRoomFloodFill);
    }

    public Utility PlaceUtility(string objectType, Tile tile, bool doRoomFloodFill = false)
    {
        if (PrototypeManager.Utility.Has(objectType) == false)
        {
            Debug.ULogErrorChannel("World", "PrototypeManager.Utility doesn't contain a proto for key: " + objectType);
            return null;
        }

        Utility util = PrototypeManager.Utility.Get(objectType);

        return PlaceUtility(util, tile, doRoomFloodFill);
    }

    /// <summary>
    /// Reserves the furniture's work spot, preventing it from being built on.
    /// </summary>
    /// <param name="furn">The furniture whose workspot will be reserved.</param>
    /// <param name="tile">The tile on which the furniture is located, for furnitures which don't have a tile, such as prototypes.</param>
    public void ReserveTileAsWorkSpot(Furniture furn, Tile tile = null)
    {
        if (tile == null)
        {
            tile = furn.Tile;
        }

        World.Current.GetTileAt(
            tile.X + (int)furn.Jobs.WorkSpotOffset.x, 
            tile.Y + (int)furn.Jobs.WorkSpotOffset.y, 
            tile.Z)
            .ReservedAsWorkSpotBy.Add(furn);
    }

    /// <summary>
    /// Unreserves the furniture's work spot, allowing it to be built on.
    /// </summary>
    /// <param name="furn">The furniture whose workspot will be unreserved.</param>
    /// <param name="tile">The tile on which the furniture is located, for furnitures which don't have a tile, such as prototypes.</param>
    public void UnreserveTileAsWorkSpot(Furniture furn, Tile tile = null)
    {
        if (tile == null)
        {
            tile = furn.Tile;
        }

        World.Current.GetTileAt(
            tile.X + (int)furn.Jobs.WorkSpotOffset.x, 
            tile.Y + (int)furn.Jobs.WorkSpotOffset.y,
            tile.Z)
            .ReservedAsWorkSpotBy.Remove(furn);
    }

    public Furniture PlaceFurniture(Furniture furniture, Tile t, bool doRoomFloodFill = true)
    {
        Furniture furn = Furniture.PlaceInstance(furniture, t);

        if (furn == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furn.Removed += OnFurnitureRemoved;
        furnitures.Add(furn);

        // Do we need to recalculate our rooms?
        if (doRoomFloodFill && furn.RoomEnclosure)
        {
            RoomManager.DoRoomFloodFill(furn.Tile, true);
        }

        if (OnFurnitureCreated != null)
        {
            OnFurnitureCreated(furn);

            if (furn.MovementCost != 1)
            {
                // Since tiles return movement cost as their base cost multiplied
                // by the furniture's movement cost, a furniture movement cost
                // of exactly 1 doesn't impact our pathfinding system, so we can
                // occasionally avoid invalidating pathfinding graphs.
                // InvalidateTileGraph();    // Reset the pathfinding system
                if (tileGraph != null)
                {
                    tileGraph.RegenerateGraphAtTile(t);
                }
            }
        }

        return furn;
    }

    public Utility PlaceUtility(Utility proto, Tile tile, bool doRoomFloodFill = true)
    {
        Utility utility = Utility.PlaceInstance(proto, tile);

        if (utility == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        utility.Removed += OnUtilityRemoved;
        utilities.Add(utility);

        if (OnUtilityCreated != null)
        {
            OnUtilityCreated(utility);
        }

        return utility;
    }

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return PrototypeManager.Furniture.Get(furnitureType).IsValidPosition(t);
    }

    public bool IsFurnitureWorkSpotClear(string furnitureType, Tile tile)
    {
        Furniture proto = PrototypeManager.Furniture.Get(furnitureType);
        if (proto.Jobs != null && GetTileAt((int)(tile.X + proto.Jobs.WorkSpotOffset.x), (int)(tile.Y + proto.Jobs.WorkSpotOffset.y), (int)tile.Z).Furniture != null)
        {
            return false;
        }

        return true;
    }

    public bool IsUtilityPlacementValid(string furnitureType, Tile tile)
    {
        return PrototypeManager.Utility.Get(furnitureType).IsValidPosition(tile);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save info here
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());
        writer.WriteAttributeString("Depth", Depth.ToString());

        writer.WriteStartElement("Rooms");
        foreach (Room r in RoomManager)
        {
            if (RoomManager.OutsideRoom == r)
            {
                // Skip the outside room. Alternatively, should SetupWorld be changed to not create one?
                continue;
            }

            writer.WriteStartElement("Room");
            r.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    if (tiles[x, y, z].Type != TileType.Empty)
                    {
                        writer.WriteStartElement("Tile");
                        tiles[x, y, z].WriteXml(writer);
                        writer.WriteEndElement();
                    }
                }
            }
        }

        writer.WriteEndElement();
        writer.WriteStartElement("Inventories");
        foreach (Inventory inventory in inventoryManager.Inventories.SelectMany(pair => pair.Value))
        {
            // If we don't have a tile, that means this is in a character's inventory (or some other non-tile location
            //      which means we shouldn't save that Inventory here, the character will take care of saving and loading
            //      the inventory properly.
            if (inventory.Tile != null)
            {
                writer.WriteStartElement("Inventory");
                inventory.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in furnitures)
        {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Utilities");
        foreach (Utility util in utilities)
        {
            writer.WriteStartElement("Utility");
            util.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        CharacterManager.WriteXml(writer);
        writer.WriteEndElement();

        writer.WriteStartElement("CameraData");
        cameraData.WriteXml(writer);
        writer.WriteEndElement();

        writer.WriteElementString("Skybox", skybox.name);

        writer.WriteStartElement("Wallet");
        foreach (Currency currency in Wallet.Currencies.Values)
        {
            writer.WriteStartElement("Currency");
            currency.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        Scheduler.Scheduler.Current.WriteXml(writer);
    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));
        Depth = int.Parse(reader.GetAttribute("Depth"));

        SetupWorld(Width, Height, Depth);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Rooms":
                    ReadXml_Rooms(reader);
                    break;
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Inventories":
                    ReadXml_Inventories(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "CameraData":
                    ReadXml_CameraData(reader);
                    break;
                case "Utilities":
                    ReadXml_Utilities(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
                case "Skybox":
                    LoadSkybox(reader.ReadElementString("Skybox"));
                    break;
                case "Wallet":
                    ReadXml_Wallet(reader);
                    break;
                case "Scheduler":
                    Scheduler.Scheduler.Current.ReadXml(reader);
                    break;
            }
        }
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        furnitures.Remove(furn);
    }

    public void OnUtilityRemoved(Utility util)
    {
        utilities.Remove(util);
    }

    private void ReadXml_Wallet(XmlReader reader)
    {
        if (reader.ReadToDescendant("Currency"))
        {
            do
            {
                Currency c = new Currency
                {
                    Name = reader.GetAttribute("Name"),
                    ShortName = reader.GetAttribute("ShortName"),
                    Balance = float.Parse(reader.GetAttribute("Balance"))
                };

                Wallet.Currencies[c.Name] = c;
            }
            while (reader.ReadToNextSibling("Character"));
        }
    }

    private void LoadSkybox(string name = null)
    {
        Material[] skyboxes = Resources.LoadAll("Skyboxes", typeof(Material)).Cast<Material>().ToArray();
        Material newSkybox = null;

        if (skyboxes.Length > 0)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (Material skybox in skyboxes)
                {
                    if (name.Equals(skybox.name))
                    {
                        newSkybox = skybox;
                        break;
                    }
                }
            }

            // Maybe we passed in a name that doesn't exist? Pick a random skybox.
            if (newSkybox == null)
            {
                newSkybox = skyboxes[(int)(UnityEngine.Random.value * skyboxes.Length)];
            }

            // Unload unused skyboxes
            foreach (Material skybox in skyboxes)
            {
                if (!newSkybox.name.Equals(skybox.name))
                {
                    Resources.UnloadAsset(skybox);
                }
            }

            this.skybox = newSkybox;
            RenderSettings.skybox = this.skybox;
        }
        else
        {
            Debug.ULogWarningChannel("World", "No skyboxes detected! Falling back to black.");
        }
    }

    private void SetupWorld(int width, int height, int depth)
    {
        jobQueue = new JobQueue();
        jobWaitingQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        Current = this;

        Width = width;
        Height = height;
        Depth = depth;

        tiles = new Tile[Width, Height, Depth];

        RoomManager = new RoomManager();
        RoomManager.Adding += (room) => roomGraph = null;
        RoomManager.Removing += (room) => roomGraph = null;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    tiles[x, y, z] = new Tile(x, y, z);
                    tiles[x, y, z].TileChanged += OnTileChangedCallback;
                    tiles[x, y, z].Room = RoomManager.OutsideRoom; // Rooms 0 is always going to be outside, and that is our default room
                }
            }
        }

        CreateWallet();

        furnitures = new List<Furniture>();
        utilities = new List<Utility>();
        CharacterManager = new CharacterManager();
        inventoryManager = new InventoryManager();
        cameraData = new CameraData();
        PowerNetwork = new ProjectPorcupine.PowerNetwork.PowerNetwork();
        temperature = new Temperature(Width, Height);

        LoadSkybox();
        AddEventListeners();
    }

    private void CreateWallet()
    {
        Wallet = new Wallet();

        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        string filePath = System.IO.Path.Combine(dataPath, "Currency.xml");
        string xmlText = System.IO.File.ReadAllText(filePath);
        LoadCurrencyFromFile(xmlText);

        DirectoryInfo[] mods = WorldController.Instance.modsManager.GetMods();
        foreach (DirectoryInfo mod in mods)
        {
            string xmlModFile = System.IO.Path.Combine(mod.FullName, "Currency.xml");
            if (File.Exists(xmlModFile))
            {
                string xmlModText = System.IO.File.ReadAllText(xmlModFile);
                LoadCurrencyFromFile(xmlModText);
            }
        }
    }

    private void LoadCurrencyFromFile(string xmlText)
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

        if (reader.ReadToDescendant("Currencies"))
        {
            try
            {
                Wallet.ReadXmlPrototype(reader);
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading Currency " + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
            }
        }
        else
        {
            Debug.LogError("Did not find a 'Currencies' element in the prototype definition file.");
        }
    }

    // Gets called whenever ANY tile changes
    private void OnTileChangedCallback(Tile t)
    {
        if (OnTileChanged == null)
        {
            return;
        }

        OnTileChanged(t);

        // InvalidateTileGraph();
        if (tileGraph != null)
        {
            tileGraph.RegenerateGraphAtTile(t);
        }
    }

    private void ReadXml_Tiles(XmlReader reader)
    {
        // We are in the "Tiles" element, so read elements until
        // we run out of "Tile" nodes.
        if (reader.ReadToDescendant("Tile"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                int z = int.Parse(reader.GetAttribute("Z"));
                tiles[x, y, z].ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    private void ReadXml_Inventories(XmlReader reader)
    {
        Debug.ULogChannel("World", "ReadXml_Inventories");

        if (reader.ReadToDescendant("Inventory"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                int z = int.Parse(reader.GetAttribute("Z"));

                // Create our inventory from the file
                Inventory inv = new Inventory(
                    reader.GetAttribute("type"),
                    int.Parse(reader.GetAttribute("stackSize")),
                    int.Parse(reader.GetAttribute("maxStackSize")))
                {
                    Locked = bool.Parse(reader.GetAttribute("locked"))
                };

                inventoryManager.PlaceInventory(tiles[x, y, z], inv);
            }
            while (reader.ReadToNextSibling("Inventory"));
        }
    }

    private void ReadXml_Furnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                int z = int.Parse(reader.GetAttribute("Z"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("type"), tiles[x, y, z], false);
                furn.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Furniture"));
        }
    }

    private void ReadXml_Utilities(XmlReader reader)
    {
        if (reader.ReadToDescendant("Utility"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                int z = int.Parse(reader.GetAttribute("Z"));

                Utility util = PlaceUtility(reader.GetAttribute("objectType"), tiles[x, y, z], false);
                util.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Utility"));
        }
    }

    private void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                Room r = new Room();
                RoomManager.Add(r);
                r.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Room"));
        }
    }

    private void ReadXml_CameraData(XmlReader reader)
    {
        cameraData.ReadXml(reader);
    }

    private void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                Character character;

                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                int z = int.Parse(reader.GetAttribute("Z"));

                if (reader.GetAttribute("r") != null)
                {
                    Color color = ColorUtilities.ParseColorFromString(reader.GetAttribute("r"), reader.GetAttribute("g"), reader.GetAttribute("b"));
                    Color colorUni = ColorUtilities.ParseColorFromString(reader.GetAttribute("rUni"), reader.GetAttribute("gUni"), reader.GetAttribute("bUni"));
                    Color colorSkin = ColorUtilities.ParseColorFromString(reader.GetAttribute("rSkin"), reader.GetAttribute("gSkin"), reader.GetAttribute("bSkin"));
                    character = CharacterManager.Create(tiles[x, y, z], color, colorUni, colorSkin);
                }
                else
                {
                    character = CharacterManager.Create(tiles[x, y, z]);
                }

                character.name = reader.GetAttribute("name");
                character.ReadXml(reader);

                // Read the children elements.
                // TODO: This should either not be XML, or use XmlSerializer.
                while (reader.Read())
                {
                    // Read until the end of the character.
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }

                    switch (reader.Name)
                    {
                        case "Stats":
                            character.ReadStatsFromSave(reader);
                            break;
                        case "Inventories":
                            if (reader.ReadToDescendant("Inventory"))
                            {
                                do
                                {
                                    // Create our inventory from the file
                                    Inventory inv = new Inventory(
                                        reader.GetAttribute("type"),
                                        int.Parse(reader.GetAttribute("stackSize")),
                                        int.Parse(reader.GetAttribute("maxStackSize")))
                                    {
                                        Locked = bool.Parse(reader.GetAttribute("locked"))
                                    };

                                    inventoryManager.PlaceInventory(character, inv);
                                }
                                while (reader.ReadToNextSibling("Inventory"));
                            }

                            break;
                    }
                }
            }
            while (reader.ReadToNextSibling("Character"));
        }
    }

    #region TestFunctions
    /// <summary>
    /// Tests the room graph generation for the default world.
    /// </summary>
    private void TestRoomGraphGeneration(World world)
    {
        // FIXME: This code is fugly!!!
        // TODO: Make it work for other testing maps?

        // roomGraph is auto-generated by Path_AStar if needed
        // doing this explicitly here to make sure we have one now
        roomGraph = new Path_RoomGraph(world);

        int errorCount = 0;

        if (roomGraph.nodes.Count() != 6)
        {
            Debug.ULogErrorChannel("Path_RoomGraph", "Generated incorrect number of nodes: " + roomGraph.nodes.Count().ToString());
            errorCount++;
        }

        foreach (Room r in world.RoomManager)
        {
            if (roomGraph.nodes.ContainsKey(r) == false)
            {
                Debug.ULogErrorChannel("Path_RoomGraph", "Does not contain room: " + r.ID);
                errorCount++;
            }
            else
            {
                Path_Node<Room> node = roomGraph.nodes[r];
                int edgeCount = node.edges.Count();
                switch (r.ID)
                {
                    case 0: // the outside room has two edges both connecting to room 2
                        if (edgeCount != 2)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 0 supposed to have 2 edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[2] || node.edges[1].node.data != world.RoomManager[2])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 0 supposed to have edges to Room 2.");
                            Debug.ULogErrorChannel(
                                "Path_RoomGraph",
                                string.Format("Instead has: {1} and {2}", node.edges[0].node.data.ID, node.edges[1].node.data.ID));

                            // "Instead has: " + node.edges[0].node.data.ID.ToString() + " and "
                            // + node.edges[1].node.data.ID.ToString());
                            errorCount++;
                        }

                        break;
                    case 1: // Room 1 has one edge connecting to room 2
                        if (edgeCount != 1)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 1 supposed to have 1 edge. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[2])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 1 supposed to have edge to Room 2.");
                            Debug.ULogErrorChannel("Path_RoomGraph", "Instead has: " + node.edges[0].node.data.ID.ToString());
                            errorCount++;
                        }

                        break;
                    case 2: // Room 2 has two edges both connecting to the outside room, one connecting to room 1 and one connecting to room 5
                        if (edgeCount != 4)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have 4 edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        // group edges by target room
                        IEnumerable<IGrouping<Room, Path_Edge<Room>>> query =
                            node.edges.GroupBy(edge => edge.node.data, edge => edge);

                        foreach (IGrouping<Room, Path_Edge<Room>> group in query)
                        {
                            switch (group.Key.ID)
                            {
                                case 0:
                                    if (group.Count() != 2)
                                    {
                                        Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have two edges to Room 0.");
                                        errorCount++;
                                    }

                                    break;
                                case 1:
                                    if (group.Count() != 1)
                                    {
                                        Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have one edge to Room 1.");
                                        errorCount++;
                                    }

                                    break;
                                case 5:
                                    if (group.Count() != 1)
                                    {
                                        Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have one edge to Room 5.");
                                        errorCount++;
                                    }

                                    break;
                                default:
                                    Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 only supposed to have edges to Rooms 0, 1 and 5.");
                                    errorCount++;
                                    break;
                            }
                        }

                        break;
                    case 3: // Room 3 has no edges
                        if (edgeCount != 0)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 3 supposed to have no edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        break;
                    case 4: // Room 4 has no edges
                        if (edgeCount != 0)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 4 supposed to have no edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        break;
                    case 5: // Room 5 has one edge to Room 2
                        if (edgeCount != 1)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 5 supposed to have 1 edge. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[2])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 5 supposed to have edge to Room 2.");
                            Debug.ULogErrorChannel("Path_RoomGraph", "Instead has: " + node.edges[0].node.data.ID.ToString());
                            errorCount++;
                        }

                        break;
                    default:
                        Debug.ULogErrorChannel("Path_RoomGraph", "Unknown room ID: " + r.ID);
                        errorCount++;
                        break;
                }
            }
        }

        if (errorCount == 0)
        {
            Debug.ULogChannel("Path_RoomGraph", "TestRoomGraphGeneration completed without errors!");
        }
    }
    #endregion
}