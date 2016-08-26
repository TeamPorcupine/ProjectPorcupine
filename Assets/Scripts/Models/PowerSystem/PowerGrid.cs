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

public class PowerGrid
{
    private readonly HashSet<PowerRelated> powerGrid;

    public PowerGrid()
    {
        powerGrid = new HashSet<PowerRelated>();
    }

    public bool IsOperating { get; private set; }

    public bool IsEmpty
    {
        get
        {
            return powerGrid.Count == 0;
        }
    }

    public bool CanPlugIn(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        return true;
    }

    public bool PlugIn(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        if (!CanPlugIn(powerRelated))
        {
            return false;
        }

        powerGrid.Add(powerRelated);
        return true;
    }

    public bool IsPluggedIn(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        return powerGrid.Contains(powerRelated);
    }

    public void Unplug(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        powerGrid.Remove(powerRelated);
    }

    public void Update()
    {
        float currentPowerLevel = 0.0f;
        foreach (PowerRelated powerRelated in powerGrid)
        {
            if (powerRelated.IsPowerProducer)
            {
                currentPowerLevel += powerRelated.OutputRate;
            }

            if (powerRelated.IsPowerConsumer)
            {
                currentPowerLevel -= powerRelated.InputRate;
            }
        }

        if (currentPowerLevel.IsZero())
        {
            IsOperating = true;
            return;
        }

        if (currentPowerLevel > 0.0f)
        {
            ChargeAccumulators(ref currentPowerLevel);
        }
        else
        {
            DischargeAccumulators(ref currentPowerLevel);
        }

        IsOperating = currentPowerLevel >= 0.0f;
    }

    private void ChargeAccumulators(ref float currentPowerLevel)
    {
        foreach (PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsFull))
        {
            if (currentPowerLevel - powerRelated.InputRate < 0.0f)
            {
                continue;
            }

            currentPowerLevel -= powerRelated.InputRate;
            powerRelated.AccumulatedPower += powerRelated.InputRate;
        }
    }

    private void DischargeAccumulators(ref float currentPowerLevel)
    {
        if (currentPowerLevel +
            powerGrid.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty).Sum(powerRelated => powerRelated.OutputRate) < 0)
        {
            return;
        }

        foreach (PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty))
        {
            currentPowerLevel += powerRelated.OutputRate;
            powerRelated.AccumulatedPower -= powerRelated.OutputRate;
            if (currentPowerLevel >= 0.0f)
            {
                break;
            }
        }
    }
}
