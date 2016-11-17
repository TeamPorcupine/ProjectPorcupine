#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectPorcupine.PowerNetwork;
using ProjectPorcupine.Rooms;
using UnityEngine;

[MoonSharpUserData]
public class World
{
    // TODO: Should this be also saved with the world data?
    // If so - beginner task!
    public readonly string GameVersion = "Someone_will_come_up_with_a_proper_naming_scheme_later";
    public Material skybox;

    // Store all temperature information
    public Temperature temperature;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;
    public Path_RoomGraph roomGraph;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;

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
        WorldGenerator.Instance.Generate(this, seed);
        Debug.ULogChannel("World", "Generated World");

        tileGraph = new Path_TileGraph(this);

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

    public event Action<Tile> OnTileChanged;

    public static World Current { get; protected set; }

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    // The tile depth of the world
    public int Depth { get; protected set; }

    /// <summary>
    /// Gets the inventory manager.
    /// </summary>
    /// <value>The inventory manager.</value>
    public InventoryManager InventoryManager { get; protected set; }

    /// <summary>
    /// Gets the character manager.
    /// </summary>
    /// <value>The character manager.</value>
    public CharacterManager CharacterManager { get; protected set; }

    /// <summary>
    /// Gets the furniture manager.
    /// </summary>
    /// <value>The furniture manager.</value>
    public FurnitureManager FurnitureManager { get; private set; }

    /// Gets the utility manager.
    /// </summary>
    /// <value>The furniture manager.</value>
    public UtilityManager UtilityManager { get; private set; }

    /// <summary>
    /// Gets the wallet.
    /// </summary>
    /// <value>The wallet.</value>
    public Wallet Wallet { get; private set; }

    /// <summary>
    /// Gets the power network.
    /// </summary>
    /// <value>The power network.</value>
    public PowerNetwork PowerNetwork { get; private set; }

    /// <summary>
    /// Gets the room manager.
    /// </summary>
    /// <value>The room manager.</value>
    public RoomManager RoomManager { get; private set; }

    /// <summary>
    /// Gets the game event manager.
    /// </summary>
    /// <value>The game event manager.</value>
    public GameEventManager GameEventManager { get; private set; }

    /// <summary>
    /// Gets the ship manager.
    /// </summary>
    /// <value>The ship manager.</value>
    public ShipManager ShipManager { get; private set; }

    /// <summary>
    /// Gets the camera data.
    /// </summary>
    /// <value>The camera data.</value>
    public CameraData CameraData { get; private set; }

    /// <summary>
    /// Adds the listeners to the required Time Manager events.
    /// </summary>
    public void AddEventListeners()
    {
        TimeManager.Instance.EveryFrameUnpaused += TickEveryFrame;
        TimeManager.Instance.FixedFrequencyUnpaused += TickFixedFrequency;
    }

    /// <summary>
    /// Notify world that the camera moved, so we can check which entities are visible to the camera.
    /// The invisible enities can be updated less frequent for better performance.
    /// </summary>
    public void OnCameraMoved(Bounds cameraBounds)
    {
        FurnitureManager.OnCameraMoved(cameraBounds);
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

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    /// <summary>
    /// Reserves the furniture's work spot, preventing it from being built on. Will not reserve a workspot inside of the furniture.
    /// </summary>
    /// <param name="furniture">The furniture whose workspot will be reserved.</param>
    /// <param name="tile">The tile on which the furniture is located, for furnitures which don't have a tile, such as prototypes.</param>
    public void ReserveTileAsWorkSpot(Furniture furniture, Tile tile = null)
    {
        if (tile == null)
        {
            tile = furniture.Tile;
        }

        // if it's an internal workspot bail before reserving.
        if (furniture.Jobs.WorkSpotIsInternal())
        {
            return;
        }

        GetTileAt(
            tile.X + (int)furniture.Jobs.WorkSpotOffset.x,
            tile.Y + (int)furniture.Jobs.WorkSpotOffset.y,
            tile.Z)
            .ReservedAsWorkSpotBy.Add(furniture);
    }

    /// <summary>
    /// Unreserves the furniture's work spot, allowing it to be built on.
    /// </summary>
    /// <param name="furniture">The furniture whose workspot will be unreserved.</param>
    /// <param name="tile">The tile on which the furniture is located, for furnitures which don't have a tile, such as prototypes.</param>
    public void UnreserveTileAsWorkSpot(Furniture furniture, Tile tile = null)
    {
        if (tile == null)
        {
            tile = furniture.Tile;
        }

        World.Current.GetTileAt(
            tile.X + (int)furniture.Jobs.WorkSpotOffset.x,
            tile.Y + (int)furniture.Jobs.WorkSpotOffset.y,
            tile.Z)
            .ReservedAsWorkSpotBy.Remove(furniture);
    }

    public bool IsRoomBehaviorValidForRoom(string roomBehaviorType, Room room)
    {
        return PrototypeManager.RoomBehavior.Get(roomBehaviorType).IsValidRoom(room);
    }

    public JToken TilesToJson()
    {
        JArray tileJArray = new JArray();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    if (tiles[x, y, z].Type != TileType.Empty)
                    {
                        tileJArray.Add(tiles[x, y, z].ToJson());
                    }
                }
            }
        }

        return tileJArray;
    }

    public void TilesFromJson(JToken tilesToken)
    {
        JArray tilesJArray = (JArray)tilesToken;

        foreach (JToken tileToken in tilesJArray)
        {
            int x = (int)tileToken["X"];
            int y = (int)tileToken["Y"];
            int z = (int)tileToken["Z"];

            tiles[x, y, z].FromJson(tileToken);
        }
    }

    public JObject ToJson()
    {
        JObject worldJson = new JObject();
        worldJson.Add("Width", Width.ToString());
        worldJson.Add("Height", Height.ToString());
        worldJson.Add("Depth", Depth.ToString());
        worldJson.Add("Rooms", RoomManager.ToJson());
        worldJson.Add("Tiles", TilesToJson());
        worldJson.Add("Inventories", InventoryManager.ToJson());
        worldJson.Add("Furnitures", FurnitureManager.ToJson());
        worldJson.Add("Utilities", UtilityManager.ToJson());
        worldJson.Add("RoomBehaviors", RoomManager.BehaviorsToJson());
        worldJson.Add("Characters", CharacterManager.ToJson());
        worldJson.Add("CameraData", CameraData.ToJson());
        worldJson.Add("Skybox", skybox.name);
        worldJson.Add("Wallet", Wallet.ToJson());
        worldJson.Add("Scheduler", Scheduler.Scheduler.Current.ToJson());
        return worldJson;
    }

    public void ReadJson(string filename)
    {
        StreamReader reader = File.OpenText(filename);
        JObject worldJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
        Width = (int)worldJson["Width"];
        Height = (int)worldJson["Height"];
        Depth = (int)worldJson["Depth"];

        SetupWorld(Width, Height, Depth);

        RoomManager.FromJson(worldJson["Rooms"]);
        TilesFromJson(worldJson["Tiles"]);
        InventoryManager.FromJson(worldJson["Inventories"]);
        FurnitureManager.FromJson(worldJson["Furnitures"]);
        UtilityManager.FromJson(worldJson["Utilities"]);
        RoomManager.BehaviorsFromJson(worldJson["RoomBehaviors"]);
        CharacterManager.FromJson(worldJson["Characters"]);
        CameraData.FromJson(worldJson["CameraData"]);
        LoadSkybox((string)worldJson["Skybox"]);
        Wallet.FromJson(worldJson["Wallet"]);
        Scheduler.Scheduler.Current.FromJson(worldJson["Scheduler"]);

        tileGraph = new Path_TileGraph(this);
    }

    private void SetupWorld(int width, int height, int depth)
    {
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

        FurnitureManager = new FurnitureManager();
        FurnitureManager.Created += OnFurnitureCreated;

        UtilityManager = new UtilityManager();
        CharacterManager = new CharacterManager();
        InventoryManager = new InventoryManager();
        jobQueue = new JobQueue();
        GameEventManager = new GameEventManager();
        PowerNetwork = new PowerNetwork();
        temperature = new Temperature();
        ShipManager = new ShipManager();
        Wallet = new Wallet();
        CameraData = new CameraData();

        LoadSkybox();
        AddEventListeners();
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

    /// <summary>
    /// Calls update on characters.
    /// Also calls "OnFastUpdate" EventActions on visible furniture.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    private void TickEveryFrame(float deltaTime)
    {
        CharacterManager.Update(deltaTime);
        FurnitureManager.TickEveryFrame(deltaTime);
        UtilityManager.TickEveryFrame(deltaTime);
        GameEventManager.Update(deltaTime);
        ShipManager.Update(deltaTime);
    }

    /// <summary>
    /// Calls the update functions on the systems that are updated on a fixed frequency.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    private void TickFixedFrequency(float deltaTime)
    {
        FurnitureManager.TickFixedFrequency(deltaTime);
        UtilityManager.TickFixedFrequency(deltaTime);

        // Progress temperature modelling
        temperature.Update();
        PowerNetwork.Update(deltaTime);
    }

    /// <summary>
    /// Called when a furniture is created so that we can regenerate the tile graph.
    /// </summary>
    /// <param name="furniture">Furniture.</param>
    private void OnFurnitureCreated(Furniture furniture)
    {
        if (furniture.MovementCost != 1)
        {
            // Since tiles return movement cost as their base cost multiplied
            // by the furniture's movement cost, a furniture movement cost
            // of exactly 1 doesn't impact our pathfinding system, so we can
            // occasionally avoid invalidating pathfinding graphs.
            // InvalidateTileGraph();    // Reset the pathfinding system
            if (tileGraph != null)
            {
                tileGraph.RegenerateGraphAtTile(furniture.Tile);
            }
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

        if (tileGraph != null)
        {
            tileGraph.RegenerateGraphAtTile(t);
            tileGraph.RegenerateGraphAtTile(t.Down());
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

        if (roomGraph.nodes.Count() != 8)
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

                        if (node.edges[0].node.data != world.RoomManager[2] || node.edges[1].node.data != world.RoomManager[4])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 0 supposed to have edges to Room 2 and Room 4.");
                            Debug.ULogErrorChannel(
                                "Path_RoomGraph",
                                string.Format("Instead has: {1} and {2}", node.edges[0].node.data.ID, node.edges[1].node.data.ID));
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

                        if (node.edges[0].node.data != world.RoomManager[3])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 1 supposed to have edge to Room 3.");
                            Debug.ULogErrorChannel("Path_RoomGraph", "Instead has: " + node.edges[0].node.data.ID.ToString());
                            errorCount++;
                        }

                        break;
                    case 2: // Room 2 has two edges both connecting to the outside room, one connecting to room 1 and one connecting to room 5
                        if (edgeCount != 2)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have 2 edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[3] || node.edges[1].node.data != world.RoomManager[0])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 2 supposed to have edges to Room 3 and Room 0.");
                            Debug.ULogErrorChannel(
                                "Path_RoomGraph",
                                string.Format("Instead has: {0} and {1}", node.edges[0].node.data.ID, node.edges[1].node.data.ID));
                            errorCount++;
                        }

                        break;
                    case 3: // Room 3 has 4 edges, connecting to Rooms 1, 2, 4, and 7
                        if (edgeCount != 4)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 3 supposed to have 4 edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[4] || node.edges[1].node.data != world.RoomManager[7] ||
                            node.edges[2].node.data != world.RoomManager[1] || node.edges[3].node.data != world.RoomManager[2])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 3 supposed to have edges to Rooms 4, 7, 1, and 2");
                            string errorMessage = string.Format(
                                "Instead has: {0}, {1}, {2}, and {3}",
                                node.edges[0].node.data.ID,
                                node.edges[1].node.data.ID,
                                node.edges[2].node.data.ID,
                                node.edges[3].node.data.ID);
                            Debug.ULogErrorChannel("Path_RoomGraph", errorMessage);
                            errorCount++;
                        }

                        break;
                    case 4: // Room 2 has two edges both connecting to the outside room, one connecting to room 1 and one connecting to room 5
                        if (edgeCount != 2)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 4 supposed to have 2 edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[0] || node.edges[1].node.data != world.RoomManager[3])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 4 supposed to have edges to Room 0 and Room 3.");
                            Debug.ULogErrorChannel(
                                "Path_RoomGraph",
                                string.Format("Instead has: {0} and {1}", node.edges[0].node.data.ID, node.edges[1].node.data.ID));

                            // "Instead has: " + node.edges[0].node.data.ID.ToString() + " and "
                            // + node.edges[1].node.data.ID.ToString());
                            errorCount++;
                        }

                        break;
                    case 5: // Room 5 has no edges
                        if (edgeCount != 0)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 5 supposed to have no edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        break;
                    case 6: // Room 4 has no edges
                        if (edgeCount != 0)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 6 supposed to have no edges. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        break;
                    case 7: // Room 5 has one edge to Room 3
                        if (edgeCount != 1)
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 7 supposed to have 1 edge. Instead has: " + edgeCount);
                            errorCount++;
                            continue;
                        }

                        if (node.edges[0].node.data != world.RoomManager[3])
                        {
                            Debug.ULogErrorChannel("Path_RoomGraph", "Room 7 supposed to have edge to Room 3.");
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
