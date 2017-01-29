#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ProjectPorcupine.Rooms
{
    [MoonSharpUserData]
    public class Room
    {
        private Dictionary<string, string> deltaGas;

        private List<Tile> tiles;

        public Room()
        {
            tiles = new List<Tile>();
            Atmosphere = new AtmosphereComponent();
            deltaGas = new Dictionary<string, string>();
            RoomBehaviors = new Dictionary<string, RoomBehavior>();
        }

        public int ID
        {
            get
            {
                return World.Current.RoomManager.GetRoomID(this);
            }
        }

        public int TileCount
        {
            get
            {
                return tiles.Count;
            }
        }

        // RoomBehavior is something like an airlock or office.
        public Dictionary<string, RoomBehavior> RoomBehaviors { get; private set; }

        public AtmosphereComponent Atmosphere { get; private set; }

        public float GetGasPressure()
        {
            return IsOutsideRoom() ? 0.0f : Atmosphere.GetGasAmount() / TileCount;
        }

        public float GetGasPressure(string gasName)
        {
            return IsOutsideRoom() ? 0.0f : Atmosphere.GetGasAmount(gasName) / TileCount;
        }

        public Tile FindExitBetween(Room room2)
        {
            List<Tile> exits = this.FindExits();

            foreach (Tile exit in exits)
            {
                if (exit.GetNeighbours().Any(tile => tile.Room == room2))
                {
                    return exit;
                }
            }

            // In theory this should never be reached, if we are passed two rooms from a roomPath, there should always be an exit between
            // But we should probably add some kind of error checking anyways.
            return null;
        }

        public void AssignTile(Tile tile)
        {
            if (tiles.Contains(tile))
            {
                // This tile already in this room.
                return;
            }

            if (tile.Room != null)
            {
                // Belongs to some other room.
                tile.Room.tiles.Remove(tile);
            }

            tile.Room = this;
            tiles.Add(tile);
        }

        public void UnassignTile(Tile tile)
        {
            if (tiles.Contains(tile) == false)
            {
                // This tile in not in this room.
                return;
            }

            tile.Room = null;
            tiles.Remove(tile);
        }

        public void ReturnTilesToOutsideRoom()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                // Assign to outside.
                tiles[i].Room = World.Current.RoomManager.OutsideRoom;
            }

            tiles.Clear();
        }

        public bool IsOutsideRoom()
        {
            return this == World.Current.RoomManager.OutsideRoom;
        }

        public List<Tile> FindExits()
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

            return exits;
        }

        public Dictionary<Tile, Room> GetNeighbours()
        {
            Dictionary<Tile, Room> neighboursRooms = new Dictionary<Tile, Room>();

            List<Tile> exits = this.FindExits();

            foreach (Tile tile in exits)
            {
                // Loop over the exits to find a different room
                Tile[] neighbours = tile.GetNeighbours();
                foreach (Tile neighbor in neighbours)
                {
                    if (neighbor == null || neighbor.Room == null)
                    {
                        continue;
                    }

                    // We have found a room
                    if (neighbor.Room != this)
                    {
                        neighboursRooms[neighbor] = neighbor.Room;
                    }
                }
            }

            return neighboursRooms;
        }

        public JObject ToJson()
        {
            JObject roomGasses = new JObject();
            foreach (string k in Atmosphere.GetGasNames())
            {
                roomGasses.Add(k, Atmosphere.GetGasAmount(k));
            }

            return roomGasses;
        }

        public void FromJson(JToken room)
        {
            foreach (JProperty gas in ((JObject)room).Properties())
            {
                string k = gas.Name;
                float v = (float)gas.Value;
                Atmosphere.SetGas(k, v);
            }
        }

        public bool DesignateRoomBehavior(RoomBehavior objInstance)
        {
            if (objInstance == null)
            {
                return false;
            }

            if (RoomBehaviors.ContainsKey(objInstance.Type))
            {
                return false;
            }

            if (objInstance.IsValidRoom(this) == false)
            {
                UnityDebugger.Debugger.LogError("Tile", "Trying to assign a RoomBehavior to a room that isn't valid!");
                return false;
            }

            objInstance.Control(this);

            RoomBehaviors.Add(objInstance.Type, objInstance);

            return true;
        }

        public bool UndesignateRoomBehavior(RoomBehavior roomBehavior)
        {
            // Just uninstalling.
            if (RoomBehaviors == null)
            {
                return false;
            }

            RoomBehaviors.Remove(roomBehavior.Type);

            return true;
        }

        public List<Tile> GetInnerTiles()
        {
            return tiles;
        }

        public List<Tile> GetBorderingTiles()
        {
            List<Tile> borderingTiles = new List<Tile>();
            foreach (Tile tile in tiles)
            {
                Tile[] neighbours = tile.GetNeighbours();
                foreach (Tile tile2 in neighbours)
                {
                    if (tile2 != null && tile2.Furniture != null)
                    {
                        if (tile2.Furniture.RoomEnclosure)
                        {
                            // We have found an enclosing furniture, which means it is on our border
                            borderingTiles.Add(tile2);
                        }
                    }
                }
            }

            return borderingTiles;
        }

        public bool HasRoomBehavior(string behaviorKey)
        {
            return RoomBehaviors.ContainsKey(behaviorKey);
        }
    }
}