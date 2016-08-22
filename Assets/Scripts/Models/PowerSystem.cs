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

public class PowerSystem
{
    private readonly HashSet<IPowerRelated> powerGrid;

    private float currentPower;

    public PowerSystem()
    {
        powerGrid = new HashSet<IPowerRelated>();
    }

    public event Action<IPowerRelated> PowerLevelChanged;

    public float PowerLevel
    {
        get
        {
            return currentPower;
        }

        private set
        {
            if (currentPower.Equals(value)) return;
            currentPower = value;
            NotifyPowerConsumers();
        }
    }

    public bool AddToPowerGrid(IPowerRelated powerRelated)
    {
        if (PowerLevel + powerRelated.PowerValue < 0)
        {
            return false;
        }

        powerGrid.Add(powerRelated);
        AdjustPowelLevel();
        powerRelated.PowerValueChanged += OnPowerValueChanged;
        Furniture furniture = powerRelated as Furniture;
        if (furniture != null)
        {            
            furniture.cbOnRemoved += RemoveFromPowerGrid;
        }

        return true;
    }

    public void RemoveFromPowerGrid(IPowerRelated powerRelated)
    {
        powerGrid.Remove(powerRelated);
        AdjustPowelLevel();
    }

    public bool RequestPower(IPowerRelated powerRelated)
    {
        return powerGrid.Contains(powerRelated);
    }

    private void AdjustPowelLevel()
    {
        PowerLevel = powerGrid.Sum(related => related.PowerValue);
        if (PowerLevel < 0.0f)
        {
            RemovePowerConsumer();
        }
    }

    private void RemovePowerConsumer()
    {
        IPowerRelated powerConsumer = powerGrid.FirstOrDefault(powerRelated => powerRelated.IsPowerConsumer);
        if (powerConsumer == null)
        {
            return;
        }

        RemoveFromPowerGrid(powerConsumer);
    }

    private void NotifyPowerConsumers()
    {
        foreach (IPowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerConsumer))
        {
            InvokePowerLevelChanged(powerRelated);
        }
    }

    private void OnPowerValueChanged(IPowerRelated powerRelated)
    {
        RemoveFromPowerGrid(powerRelated);
        AddToPowerGrid(powerRelated);
    }

    private void InvokePowerLevelChanged(IPowerRelated powerRelated)
    {
        Action<IPowerRelated> handler = PowerLevelChanged;
        if (handler != null)
        {
            handler(powerRelated);
        }
    }
}
