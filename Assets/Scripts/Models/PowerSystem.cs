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

public class PowerSystem {


    // List of furniture providing power into the system.
    List<Furniture> powerGenerators;

    List<Furniture> powerConsumers;

    // Current Power in the system
    float currentPower;

    public event Action<Furniture> cbOnChanged;

    public PowerSystem()
    {
        powerGenerators = new List<Furniture>();
        powerConsumers = new List<Furniture>();
    }

    public void RegisterPowerSupply(Furniture furn)
    {
        powerGenerators.Add(furn);
        CalculatePower();

        furn.cbOnRemoved += RemovePowerSupply;
    }

    public void RemovePowerSupply(Furniture furn)
    {
        powerGenerators.Remove(furn);
        CalculatePower();
    }

    public void RegisterPowerConsumer(Furniture furn)
    {
        if (PowerLevel + furn.powerValue < 0)
        {
            return;
        }

        powerConsumers.Add(furn);
        CalculatePower();

        furn.cbOnRemoved += RemovePowerConsumer;
    }

    public void RemovePowerConsumer(Furniture furn)
    {
        powerGenerators.Remove(furn);
        CalculatePower();
    }


    public bool RequestPower(Furniture furn)
    {
        if (powerConsumers.Contains(furn))
        {
            return true;
        }
        
        return false;
    }

    void CalculatePower()
    {
        float powerValues = 0;

        foreach (Furniture furn in powerConsumers) 
        {
            powerValues += furn.powerValue;            
        }

        foreach (Furniture furn in powerGenerators)
        {
            powerValues += furn.powerValue;            
        }

        PowerLevel = powerValues;

        Logger.Log("Current Power level: " + PowerLevel);
    }

    public float PowerLevel {
        get { return currentPower; }
        set 
        {
            float oldPower = currentPower;
            currentPower = value;

            if(oldPower != currentPower) 
            {
                foreach (Furniture furn in powerConsumers)
                {
                    cbOnChanged(furn);
                }                
            }
        }
    }
}
