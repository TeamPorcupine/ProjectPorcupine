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
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using ProjectPorcupine.Localization;
using ProjectPorcupine.Pathfinding;
using ProjectPorcupine.Rooms;
using UnityEngine;

public enum Enterability
{
    Yes,
    Never,
    Soon
}

[MoonSharpUserData]
[System.Diagnostics.DebuggerDisplay("Tile {X},{Y},{Z}")]
public class Tile : ISelectable, IContextActionProvider, IComparable, IEquatable<Tile>
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
        Utilities = new Dictionary<string, Utility>();
        ReservedAsWorkSpotBy = new HashSet<Furniture>();
        PendingBuildJobs = new HashSet<Job>();
    }

    // The function we callback any time our tile's data changes
    public event Action<Tile> TileChanged;

    // The function we callback any time our tile's type changes
    public event Action<Tile> TileTypeChanged;

    #region Accessors
    public TileType Type
    {
        get
        {
            return type;
        }
    }

    public HashSet<Furniture> ReservedAsWorkSpotBy { get; private set; }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    public Inventory Inventory { get; set; }

    public Room Room { get; set; }

    public List<Character> Characters { get; set; }

    // Furniture is something like a wall, door, or sofa.
    public Furniture Furniture { get; private set; }

    // The number of times this tile has been walked on since last cleaned.
    public int WalkCount { get; protected set; }

    // Utility is something like a Power Cables or Water Pipes.
    public Dictionary<string, Utility> Utilities { get; private set; }

    /// <summary>
    /// The total pathfinding cost of entering this tile.
    /// The final cost is equal to the Tile's BaseMovementCost * Tile's PathfindingWeight * Furniture's PathfindingWeight * Furniture's MovementCost +
    /// Tile's PathfindingModifier + Furniture's PathfindingModifier.
    /// </summary>
    public float PathfindingCost
    {
        get
        {
            // If Tile's BaseMovementCost, PathFindingWeight or Furniture's MovementCost, PathFindingWeight = 0 (i.e. impassable) we should always return 0 (stay impassable)
            if (Type.BaseMovementCost.AreEqual(0) || Type.PathfindingWeight.AreEqual(0) || (Furniture != null && (Furniture.MovementCost.AreEqual(0) || Furniture.PathfindingWeight.AreEqual(0))))
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
    public HashSet<Job> PendingBuildJobs { get; set; }

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

    public bool ForceTileUpdate { get; protected set; }

    public Vector3 Vector3
    {
        get
        {
            return new Vector3(X, Y, Z);
        }
    }
    #endregion

    #region Manage Objects
    // Called when the character has completed the job to change tile type
    public static void ChangeTileTypeJobComplete(Job theJob)
    {
        theJob.tile.SetTileType(theJob.JobTileType);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJobs.Remove(theJob);
    }

    public void SetTileType(TileType newTileType, bool doRoomFloodFill = true)
    {
        if (type == newTileType)
        {
            return;
        }

        type = newTileType;
        ForceTileUpdate = true;

        bool splitting = true;
        if (newTileType == TileType.Empty)
        {
            splitting = false;
        }

        if (doRoomFloodFill)
        {
            World.Current.RoomManager.DoRoomFloodFill(this, splitting, true);
        }

        OnTileClean();

        if (TileTypeChanged != null)
        {
            TileTypeChanged(this);
        }
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
            UnityDebugger.Debugger.LogError("Tile", "Trying to assign a furniture to a tile that isn't valid!");
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

    public bool UnplaceUtility(Utility utility)
    {
        // Just uninstalling.
        if (Utilities == null)
        {
            UnityDebugger.Debugger.LogError("Tile", "Utilities null when trying to unplace a Utility, this should never happen!");
            return false;
        }

        Utilities.Remove(utility.Type);

        return true;
    }

    public bool PlaceUtility(Utility objInstance)
    {
        if (objInstance == null)
        {
            return false;
        }

        if (objInstance.IsValidPosition(this) == false)
        {
            UnityDebugger.Debugger.LogError("Tile", "Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }

        Utilities.Add(objInstance.Type, objInstance);

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
                UnityDebugger.Debugger.LogError("Tile", "Trying to assign inventory to a tile that already has some of a different type.");
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

    #endregion

    #region Manage Gas
    public void EqualiseGas(float leakFactor)
    {
        World.Current.RoomManager.EqualiseGasByTile(this, leakFactor);
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
    #endregion

    #region Neighbors and pathfinding
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
    /// <param name="vertOkay">Is vertical movement okay?.</param>
    /// <param name="nullOkay">Is returning null tiles okay?.</param>
    public Tile[] GetNeighbours(bool diagOkay = false, bool vertOkay = false, bool nullOkay = false)
    {
        Tile[] tiles = new Tile[10];
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

        // FIXME: This is a bit of a dirty hack, but it works for preventing characters from phasing through the floor for now.
        if (vertOkay)
        {
            Tile[] vertTiles = GetVerticalNeighbors(true);
            tiles[8] = vertTiles[0];
            tiles[9] = vertTiles[1];
        }

        if (!nullOkay)
        {
            return tiles.Where(tile => tile != null).ToArray();
        }
        else
        {
            return tiles;
        }
    }

    public Tile[] GetVerticalNeighbors(bool nullOkay = false)
    {
        Tile[] tiles = new Tile[2];
        Tile tileup = World.Current.GetTileAt(X, Y, Z - 1);
        if (tileup != null && tileup.Type == TileType.Empty)
        {
            tiles[0] = World.Current.GetTileAt(X, Y, Z - 1);
        }

        if (Type == TileType.Empty)
        {
            tiles[1] = World.Current.GetTileAt(X, Y, Z + 1);
        }

        if (!nullOkay)
        {
            return tiles.Where(tile => tile != null).ToArray();
        }
        else
        {
            return tiles;
        }
    }

    /// <summary>
    /// If one of the 8 neighbouring tiles is of TileType type then this returns true.
    /// </summary>
    public bool HasNeighboursOfType(TileType tileType)
    {
        return GetNeighbours(true, true).Any(tile => (tile != null && tile.Type == tileType));
    }

    /// <summary>
    /// Returns true if any of the neighours can reach this tile. Checks for clipping of diagonal paths.
    /// </summary>
    /// <param name="checkDiagonals">Will test diagonals as well if true.</param>
    public bool IsReachableFromAnyNeighbor(bool checkDiagonals = false)
    {
        bool reachableFromSameLevel = GetNeighbours(checkDiagonals).Any(tile => tile != null && tile.MovementCost > 0 && (checkDiagonals == false || IsClippingCorner(tile) == false));
        bool reachableVertically = GetVerticalNeighbors().Length > 0;
        return reachableFromSameLevel || reachableVertically;
    }

    public bool IsClippingCorner(Tile neighborTile)
    {
        // If the movement from curr to neigh is diagonal (e.g. N-E)
        // Then check to make sure we aren't clipping (e.g. N and E are both walkable)
        int dX = this.X - neighborTile.X;
        int dY = this.Y - neighborTile.Y;

        if (Mathf.Abs(dX) + Mathf.Abs(dY) == 2)
        {
            // We are diagonal
            if (World.Current.GetTileAt(X - dX, Y, Z).PathfindingCost.AreEqual(0f))
            {
                // East or West is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            if (World.Current.GetTileAt(X, Y - dY, Z).PathfindingCost.AreEqual(0f))
            {
                // North or South is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            // If we reach here, we are diagonal, but not clipping
        }

        // If we are here, we are either not clipping, or not diagonal
        return false;
    }

    public bool HasClearLineToBottom()
    {
        if (type != TileType.Empty)
        {
            return false;
        }

        if (Down() == null)
        {
            return true;
        }
        else
        {
            return Down().HasClearLineToBottom();
        }
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

    public Tile Up()
    {
        return World.Current.GetTileAt(X, Y, Z - 1);
    }

    public Tile Down()
    {
        return World.Current.GetTileAt(X, Y, Z + 1);
    }

    /// <summary>
    /// Gets the nearest room.
    /// </summary>
    /// <returns>The nearest room. If this tile has a room, it will return this tile's room.</returns>
    public Room GetNearestRoom()
    {
        if (Room != null)
        {
            return this.Room;
        }

        // Using GetNeighbors we will prefer tiles linearally adjacent, then diagonal, and only then vertical
        foreach (Tile neighbor in GetNeighbours(true, true))
        {
            if (neighbor.Room != null)
            {
                return neighbor.Room;
            }
        }

        return Pathfinder.FindNearestRoom(this);
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

    public void OnEnter()
    {
        WalkCount++;
        ReportTileChanged();
    }

    public void OnTileClean()
    {
        WalkCount = 0;
        ReportTileChanged();
    }
    #endregion

    public object ToJson()
    {
        return new JObject(
            new JProperty("X", X),
            new JProperty("Y", Y),
            new JProperty("Z", Z),
            new JProperty("TimesWalked", WalkCount),
            new JProperty("RoomID", Room == null ? -1 : Room.ID),
            new JProperty("Type", Type.Type));
    }

    public void FromJson(JToken tileToken)
    {
        Room = World.Current.RoomManager[(int)tileToken["RoomID"]];
        if (Room != null)
        {
            Room.AssignTile(this);
        }

        // Since we are loading from a save here, we don't want to do a RoomFloodfill here.
        SetTileType(PrototypeManager.TileType.Get((string)tileToken["Type"]), false);
        WalkCount = (int)tileToken["TimesWalked"];
        ForceTileUpdate = true;
    }

    #region ISelectableInterface implementation

    public string GetName()
    {
        return type.LocalizationCode;
    }

    public string GetDescription()
    {
        return type.UnlocalizedDescription;
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
        if (PendingBuildJobs != null)
        {
            foreach (Job pendingJob in PendingBuildJobs)
            {
                yield return new ContextMenuAction
                {
                    LocalizationKey = LocalizationTable.GetLocalization("cancel_job", pendingJob.GetName()),
                    RequireCharacterSelected = false,
                    Action = (cm, c) =>
                    {
                        pendingJob.CancelJob();
                    }
                };
                if (!pendingJob.IsBeingWorked)
                {
                    yield return new ContextMenuAction
                    {
                        LocalizationKey = LocalizationTable.GetLocalization("prioritize", pendingJob.GetName()),
                        RequireCharacterSelected = true,
                        Action = (cm, c) =>
                        {
                            c.PrioritizeJob(pendingJob);
                        }
                    };
                }
            }
        }
    }

    public bool IsReservedWorkSpot()
    {
        return ReservedAsWorkSpotBy.Count > 0;
    }

    #region IComparable

    public int CompareTo(object other)
    {
        if (other == null)
        {
            return 1;
        }

        Tile otherTile = other as Tile;
        if (otherTile != null)
        {
            int result = this.Z.CompareTo(otherTile.Z);
            if (result != 0)
            {
                return result;
            }

            result = this.Y.CompareTo(otherTile.Y);
            if (result != 0)
            {
                return result;
            }

            return this.X.CompareTo(otherTile.X);
        }
        else
        {
            throw new ArgumentException("Object is not a Tile");
        }
    }

    #endregion

    #region IEquatable<T>

    public bool Equals(Tile otherTile)
    {
        if (otherTile == null)
        {
            return false;
        }

        return X == otherTile.X && Y == otherTile.Y && Z == otherTile.Z;
    }
    #endregion

    public void MoveTile(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return string.Format("[{0} {1}, {2}, {3}]", Type, X, Y, Z);
    }

    private void ReportTileChanged()
    {
        // Call the callback and let things know we've changed.
        if (TileChanged != null)
        {
            TileChanged(this);
        }

        ForceTileUpdate = false;
    }
}
