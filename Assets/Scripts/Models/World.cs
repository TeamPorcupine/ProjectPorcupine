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
    private Tile[,] _tiles;
    public List<Character> Characters { get; set; }
    public List<Furniture> Furnitures { get; set; }
    public List<Room> Rooms { get; set; }
    public InventoryManager InventoryManager { get; set; }
    public PowerSystem PowerSystem { get; set; }

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph TileGraph { get; set; }

    public Dictionary<string, Furniture> FurniturePrototypes { get; set; }
    public Dictionary<string, Job> FurnitureJobPrototypes { get; set; }

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    public event Action<Furniture> FurnitureCreated;
    public event Action<Character> CharacterCreated;
    public event Action<Inventory> InventoryCreated;
    public event Action<Tile> TileChanged;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue JobQueue { get; set; }

    public static World Current { get; protected set; }

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
        //CreateCharacter( GetTileAt( Width/2, Height/2 ) );
        //CreateCharacter( GetTileAt( Width/2, Height/2 ) );
    }

    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    public World()
    {

    }



    public Room GetOutsideRoom()
    {
        return Rooms[0];
    }

    public int GetRoomID(Room r)
    {
        return Rooms.IndexOf(r);
    }

    public Room GetRoomFromID(int i)
    {
        if (i < 0 || i > Rooms.Count - 1)
            return null;
		
        return Rooms[i];
    }

    public void AddRoom(Room r)
    {
        Rooms.Add(r);
    }

    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete the outside room.");
            return;
        }

        // Remove this room from our rooms list.
        Rooms.Remove(r);

        // All tiles that belonged to this room should be re-assigned to
        // the outside.
        r.ReturnTilesToOutsideRoom();
    }

    private void SetupWorld(int width, int height)
    {

        JobQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        Current = this;

        Width = width;
        Height = height;

        _tiles = new Tile[Width, Height];

        Rooms = new List<Room>();
        Rooms.Add(new Room()); // Create the outside?

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _tiles[x, y] = new Tile(x, y);
                _tiles[x, y].TileChanged += OnTileChanged;
                _tiles[x, y].Room = GetOutsideRoom(); // Rooms 0 is always going to be outside, and that is our default room
            }
        }

        Debug.Log("World created with " + (Width * Height) + " tiles.");

        CreateFurniturePrototypes();

        Characters = new List<Character>();
        Furnitures = new List<Furniture>();
        InventoryManager = new InventoryManager();
        PowerSystem = new PowerSystem();

    }

    public void Update(float deltaTime)
    {
        foreach (Character c in Characters)
        {
            c.Update(deltaTime);
        }

        foreach (Furniture f in Furnitures)
        {
            f.Update(deltaTime);
        }

    }

    public Character CreateCharacter(Tile t)
    {
        Debug.Log("CreateCharacter");
        Character c = new Character(t); 

        Characters.Add(c);

        if (CharacterCreated != null)
            CharacterCreated(c);

        return c;
    }

    public void SetFurnitureJobPrototype(Job j, Furniture f)
    {
        FurnitureJobPrototypes[f.ObjectType] = j;
    }

    private void LoadFurnitureLua()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "Furniture.lua");
        string myLuaCode = System.IO.File.ReadAllText(filePath);

        //Debug.Log("My LUA Code");
        //Debug.Log(myLuaCode);

        // Instantiate the singleton
        new FurnitureActions(myLuaCode);

    }

    private void CreateFurniturePrototypes()
    {
        LoadFurnitureLua();


        FurniturePrototypes = new Dictionary<string, Furniture>();
        FurnitureJobPrototypes = new Dictionary<string, Job>();

        // READ FURNITURE PROTOTYPE XML FILE HERE
        // TODO:  Probably we should be getting past a StreamIO handle or the raw
        // text here, rather than opening the file ourselves.

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Furniture.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

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
                    catch {
                        Debug.LogError("Error reading furniture prototype for: " + furn.ObjectType);
                    }


                    FurniturePrototypes[furn.ObjectType] = furn;



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

        Debug.Log("Furniture prototypes read: " + furnCount.ToString());

        // This bit will come from parsing a LUA file later, but for now we still need to
        // implement furniture behaviour directly in C# code.
        //furniturePrototypes["Door"].RegisterUpdateAction( FurnitureActions.Door_UpdateAction );
        //furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;

    }


    /*	void CreateFurniturePrototypes() {
		// This will be replaced by a function that reads all of our furniture data
		// from a text file in the future.

		furniturePrototypes = new Dictionary<string, Furniture>();
		furnitureJobPrototypes = new Dictionary<string, Job>();

		furniturePrototypes.Add("furn_SteelWall", 
			new Furniture(
				"furn_SteelWall",
				0,	// Impassable
				1,  // Width
				1,  // Height
				true, // Links to neighbours and "sort of" becomes part of a large object
				true  // Enclose rooms
			)
		);
		furniturePrototypes["furn_SteelWall"].Name = "Basic Wall";
		furnitureJobPrototypes.Add("furn_SteelWall",
			new Job( null, 
				"furn_SteelWall", 
				FurnitureActions.JobComplete_FurnitureBuilding, 1f, 
				new Inventory[]{ new Inventory("Steel Plate", 5, 0) } 
			)
		);

		furniturePrototypes.Add("Door", 
			new Furniture(
				"Door",
				1,	// Door pathfinding cost
				1,  // Width
				1,  // Height
				false, // Links to neighbours and "sort of" becomes part of a large object
				true  // Enclose rooms
			)
		);

		// What if the object behaviours were scriptable? And therefore were part of the text file
		// we are reading in now?

		furniturePrototypes["Door"].SetParameter("openness", 0);
		furniturePrototypes["Door"].SetParameter("is_opening", 0);
		furniturePrototypes["Door"].RegisterUpdateAction( FurnitureActions.Door_UpdateAction );

		furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;


		furniturePrototypes.Add("Stockpile", 
			new Furniture(
				"Stockpile",
				1,	// Impassable
				1,  // Width
				1,  // Height
				true, // Links to neighbours and "sort of" becomes part of a large object
				false  // Enclose rooms
			)
		);
		furniturePrototypes["Stockpile"].RegisterUpdateAction( FurnitureActions.Stockpile_UpdateAction );
		furniturePrototypes["Stockpile"].tint = new Color32( 186, 31, 31, 255 );
		furnitureJobPrototypes.Add("Stockpile",
			new Job( 
				null, 
				"Stockpile", 
				FurnitureActions.JobComplete_FurnitureBuilding,
				-1,
				null
			)
		);



		furniturePrototypes.Add("Oxygen Generator", 
			new Furniture(
				"Oxygen Generator",
				10,	// Door pathfinding cost
				2,  // Width
				2,  // Height
				false, // Links to neighbours and "sort of" becomes part of a large object
				false  // Enclose rooms
			)
		);
		furniturePrototypes["Oxygen Generator"].RegisterUpdateAction( FurnitureActions.OxygenGenerator_UpdateAction );



		furniturePrototypes.Add("Mining Drone Station", 
			new Furniture(
				"Mining Drone Station",
				1,	// Pathfinding cost
				3,  // Width			
				3,  // Height		// TODO: In the future, the mining drone station will be a 3x2 object with an offset work spot
				false, // Links to neighbours and "sort of" becomes part of a large object
				false  // Enclose rooms
			)
		);
		furniturePrototypes["Mining Drone Station"].jobSpotOffset = new Vector2( 1, 0 );

		furniturePrototypes["Mining Drone Station"].RegisterUpdateAction( FurnitureActions.MiningDroneStation_UpdateAction );



	}
*/

    /// <summary>
    /// A function for testing out the system
    /// </summary>
    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {

                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    _tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    _tiles[x, y].Type = TileType.Floor;
                }

            }
        }
    }

    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                _tiles[x, y].Type = TileType.Floor;


                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceFurniture("furn_SteelWall", _tiles[x, y]);
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
            //Debug.LogError("Tile ("+x+","+y+") is out of range.");
            return null;
        }
        return _tiles[x, y];
    }


    public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
    {
        //Debug.Log("PlaceInstalledObject");
        // TODO: This function assumes 1x1 tiles -- change this later!

        if (FurniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(FurniturePrototypes[objectType], t);

        if (furn == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furn.OnRemoved += OnFurnitureRemoved;
        Furnitures.Add(furn);

        // Do we need to recalculate our rooms?
        if (doRoomFloodFill && furn.RoomEnclosure)
        {
            Room.DoRoomFloodFill(furn.Tile);
        }

        if (FurnitureCreated != null)
        {
            FurnitureCreated(furn);

            if (furn.MovementCost != 1)
            {
                // Since tiles return movement cost as their base cost multiplied
                // buy the furniture's movement cost, a furniture movement cost
                // of exactly 1 doesn't impact our pathfinding system, so we can
                // occasionally avoid invalidating pathfinding graphs
                //InvalidateTileGraph();	// Reset the pathfinding system
                if (TileGraph != null)
                {
                    TileGraph.RegenerateGraphAtTile(t);
                }
            }
        }

        return furn;
    }

    // Gets called whenever ANY tile changes
    private void OnTileChanged(Tile t)
    {
        if (TileChanged == null)
            return;
		
        TileChanged(t);

        //InvalidateTileGraph();
        if (TileGraph != null)
        {
            TileGraph.RegenerateGraphAtTile(t);
        }
    }

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        TileGraph = null;
    }


    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return FurniturePrototypes[furnitureType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string objectType)
    {
        if (FurniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return FurniturePrototypes[objectType];
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
        foreach (Room r in Rooms)
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
                if (_tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    _tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Inventories");
        foreach (String objectType in InventoryManager.Inventories.Keys)
        {
            foreach (Inventory inv in InventoryManager.Inventories[objectType])
            {
                writer.WriteStartElement("Inventory");
                inv.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in Furnitures)
        {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in Characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

/*		writer.WriteStartElement("Width");
		writer.WriteValue(Width);
		writer.WriteEndElement();
*/

        //Debug.Log(writer.ToString());
	
    }

    public void ReadXml(XmlReader reader)
    {
        Debug.Log("World::ReadXml");
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
        InventoryManager.PlaceInventory(t, inv);
        if (InventoryCreated != null)
        {
            InventoryCreated(t.Inventory);
        }

        inv = new Inventory("Steel Plate", 50, 4);
        t = GetTileAt(Width / 2 + 2, Height / 2);
        InventoryManager.PlaceInventory(t, inv);
        if (InventoryCreated != null)
        {
            InventoryCreated(t.Inventory);
        }

        inv = new Inventory("Steel Plate", 50, 3);
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        InventoryManager.PlaceInventory(t, inv);
        if (InventoryCreated != null)
        {
            InventoryCreated(t.Inventory);
        }
    }

    private void ReadXml_Tiles(XmlReader reader)
    {
        Debug.Log("ReadXml_Tiles");
        // We are in the "Tiles" element, so read elements until
        // we run out of "Tile" nodes.

        if (reader.ReadToDescendant("Tile"))
        {
            // We have at least one tile, so do something with it.

            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                _tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));

        }

    }

    private void ReadXml_Inventories(XmlReader reader)
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
                
                InventoryManager.PlaceInventory(_tiles[x,y],inv);
            } while(reader.ReadToNextSibling("Inventory"));
        }
    }

    private void ReadXml_Furnitures(XmlReader reader)
    {
        Debug.Log("ReadXml_Furnitures");

        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), _tiles[x, y], false);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));

/*			We don't need to do a flood fill on load, because we're getting room info
 			from the save file
 			
 			foreach(Furniture furn in furnitures) {
				Room.DoRoomFloodFill( furn.tile, true );
			}
*/
        }

    }

    private void ReadXml_Rooms(XmlReader reader)
    {
        Debug.Log("ReadXml_Rooms");

        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                /*int x = int.Parse( reader.GetAttribute("X") );
				int y = int.Parse( reader.GetAttribute("Y") );

				Furniture furn = PlaceFurniture( reader.GetAttribute("objectType"), tiles[x,y], false );*/

                //furn.ReadXml(reader);

                Room r = new Room();
                Rooms.Add(r);
                r.ReadXml(reader);
            } while (reader.ReadToNextSibling("Room"));

        }

    }



    private void ReadXml_Characters(XmlReader reader)
    {
        Debug.Log("ReadXml_Characters");
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(_tiles[x, y]);
                c.ReadXml(reader);
            } while(reader.ReadToNextSibling("Character"));
        }

    }

    public void OnInventoryCreated(Inventory inv)
    {
        if (InventoryCreated != null)
            InventoryCreated(inv);
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        Furnitures.Remove(furn);
    }
}