using UnityEngine;
using System.Collections.Generic;

public class PowerSystem {


    // List of furniture providing power into the system.
    List<Furniture> powerGenerators;

    List<Furniture> powerConsumers;

    // Current Power in the system
    float currentPower;

    public PowerSystem()
    {
        powerGenerators = new List<Furniture>();
    }

    public void RegisterPowerSupply(Furniture furn)
    {
        powerGenerators.Add(furn);
        CalculatePower();

        furn.RegisterOnRemovedCallback(RemovePowerSupply);
    }

    public void RemovePowerSupply(Furniture furn)
    {
        powerGenerators.Remove(furn);
        CalculatePower();
    }

    public void RegisterPowerConsumer(Furniture furn)
    {
        powerConsumers.Add(furn);

        furn.RegisterOnRemovedCallback(RemovePowerConsumer);
    }

    public void RemovePowerConsumer(Furniture furn)
    {
        powerGenerators.Remove(furn);
        CalculatePower();
    }


    public bool RequestPower(Furniture furn)
    {


        return false;
    }

    void CalculatePower()
    {
        float powerValues = 0;

        foreach (Furniture furn in powerGenerators)
        {
            powerValues += furn.powerValue;
        }

        currentPower = powerValues;

        Debug.Log("Current Power level: " + currentPower);
    }


}
