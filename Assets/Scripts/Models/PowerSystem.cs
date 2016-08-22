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
    private HashSet<Furniture> powerGrid;

    private float currentPower;

    public PowerSystem()
    {
        powerGrid = new HashSet<Furniture>();
    }

    public event Action<Furniture> PowerLevelChanged;

    public float PowerLevel
    {
        get
        {
            return currentPower;
        }

        private set
        {
            if (!currentPower.Equals(value))
            {
                currentPower = value;
                NotifyPowerConsumers();
            }
        }
    }

    public bool AddToPowerGrid(Furniture furniture)
    {
        if (PowerLevel + furniture.powerValue < 0)
        {
            return false;
        }

        powerGrid.Add(furniture);
        AdjustPowelLevel(furniture);
        furniture.cbOnRemoved += RemoveFromPowerGrid;
        return true;
    }

    public void RemoveFromPowerGrid(Furniture furniture)
    {
        powerGrid.Remove(furniture);
        AdjustPowelLevel(furniture, false);
    }

    public bool RequestPower(Furniture furniture)
    {
        return powerGrid.Contains(furniture);
    }

    private void AdjustPowelLevel(Furniture furniture, bool isFurnitureAdded = true)
    {
        if (isFurnitureAdded)
        {
            PowerLevel += furniture.powerValue;
        }
        else
        {
            PowerLevel -= furniture.powerValue;
        }
        if (PowerLevel < 0)
        {
            RemovePowerConsumer();
        }
    }

    private void RemovePowerConsumer()
    {
        Furniture powerConsumer = powerGrid.FirstOrDefault(furniture => furniture.IsPowerConsumer);
        if (powerConsumer == null) { return; }
        RemoveFromPowerGrid(powerConsumer);
    }

    private void NotifyPowerConsumers()
    {
        foreach (Furniture furniture in powerGrid.Where(furniture => furniture.IsPowerConsumer))
        {
            InvokePowerLevelChanged(furniture);
        }
    }

    private void InvokePowerLevelChanged(Furniture furniture)
    {
        Action<Furniture> handler = PowerLevelChanged;
        if (handler != null)
        {
            handler(furniture);
        }
    }
}
