﻿//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;


// TileType is the base type of the tile. In some tile-based games, that might be
// the terrain type. For us, we only need to differentiate between empty space
// and floor (a.k.a. the station structure/scaffold). Walls/Doors/etc... will be
// InstalledObjects sitting on top of the floor.
public enum TileType
{
    Empty,
    Floor
};

public enum ENTERABILITY
{
    Yes,
    Never,
    Soon
};

[MoonSharpUserData]
public class Tile :IXmlSerializable, ISelectable
{
    private TileType _type = TileType.Empty;

    public TileType Type
    {
        get { return _type; }
        set
        {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed.

            if (cbTileChanged != null && oldType != _type)
            {
                cbTileChanged(this);
            }
        }
    }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    public Inventory inventory;

    public Room room;

    public List<Character> characters;

    // Furniture is something like a wall, door, or sofa.
    public Furniture furniture
    {
        get;
        protected set;
    }

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job pendingFurnitureJob;

    public int X { get; protected set; }

    public int Y { get; protected set; }

    // FIXME: This is just hardcoded for now.  Basically just a reminder of something we
    // might want to do more with in the future.
    const float baseTileMovementCost = 1;

    public float movementCost
    {
        get
        {

            if (Type == TileType.Empty)
                return 0;	// 0 is unwalkable

            if (furniture == null)
                return baseTileMovementCost;

            return baseTileMovementCost * furniture.movementCost;
        }
    }

    // The function we callback any time our tile's data changes
    Action<Tile> cbTileChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="World.current">A World.current instance.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
        characters = new List<Character>();
    }

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    private static int[][] offsets = new int[][] { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };

    public bool UnplaceFurniture()
    {
        // Just uninstalling.  FIXME:  What if we have a multi-tile furniture?
        List<Tile> affectedTiles = new List<Tile>();


        if (furniture == null)
            return false;

        Furniture f = furniture;

        f.CancelJobs (); // Canceles all jobs by this furniture.

        for (int x_off = X; x_off < (X + f.Width); x_off++)
        {
            for (int y_off = Y; y_off < (Y + f.Height); y_off++)
            {

                Tile t = World.current.GetTileAt(x_off, y_off);
                if (t.furniture.linksToNeighbour)
                    affectedTiles.Add (t);
                t.furniture = null;
            }
        }

        if (!f.linksToNeighbour)
            return true;

        //This will update all neighbour tiles.

        foreach (Tile t in affectedTiles)
        {
            foreach (int[] offset in offsets) { // Using hardcoded offsets. Is there better way?
                Tile tmpTile = World.current.GetTileAt (t.X+offset[0], t.Y+offset[1]);
                if (!affectedTiles.Contains (tmpTile))
                {
                    if (tmpTile.furniture != null && tmpTile.furniture.objectType == f.objectType) {
                        tmpTile.furniture.cbOnChanged (tmpTile.furniture);
                    }
                }
            }
        }

        return true;
    }

    public bool PlaceFurniture(Furniture objInstance)
    {

        if (objInstance == null)
        {
            return UnplaceFurniture();
        }

        if (objInstance.IsValidPosition(this) == false)
        {
            Debug.LogError("Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }
		
        for (int x_off = X; x_off < (X + objInstance.Width); x_off++)
        {
            for (int y_off = Y; y_off < (Y + objInstance.Height); y_off++)
            {

                Tile t = World.current.GetTileAt(x_off, y_off);
                t.furniture = objInstance;

            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inv)
    {
        if (inv == null)
        {
            inventory = null;
            return true;
        }

        if (inventory != null)
        {
            // There's already inventory here. Maybe we can combine a stack?

            if (inventory.objectType != inv.objectType)
            {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inv.stackSize;
            if (inventory.stackSize + numToMove > inventory.maxStackSize)
            {
                numToMove = inventory.maxStackSize - inventory.stackSize;
            }

            inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            return true;
        }

        // At this point, we know that our current inventory is actually
        // null.  Now we can't just do a direct assignment, because
        // the inventory manager needs to know that the old stack is now
        // empty and has to be removed from the previous lists.

        inventory = inv.Clone();
        inventory.tile = this;
        inv.stackSize = 0;

        return true;
    }

    // Tells us if two tiles are adjacent.
    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        // Check to see if we have a difference of exactly ONE between the two
        // tile coordinates.  Is so, then we are vertical or horizontal neighbours.
        return 
			Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1 || // Check hori/vert adjacency
        (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1)); // Check diag adjacency
    }

    /// <summary>
    /// Gets the neighbours.
    /// </summary>
    /// <returns>The neighbours.</returns>
    /// <param name="diagOkay">Is diagonal movement okay?.</param>
    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;

        if (diagOkay == false)
        {
            ns = new Tile[4];	// Tile order: N E S W
        }
        else
        {
            ns = new Tile[8];	// Tile order : N E S W NE SE SW NW
        }

        Tile n;

        n = World.current.GetTileAt(X, Y + 1);
        ns[0] = n;	// Could be null, but that's okay.
        n = World.current.GetTileAt(X + 1, Y);
        ns[1] = n;	// Could be null, but that's okay.
        n = World.current.GetTileAt(X, Y - 1);
        ns[2] = n;	// Could be null, but that's okay.
        n = World.current.GetTileAt(X - 1, Y);
        ns[3] = n;	// Could be null, but that's okay.

        if (diagOkay == true)
        {
            n = World.current.GetTileAt(X + 1, Y + 1);
            ns[4] = n;	// Could be null, but that's okay.
            n = World.current.GetTileAt(X + 1, Y - 1);
            ns[5] = n;	// Could be null, but that's okay.
            n = World.current.GetTileAt(X - 1, Y - 1);
            ns[6] = n;	// Could be null, but that's okay.
            n = World.current.GetTileAt(X - 1, Y + 1);
            ns[7] = n;	// Could be null, but that's okay.
        }

        return ns;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("RoomID", room == null ? "-1" : room.ID.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        // X and Y have already been read/processed

        room = World.current.GetRoomFromID(int.Parse(reader.GetAttribute("RoomID")));

        Type = (TileType)int.Parse(reader.GetAttribute("Type"));


    }

    public ENTERABILITY IsEnterable()
    {
        // This returns true if you can enter this tile right this moment.
        if (movementCost == 0)
            return ENTERABILITY.Never;

        // Check out furniture to see if it has a special block on enterability
        if (furniture != null)
        {
            return furniture.IsEnterable();
        }

        return ENTERABILITY.Yes;
    }

    public Tile North()
    {
        return World.current.GetTileAt(X, Y + 1);
    }

    public Tile South()
    {
        return World.current.GetTileAt(X, Y - 1);
    }

    public Tile East()
    {
        return World.current.GetTileAt(X + 1, Y);
    }

    public Tile West()
    {
        return World.current.GetTileAt(X - 1, Y);
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return this._type.ToString();
    }

    public string GetDescription()
    {
        return "The tile.";
    }

    public string GetHitPointString()
    {
        return "";	// Do tiles have hitpoints? Can flooring be damaged? Obviously "empty" is indestructible.
    }

    #endregion
}
