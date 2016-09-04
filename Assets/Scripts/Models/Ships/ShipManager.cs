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

<<<<<<< e30f26f35030da7da4de9f989234db5f678a27e3
    public Ship AddShip(string type, float x, float y)
    {
        Ship ship = new Ship(this, PrototypeManager.Ship.Get(type));
        ship.Position = new Vector2(x, y);
=======
    public delegate void ShipEventHandler(Ship ship);

    public event ShipEventHandler ShipCreated, ShipRemoved;

    public Ship AddShip(string type, Vector2 position)
    {
        Ship ship = new Ship(PrototypeManager.Ship.GetPrototype(type));
        ship.Position = position;

>>>>>>> Style cop
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