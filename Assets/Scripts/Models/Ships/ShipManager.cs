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
    private Dictionary<string, Ship> shipPrototypes;
    private List<Ship> shipsInWorld;

    public ShipManager()
    {
        shipPrototypes = new Dictionary<string, Ship>();
        shipsInWorld = new List<Ship>();
    }

    public Ship AddShip(string type, Vector2 position)
    {
        Ship ship = new Ship(shipPrototypes[type]);
        shipsInWorld.Add(ship);
        return ship;
    }
}