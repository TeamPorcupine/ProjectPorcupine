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

public class LivePowerSystem
{
    private readonly HashSet<PowerGrid> powerGrids;

    public LivePowerSystem()
    {
        powerGrids = new HashSet<PowerGrid>();
    }

    public bool IsEmpty
    {
        get
        {
            return powerGrids.Count == 0;
        }
    }

    public bool CanPlugIn(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        return powerGrids.Any(grid => grid.CanPlugIn(powerRelated));
    }

    public bool PlugIn(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        if (IsEmpty)
        {
            powerGrids.Add(new PowerGrid());
        }

        PowerGrid powerGrid = powerGrids.FirstOrDefault(grid => grid.CanPlugIn(powerRelated));
        return PlugIn(powerRelated, powerGrid);
    }

    public bool PlugIn(PowerRelated powerRelated, PowerGrid powerGrid)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        return powerGrid != null && powerGrid.PlugIn(powerRelated);
    }

    public bool IsPluggedIn(PowerRelated powerRelated, out PowerGrid powerGrid)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        if (IsEmpty)
        {
            powerGrid = null;
            return false;
        }

        powerGrid = powerGrids.FirstOrDefault(grid => grid.IsPluggedIn(powerRelated));
        return powerGrid != null;
    }

    public void Unplug(PowerRelated powerRelated)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        PowerGrid powerGrid;
        IsPluggedIn(powerRelated, out powerGrid);
        if (powerGrid == null)
        {
            return;
        }

        Unplug(powerRelated, powerGrid);
    }

    public void Unplug(PowerRelated powerRelated, PowerGrid powerGrid)
    {
        if (powerRelated == null)
        {
            throw new ArgumentNullException("powerRelated");
        }

        if (powerGrid == null)
        {
            throw new ArgumentNullException("powerGrid");
        }

        powerGrid.Unplug(powerRelated);
    }

    public bool HasPower(PowerRelated powerRelated)
    {
        PowerGrid powerGrid;
        IsPluggedIn(powerRelated, out powerGrid);
        return powerGrid != null && powerGrid.IsOperating;
    }

    public void Update()
    {
        if (IsEmpty)
        {
            return;
        }

        powerGrids.RemoveWhere(grid => grid.IsEmpty);
        foreach (PowerGrid powerGrid in powerGrids)
        {
            powerGrid.Update();
        }
    }
}
