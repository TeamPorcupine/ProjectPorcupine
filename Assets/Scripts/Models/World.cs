#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================


#endregion
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class World : IXmlSerializable
{
    // TODO: Should this be also saved with the world data?
    // If so - beginner task!
    public readonly string currentGameVersion = "Someone_will_come_up_with_a_proper_naming_scheme_later";

    // A two-dimensional array to hold our tile data.
    Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;
    public InventoryManager inventoryManager;
    public PowerSystem powerSystem;
    // Store all temperature information
    public Temperature temperature;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;
    public Dictionary<string, InventoryCommon> inventoryPrototypes;

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    public event Action<Furniture> cbFurnitureCreated;
    public event Action<Character> cbCharacterCreated;
    public event Action<Inventory> cbInventoryCreated;
    public event Action<Tile> cbTileChanged;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;
    public JobQueue jobWaitingQueue;

    static public World current { get; protected set; }

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

        // Make one character
        CreateCharacter(GetTileAt(Width / 2, Height / 2));
    }

    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    public World()
    {

    }



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
            return null;
		
        return rooms[i];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
    }

    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete the outside room.");
            return;
        }

        // Remove this room from our rooms list.
        rooms.Remove(r);

        // All tiles that belonged to this room should be re-assigned to
        // the outside.
        r.ReturnTilesToOutsideRoom();
    }

    void SetupWorld(int width, int height)
    {

        jobQueue = new JobQueue();
        jobWaitingQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        current = this;

        Width = width;
        Height = height;

        tiles = new Tile[Width, Height];

        rooms = new List<Room>();
        rooms.Add(new Room()); // Create the outside?

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x, y] = new Tile(x, y);
                tiles[x, y].cbTileChanged += OnTileChanged;
                tiles[x, y].Room = GetOutsideRoom(); // Rooms 0 is always going to be outside, and that is our default room
            }
        }

        CreateFurniturePrototypes();

        CreateInventoryPrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();
        powerSystem = new PowerSystem();
        temperature = new Temperature(Width, Height);
    }

    public void Update(float deltaTime)
    {
        //Change from a foreach due to the collection being modified while its being looped through
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].Update(deltaTime);
        }

        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

        // Progress temperature modelling
        temperature.Update();
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);

        // Adds a random name to the Character
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "CharacterNames.txt");
        string names = System.IO.File.ReadAllText(filePath);

        string[] lines = Regex.Split( names, "\r\n" );
        c.name = lines[UnityEngine.Random.Range(0, lines.Length-1)];

        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    public Character CreateCharacter(Tile t, Color color)
    {
        Debug.Log("CreateCharacter");
        Character c = new Character(t, color); 

        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);
            
        return c;
    }
    
    public void SetFurnitureJobPrototype(Job j, Furniture f)
    {
        furnitureJobPrototypes[f.objectType] = j;
    }

    void LoadFurnitureLua(string filePath)
    {
        string myLuaCode = System.IO.File.ReadAllText(filePath);

        // Instantiate the singleton

        FurnitureActions.addScript(myLuaCode);
    }

    void CreateFurniturePrototypes()
    {
        new FurnitureActions();
        string luaFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        luaFilePath = System.IO.Path.Combine(luaFilePath, "Furniture.lua");
        LoadFurnitureLua(luaFilePath);


        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        // READ FURNITURE PROTOTYPE XML FILE HERE
        // TODO:  Probably we should be getting past a StreamIO handle or the raw
        // text here, rather than opening the file ourselves.

        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        string filePath = System.IO.Path.Combine(dataPath, "Furniture.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);
        LoadFurniturePrototypesFromFile(furnitureXmlText);


        DirectoryInfo[] mods = WorldController.Instance.modsManager.GetMods();
        foreach (DirectoryInfo mod in mods)
        {
            string furnitureLuaModFile = System.IO.Path.Combine(mod.FullName, "Furniture.lua");
            if (File.Exists(furnitureLuaModFile))
            {
                LoadFurnitureLua(furnitureLuaModFile);
            }

            string furnitureXmlModFile = System.IO.Path.Combine(mod.FullName, "Furniture.xml");
            if (File.Exists(furnitureXmlModFile))
            {
                string furnitureXmlModText = System.IO.File.ReadAllText(furnitureXmlModFile);
                LoadFurniturePrototypesFromFile(furnitureXmlModText);
            }
        }
    }

    void LoadFurniturePrototypesFromFile(string furnitureXmlText) 
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

        int furnCount = 0;
        if (reader.ReadToDescendant("Furnitures"))
        {
            if (reader.ReadToDescendant("Furniture"))
            {
                do
                {
                    furnCount++;

                    Furniture furn = new Furniture();
                    try
                    {
                        furn.ReadXmlPrototype(reader);
                    }
                    catch (Exception e) {
                        Debug.LogError("Error reading furniture prototype for: " + furn.objectType + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
                    }


                    furniturePrototypes[furn.objectType] = furn;



                } while (reader.ReadToNextSibling("Furniture"));
            }
            else
            {
                Debug.LogError("The furniture prototype definition file doesn't have any 'Furniture' elements.");
            }
        }
        else
        {
            Debug.LogError("Did not find a 'Furnitures' element in the prototype definition file.");
        }
    }

    void CreateInventoryPrototypes()
    {
        inventoryPrototypes = new Dictionary<string, InventoryCommon>();

        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        string filePath = System.IO.Path.Combine(dataPath, "Inventory.xml");
        string inventoryXmlText = System.IO.File.ReadAllText(filePath);
        LoadInventoryPrototypesFromFile(inventoryXmlText);


        DirectoryInfo[] mods = WorldController.Instance.modsManager.GetMods();
        foreach (DirectoryInfo mod in mods)
        {
            string inventoryXmlModFile = System.IO.Path.Combine(mod.FullName, "Inventory.xml");
            if (File.Exists(inventoryXmlModFile))
            {
                string inventoryXmlModText = System.IO.File.ReadAllText(inventoryXmlModFile);
                LoadInventoryPrototypesFromFile(inventoryXmlModText);
            }
        }
    }

    void LoadInventoryPrototypesFromFile(string inventoryXmlText)
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(inventoryXmlText));

        int inventoryCount = 0;
        if (reader.ReadToDescendant("Inventories"))
        {
            if (reader.ReadToDescendant("Inventory"))
            {
                do
                {
                    inventoryCount++;

                    InventoryCommon inv = new InventoryCommon();
                    try
                    {
                        inv.ReadXmlPrototype(reader);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error reading inventory prototype for: " + inv.objectType + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
                    }


                    inventoryPrototypes[inv.objectType] = inv;



                } while (reader.ReadToNextSibling("Inventory"));
            }
            else
            {
                Debug.LogError("The inventory prototype definition file doesn't have any 'Inventory' elements.");
            }
        }
        else
        {
            Debug.LogError("Did not find a 'Inventories' element in the prototype definition file.");
        }
    }

    /// <summary>
    /// A function for testing out the system
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

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

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
    /// <returns>The <see cref="Tile"/>.</returns>
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


    public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
    {
        // TODO: This function assumes 1x1 tiles -- change this later!

        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (furn == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furn.cbOnRemoved += OnFurnitureRemoved;
        furnitures.Add(furn);

        // Do we need to recalculate our rooms?
        if (doRoomFloodFill && furn.roomEnclosure)
        {
            Room.DoRoomFloodFill(furn.tile);
        }

        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(furn);

            if (furn.movementCost != 1)
            {
                // Since tiles return movement cost as their base cost multiplied
                // buy the furniture's movement cost, a furniture movement cost
                // of exactly 1 doesn't impact our pathfinding system, so we can
                // occasionally avoid invalidating pathfinding graphs
                //InvalidateTileGraph();	// Reset the pathfinding system
                if (tileGraph != null)
                {
                    tileGraph.RegenerateGraphAtTile(t);
                }
            }
        }

        return furn;
    }
    
    // Gets called whenever ANY tile changes
    void OnTileChanged(Tile t)
    {
        if (cbTileChanged == null)
            return;
		
        cbTileChanged(t);

        //InvalidateTileGraph();
        if (tileGraph != null)
        {
            tileGraph.RegenerateGraphAtTile(t);
        }
    }

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }


    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return furniturePrototypes[furnitureType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string objectType)
    {
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return furniturePrototypes[objectType];
    }

    //////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 						SAVING & LOADING
    /// 
    //////////////////////////////////////////////////////////////////////////////////////

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
                continue;	// Skip the outside room. Alternatively, should SetupWorld be changed to not create one?

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
        foreach (String objectType in inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in inventoryManager.inventories[objectType])
            {
                writer.WriteStartElement("Inventory");
                inv.WriteXml(writer);
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

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
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
            }
        }

        // DEBUGGING ONLY!  REMOVE ME LATER!
        // Create an Inventory Item
        Inventory inv = new Inventory("Steel Plate", 50, 50);
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.Inventory);
        }

        inv = new Inventory("Steel Plate", 50, 4);
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.Inventory);
        }

        inv = new Inventory("Copper Wire", 50, 3);
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.Inventory);
        }
    }

    void ReadXml_Tiles(XmlReader reader)
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
            } while (reader.ReadToNextSibling("Tile"));

        }

    }

    void ReadXml_Inventories(XmlReader reader)
    {
        Debug.Log("ReadXml_Inventories");

        if(reader.ReadToDescendant("Inventory"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                //Create our inventory from the file
                Inventory inv = new Inventory(reader.GetAttribute("objectType"),
                    int.Parse(reader.GetAttribute("maxStackSize")),
                    int.Parse(reader.GetAttribute("stackSize")));
                
                inventoryManager.PlaceInventory(tiles[x,y],inv);
            } while(reader.ReadToNextSibling("Inventory"));
        }
    }

    void ReadXml_Furnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y], false);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));
        }

    }

    void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                Room r = new Room();
                rooms.Add(r);
                r.ReadXml(reader);
            } while (reader.ReadToNextSibling("Room"));

        }

    }



    void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                if(reader.GetAttribute("r") != null)
                {
                    float r = float.Parse(reader.GetAttribute("r"));
                    float b = float.Parse(reader.GetAttribute("b"));;
                    float g = float.Parse(reader.GetAttribute("g"));;
                    Color color = new Color(r, g, b, 1.0f);
                    Character c = CreateCharacter(tiles[x, y], color);
                    c.ReadXml(reader);
                }

                else
                {
                    Character c = CreateCharacter(tiles[x, y]);
                    c.ReadXml(reader);
                }
                
            } while(reader.ReadToNextSibling("Character"));
        }

    }

    public void OnInventoryCreated(Inventory inv)
    {
        if (cbInventoryCreated != null)
            cbInventoryCreated(inv);
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        furnitures.Remove(furn);
    }
}
