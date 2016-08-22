#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
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
    Floor,
    Ladder
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
            if(_type != value)
            {
                _type = value;

                // Call the callback and let things know we've changed.
                if (cbTileChanged != null)
                {
                    cbTileChanged(this);
                }
            }
        }
    }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    public Inventory Inventory { get; set; }

    public Room Room { get; set; }

    public List<Character> Characters { get; set; }

    // Furniture is something like a wall, door, or sofa.
    public Furniture Furniture
    {
        get;
        protected set;
    }

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job PendingBuildJob { get; set; }

    public int X { get; protected set; }

    public int Y { get; protected set; }

    // FIXME: This is just hardcoded for now.  Basically just a reminder of something we
    // might want to do more with in the future.
    const float baseTileMovementCost = 1;

    public float MovementCost
    {
        get
        {
            // This prevented the character from walking in empty tiles. It has been diasbled to allow the character to construct floor tiles.
            // TODO: Permanent solution for handeling when a character can walk in empty tiles is required
            //if (Type == TileType.Empty)
            //    return 0;	// 0 is unwalkable

            if(Type == TileType.Empty)
            {
                Tile[] ns = GetNeighbours();

                bool canMove = false;

                // Loop through all the horizontal/vertical neighbours of the empty tile.
                foreach (Tile n in ns)
                {
                    // If the neighbour is a floor tile, set canMove to true.
                    canMove = canMove || (n != null && (n.Type == TileType.Floor || n.Type == TileType.Ladder));
                }

                // If canMove is true, return baseTileMovementCost, else, return 0f.
                return canMove ? baseTileMovementCost : 0f;
            }

            if (Furniture == null)
                return baseTileMovementCost;

            return baseTileMovementCost * Furniture.movementCost;
        }
    }

    // The function we callback any time our tile's data changes
    public event Action<Tile> cbTileChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
        Characters = new List<Character>();
    }

    public bool UnplaceFurniture()
    {
        // Just uninstalling.  FIXME:  What if we have a multi-tile furniture?

        if (Furniture == null)
            return false;

        Furniture f = Furniture;

        for (int x_off = X; x_off < (X + f.Width); x_off++)
        {
            for (int y_off = Y; y_off < (Y + f.Height); y_off++)
            {

                Tile t = World.current.GetTileAt(x_off, y_off);
                t.Furniture = null;
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
                t.Furniture = objInstance;

            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inv)
    {
        if (inv == null)
        {
            Inventory = null;
            return true;
        }

        if (Inventory != null)
        {
            // There's already inventory here. Maybe we can combine a stack?

            if (Inventory.objectType != inv.objectType)
            {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inv.stackSize;
            if (Inventory.stackSize + numToMove > Inventory.maxStackSize)
            {
                numToMove = Inventory.maxStackSize - Inventory.stackSize;
            }

            Inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            return true;
        }

        // At this point, we know that our current inventory is actually
        // null.  Now we can't just do a direct assignment, because
        // the inventory manager needs to know that the old stack is now
        // empty and has to be removed from the previous lists.

        Inventory = inv.Clone();
        Inventory.tile = this;
        inv.stackSize = 0;

        return true;
    }

    // Called when the character has completed the job to change tile type
    public static void ChangeTileTypeJobComplete(Job theJob)
    {
        // FIXME: For now this is hardcoded to build floor
        theJob.tile.Type = theJob.jobTileType;

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJob = null;
    }

    public void EqualiseGas(float leakFactor)
    {
        Room.EqualiseGasByTile(this, leakFactor);
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

    /// <summary>
    /// If one of the 8 neighbouring tiles is of TileType type then this returns true.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasNeighboursOfType(TileType type)
    {
        foreach (Tile tile in GetNeighbours(true))
        {
            if (tile.Type == type)
            {
                return true;
            }
        }
        return false;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("RoomID", Room == null ? "-1" : Room.ID.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        // X and Y have already been read/processed

        Room = World.current.GetRoomFromID(int.Parse(reader.GetAttribute("RoomID")));
        if (Room != null)
        {
            Room.AssignTile(this);
        }

        Type = (TileType)int.Parse(reader.GetAttribute("Type"));


    }

    public ENTERABILITY IsEnterable()
    {
        // This returns true if you can enter this tile right this moment.
        if (MovementCost == 0)
            return ENTERABILITY.Never;

        // Check out furniture to see if it has a special block on enterability
        if (Furniture != null)
        {
            return Furniture.IsEnterable();
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
        return "tile_"+this._type.ToString();
    }

    public string GetDescription()
    {
        return "tile_"+this._type.ToString()+"_desc";
    }

    public string GetHitPointString()
    {
        return "";	// Do tiles have hitpoints? Can flooring be damaged? Obviously "empty" is indestructible.
    }

    #endregion
}
