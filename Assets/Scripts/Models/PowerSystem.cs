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
        if (currentPower + furn.powerValue < 0)
        {
<<<<<<< HEAD
            return;
        }

=======
            //Logger.LogWarning("Not enough power for " + furn.Name + " to run");
            return;
        }

        //Logger.Log("Added " + furn.Name + " to power consumer list");
>>>>>>> refs/remotes/TeamPorcupine/master
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

        foreach (Furniture furn in powerGenerators)
        {
            powerValues += furn.powerValue;
        }

        foreach (Furniture furn in powerConsumers)
        {
            powerValues += furn.powerValue;
        }

        currentPower = powerValues;

        Logger.Log("Current Power level: " + currentPower);
    }


}
