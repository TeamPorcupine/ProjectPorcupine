using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;


[MoonSharpUserData]
public class Room : IXmlSerializable {

	Dictionary<string, float> atmosphericGasses;

	List<Tile> tiles;

	public Room() {
		tiles = new List<Tile>();
		atmosphericGasses = new Dictionary<string, float>();
	}

	public int ID {
		get {
			return World.current.GetRoomID( this );
		}
	}

	public void AssignTile( Tile t ) {
		if(tiles.Contains(t)) {
			// This tile already in this room.
			return;
		}

		if(t.room != null) {
			// Belongs to some other room
			t.room.tiles.Remove(t);
		}
			
		t.room = this;
		tiles.Add(t);
	}

	public void ReturnTilesToOutsideRoom() {
		for (int i = 0; i < tiles.Count; i++) {
			tiles[i].room = World.current.GetOutsideRoom();	// Assign to outside
		}
		tiles = new List<Tile>();
	}

	public bool IsOutsideRoom() {
		return this == World.current.GetOutsideRoom();
	}

	public void ChangeGas(string name, float amount) {
		if(IsOutsideRoom())
			return;

		if( atmosphericGasses.ContainsKey(name) ) {
			atmosphericGasses[name] += amount;
		}
		else {
			atmosphericGasses[name] = amount;
		}

		if(atmosphericGasses[name] < 0)
			atmosphericGasses[name] = 0;

	}

	public float GetGasAmount(string name) {
		if( atmosphericGasses.ContainsKey(name) ) {
			return atmosphericGasses[name];
		}

		return 0;
	}

	public float GetGasPercentage(string name) {
		if( atmosphericGasses.ContainsKey(name) == false ) {
			return 0;
		}

		float t = 0;

		foreach(string n in atmosphericGasses.Keys) {
			t += atmosphericGasses[n];
		}

		return atmosphericGasses[name] / t;
	}

	public string[] GetGasNames() {
		return atmosphericGasses.Keys.ToArray();
	}

	public static void DoRoomFloodFill(Tile sourceTile, bool onlyIfOutside = false) {
		// sourceFurniture is the piece of furniture that may be
		// splitting two existing rooms, or may be the final 
		// enclosing piece to form a new room.
		// Check the NESW neighbours of the furniture's tile
		// and do flood fill from them

		World world = World.current;

		Room oldRoom = sourceTile.room;

		if(oldRoom != null) {
			// The source tile had a room, so this must be a new piece of furniture
			// that is potentially dividing this old room into as many as four new rooms.

			// Try building new rooms for each of our NESW directions
			foreach(Tile t in sourceTile.GetNeighbours() ) {
				if( t.room != null && (onlyIfOutside == false || t.room.IsOutsideRoom()) ) {
					ActualFloodFill( t, oldRoom );
				}
			}

			sourceTile.room = null;

			oldRoom.tiles.Remove(sourceTile);

			// If this piece of furniture was added to an existing room
			// (which should always be true assuming with consider "outside" to be a big room)
			// delete that room and assign all tiles within to be "outside" for now

			if(oldRoom.IsOutsideRoom() == false) {
				// At this point, oldRoom shouldn't have any more tiles left in it,
				// so in practice this "DeleteRoom" should mostly only need
				// to remove the room from the world's list.

				if(oldRoom.tiles.Count > 0) {
					Debug.LogError("'oldRoom' still has tiles assigned to it. This is clearly wrong.");
				}

				world.DeleteRoom(oldRoom);
			}
		}
		else {
			// oldRoom is null, which means the source tile was probably a wall,
			// though this MAY not be the case any longer (i.e. the wall was 
			// probably deconstructed. So the only thing we have to try is
			// to spawn ONE new room starting from the tile in question.
			ActualFloodFill( sourceTile, null );
		}


	}

	protected static void ActualFloodFill(Tile tile, Room oldRoom) {
		//Debug.Log("ActualFloodFill");

		if(tile == null) {
			// We are trying to flood fill off the map, so just return
			// without doing anything.
			return;
		}

		if(tile.room != oldRoom) {
			// This tile was already assigned to another "new" room, which means
			// that the direction picked isn't isolated. So we can just return
			// without creating a new room.
			return;
		}

		if(tile.furniture != null && tile.furniture.roomEnclosure) {
			// This tile has a wall/door/whatever in it, so clearly
			// we can't do a room here.
			return;
		}

		if(tile.Type == TileType.Empty) {
			// This tile is empty space and must remain part of the outside.
			return;
		}
			

		// If we get to this point, then we know that we need to create a new room.

		Room newRoom = new Room();
		Queue<Tile> tilesToCheck = new Queue<Tile>();
		tilesToCheck.Enqueue(tile);

		bool isConnectedToSpace = false;
		int processedTiles = 0;

		while(tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue();

			processedTiles++;


			if( t.room != newRoom ) {
				newRoom.AssignTile(t);

				Tile[] ns = t.GetNeighbours( );
				foreach(Tile t2 in ns) {
					if(t2 == null || t2.Type == TileType.Empty) {
						// We have hit open space (either by being the edge of the map or being an empty tile)
						// so this "room" we're building is actually part of the Outside.
						// Therefore, we can immediately end the flood fill (which otherwise would take ages)
						// and more importantly, we need to delete this "newRoom" and re-assign
						// all the tiles to Outside.

						isConnectedToSpace = true;

						/*if(oldRoom != null) {
							newRoom.ReturnTilesToOutsideRoom();
							return;
						}*/
					}
					else {
						// We know t2 is not null nor is it an empty tile, so just make sure it
						// hasn't already been processed and isn't a "wall" type tile.
						if(
							t2.room != newRoom && (t2.furniture == null || t2.furniture.roomEnclosure == false) ) {
							tilesToCheck.Enqueue( t2 );
						}
					}
				}

			}
		}

		//Debug.Log("ActualFloodFill -- Processed Tiles: " + processedTiles);

		if(isConnectedToSpace) {
			// All tiles that were found by this flood fill should
			// actually be "assigned" to outside
			newRoom.ReturnTilesToOutsideRoom();
			return;
		}

		// Copy data from the old room into the new room.
		if(oldRoom != null) {
			// In this case we are splitting one room into two or more,
			// so we can just copy the old gas ratios.
			newRoom.CopyGas(oldRoom);
		}
		else {
			// In THIS case, we are MERGING one or more rooms together,
			// so we need to actually figure out the total volume of gas
			// in the old room vs the new room and correctly adjust
			// atmospheric quantities.

			// TODO
		}

		// Tell the world that a new room has been formed.
		World.current.AddRoom(newRoom);
	}

	void CopyGas(Room other) {
		foreach(string n in other.atmosphericGasses.Keys) {
			this.atmosphericGasses[n] = other.atmosphericGasses[n];
		}
	}

	public XmlSchema GetSchema() {
		return null;
	}


	public void WriteXml(XmlWriter writer) {
		// Write out gas info
		foreach(string k in atmosphericGasses.Keys) {
			writer.WriteStartElement("Param");
			writer.WriteAttributeString("name", k);
			writer.WriteAttributeString("value", atmosphericGasses[k].ToString());
			writer.WriteEndElement();
		}

	}

	public void ReadXml(XmlReader reader) {
		// Read gas info
		if(reader.ReadToDescendant("Param")) {
			do {
				string k = reader.GetAttribute("name");
				float v = float.Parse( reader.GetAttribute("value") );
				atmosphericGasses[k] = v;
			} while (reader.ReadToNextSibling("Param"));
		}
	}



}
