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

namespace ProjectPorcupine.Rooms
{
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

        public float Temperature
        {
            get
            {
                return temperature;
            }
        }

        // RoomBehavior is something like an airlock or office.
        public Dictionary<string, RoomBehavior> RoomBehaviors { get; private set; }

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

        // Changes gas by an amount in preasure(in atm) multiplyed by number of tiles, limited to a pressure
        public void ChangeGas(string name, float amount, float pressureLimit)
        {
            ChangeGas(name, Mathf.Min(amount, (TileCount * pressureLimit) - GetGasAmount(name)));
        }

        // Changes gas by an amount in pressure(in atm) multiplyed by number of tiles
        // Note: This description is somewhat misleading. Pressure is stored as the sum total pressure 
        //       of every tile's pressure, so while it is technically accurate, it implies that
        //       this method does the multiplication, when it is already stored that way
        public void ChangeGas(string name, float amount)
        {
            if (IsOutsideRoom())
            {
                return;
            }

            if (atmosphericGasses.ContainsKey(name))
            {
                atmosphericGasses[name] += amount;
                if (amount.IsZero())
                {
                    deltaGas[name] = "=";
                }
                else if (Mathf.Sign(amount) == 1)
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

        public string ChangeInGas(string name)
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
                return atmosphericGasses[name] / TileCount;
            }

            return 0;
        }

        public float GetGasFraction(string name)
        {
            if (atmosphericGasses.ContainsKey(name) == false)
            {
                return 0;
            }

            float totalGasses = GetTotalGas();
            return totalGasses == 0 ? 0 : atmosphericGasses[name] / totalGasses;
        }

        public float GetTotalGasPressure()
        {
            float totalPressure = 0;

            foreach (float pressure in atmosphericGasses.Values)
            {
                totalPressure += pressure / TileCount;
            }

            return totalPressure;
        }

        public float GetTotalGas()
        {
            float totalGas = 0;

            foreach (string n in atmosphericGasses.Keys)
            {
                totalGas += atmosphericGasses[n];
            }

            return totalGas;
        }

        public void MoveGasTo(Room room, float amount, float pressureLimit)
        {
            MoveGasTo(room, Mathf.Min(amount, (room.TileCount * pressureLimit) - room.GetTotalGas()));
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

        public void SplitGas(Room other, int sizeOfOtherRoom)
        {
            foreach (string n in other.atmosphericGasses.Keys)
            {
                this.atmosphericGasses[n] = other.atmosphericGasses[n] * TileCount / sizeOfOtherRoom;
            }
        }

        public void MoveGas(Room other)
        {
            foreach (string n in other.atmosphericGasses.Keys)
            {
                this.ChangeGas(n, other.atmosphericGasses[n]);
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
                Debug.ULogErrorChannel("Tile", "Trying to assign a RoomBehavior to a room that isn't valid!");
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