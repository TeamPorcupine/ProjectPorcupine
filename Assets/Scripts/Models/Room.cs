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
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Room : IXmlSerializable
{
    // Dictionary with the amount of gas in room stored in preasure(in atm) multiplyed by number of tiles.
    private Dictionary<string, float> atmosphericGasses;
    private Dictionary<string, string> deltaGas;

    private List<Tile> tiles;
    private float temperature;

    public Room()
    {
        tiles = new List<Tile>();
        atmosphericGasses = new Dictionary<string, float>();
        deltaGas = new Dictionary<string, string>();
    }

    public int ID
    {
        get
        {
            return World.Current.GetRoomID(this);
        }
    }

    public float Temperature
    {
        get
        {
            return temperature;
        }
    }

    public static void EqualiseGasByTile(Tile tile, float leakFactor)
    {
        List<Room> roomsDone = new List<Room>();
        foreach (Tile t in tile.GetNeighbours())
        {
            // Skip tiles with a null room (i.e. outside).
            // TODO: Verify that gas still leaks to the outside
            // somehow.
            if (t.Room == null)
            {
                continue;
            }

            if (roomsDone.Contains(t.Room) == false)
            {
                foreach (Room r in roomsDone)
                {
                    t.Room.EqualiseGas(r, leakFactor);
                }

                roomsDone.Add(t.Room);
            }
        }
    }

    public static void DoRoomFloodFill(Tile sourceTile, bool onlyIfOutside = false)
    {
        // SourceFurniture is the piece of furniture that may be
        // splitting two existing rooms, or may be the final 
        // enclosing piece to form a new room.
        // Check the NESW neighbours of the furniture's tile
        // and do flood fill from them.
        World world = World.Current;

        Room oldRoom = sourceTile.Room;

        if (oldRoom != null)
        {
            // Save the size of old room before we start removing tiles.
            // Needed for gas calculations.
            int sizeOfOldRoom = oldRoom.GetSize();

            // The source tile had a room, so this must be a new piece of furniture
            // that is potentially dividing this old room into as many as four new rooms.

            // Try building new rooms for each of our NESW directions.
            foreach (Tile t in sourceTile.GetNeighbours())
            {
                if (t != null && t.Room != null && (onlyIfOutside == false || t.Room.IsOutsideRoom()))
                {
                    ActualFloodFill(t, oldRoom, sizeOfOldRoom);
                }
            }

            sourceTile.Room = null;

            oldRoom.tiles.Remove(sourceTile);

            // If this piece of furniture was added to an existing room
            // (which should always be true assuming with consider "outside" to be a big room)
            // delete that room and assign all tiles within to be "outside" for now.
            if (oldRoom.IsOutsideRoom() == false)
            {
                // At this point, oldRoom shouldn't have any more tiles left in it,
                // so in practice this "DeleteRoom" should mostly only need
                // to remove the room from the world's list.
                if (oldRoom.tiles.Count > 0)
                {
                    Debug.ULogErrorChannel("Room", "'oldRoom' still has tiles assigned to it. This is clearly wrong.");
                }

                world.DeleteRoom(oldRoom);
            }
        }
        else
        {
            // oldRoom is null, which means the source tile was probably a wall,
            // though this MAY not be the case any longer (i.e. the wall was 
            // probably deconstructed. So the only thing we have to try is
            // to spawn ONE new room starting from the tile in question.

            // You need to delete the surrounding rooms so a new room can be created
            // This doesn't work for the gas calculations and needs to be fixed.
            foreach (Tile t in sourceTile.GetNeighbours())
            {
                if (t != null && t.Room != null && (onlyIfOutside == false || t.Room.IsOutsideRoom()))
                {
                    world.DeleteRoom(t.Room);
                }
            }

            ActualFloodFill(sourceTile, null, 0);
        }
    }

    public void AssignTile(Tile t)
    {
        if (tiles.Contains(t))
        {
            // This tile already in this room.
            return;
        }

        if (t.Room != null)
        {
            // Belongs to some other room.
            t.Room.tiles.Remove(t);
        }

        t.Room = this;
        tiles.Add(t);
    }

    public void ReturnTilesToOutsideRoom()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            // Assign to outside.
            tiles[i].Room = World.Current.GetOutsideRoom();
        }

        tiles = new List<Tile>();
    }

    public bool IsOutsideRoom()
    {
        return this == World.Current.GetOutsideRoom();
    }

    public int GetSize()
    {
        return tiles.Count();
    }

    // Changes gas by amount in pressure (atm) per tile, evenly distributed over all gases present
    // Possibly deprecated. Use MoveGasTo(room, amount)
    public void ChangeGas(float amount)
    {
        if (IsOutsideRoom())
        {
            return;
        }

        List<string> names = new List<string>(atmosphericGasses.Keys);
        foreach (string name in names)
        {
            float fraction = GetGasFraction(name);
            ChangeGas(name, amount * fraction);
        }
    }

    public Tile[] FindExits()
    {
        List<Tile> exits = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            Tile[] neighbours = tile.GetNeighbours();
            foreach (Tile tile2 in neighbours)
            {
                if (tile2 != null && tile2.Furniture != null)
                {
                    if (tile2.Furniture.IsExit())
                    {
                        // We have found an exit
                        exits.Add(tile2);
                    }
                }      
            }
        }

        return exits.ToArray();
    }
        
    public Dictionary<Tile, Room> GetNeighbours()
    {
        Dictionary<Tile, Room> neighboursRooms = new Dictionary<Tile, Room>();

        Tile[] exits = this.FindExits();

        foreach (Tile t in exits)
        {
            // Loop over the exits to find a different room
            Tile[] neighbours = t.GetNeighbours();
            foreach (Tile tile2 in neighbours)
            {
                if (tile2 == null || tile2.Room == null)
                {
                    continue;
                }

                // We have found a room
                if (tile2.Room != this)
                {
                    neighboursRooms[tile2] = tile2.Room;
                }
            }
        }

        return neighboursRooms;
    }

    // Changes gas by an amount in preasure(in atm) multiplyed by number of tiles
    public void ChangeGas(string name, float amount)
    {
        if (IsOutsideRoom())
        {
            return;
        }

        if (atmosphericGasses.ContainsKey(name))
        {
            atmosphericGasses[name] += amount;
            if (Mathf.Sign(amount) == 1)
            {
                deltaGas[name] = "+";
            }
            else
            {
                deltaGas[name] = "-";
            }
        }
        else
        {
            atmosphericGasses[name] = amount;
            deltaGas[name] = "=";
        }

        if (atmosphericGasses[name] < 0)
        {
            atmosphericGasses[name] = 0;
        }
    }

    public string ChangedGases(string name)
    {
        if (deltaGas.ContainsKey(name))
        {
            return deltaGas[name];
        }

        return "=";
    }

    public void EqualiseGas(Room otherRoom, float leakFactor)
    {
        if (otherRoom == null)
        {
            return;
        }

        List<string> gasses = this.GetGasNames().ToList();
        gasses = gasses.Union(otherRoom.GetGasNames().ToList()).ToList();
        foreach (string gas in gasses)
        {
            float pressureDifference = this.GetGasPressure(gas) - otherRoom.GetGasPressure(gas);
            this.ChangeGas(gas, (-1) * pressureDifference * leakFactor);
            otherRoom.ChangeGas(gas, pressureDifference * leakFactor);
        }
    }

    // Gets absolute gas amount in pressure(in atm) multiplied by number of tiles.
    public float GetGasAmount(string name)
    {
        if (atmosphericGasses.ContainsKey(name))
        {
            return atmosphericGasses[name];
        }

        return 0;
    }

    // Gets gas amount in pressure(in atm).
    public float GetGasPressure()
    {
        float pressure = 0;
        foreach (float p in atmosphericGasses.Values)
        {
            pressure += p;
        }

        return pressure;
    }

    // Gets gas amount in pressure (in atm).
    public float GetGasPressure(string name)
    {
        if (atmosphericGasses.ContainsKey(name))
        {
            return atmosphericGasses[name] / GetSize();
        }

        return 0;
    }

    public float GetGasFraction(string name)
    {
        if (atmosphericGasses.ContainsKey(name) == false)
        {
            return 0;
        }

        float t = 0;

        foreach (string n in atmosphericGasses.Keys)
        {
            t += atmosphericGasses[n];
        }

        return t == 0 ? 0 : atmosphericGasses[name] / t;
    }

    public float GetTotalGasPressure()
    {
        float t = 0;

        foreach (string n in atmosphericGasses.Keys)
        {
            t += atmosphericGasses[n] / GetSize();
        }

        return t;
    }

    public void MoveGasTo(Room room, float amount)
    {
        List<string> names = new List<string>(atmosphericGasses.Keys);
        foreach (string name in names)
        {
            MoveGasTo(room, name, amount * GetGasFraction(name));
        }
    }

    public void MoveGasTo(Room room, string name, float amount)
    {
        float amountMoved = Mathf.Min(amount, GetGasAmount(name));
        this.ChangeGas(name, -amountMoved);
        room.ChangeGas(name, amountMoved);
    }

    public float GetTemperature()
    {
        return temperature;
    }

    public void ChangeTemperature(float change)
    {
        temperature += change;
    }

    public string[] GetGasNames()
    {
        return atmosphericGasses.Keys.ToArray();
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Write out gas info.
        foreach (string k in atmosphericGasses.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", atmosphericGasses[k].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader)
    {
        // Read gas info.
        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                atmosphericGasses[k] = v;
            }
            while (reader.ReadToNextSibling("Param"));
        }
    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom, int sizeOfOldRoom)
    {
        if (tile == null)
        {
            // We are trying to flood fill off the map, so just return
            // without doing anything.
            return;
        }

        if (tile.Room != oldRoom)
        {
            // This tile was already assigned to another "new" room, which means
            // that the direction picked isn't isolated. So we can just return
            // without creating a new room.
            return;
        }

        if (tile.Furniture != null && tile.Furniture.RoomEnclosure)
        {
            // This tile has a wall/door/whatever in it, so clearly
            // we can't do a room here.
            return;
        }

        if (tile.Type == TileType.Empty)
        {
            // This tile is empty space and must remain part of the outside.
            return;
        }

        // If we get to this point, then we know that we need to create a new room.
        List<Room> listOfOldRooms = new List<Room>();

        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        bool connectedToSpace = false;
        int processedTiles = 0;

        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();

            processedTiles++;

            if (t.Room != newRoom)
            {
                if (t.Room != null && listOfOldRooms.Contains(t.Room) == false)
                {
                    listOfOldRooms.Add(t.Room);
                    newRoom.MoveGas(t.Room);
                }

                newRoom.AssignTile(t);

                Tile[] ns = t.GetNeighbours();
                foreach (Tile t2 in ns)
                {
                    if (t2 == null || t2.Type == TileType.Empty)
                    {
                        // We have hit open space (either by being the edge of the map or being an empty tile)
                        // so this "room" we're building is actually part of the Outside.
                        // Therefore, we can immediately end the flood fill (which otherwise would take ages)
                        // and more importantly, we need to delete this "newRoom" and re-assign
                        // all the tiles to Outside.
                        connectedToSpace = true;
                    }
                    else
                    {
                        // We know t2 is not null nor is it an empty tile, so just make sure it
                        // hasn't already been processed and isn't a "wall" type tile.
                        if (
                            t2.Room != newRoom && (t2.Furniture == null || t2.Furniture.RoomEnclosure == false))
                        {
                            tilesToCheck.Enqueue(t2);
                        }
                    }
                }
            }
        }

        if (connectedToSpace)
        {
            // All tiles that were found by this flood fill should
            // actually be "assigned" to outside.
            newRoom.ReturnTilesToOutsideRoom();
            return;
        }

        // Copy data from the old room into the new room.
        if (oldRoom != null)
        {
            // In this case we are splitting one room into two or more,
            // so we can just copy the old gas ratios.
            newRoom.CopyGasPreasure(oldRoom, sizeOfOldRoom);
        }

        newRoom.FindExits();

        // Tell the world that a new room has been formed.
        World.Current.AddRoom(newRoom);
    }

    private void CopyGasPreasure(Room other, int sizeOfOtherRoom)
    {
        foreach (string n in other.atmosphericGasses.Keys)
        {
            this.atmosphericGasses[n] = other.atmosphericGasses[n] / sizeOfOtherRoom * this.GetSize();
        }
    }

    private void MoveGas(Room other)
    {
        foreach (string n in other.atmosphericGasses.Keys)
        {
            this.ChangeGas(n, other.atmosphericGasses[n]);
        }
    }
}
