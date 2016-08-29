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
using Power;
using UnityEngine;

[MoonSharpUserData]
public class World : IXmlSerializable
{
    // TODO: Should this be also saved with the world data?
    // If so - beginner task!
    public readonly string GameVersion = "Someone_will_come_up_with_a_proper_naming_scheme_later";
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;
    public InventoryManager inventoryManager;
    public Material skybox;

    // Store all temperature information
    public Temperature temperature;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Wallet Wallet;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;
    public JobQueue jobWaitingQueue;

    // A two-dimensional array to hold our tile data.
    private Tile[,] tiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="width">Width in tiles.</param>
    /// <param name="height">Height in tiles.</param>
    public World(int width, int height)
    {
        // Creates an empty world.
        SetupWorld(width, height);
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        WorldGenerator.Generate(this, seed);
        Debug.ULogChannel("World", "Generated World");

        // adding air to enclosed rooms
        foreach (Room room in this.rooms)
        {
            if (room.ID > 0)
            {
                room.ChangeGas("O2", 0.2f * room.GetSize());
                room.ChangeGas("N2", 0.8f * room.GetSize());
            }
        }
        
        // Make one character.
        CreateCharacter(GetTileAt(Width / 2, Height / 2));
    }

    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    public World()
    {
    }

    public event Action<Furniture> OnFurnitureCreated;

    public event Action<Character> OnCharacterCreated;

    public event Action<Inventory> OnInventoryCreated;

    public event Action<Tile> OnTileChanged;

    public static World Current { get; protected set; }

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    public Syster PowerSystem { get; private set; }

    public Room GetOutsideRoom()
    {
        return rooms[0];
    }

    public int GetRoomID(Room r)
    {
        return rooms.IndexOf(r);
    }

    public Room GetRoomFromID(int i)
    {
        if (i < 0 || i > rooms.Count - 1)
        {
            return null;
        }

        return rooms[i];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
        Debug.ULogChannel("Rooms", "creating room:" + r.ID);
    }

    public int CountFurnitureType(string objectType)
    {
        int count = furnitures.Count(f => f.ObjectType == objectType);
        return count;
    }

    public void DeleteRoom(Room r)
    {
        if (r.IsOutsideRoom())
        {
            Debug.ULogErrorChannel("World", "Tried to delete the outside room.");
            return;
        }

        Debug.ULogChannel("Rooms", "Deleting room:" + r.ID);

        // Remove this room from our rooms list.
        rooms.Remove(r);

        // All tiles that belonged to this room should be re-assigned to
        // the outside.
        r.ReturnTilesToOutsideRoom();
    }

    public void UpdateCharacters(float deltaTime)
    {
        // Change from a foreach due to the collection being modified while its being looped through
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].Update(deltaTime);
        }
    }

    public void Tick(float deltaTime)
    {
        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

        // Progress temperature modelling
        temperature.Update();
        PowerSystem.Update(deltaTime);
    }

    public Character CreateCharacter(Tile t)
    {
        return CreateCharacter(t, UnityEngine.Random.ColorHSV());
    }

    public Character CreateCharacter(Tile t, Color color)
    {
        Debug.ULogChannel("World", "CreateCharacter");
        Character c = new Character(t, color);

        // Adds a random name to the Character
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "CharacterNames.txt");

        string[] names = File.ReadAllLines(filePath);
        c.name = names[UnityEngine.Random.Range(0, names.Length - 1)];
        characters.Add(c);

        if (OnCharacterCreated != null)
        {
            OnCharacterCreated(c);
        }

        return c;
    }

    /// <summary>
    /// A function for testing out the system.
    /// </summary>
    public void RandomizeTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }
    }

    public void SetupPathfindingExample()
    { 
        // Make a set of floors/walls to test pathfinding with.
        int l = (Width / 2) - 5;
        int b = (Height / 2) - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;

                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceFurniture("furn_SteelWall", tiles[x, y]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the tile data at x and y.
    /// </summary>
    /// <returns>The <see cref="Tile"/> or null if called with invalid arguments.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int y)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            return null;
        }

        return tiles[x, y];
    }

    public Tile GetCenterTile()
    {
        return GetTileAt(Width / 2, Height / 2);
    }

    public Tile GetFirstCenterTileWithNoInventory(int maxOffset)
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
                tile = GetTileAt((Width / 2) + offsetX, (Height / 2) + offsetY);
                if (tile.Inventory == null)
                {
                    return tile;
                }

                offsetY = -offset;
                tile = GetTileAt((Width / 2) + offsetX, (Height / 2) + offsetY);
                if (tile.Inventory == null)
                {
                    return tile;
                }
            }

            // searching left & rigth line of the square
            for (offsetY = -offset; offsetY <= offset; offsetY++)
            {
                offsetX = offset;
                tile = GetTileAt((Width / 2) + offsetX, (Height / 2) + offsetY);
                if (tile.Inventory == null)
                {
                    return tile;
                }

                offsetX = -offset;
                tile = GetTileAt((Width / 2) + offsetX, (Height / 2) + offsetY);
                if (tile.Inventory == null)
                {
                    return tile;
                }
            }
        }

        return null;
    }

    public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
    {
        // TODO: This function assumes 1x1 tiles -- change this later!
        if (PrototypeManager.Furniture.HasPrototype(objectType) == false)
        {
            Debug.ULogErrorChannel("World", "furniturePrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(PrototypeManager.Furniture.GetPrototype(objectType), t);

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
            Room.DoRoomFloodFill(furn.Tile);
        }

        if (OnFurnitureCreated != null)
        {
            OnFurnitureCreated(furn);

            if (furn.MovementCost != 1)
            {
                // Since tiles return movement cost as their base cost multiplied
                // buy the furniture's movement cost, a furniture movement cost
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

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return PrototypeManager.Furniture.GetPrototype(furnitureType).IsValidPosition(t);
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

        writer.WriteStartElement("Rooms");
        foreach (Room r in rooms)
        {
            if (GetOutsideRoom() == r)
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
                if (tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Inventories");
        foreach (string objectType in inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in inventoryManager.inventories[objectType])
            {
                // If we don't have a tile, that means this is in a character's inventory (or some other non-tile location
                //      which means we shouldn't save that Inventory here, the character will take care of saving and loading
                //      the inventory properly.
                if (inv.tile != null)
                {
                    writer.WriteStartElement("Inventory");
                    inv.WriteXml(writer);
                    writer.WriteEndElement();
                }
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

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }

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
    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width, Height);

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
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
                case "Skybox":
                    LoadSkybox(reader.ReadElementString("Skybox"));
                    break;
                case "Wallet":
                    ReadXml_Wallet(reader);
                    break;
            }
        }

        // DEBUGGING ONLY!  REMOVE ME LATER!
        // Create an Inventory Item
        Inventory inv = new Inventory("Steel Plate", 50, 50);
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (OnInventoryCreated != null)
        {
            OnInventoryCreated(t.Inventory);
        }

        inv = new Inventory("Steel Plate", 50, 4);
        t = GetTileAt((Width / 2) + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (OnInventoryCreated != null)
        {
            OnInventoryCreated(t.Inventory);
        }

        inv = new Inventory("Copper Wire", 50, 3);
        t = GetTileAt((Width / 2) + 1, (Height / 2) + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (OnInventoryCreated != null)
        {
            OnInventoryCreated(t.Inventory);
        }
    }

    public void OnInventoryCreatedCallback(Inventory inv)
    {
        if (OnInventoryCreated != null)
        { 
            OnInventoryCreated(inv);
        }
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        furnitures.Remove(furn);
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
        DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/Skyboxes"));
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        FileInfo[] files = dirInfo.GetFiles("*.mat", SearchOption.AllDirectories);

        if (files.Length > 0)
        {
            string resourcePath = string.Empty;
            FileInfo file = null;
            if (!string.IsNullOrEmpty(name))
            {
                foreach (FileInfo fileInfo in files)
                {
                    if (name.Equals(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf("."))))
                    {
                        file = fileInfo;
                        break;
                    }
                }
            }

            // Maybe we passed in a name that doesn't exist? Pick a random skybox.
            if (file == null)
            {
                // Get random file
                file = files[(int)(UnityEngine.Random.value * files.Length)];
            }

            resourcePath = Path.Combine(file.DirectoryName.Substring(file.DirectoryName.IndexOf("Skyboxes")), file.Name);

            if (resourcePath.Contains("."))
            {
                resourcePath = resourcePath.Remove(resourcePath.LastIndexOf("."));
            }

            skybox = Resources.Load<Material>(resourcePath);
            RenderSettings.skybox = skybox;
        }
        else
        {
            Debug.ULogWarningChannel("World", "No skyboxes detected! Falling back to black.");
        }
    }

    private void SetupWorld(int width, int height)
    {
        // Setup furniture actions before any other things are loaded.
        new FurnitureActions();

        jobQueue = new JobQueue();
        jobWaitingQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        Current = this;

        Width = width;
        Height = height;

        TileType.LoadTileTypes();

        tiles = new Tile[Width, Height];

        rooms = new List<Room>();
        rooms.Add(new Room()); // Create the outside?

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x, y] = new Tile(x, y);
                tiles[x, y].TileChanged += OnTileChangedCallback;
                tiles[x, y].Room = GetOutsideRoom(); // Rooms 0 is always going to be outside, and that is our default room
            }
        }

        CreateWallet();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();
        PowerSystem = new Syster();
        temperature = new Temperature(Width, Height);
        LoadSkybox();
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
                tiles[x, y].ReadXml(reader);
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

                // Create our inventory from the file
                Inventory inv = new Inventory(
                    reader.GetAttribute("objectType"),
                    int.Parse(reader.GetAttribute("maxStackSize")),
                    int.Parse(reader.GetAttribute("stackSize")));

                inventoryManager.PlaceInventory(tiles[x, y], inv);
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

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y], false);
                furn.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Furniture"));
        }
    }

    private void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                Room r = new Room();
                rooms.Add(r);
                r.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Room"));
        }
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
                if (reader.GetAttribute("r") != null)
                {
                    float r = float.Parse(reader.GetAttribute("r"));
                    float b = float.Parse(reader.GetAttribute("b"));
                    float g = float.Parse(reader.GetAttribute("g"));
                    Color color = new Color(r, g, b, 1.0f);
                    character = CreateCharacter(tiles[x, y], color);
                }
                else
                {
                    character = CreateCharacter(tiles[x, y]);
                }

                character.name = reader.GetAttribute("name");
                character.ReadXml(reader);
                if (reader.ReadToDescendant("Inventories")) 
                {
                    if (reader.ReadToDescendant("Inventory"))
                    {
                        do
                        {
                            // Create our inventory from the file
                            Inventory inv = new Inventory(
                                reader.GetAttribute("objectType"),
                                int.Parse(reader.GetAttribute("maxStackSize")),
                                int.Parse(reader.GetAttribute("stackSize")));

                            inventoryManager.PlaceInventory(character, inv);
                        }
                        while (reader.ReadToNextSibling("Inventory"));

                        // One more read to step out of Inventories, so ReadToNextSibling will find sibling Character
                        reader.Read();
                    }
                }
            }
            while (reader.ReadToNextSibling("Character"));
        }
    }
}
