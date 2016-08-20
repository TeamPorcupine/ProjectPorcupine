//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class World : IXmlSerializable
{
    // A two-dimensional array to hold our tile data.
    Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;
    public InventoryManager inventoryManager;
    public PowerSystem powerSystem;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;

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

    public static World current { get; protected set; }

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
            Logger.LogError("Tried to delete the outside room.");
            return;
        }

        // Remove this room from our rooms list.
        rooms.Remove(r);

        // All tiles that belonged to this room should be re-assigned to
        // the outside.
        r.ReturnTilesToOutsideRoom();
    }

    private void SetupWorld(int width, int height)
    {

        jobQueue = new JobQueue();

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
                tiles[x, y].room = GetOutsideRoom(); // Rooms 0 is always going to be outside, and that is our default room
            }
        }

        CreateFurniturePrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();
        powerSystem = new PowerSystem();

    }

    public void Update(float deltaTime)
    {
        foreach (Character c in characters)
        {
            c.Update(deltaTime);
        }

        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);

        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    public void SetFurnitureJobPrototype(Job j, Furniture f)
    {
        furnitureJobPrototypes[f.objectType] = j;
    }

    private void LoadFurnitureLua()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "Furniture.lua");
        string myLuaCode = System.IO.File.ReadAllText(filePath);

        // Instantiate the singleton
        new FurnitureActions(myLuaCode);

    }

    private void CreateFurniturePrototypes()
    {
        LoadFurnitureLua();


        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        // READ FURNITURE PROTOTYPE XML FILE HERE
        // TODO:  Probably we should be getting past a StreamIO handle or the raw
        // text here, rather than opening the file ourselves.

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Furniture.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

        int furnCount = 0;
        if (reader.ReadToDescendant(FurnituresXMLNodeName))
        {
            if (reader.ReadToDescendant(FurnitureXMLNodeeName))
            {
                do
                {
                    furnCount++;
                    Furniture furn = new Furniture();
                    try
                    {
                        furn.ReadXmlPrototype(reader);
                    }
                    catch
                    {
                        Logger.LogError("Error reading furniture prototype for: " + furn.objectType);
                    }
                    furniturePrototypes[furn.objectType] = furn;
                } while (reader.ReadToNextSibling(FurnitureXMLNodeeName));
            }
            else
            {
                Logger.LogError("The furniture prototype definition file doesn't have any 'Furniture' elements.");
            }
        }
        else
        {
            Logger.LogError("Did not find a 'Furnitures' element in the prototype definition file.");
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
            Logger.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
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
            Logger.LogError("No furniture with type: " + objectType);
            return null;
        }

        return furniturePrototypes[objectType];
    }

    //////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 						SAVING & LOADING
    /// 
    //////////////////////////////////////////////////////////////////////////////////////

    private const string WidthXMLAttributeName = "Width";
    private const string HeightXMLAttributeName = "Height";
    private const string RoorsXMLNodeName = "Rooms";
    private const string RoomXMLNodeeName = "Room";
    private const string TilesXMLNodeName = "Tiles";
    private const string TileXMLNodeeName = "Tile";
    private const string XCoordinateXMLAttributeName = "X";
    private const string YCoordinateXMLAttributeName = "Y";
    private const string InventoriesXMLNodeName = "Inventories";
    private const string InventoryXMLNodeeName = "Inventory";
    private const string ObjectTypeXMLAttributeName = "objectType";
    private const string MaxStackSizeXMLAttributeName = "maxStackSize";
    private const string StackSizeXMLAttributeName = "stackSize";
    private const string FurnituresXMLNodeName = "Furnitures";
    private const string FurnitureXMLNodeeName = "Furniture";
    private const string CharactersXMLNodeName = "Characters";
    private const string CharacterXMLNodeeName = "Character";
    
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save info here
        writer.WriteAttributeString(WidthXMLAttributeName, Width.ToString());
        writer.WriteAttributeString(HeightXMLAttributeName, Height.ToString());

        writer.WriteStartElement(RoorsXMLNodeName);
        foreach (Room r in rooms)
        {
            if (GetOutsideRoom() == r)
                continue;	// Skip the outside room. Alternatively, should SetupWorld be changed to not create one?

            writer.WriteStartElement(RoomXMLNodeeName);
            r.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement(TilesXMLNodeName);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement(TileXMLNodeeName);
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement(InventoriesXMLNodeName);
        foreach (String objectType in inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in inventoryManager.inventories[objectType])
            {
                writer.WriteStartElement(InventoryXMLNodeeName);
                inv.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement(FurnituresXMLNodeName);
        foreach (Furniture furn in furnitures)
        {
            writer.WriteStartElement(FurnitureXMLNodeeName);
            furn.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteStartElement(CharactersXMLNodeName);
        foreach (Character c in characters)
        {
            writer.WriteStartElement(CharacterXMLNodeeName);
            c.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here
        Width = int.Parse(reader.GetAttribute(WidthXMLAttributeName));
        Height = int.Parse(reader.GetAttribute(HeightXMLAttributeName));

        SetupWorld(Width, Height);

        while (reader.Read())
        {
            switch (reader.Name)
            {
            case RoorsXMLNodeName:
                ReadXml_Rooms(reader);
                break;
            case TilesXMLNodeName:
                ReadXml_Tiles(reader);
                break;
            case InventoriesXMLNodeName:
                ReadXml_Inventories(reader);
                break;
            case FurnituresXMLNodeName:
                ReadXml_Furnitures(reader);
                break;
            case CharactersXMLNodeName:
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
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel Plate", 50, 4);
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel Plate", 50, 3);
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }
    }

    private void ReadXml_Tiles(XmlReader reader)
    {
        // We are in the "Tiles" element, so read elements until
        // we run out of "Tile" nodes.
        if (reader.ReadToDescendant(TileXMLNodeeName))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute(XCoordinateXMLAttributeName));
                int y = int.Parse(reader.GetAttribute(YCoordinateXMLAttributeName));
                tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling(TileXMLNodeeName));
        }
    }

    private void ReadXml_Inventories(XmlReader reader)
    {
        Logger.Log("ReadXml_Inventories");

        if (reader.ReadToDescendant(InventoryXMLNodeeName))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute(XCoordinateXMLAttributeName));
                int y = int.Parse(reader.GetAttribute(YCoordinateXMLAttributeName));

                //Create our inventory from the file
                Inventory inv = new Inventory(reader.GetAttribute(ObjectTypeXMLAttributeName),
                    int.Parse(reader.GetAttribute(MaxStackSizeXMLAttributeName)),
                    int.Parse(reader.GetAttribute(StackSizeXMLAttributeName)));

                inventoryManager.PlaceInventory(tiles[x, y], inv);
            } while (reader.ReadToNextSibling(InventoryXMLNodeeName));
        }
    }

    private void ReadXml_Furnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant(FurnitureXMLNodeeName))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute(XCoordinateXMLAttributeName));
                int y = int.Parse(reader.GetAttribute(YCoordinateXMLAttributeName));

                Furniture furn = PlaceFurniture(reader.GetAttribute(ObjectTypeXMLAttributeName), tiles[x, y], false);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling(FurnitureXMLNodeeName));
        }
    }

    private void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant(RoomXMLNodeeName))
        {
            do
            {
                Room r = new Room();
                rooms.Add(r);
                r.ReadXml(reader);
            } while (reader.ReadToNextSibling(RoomXMLNodeeName));
        }
    }

    private void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant(CharacterXMLNodeeName))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute(XCoordinateXMLAttributeName));
                int y = int.Parse(reader.GetAttribute(YCoordinateXMLAttributeName));

                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling(CharacterXMLNodeeName));
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