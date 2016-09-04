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
    private List<Ship> shipsInWorld;
    private Dictionary<Furniture, Ship> berthShipMap;

    public ShipManager()
    {
        shipsInWorld = new List<Ship>();
        berthShipMap = new Dictionary<Furniture, Ship>();
    }

    public delegate void ShipEventHandler(Ship ship);

    public event ShipEventHandler ShipCreated, ShipRemoved;

    public void Update(float deltaTime)
    {
        foreach (Ship s in shipsInWorld)
        {
            s.Update(deltaTime);
        }
    }

    public Ship AddShip(string type, float x, float y)
    {
        Ship ship = new Ship(this, PrototypeManager.Ship.Get(type));
        ship.Position = new Vector2(x, y);

        shipsInWorld.Add(ship);

        if (ShipCreated != null)
        {
            ShipCreated(ship);
        }

        return ship;
    }

    public void RemoveShip(Ship ship)
    {
        shipsInWorld.Remove(ship);

        if (ShipRemoved != null)
        {
            ShipRemoved(ship);
        }
    }

    public bool IsOccupied(Furniture berth)
    {
        return berthShipMap.ContainsKey(berth) && berthShipMap[berth] != null;
    }

    public Ship GetBerthedShip(Furniture berth)
    {
        return berthShipMap.ContainsKey(berth) ? berthShipMap[berth] : null;
    }

    public void BerthShip(Furniture berth, Ship ship)
    {
        ship.UnwrapAtBerth();
        ship.State = ShipState.BERTHED;
        berthShipMap[berth] = ship;
        //// berth.EventActions.Trigger("OnBerth", berth);
    }

    public void DeberthShip(Furniture berth)
    {
        if (berthShipMap.ContainsKey(berth) == false || berthShipMap[berth] == null)
        {
            Debug.ULogErrorChannel("Ships", "No ship berthed here: " + berth.Tile.ToString());
            return;
        }

        berthShipMap[berth].State = ShipState.TRANSIT;
        berthShipMap[berth].Wrap();
        berthShipMap[berth] = null;
        //// berth.EventActions.Trigger("OnDeberth", berth);
    }
}