#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using UnityEngine;

public class ShipManager
{
    public delegate void ShipEventHandler(Ship ship);
    public event ShipEventHandler ShipCreated, ShipRemoved;

    private List<Ship> shipsInWorld;

    public ShipManager()
    {
        shipsInWorld = new List<Ship>();
    }

    public Ship AddShip(string type, Vector2 position)
    {
        Ship ship = new Ship(PrototypeManager.Ship.GetPrototype(type));
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
}