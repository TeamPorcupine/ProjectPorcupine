using UnityEngine;
using System.Collections.Generic;

public class PowerSystem {
    // List of furniture providing power into the system.
    private List<Furniture> _powerGenerators;

    private List<Furniture> _powerConsumers;

    // Current Power in the system
    float currentPower;

    public PowerSystem()
    {
        _powerGenerators = new List<Furniture>();
        _powerConsumers = new List<Furniture>();
    }

    public void RegisterPowerSupply(Furniture furn)
    {
        _powerGenerators.Add(furn);
        CalculatePower();

        furn.OnRemoved += RemovePowerSupply;
    }

    public void RemovePowerSupply(Furniture furn)
    {
        _powerGenerators.Remove(furn);
        CalculatePower();
    }

    public void RegisterPowerConsumer(Furniture furn)
    {
        if (currentPower < furn.powerValue)
        {
            //Debug.LogWarning("Not enough power for " + furn.Name + " to run");
            return;
        }

        //Debug.Log("Added " + furn.Name + " to power consumer list");
        _powerConsumers.Add(furn);
        CalculatePower();

        furn.OnRemoved += RemovePowerConsumer;
    }

    public void RemovePowerConsumer(Furniture furn)
    {
        _powerGenerators.Remove(furn);
        CalculatePower();
    }


    public bool RequestPower(Furniture furn)
    {
        if (_powerConsumers.Contains(furn))
        {
            return true;
        }
        
        return false;
    }

    private void CalculatePower()
    {
        float powerValues = 0;

        foreach (Furniture furn in _powerGenerators)
        {
            powerValues += furn.powerValue;
        }

        foreach (Furniture furn in _powerConsumers)
        {
            powerValues -= furn.powerValue;
        }

        currentPower = powerValues;

        Debug.Log("Current Power level: " + currentPower);
    }
}