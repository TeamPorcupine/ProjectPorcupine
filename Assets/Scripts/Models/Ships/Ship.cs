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
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

public enum BerthDirection
{
    NORTH, SOUTH, EAST, WEST
}

public enum ShipState
{
    WRAPPED, UNWRAPPED
}

[MoonSharpUserData]
public class Ship : IPrototypable
{
    private ShipManager shipManager;
    private List<ShipStorage> storages;
    private string[,] tileTypes;
    private string[,] furnitureTypes;

    private ShipState state;
    private Vector2 position;
    private Vector2 destination;
    private Furniture berth;

    /// <summary>
    /// Constructs a blank prototype object of the <see cref="Ship"/> class.
    /// Only type-defning variables are initialised. Other variables are not safe to use.
    /// </summary>
    public Ship()
    {
        Type = null;
        Width = 0;
        Height = 0;
        BerthPointX = 0;
        BerthPointY = 0;
        BerthDirection = BerthDirection.NORTH;
        storages = new List<ShipStorage>();
        tileTypes = null;
        furnitureTypes = null;
    }

    /// <summary>
    /// Constructs a <see cref="Ship"/> from a prototype. All type-defining variables are copied over.
    /// The prototype and the instance share no references to objects.
    /// In-game changing variables are initialised to default values.
    /// </summary>
    /// <param name="shipManager">The ship manager that will serve as interface for the ship.</param>
    /// <param name="proto">The template prototype that this ship is based on.</param>
    public Ship(ShipManager shipManager, Ship proto)
    {
        this.shipManager = shipManager;
        Type = proto.Type;
        Width = proto.Width;
        Height = proto.Height;
        BerthPointX = proto.BerthPointX;
        BerthPointY = proto.BerthPointY;
        BerthDirection = proto.BerthDirection;
        storages = new List<ShipStorage>();
        foreach (ShipStorage s in proto.storages)
        {
            this.storages.Add(new ShipStorage(s.X, s.Y));
        }

        InstantiateTiles();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tileTypes[x, y] = proto.tileTypes[x, y];
                furnitureTypes[x, y] = proto.furnitureTypes[x, y];
            }
        }

        State = ShipState.WRAPPED;
        Position = Vector2.zero;
    }

    /// <summary>
    /// Called when a change is made to the ship's instance variables.
    /// </summary>
    public event ShipManager.ShipEventHandler ShipChanged;

    /// <summary>
    /// The type of the ship. All ships with the same type share their type-specific variables.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// The width of the ship in tiles when unwrapped.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the ship in tiles when unwrapped.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The X coordinate of the berth point. The berth point will overlap with the
    /// center of the berth furniture when the ship is berthed, so it will usually be
    /// outside the ship's tile space.
    /// </summary>
    public int BerthPointX { get; private set; }

    /// <summary>
    /// The Y coordinate of the berth point. The berth point will overlap with the
    /// center of the berth furniture when the ship is berthed, so it will usually be
    /// outside the ship's tile space.
    /// </summary>
    public int BerthPointY { get; private set; }

    /// <summary>
    /// The direction the ship is berthed in. This is one of the 4 cardinal directions.
    /// Keep in mind the ship will be positioned opposite to this direction.
    /// </summary>
    public BerthDirection BerthDirection { get; private set; }

    /// <summary>
    /// The state the ship is in. This is either WRAPPED, meaning the ship is in its sprite form, or
    /// UNWRAPPED, meaning it exists as a collection of tiles in the world.
    /// </summary>
    public ShipState State
    {
        get
        {
            return state;
        }

        set
        {
            if (state != value)
            {
                state = value;
                if (ShipChanged != null)
                {
                    ShipChanged(this);
                }
            }
        }
    }

    /// <summary>
    /// The world position of the ship.
    /// </summary>
    public Vector2 Position
    {
        get
        {
            return position;
        }

        set
        {
            if (position != value)
            {
                position = value;
                if (ShipChanged != null)
                {
                    ShipChanged(this);
                }
            }
        }
    }

    /// <summary>
    /// The destination in world space of the ship.
    /// </summary>
    public Vector2 Destination
    {
        get
        {
            return destination;
        }

        set
        {
            if (destination != value)
            {
                destination = value;
                if (ShipChanged != null)
                {
                    ShipChanged(this);
                }
            }
        }
    }

    /// <summary>
    /// The destination berth of the ship. A value of null indicates the ship has a destination somewhere else
    /// in the world and shouldn't berth.
    /// </summary>
    public Furniture Berth
    {
        get
        {
            return berth;
        }

        set
        {
            berth = value;
            if (ShipChanged != null)
            {
                ShipChanged(this);
            }
        }
    }

    /// <summary>
    /// Sets the destination to an arbitrary world space location.
    /// </summary>
    public void SetDestination(float x, float y)
    {
        Berth = null;
        Destination = new Vector2(x, y);
    }

    /// <summary>
    /// Sets the destination to a berth. Should not be called with null value.
    /// If cancelling intention to berth, use `ship.Berth = null` directly.
    /// </summary>
    /// <param name="goalBerth">Goal berth.</param>
    public void SetDestination(Furniture goalBerth)
    {
        if (goalBerth == null)
        {
            UnityDebugger.Debugger.LogError("Ships", "Destination berth should not be set to null this way. Use Berth property or SetDestination(x,y) instead.");
            return;
        }

        Berth = goalBerth;
        Destination = new Vector2(goalBerth.Tile.X, goalBerth.Tile.Y);
    }

    /// <summary>
    /// If unwrapped, the ship does nothing. If wrapped it will move to its
    /// destination with a fixed speed of 5 tiles per seoond. If it reaches its destination
    /// it is removed if it an arbitrary position (assumed to be the edge of the map) or berth
    /// if it was heading to a berth.
    /// </summary>
    /// <param name="deltaTime">The time in seconds elapsed since the last update.</param>
    public void Update(float deltaTime)
    {
        switch (State)
        {
            case ShipState.UNWRAPPED:
                break;
            case ShipState.WRAPPED:
                Move(deltaTime);
                if (Berth == null)
                {
                    if (Vector2.Distance(Position, Destination) < 0.1f)
                    {
                        shipManager.RemoveShip(this);
                    }
                }
                else
                {
                    if (Vector2.Distance(Position, Destination) < 0.1f)
                    {
                        shipManager.BerthShip(Berth, this);
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Unwraps the ship into a collection of tiles according to the types defined in the tile and furniture arrays.
    /// Tiles are counted from the Berth, so this function should only be called when a destination berth is defined.
    /// Assumes that these tiles were empty before. The state of the ship is not changed.
    /// That is handled by <see cref="ShipManager"/>.
    /// </summary>
    public void UnwrapAtBerth()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = GetTile(World.Current, Berth, x, y, 0);

                // Change tile to defined contents
                if (tile.Type.Equals(TileType.Empty) == false || tile.Furniture != null)
                {
                    UnityDebugger.Debugger.LogError("Ships", "Tile " + tile.X + "," + tile.Y + " is not empty. Replacing anyway.");
                }

                tile.SetTileType(PrototypeManager.TileType.Get(tileTypes[x, y]));
                if (furnitureTypes[x, y] != null)
                {
                    World.Current.FurnitureManager.PlaceFurniture(furnitureTypes[x, y], tile, false);
                }
            }
        }
    }

    /// <summary>
    /// Wraps the ship by turning the affected tiles back into empty tiles.
    /// Tiles are counted from the Berth, so this function should only be called when a destination berth is defined.
    /// </summary>
    public void Wrap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = GetTile(World.Current, Berth, x, y, 0);

                // Change tile to empty
                // Order reversed in case furniture on an empty tile leads to problems
                if (furnitureTypes[x, y] != null)
                {
                    tile.Furniture.Deconstruct();
                }

                tile.SetTileType(TileType.Empty);
            }
        }
    }

    /// <summary>
    /// Reads the xml definition for a <see cref="Ship"/> prototype.
    /// </summary>
    /// <param name="parentReader">The parent reader that reads the bigger file.</param>
    public void ReadXmlPrototype(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("type");
        Width = int.Parse(parentReader.GetAttribute("width"));
        Height = int.Parse(parentReader.GetAttribute("height"));

        InstantiateTiles();

        XmlReader reader = parentReader.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "BerthPoint":
                    BerthPointX = int.Parse(reader.GetAttribute("x"));
                    BerthPointY = int.Parse(reader.GetAttribute("y"));
                    BerthDirection = (BerthDirection)Enum.Parse(typeof(BerthDirection), reader.GetAttribute("direction"));
                    break;
                case "Storages":
                    ReadXmlStorages(reader);
                    break;
                case "Tiles":
                    ReadXmlTiles(reader);
                    break;
                case "Furnitures":
                    ReadXmlFurnitures(reader);
                    break;
            }
        }
    }

    /// <summary>
    /// Checks if there is enough room around the given berth for the ship to fit into.
    /// </summary>
    /// <returns><c>true</c>, if all potentially occupied tiles are indeed empty, <c>false</c> otherwise.</returns>
    /// <param name="berth">The berth to test on.</param>
    public bool WouldFitInBerth(Furniture berth)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = GetTile(World.Current, berth, x, y, 0);

                if (tile.Type.Equals(TileType.Empty) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Moves the ship by a fixed 5 tiles per second towards its destination, or to its destination if it is within reach.
    private void Move(float deltaTime)
    {
        Vector2 direction = destination - position;
        float distance = 5f * deltaTime;
        Position += Vector2.ClampMagnitude(direction, distance);
    }
        
    /// <summary>
    /// Gets tile in world that corresponds to the relative coordinates in the ship definition
    /// counted from the berth.
    /// </summary>
    private Tile GetTile(World world, Furniture berth, int x, int y, int z)
    {
        int relative_x = x - BerthPointX;
        int relative_y = y - BerthPointY;
        int berth_x = berth.Tile.X + 1;
        int berth_y = berth.Tile.Y;
        int worldX, worldY;
        switch (BerthDirection)
        {
            case BerthDirection.NORTH:
                worldX = berth_x - relative_x;
                worldY = berth_y - relative_y;
                break;
            case BerthDirection.SOUTH:
                worldX = berth_x + relative_x;
                worldY = berth_y + relative_y;
                break;
            case BerthDirection.WEST:
                worldX = berth_x + relative_y;
                worldY = berth_y - relative_x;
                break;
            case BerthDirection.EAST:
                worldX = berth_x - relative_y;
                worldY = berth_y + relative_x;
                break;
            default:
                worldX = 0;
                worldY = 0;
                UnityDebugger.Debugger.LogError("Ships", "Invalid berthing direction: " + BerthDirection);
                break;
        }

        return world.GetTileAt(worldX, worldY, z);
    }

    /// <summary>
    /// Reads storage locations from XML and stores them in the prototype.
    /// </summary>
    /// <param name="reader">The XML reader.</param>
    private void ReadXmlStorages(XmlReader reader)
    {
        if (reader.ReadToDescendant("Storage"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("x"));
                ShipStorage storage = new ShipStorage(x, y);
                storages.Add(storage);
            }
            while (reader.ReadToNextSibling("Storage"));
        }
    }

    /// <summary>
    /// Reads Tiles XML tag that defines what tiles the ship occupies when unwrapped 
    /// and writes them to the tile type array.
    /// </summary>
    /// <param name="reader">The XML reader.</param>
    private void ReadXmlTiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                tileTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    /// <summary>
    /// Reads furniture types that the ship contains while unwrapped and
    /// writes them to the furniture type array.
    /// </summary>
    /// <param name="reader">The XML reader.</param>
    private void ReadXmlFurnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                furnitureTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Furniture"));
        }
    }

    /// <summary>
    /// Instantiates the tile and furniture type arrays and populates them with null values by default.
    /// </summary>
    private void InstantiateTiles()
    {
        tileTypes = new string[Width, Height];
        furnitureTypes = new string[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tileTypes[x, y] = null;
                furnitureTypes[x, y] = null;
            }
        }
    }
}