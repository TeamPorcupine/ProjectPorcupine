#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProjectPorcupine
{
    public class RoomManager : IEnumerable<Room>
    {
        private List<Room> rooms;

        public RoomManager()
        {
            rooms = new List<Room>();

            // Create the outside room.
            OutsideRoom = new Room();

            // Add it to the list just in case.
            rooms.Add(OutsideRoom);

            // TODO REMOVE THIS
            Adding += (room) => { Debug.ULogError("Adding New Room! " + room); };
            Added += (room) => { Debug.ULogError("Added a New Room! with id: " + room.ID); };
            Split += (oldRoom, newRoom) => { Debug.ULogError("Spliting " + oldRoom.ID + " into: " + newRoom.ID); };
            Joined += (oldRoom, newRoom) => { Debug.ULogError("Joining " + oldRoom.ID + " into: " + newRoom.ID); };
            Removing += (room) => { Debug.ULogError("Removing a Room! with id: " + room.ID); };
            Removed += (room) => { Debug.ULogError("Removed a Room! " + room); };
        }

        #region Events

        public event Action<Room> Adding;

        public event Action<Room> Added;

        public event Action<Room, Room> Split;

        public event Action<Room, Room> Joined;

        public event Action<Room> Removing;

        public event Action<Room> Removed;

        #endregion

        public int Rooms 
        {
            get 
            { 
                return rooms.Count;
            }
        }

        public Room OutsideRoom
        {
            get;
            private set;
        }

        public int GetRoomID(Room room)
        {
            return rooms.IndexOf(room);
        }

        public Room GetRoomFromID(int index)
        {
            if (index < 0 || index > Rooms - 1)
            {
                return null;
            }

            return rooms[index];
        }

        public void Add(Room room)
        {
            World.Current.roomGraph = null;

            if (Adding != null)
            {
                Adding(room);
            }

            rooms.Add(room);

            Debug.ULogChannel("Rooms", "added room:" + room.ID);

            if (Added != null)
            {
                Added(room);
            }
        }

        public void Remove(Room room)
        {
            if (room.IsOutsideRoom())
            {
                Debug.ULogErrorChannel("World", "Tried to delete the outside room.");
                return;
            }

            Debug.ULogChannel("Rooms", "Removing room:" + room.ID);

            World.Current.roomGraph = null;

            if (Removing != null)
            {
                Removing(room);
            }

            // Remove this room from our rooms list.
            rooms.Remove(room);

            // All tiles that belonged to this room should be re-assigned to
            // the outside.
            room.ReturnTilesToOutsideRoom();

            if (Removed != null)
            {
                Removed(room);
            }
        }

        public void EqualiseGasByTile(Tile tile, float leakFactor)
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

        public void DoRoomFloodFill(Tile sourceTile, bool splitting)
        {
            // SourceFurniture is the piece of furniture that may be
            // splitting two existing rooms, or may be the final 
            // enclosing piece to form a new room.
            // Check the NESW neighbours of the furniture's tile
            // and do flood fill from them.
            Room oldRoom = sourceTile.Room;

            if (oldRoom != null && splitting)
            {
                // The source tile had a room, so this must be a new piece of furniture
                // that is potentially dividing this old room into as many as four new rooms.

                // Save the size of old room before we start removing tiles.
                // Needed for gas calculations.
                int sizeOfOldRoom = oldRoom.Tiles;

                // Try building new rooms for each of our NESW directions.
                foreach (Tile t in sourceTile.GetNeighbours())
                {
                    if (t != null && t.Room != null && t.Room.IsOutsideRoom())
                    {
                        Room newRoom = ActualFloodFill(t, oldRoom, sizeOfOldRoom);

                        if (newRoom != null && Split != null)
                        {
                            Split(oldRoom, newRoom);
                        }
                    }
                }

                sourceTile.Room = null;

                oldRoom.UnassignTile(sourceTile);

                // If this piece of furniture was added to an existing room
                // (which should always be true assuming with consider "outside" to be a big room)
                // delete that room and assign all tiles within to be "outside" for now.
                if (oldRoom.IsOutsideRoom() == false)
                {
                    // At this point, oldRoom shouldn't have any more tiles left in it,
                    // so in practice this "DeleteRoom" should mostly only need
                    // to remove the room from the world's list.
                    if (oldRoom.Tiles > 0)
                    {
                        Debug.ULogErrorChannel("Room", "'oldRoom' still has tiles assigned to it. This is clearly wrong.");
                    }

                    Remove(oldRoom);
                }
            }
            else if (oldRoom == null && splitting == false)
            {
                // oldRoom is null, which means the source tile was probably a wall,
                // though this MAY not be the case any longer (i.e. the wall was 
                // probably deconstructed. So the only thing we have to try is
                // to spawn ONE new room starting from the tile in question.

                // Save a list of all the rooms to be removed for later calls
                // TODO: find a way of not doing this, because at the time of the
                // later calls, this is stale data.
                List<Room> oldRooms = new List<Room>();

                // You need to delete the surrounding rooms so a new room can be created
                // FIXME: This doesn't work for the gas calculations and needs to be fixed.
                foreach (Tile t in sourceTile.GetNeighbours())
                {
                    if (t != null && t.Room != null && t.Room.IsOutsideRoom())
                    {
                        oldRooms.Add(t.Room);

                        Remove(t.Room);
                    }
                }

                // FIXME: find a better way to do this since right now it 
                // requires using stale data.
                Room newRoom = ActualFloodFill(sourceTile, null, 0);
                if (newRoom != null && oldRooms.Count > 0 && Joined != null)
                {
                    foreach (Room r in oldRooms)
                    {
                        Joined(r, newRoom);
                    }
                }
            }
        }

        #region IEnumerable implementation

        public IEnumerator GetEnumerator()
        {
            return rooms.GetEnumerator();
        }

        IEnumerator<Room> IEnumerable<Room>.GetEnumerator()
        {
            foreach (Room room in rooms)
            {
                yield return room;
            }
        }

        #endregion

        protected Room ActualFloodFill(Tile tile, Room oldRoom, int sizeOfOldRoom)
        {
            if (tile == null)
            {
                // We are trying to flood fill off the map, so just return
                // without doing anything.
                return null;
            }

            if (tile.Room != oldRoom)
            {
                // This tile was already assigned to another "new" room, which means
                // that the direction picked isn't isolated. So we can just return
                // without creating a new room.
                return null;
            }

            if (tile.Furniture != null && tile.Furniture.RoomEnclosure)
            {
                // This tile has a wall/door/whatever in it, so clearly
                // we can't do a room here.
                return null;
            }

            if (tile.Type == TileType.Empty)
            {
                // This tile is empty space and must remain part of the outside.
                return null;
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
                return null;
            }

            // Copy data from the old room into the new room.
            if (oldRoom != null)
            {
                // In this case we are splitting one room into two or more,
                // so we can just copy the old gas ratios.
                newRoom.CopyGasPreasure(oldRoom, sizeOfOldRoom);
            }

            // Tell the world that a new room has been formed.
            Add(newRoom);

            return newRoom;
        }
    }
}

