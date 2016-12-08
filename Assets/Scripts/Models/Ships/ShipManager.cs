#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class ShipManager
{
    // The list of ships currently present in the world.
    private List<Ship> shipsInWorld;

    // Maps a berth furniture to the ship that is currently berthed at it.
    // A non-existent key or value of null means the berth is unoccupied.
    private Dictionary<Furniture, Ship> berthShipMap;

    /// <summary>
    /// Creates a ShipManager with no ships or berths registered.
    /// </summary>
    public ShipManager()
    {
        shipsInWorld = new List<Ship>();
        berthShipMap = new Dictionary<Furniture, Ship>();
    }

    public delegate void ShipEventHandler(Ship ship);

    /// <summary>
    /// Called when a ship is added to the world through this manager.
    /// </summary>
    public event ShipEventHandler ShipCreated;

    /// <summary>
    /// Called when a ship is removed from the world through this manager.
    /// </summary>
    public event ShipEventHandler ShipRemoved;

    /// <summary>
    /// Updates all ships according to the elapsed time.
    /// </summary>
    /// <param name="deltaTime">The time in seconds since the last call of this function.</param>
    public void Update(float deltaTime)
    {
        List<Ship> tempShipsInWorld = new List<Ship>(shipsInWorld);
        foreach (Ship s in tempShipsInWorld)
        {
            s.Update(deltaTime);
        }
    }

    /// <summary>
    /// Adds a ship to the world and puts its position at (x,y).
    /// Returns the created ship instance.
    /// Calls the ShipCreated event.
    /// </summary>
    /// <param name="type">The ship type to spawn.</param>
    /// <param name="x">The x coordinate where the ship should appear.</param>
    /// <param name="y">The y coordinate where the ship should appear.</param>
    public Ship AddShip(string type, float x, float y)
    {
        if (PrototypeManager.Ship.Has(type) == false)
        {
            UnityDebugger.Debugger.LogError("Ships", "Prototype `" + type + "` does not exist");
            return null;
        }

        Ship ship = new Ship(this, PrototypeManager.Ship.Get(type));
        ship.Position = new Vector2(x, y);
        shipsInWorld.Add(ship);

        if (ShipCreated != null)
        {
            ShipCreated(ship);
        }

        return ship;
    }

    /// <summary>
    /// Removes this ship from the world and calls the ShipRemoved event.
    /// </summary>
    public void RemoveShip(Ship ship)
    {
        shipsInWorld.Remove(ship);

        if (ShipRemoved != null)
        {
            ShipRemoved(ship);
        }
    }

    /// <summary>
    /// Returns true if this berth is currently holding a ship. Returns false if the furniture is not a berth.
    /// </summary>
    /// <param name="berth">The berth furniture object.</param>
    public bool IsOccupied(Furniture berth)
    {
        return berthShipMap.ContainsKey(berth) && berthShipMap[berth] != null;
    }

    /// <summary>
    /// Gets the ship currently at this berth. Returns null if berth is unoccupied.
    /// </summary>
    /// <param name="berth">The berth furniture object.</param>
    public Ship GetBerthedShip(Furniture berth)
    {
        return berthShipMap.ContainsKey(berth) ? berthShipMap[berth] : null;
    }

    /// <summary>
    /// Berths the ship at this berth. Cahgnes state of ship. Ship berthing should always be handled
    /// through this function to make sure the states are properly updated on both sides.
    /// </summary>
    /// <param name="berth">The berth furniture object.</param>
    /// <param name="ship">The ship to berth.</param>
    public void BerthShip(Furniture berth, Ship ship)
    {
        ship.UnwrapAtBerth();
        ship.State = ShipState.UNWRAPPED;
        berthShipMap[berth] = ship;

        berth.EventActions.Trigger("OnBerth", berth);
    }

    /// <summary>
    /// Deberths the ship from this berth. Throws an error if no ship was berthed here.
    /// Changes the ship state to WRAPPED.
    /// Deberthing should always be handled through this function to make sure states are properly updated on both ends.
    /// </summary>
    /// <param name="berth">The berth furniture object.</param>
    public void DeberthShip(Furniture berth)
    {
        if (berthShipMap.ContainsKey(berth) == false || berthShipMap[berth] == null)
        {
            UnityDebugger.Debugger.LogError("Ships", "No ship berthed here: " + berth.Tile.ToString());
            return;
        }

        berthShipMap[berth].State = ShipState.WRAPPED;
        berthShipMap[berth].Wrap();
        berthShipMap[berth] = null;

        berth.EventActions.Trigger("OnDeberth", berth);
    }
}