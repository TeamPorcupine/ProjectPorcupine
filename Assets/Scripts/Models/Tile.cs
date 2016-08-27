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
    public Tile(int x, int y)
    {
        X = x;
        Y = y;
        Characters = new List<Character>();
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

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job PendingBuildJob { get; set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public float MovementCost
    {
        get
        {
            // This prevented the character from walking in empty tiles. It has been diasbled to allow the character to construct floor tiles.
            // TODO: Permanent solution for handeling when a character can walk in empty tiles is required
            if (Type.MovementCostLua == null)
            {
                return Type.BaseMovementCost * (Furniture != null ? Furniture.MovementCost : 1);
            }

            return (float)LuaUtilities.CallFunction(Type.MovementCostLua, this).Number;
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
        // Just uninstalling.  FIXME:  What if we have a multi-tile furniture?
        if (Furniture == null)
        {
            return false;
        }

        Furniture furniture = Furniture;
        for (int x_off = X; x_off < X + furniture.Width; x_off++)
        {
            for (int y_off = Y; y_off < Y + furniture.Height; y_off++)
            {
                Tile tile = World.Current.GetTileAt(x_off, y_off);
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
            Debug.LogError("Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }

        for (int x_off = X; x_off < X + objInstance.Width; x_off++)
        {
            for (int y_off = Y; y_off < Y + objInstance.Height; y_off++)
            {
                Tile t = World.Current.GetTileAt(x_off, y_off);
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
            if (Inventory.objectType != inventory.objectType)
            {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inventory.StackSize;
            if (Inventory.StackSize + numToMove > Inventory.maxStackSize)
            {
                numToMove = Inventory.maxStackSize - Inventory.StackSize;
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
        Inventory.tile = this;
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
        Tile[] ns = diagOkay == false ? new Tile[4] : new Tile[8];

        Tile tile = World.Current.GetTileAt(X, Y + 1);
        ns[0] = tile; // Could be null, but that's okay.
        tile = World.Current.GetTileAt(X + 1, Y);
        ns[1] = tile; // Could be null, but that's okay.
        tile = World.Current.GetTileAt(X, Y - 1);
        ns[2] = tile; // Could be null, but that's okay.
        tile = World.Current.GetTileAt(X - 1, Y);
        ns[3] = tile; // Could be null, but that's okay.

        if (diagOkay == true)
        {
            tile = World.Current.GetTileAt(X + 1, Y + 1);
            ns[4] = tile; // Could be null, but that's okay.
            tile = World.Current.GetTileAt(X + 1, Y - 1);
            ns[5] = tile; // Could be null, but that's okay.
            tile = World.Current.GetTileAt(X - 1, Y - 1);
            ns[6] = tile; // Could be null, but that's okay.
            tile = World.Current.GetTileAt(X - 1, Y + 1);
            ns[7] = tile; // Could be null, but that's okay.
        }

        return ns;
    }

    /// <summary>
    /// If one of the 8 neighbouring tiles is of TileType type then this returns true.
    /// </summary>
    public bool HasNeighboursOfType(TileType tileType)
    {
        return GetNeighbours(true).Any(tile => tile.Type == tileType);
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
        return World.Current.GetTileAt(X, Y + 1);
    }

    public Tile South()
    {
        return World.Current.GetTileAt(X, Y - 1);
    }

    public Tile East()
    {
        return World.Current.GetTileAt(X + 1, Y);
    }

    public Tile West()
    {
        return World.Current.GetTileAt(X - 1, Y);
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

    public string GetHitPointString()
    {
        return string.Empty; // Do tiles have hitpoints? Can flooring be damaged? Obviously "empty" is indestructible.
    }

    public string GetJobDescription()
    {
        return string.Empty;
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
                    Text = "Prioritize Job",
                    RequireCharacterSelected = true,
                    Action = (cm, c) => { c.PrioritizeJob(PendingBuildJob); }
                };
            }
        }
    }
}
