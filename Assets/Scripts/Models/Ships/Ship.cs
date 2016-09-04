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
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

public enum BerthDirection
{
    NORTH, SOUTH, EAST, WEST
}

public enum ShipState
{
    TRANSIT_IN, TRANSIT_OUT, BERTHED
}

[MoonSharpUserData]
public class Ship
{
    private ShipManager shipManager;
    private List<ShipStorage> storages;
    private string[,] tileTypes;
    private string[,] furnitureTypes;

    private ShipState state;
    private Vector2 position;
    private Vector2 destination;
    private Furniture berth;

    public Ship()
    {
        ShipType = null;
        Width = 0;
        Height = 0;
        BerthPointX = 0;
        BerthPointY = 0;
        BerthDirection = BerthDirection.NORTH;
        storages = new List<ShipStorage>();
        tileTypes = null;
        furnitureTypes = null;
    }

    public Ship(ShipManager shipManager, Ship proto)
    {
        this.shipManager = shipManager;
        ShipType = proto.ShipType;
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

        State = ShipState.TRANSIT_IN;
        Position = Vector2.zero;
    }

    public event ShipManager.ShipEventHandler ShipChanged;

    public string ShipType { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int BerthPointX { get; private set; }

    public int BerthPointY { get; private set; }

    public BerthDirection BerthDirection { get; private set; }

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

    public void SetDestination(float x, float y)
    {
<<<<<<< 3f241a70a7102aae561228e542afa6017a50f288
        Berth = null;
        Destination = new Vector2(x, y);
    }

    public void SetDestination(Furniture goalBerth)
    {
        Berth = goalBerth;
        Destination = new Vector2(goalBerth.Tile.X, goalBerth.Tile.Y);
=======
        berth = null;
        Destination = new Vector2(x,y);
>>>>>>> Implemented unwrapping at berth
    }
    public void SetDestination(Furniture goalBerth)
    {
        berth = goalBerth;
        Destination = new Vector2(goalBerth.Tile.X, goalBerth.Tile.Y);
    }

    public void Update(float deltaTime)
    {
<<<<<<< 3f241a70a7102aae561228e542afa6017a50f288
        switch (State)
        {
            case ShipState.BERTHED:
                break;
            case ShipState.TRANSIT:
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

    public void UnwrapAtBerth()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = GetTile(World.Current, x, y, 0);

                // Change tile to defined contents
                if (tile.Type.Equals(TileType.Empty) == false || tile.Furniture != null)
                {
                    Debug.ULogErrorChannel("Ships", "Tile " + tile.X + "," + tile.Y + " is not empty. Replacing anyway.");
                }

                tile.Type = TileType.GetTileType(tileTypes[x, y]);
                if (furnitureTypes[x, y] != null)
                {
                    World.Current.PlaceFurniture(furnitureTypes[x, y], tile, false);
                }
            }
        }
    }

    public void Wrap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = GetTile(World.Current, x, y, 0);

                // Change tile to empty
                // Order reversed in case furniture on an empty tile leads to problems
                if (furnitureTypes[x, y] != null)
                {
                    tile.Furniture.Deconstruct();
                }

                tile.Type = TileType.Empty;
            }
        }
=======
        switch(State)
        {
            case ShipState.BERTHED:
                break;
            case ShipState.TRANSIT_IN:
                Move(deltaTime);
                if (Vector2.Distance(Position, Destination) < 0.1f)
                {
                    State = ShipState.BERTHED;
                    UnwrapAtBerth();
                }

                break;
            case ShipState.TRANSIT_OUT:
                Move(deltaTime);
                if (Vector2.Distance(Position, Destination) < 0.1f)
                {
                    World.Current.shipManager.RemoveShip(this);
                }

                break;

        }
    }

    private void Move(float deltaTime)
    {
        Vector2 direction = destination - position;
        float distance = 5f * deltaTime;
        Position += Vector2.ClampMagnitude(direction, distance);
>>>>>>> Implemented unwrapping at berth
    }

    private void UnwrapAtBerth()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int relative_x = x - BerthPointX;
                int relative_y = y - BerthPointY;
                int worldX, worldY;
                GetWorldCoordinates(x, y, out worldX, out worldY);

                Tile tile = World.Current.GetTileAt(worldX, worldY, 0);

                // Change tile to defined contents
                if (tile.Type.Equals(TileType.Empty) == false || tile.Furniture != null)
                {
                    Debug.ULogErrorChannel("Ships", "Tile " + tile.X + "," + tile.Y + " is not empty. Replacing anyway.");
                }

                tile.Type = TileType.GetTileType(tileTypes[x, y]);
                if (furnitureTypes[x, y] != null)
                {
                    World.Current.PlaceFurniture(furnitureTypes[x, y], tile, false);
                }
            }
        }
    }

    private void Wrap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int relative_x = x - BerthPointX;
                int relative_y = y - BerthPointY;
                int worldX, worldY;
                GetWorldCoordinates(x, y, out worldX, out worldY);

                Tile tile = World.Current.GetTileAt(worldX, worldY, 0);

                // Change tile to empty
                // Order reversed in case furniture on an empty tile leads to problems
                if (furnitureTypes[x, y] != null)
                {
                    tile.UnplaceFurniture();
                }

                tile.Type = TileType.Empty;
            }
        }
    }

    private void GetWorldCoordinates(int x, int y, out int worldX, out int worldY)
    {
        int relative_x = x - BerthPointX;
        int relative_y = y - BerthPointY;
        int berth_x = Berth.Tile.X + 1;
        int berth_y = Berth.Tile.Y;
        switch(BerthDirection)
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
                Debug.ULogErrorChannel("Ships", "Invalid berthing direction: " + BerthDirection);
                break;
        }
    }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        ShipType = parentReader.GetAttribute("type");
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

    private void Move(float deltaTime)
    {
        Vector2 direction = destination - position;
        float distance = 5f * deltaTime;
        Position += Vector2.ClampMagnitude(direction, distance);
    }

    // Gets tile in world that corresponds to the relative coordinates in the ship definition
    // counted from the berth
    private Tile GetTile(World world, int x, int y, int z)
    {
        int relative_x = x - BerthPointX;
        int relative_y = y - BerthPointY;
        int berth_x = Berth.Tile.X + 1;
        int berth_y = Berth.Tile.Y;
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
                Debug.ULogErrorChannel("Ships", "Invalid berthing direction: " + BerthDirection);
                break;
        }
        return world.GetTileAt(worldX, worldY, z);
    }

    private void ReadXmlStorages(XmlReader reader)
    {
        if (reader.ReadToDescendant("Storage"))
        {
            // We have at least one tile, so do something with it.
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

    private void ReadXmlTiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                tileTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    private void ReadXmlFurnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            // We have at least one tile, so do something with it.
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                furnitureTypes[x, y] = reader.GetAttribute("type");
            }
            while (reader.ReadToNextSibling("Furniture"));
        }
    }

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