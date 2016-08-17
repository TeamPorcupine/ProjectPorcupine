using UnityEngine;
using System.Collections.Generic;

public class PowerSystem {


    // List of furniture providing power into the system.
    List<Furniture> powerGenerators;


    public PowerSystem()
    {
        powerGenerators = new List<Furniture>();
    }


    public void RegisterPowerSupply(Furniture powerGen)
    {
        
    }

    public bool RequestPower()
    {


        return false;
    }
    
}
