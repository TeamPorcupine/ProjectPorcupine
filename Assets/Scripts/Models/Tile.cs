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
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

public enum Enterability
{
    Yes,
    Never,
    Soon
}

[MoonSharpUserData]
public class Tile : IXmlSerializable, ISelectable, IContextActionProvider
{
    private TileType type = TileType.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
        Characters = new List<Character>();
        MovementModifier = 1;
    }

    // The function we callback any time our tile's data changes
    public event Action<Tile> TileChanged;

    public TileType Type
    {
        get
        {
            return type;
        }

        set
        {
            if (type == value)
            {
                return;
            }

            type = value;

            // Call the callback and let things know we've changed.
            if (TileChanged != null)
            {
                TileChanged(this);
            }
        }
    }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    public Inventory Inventory { get; set; }

    public Room Room { get; set; }

    public List<Character> Characters { get; set; }

    // Furniture is something like a wall, door, or sofa.
    public Furniture Furniture { get; private set; }

    /// <summary>
    /// The total pathfinding cost of entering this tile.
    /// The final cost is equal to the Tile's BaseMovementCost * Tile's PathfindingWeight * Furniture's PathfindingWeight * Furniture's MovementCost +
    /// Tile's PathfindingModifier + Furniture's PathfindingModifier.
    /// </summary>
    public float PathfindingCost
    {
        get
        {
            // If Tile's BaseMovementCost or Furniture's MovementCost = 0 (i.e. impassable) we should always return 0 (stay impassable)
            if (Type.BaseMovementCost == 0 || (Furniture != null && Furniture.MovementCost == 0))
            {
                return 0f;
            }

            if (Furniture != null)
            {
                return (Furniture.PathfindingWeight * Furniture.MovementCost * Type.PathfindingWeight * Type.BaseMovementCost) +
                Furniture.PathfindingModifier + Type.PathfindingModifier;
            }
            else
            {
                return (Type.PathfindingWeight * Type.BaseMovementCost) + Type.PathfindingModifier;
            }
        }
    }

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job PendingBuildJob { get; set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Z { get; private set; }

    public float MovementModifier { get; set; }

    public float MovementCost
    {
        get
        {
            // This prevented the character from walking in empty tiles. It has been diasbled to allow the character to construct floor tiles.
            // TODO: Permanent solution for handeling when a character can walk in empty tiles is required
            return Type.BaseMovementCost * MovementModifier * (Furniture != null ? Furniture.MovementCost : 1);
        }
    }

    public bool IsSelected { get; set; }

    // Called when the character has completed the job to change tile type
    public static void ChangeTileTypeJobComplete(Job theJob)
    {
        // FIXME: For now this is hardcoded to build floor
        theJob.tile.Type = theJob.JobTileType;

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJob = null;
    }

    public bool UnplaceFurniture()
    {
        // Just uninstalling.
        if (Furniture == null)
        {
            return false;
        }

        Furniture furniture = Furniture;
        for (int x_off = X; x_off < X + furniture.Width; x_off++)
        {
            for (int y_off = Y; y_off < Y + furniture.Height; y_off++)
            {
                Tile tile = World.Current.GetTileAt(x_off, y_off, Z);
                tile.Furniture = null;
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
            Debug.ULogErrorChannel("Tile", "Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }

        for (int x_off = X; x_off < X + objInstance.Width; x_off++)
        {
            for (int y_off = Y; y_off < Y + objInstance.Height; y_off++)
            {
                Tile t = World.Current.GetTileAt(x_off, y_off, Z);
                t.Furniture = objInstance;
            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inventory)
    {
        if (inventory == null)
        {
            Inventory = null;
            return true;
        }

        if (Inventory != null)
        {
            // There's already inventory here. Maybe we can combine a stack?
            if (Inventory.Type != inventory.Type)
            {
                Debug.ULogErrorChannel("Tile", "Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inventory.StackSize;
            if (Inventory.StackSize + numToMove > Inventory.MaxStackSize)
            {
                numToMove = Inventory.MaxStackSize - Inventory.StackSize;
            }

            Inventory.StackSize += numToMove;
            inventory.StackSize -= numToMove;

            return true;
        }

        // At this point, we know that our current inventory is actually
        // null.  Now we can't just do a direct assignment, because
        // the inventory manager needs to know that the old stack is now
        // empty and has to be removed from the previous lists.
        Inventory = inventory.Clone();
        Inventory.Tile = this;
        inventory.StackSize = 0;

        return true;
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
        // Note: We do not care about vertical adjacency, only horizontal (X and Y)
        return
            Math.Abs(X - tile.X) + Math.Abs(Y - tile.Y) == 1 || // Check hori/vert adjacency
        (diagOkay && Math.Abs(X - tile.X) == 1 && Math.Abs(Y - tile.Y) == 1); // Check diag adjacency
    }

    /// <summary>
    /// Gets the neighbours.
    /// </summary>
    /// <returns>The neighbours.</returns>
    /// <param name="diagOkay">Is diagonal movement okay?.</param>
    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] tiles = diagOkay == false ? new Tile[4] : new Tile[8];
        tiles[0] = World.Current.GetTileAt(X, Y + 1, Z);
        tiles[1] = World.Current.GetTileAt(X + 1, Y, Z);
        tiles[2] = World.Current.GetTileAt(X, Y - 1, Z);
        tiles[3] = World.Current.GetTileAt(X - 1, Y, Z);

        if (diagOkay == true)
        {
            tiles[4] = World.Current.GetTileAt(X + 1, Y + 1, Z);
            tiles[5] = World.Current.GetTileAt(X + 1, Y - 1, Z);
            tiles[6] = World.Current.GetTileAt(X - 1, Y - 1, Z);
            tiles[7] = World.Current.GetTileAt(X - 1, Y + 1, Z);
        }

        return tiles.Where(tile => tile != null).ToArray();
    }

    /// <summary>
    /// If one of the 8 neighbouring tiles is of TileType type then this returns true.
    /// </summary>
    public bool HasNeighboursOfType(TileType tileType)
    {
        return GetNeighbours(true).Any(tile => (tile != null && tile.Type == tileType));
    }

    /// <summary>
    /// Returns true if any of the neighours is walkable.
    /// </summary>
    /// <param name="checkDiagonals">Will test diagonals as well if true.</param>
    public bool HasWalkableNeighbours(bool checkDiagonals = false)
    {
        return GetNeighbours(checkDiagonals).Any(tile => tile != null && tile.MovementCost > 0);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Z", Z.ToString());
        writer.WriteAttributeString("RoomID", Room == null ? "-1" : Room.ID.ToString());
        writer.WriteAttributeString("Type", Type.Type);
    }

    public void ReadXml(XmlReader reader)
    {
        // X and Y have already been read/processed
        Room = World.Current.GetRoomFromID(int.Parse(reader.GetAttribute("RoomID")));
        if (Room != null)
        {
            Room.AssignTile(this);
        }

        Type = TileType.GetTileType(reader.GetAttribute("Type"));
    }

    public Enterability IsEnterable()
    {
        // This returns true if you can enter this tile right this moment.
        if (MovementCost.IsZero())
        {
            return Enterability.Never;
        }

        // Check out furniture to see if it has a special block on enterability
        if (Furniture != null)
        {
            return Furniture.IsEnterable();
        }

        return Enterability.Yes;
    }

    public Tile North()
    {
        return World.Current.GetTileAt(X, Y + 1, Z);
    }

    public Tile South()
    {
        return World.Current.GetTileAt(X, Y - 1, Z);
    }

    public Tile East()
    {
        return World.Current.GetTileAt(X + 1, Y, Z);
    }

    public Tile West()
    {
        return World.Current.GetTileAt(X - 1, Y, Z);
    }

    public float GetGasPressure(string gas)
    {
        if (Room == null)
        {
            float pressure = Mathf.Infinity;
            if (North().Room != null && North().GetGasPressure(gas) < pressure)
            {
                pressure = North().GetGasPressure(gas);
            }

            if (East().Room != null && East().GetGasPressure(gas) < pressure)
            {
                pressure = East().GetGasPressure(gas);
            }

            if (South().Room != null && South().GetGasPressure(gas) < pressure)
            {
                pressure = South().GetGasPressure(gas);
            }

            if (West().Room != null && West().GetGasPressure(gas) < pressure)
            {
                pressure = West().GetGasPressure(gas);
            }

            if (pressure == Mathf.Infinity)
            {
                return 0f;
            }

            return pressure;
        }

        return Room.GetGasPressure(gas);
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return "tile_" + type.ToString();
    }

    public string GetDescription()
    {
        return "tile_" + type.ToString() + "_desc";
    }

    public string GetJobDescription()
    {
        return string.Empty;
    }

    public IEnumerable<string> GetAdditionalInfo()
    {
        // Do tiles have hitpoints? Can flooring be damaged? Obviously "empty" is indestructible.
        yield break;
    }

    #endregion

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        if (PendingBuildJob != null)
        {
            yield return new ContextMenuAction
            {
                Text = "Cancel Job",
                RequireCharacterSelected = false,
                Action = (cm, c) =>
                {
                    if (PendingBuildJob != null)
                    {
                        PendingBuildJob.CancelJob();
                    }
                }
            };

            if (!PendingBuildJob.IsBeingWorked)
            {
                yield return new ContextMenuAction
                {
                    Text = "Prioritize " + PendingBuildJob.GetName(),
                    RequireCharacterSelected = true,
                    Action = (cm, c) =>
                    {
                        c.PrioritizeJob(PendingBuildJob);
                    }
                };
            }
        }
    }
}
