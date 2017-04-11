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
    public TemperatureDiffusion temperature;

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
        Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        if (SceneController.NewWorldSize != Vector3.zero)
        {
            Seed = SceneController.Seed;
        }

        Debug.LogWarning("World Seed: " + Seed);
        WorldGenerator.Instance.Generate(this, Seed);
        UnityDebugger.Debugger.Log("World", "Generated World");

        tileGraph = new Path_TileGraph(this);
        roomGraph = new Path_RoomGraph(this);

        // Make one character.
        CharacterManager.Create(GetTileAt((Width / 2) - 1, Height / 2, 0));
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

    public event Action<Tile> OnTileTypeChanged;

    public static World Current { get; protected set; }

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    // The tile depth of the world
    public int Depth { get; protected set; }

    /// <summary>
    /// Gets or sets the world seed.
    /// </summary>
    /// <value>The world seed.</value>
    public int Seed { get; protected set; }

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
    /// Gets the power network.
    /// </summary>
    /// <value>The power network.</value>
    public FluidNetwork FluidNetwork { get; private set; }

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

    public JToken RandomStateToJson()
    {
        // JSON.Net can't serialize the Random.State so we use JsonUtility
        return JToken.Parse(JsonUtility.ToJson(UnityEngine.Random.state));
    }

    public void RandomStateFromJson(JToken randomState)
    {
        if (randomState != null)
        {
            // JSON.Net can't serialize the Random.State so we use JsonUtility
            UnityEngine.Random.state = JsonUtility.FromJson<UnityEngine.Random.State>(randomState.ToString());
        }
    }

    public JObject ToJson()
    {
        JObject worldJson = new JObject();
        worldJson.Add("Seed", Seed);
        worldJson.Add("RandomState", RandomStateToJson());
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
        worldJson.Add("Time", TimeManager.Instance.ToJson());
        worldJson.Add("Scheduler", Scheduler.Scheduler.Current.ToJson());
        return worldJson;
    }

    public void ReadJson(string filename)
    {
        StreamReader reader = File.OpenText(filename);
        ReadJson((JObject)JToken.ReadFrom(new JsonTextReader(reader)));
    }

    public void ReadJson(JObject worldJson)
    {
        if (worldJson["Seed"] != null)
        {
            Seed = (int)worldJson["Seed"];
        }

        RandomStateFromJson(worldJson["RandomState"]);

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
        TimeManager.Instance.FromJson(worldJson["Time"]);
        Scheduler.Scheduler.Current.FromJson(worldJson["Scheduler"]);

        tileGraph = new Path_TileGraph(this);
    }

    public void ResizeWorld(Vector3 worldSize)
    {
        ResizeWorld((int)worldSize.x, (int)worldSize.y, (int)worldSize.z);
    }

    public void ResizeWorld(int width, int height, int depth)
    {
        if (width < Width || height < Height || depth < Depth)
        {
            if (width < Width)
            {
                UnityDebugger.Debugger.LogWarning("World", "Width too small: " + Width + " " + width);
            }

            if (height < Height)
            {
                UnityDebugger.Debugger.LogWarning("World", "Height too small: " + Height + " " + height);
            }

            if (depth < Depth)
            {
                UnityDebugger.Debugger.LogWarning("World", "Depth too small: " + Depth + " " + depth);
            }

            UnityDebugger.Debugger.LogError("World", "Shrinking the world is not presently supported");
            return;
        }

        if (width == Width && height == Height && depth == Depth)
        {
            // No change, just bail
            return;
        }

        int offsetX = (width - Width) / 2;
        int offsetY = ((height - Height) / 2) + 1;

        Tile[,,] oldTiles = (Tile[,,])tiles.Clone();
        tiles = new Tile[width, height, depth];

        int oldWidth = Width;
        int oldHeight = Height;
        int oldDepth = Depth;

        Width = width;
        Height = height;
        Depth = depth;

        FillTilesArray();
        tileGraph = null;
        roomGraph = null;

        // Reset temperature, so it properly sizes arrays to the new world size
        temperature.Resize();

        for (int x = 0; x < oldWidth; x++)
        {
            for (int y = 0; y < oldHeight; y++)
            {
                for (int z = 0; z < oldDepth; z++)
                {
                    tiles[x + offsetX, y + offsetY, z] = oldTiles[x, y, z];
                    oldTiles[x, y, z].MoveTile(x + offsetX, y + offsetY, z);
                }
            }   
        }
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

        FillTilesArray();

        FurnitureManager = new FurnitureManager();
        FurnitureManager.Created += OnFurnitureCreated;

        UtilityManager = new UtilityManager();
        CharacterManager = new CharacterManager();
        InventoryManager = new InventoryManager();
        jobQueue = new JobQueue();
        GameEventManager = new GameEventManager();
        PowerNetwork = new PowerNetwork();
        FluidNetwork = new FluidNetwork();
        temperature = new TemperatureDiffusion();
        ShipManager = new ShipManager();
        Wallet = new Wallet();
        CameraData = new CameraData();

        LoadSkybox();
        AddEventListeners();
    }

    private void FillTilesArray()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    tiles[x, y, z] = new Tile(x, y, z);
                    tiles[x, y, z].TileChanged += OnTileChangedCallback;
                    tiles[x, y, z].TileTypeChanged += OnTileTypeChangedCallback;
                    tiles[x, y, z].Room = RoomManager.OutsideRoom; // Rooms 0 is always going to be outside, and that is our default room
                }
            }
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
            UnityDebugger.Debugger.LogWarning("World", "No skyboxes detected! Falling back to black.");
        }
    }

    /// <summary>
    /// Calls update on characters.
    /// Also calls "OnFastUpdate" EventActions on visible furniture.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    private void TickEveryFrame(float deltaTime)
    {
        GameEventManager.Update(deltaTime);
        ShipManager.Update(deltaTime);
    }

    /// <summary>
    /// Calls the update functions on the systems that are updated on a fixed frequency.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    private void TickFixedFrequency(float deltaTime)
    {
        // Progress temperature modelling
        temperature.Update(deltaTime);
        PowerNetwork.Update(deltaTime);
        FluidNetwork.Update(deltaTime);
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
            // InvalidateTileGraph();    
            // Reset the pathfinding system
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
    }

    // Gets called whenever ANY tile changes type
    private void OnTileTypeChangedCallback(Tile t)
    {
        if (OnTileTypeChanged == null)
        {
            return;
        }

        OnTileTypeChanged(t);

        if (tileGraph != null)
        {
            tileGraph.RegenerateGraphAtTile(t);
            tileGraph.RegenerateGraphAtTile(t.Down());
        }
    }
}
