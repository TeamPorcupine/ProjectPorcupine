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
using UnityEngine;

/// <summary>
/// Saves trader data for trader persistence (traders return time and time again to rip you off).
/// </summary>
public class TradersController
{
    // TODO save trader data
    public Dictionary<string, Trader> traders;
        
    public void AddTrader(Trader t)
    {
        if (traders == null)
        {
            traders = new Dictionary<string, Trader>();
        }
            
        traders.Add(t.Name, t);
    }
        
    // Gets a random trader from the list
    public Trader RequestRandomTrader()
    {
        Trader joe = traders.Values.ToList()[UnityEngine.Random.Range(0, traders.Count)];
        joe.RefreshInventory();
        return joe;
    }  
}
