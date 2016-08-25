#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;


public class FurnitureJobPrototypes
{
    
    private Dictionary<string, Job> prototypes;

    public FurnitureJobPrototypes()
    {
        prototypes = new Dictionary<string, Job>();
    }



    public bool HasPrototype(string objectType)
    {
        return prototypes.ContainsKey(objectType);
    }

    public Job GetPrototype(string objectType)
    {
        if (HasPrototype(objectType))
        {
            return prototypes[objectType].Clone();
        }
        return null;
    }

    public void SetPrototype(Job job, Furniture furn)
    {
        prototypes[furn.objectType] = job;
    }
}
