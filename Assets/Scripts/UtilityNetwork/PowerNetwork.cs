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

namespace ProjectPorcupine.PowerNetwork
{
    public class PowerNetwork
    {
        private readonly HashSet<Grid> powerGrids;
        private readonly float secondsToTick = 1.0f;
        private float secondsPassed;

        public PowerNetwork()
        {
            powerGrids = new HashSet<Grid>();
        }

        public bool IsEmpty
        {
            get { return powerGrids.Count == 0; }
        }

        public bool CanPlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return powerGrids.Any(grid => grid.CanPlugIn(connection));
        }

        public bool PlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (IsEmpty)
            {
                powerGrids.Add(new Grid());
            }

            Grid powerGrid = powerGrids.First(grid => grid.CanPlugIn(connection));
            return PlugIn(connection, powerGrid);
        }

        public bool PlugIn(IPluggable connection, Grid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!powerGrids.Contains(grid))
            {
                powerGrids.Add(grid);
            }

            return grid != null && grid.PlugIn(connection);
        }

        public bool IsPluggedIn(IPluggable connection, out Grid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (IsEmpty)
            {
                grid = null;
                return false;
            }

            grid = powerGrids.FirstOrDefault(powerGrid => powerGrid.IsPluggedIn(connection));
            return grid != null;
        }

        public void Unplug(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            Grid grid;
            IsPluggedIn(connection, out grid);
            if (grid == null)
            {
                return;
            }

            Unplug(connection, grid);
        }

        public void RegisterGrid(Grid grid)
        {
            if (powerGrids.Contains(grid))
            {
                return;
            }

            powerGrids.Add(grid);
        }

        public void UnregisterGrid(Grid grid)
        {
            if (!powerGrids.Contains(grid))
            {
                return;
            }

            powerGrids.Remove(grid);
        }

        public int FindId(Grid grid)
        {
            return powerGrids.ToList().IndexOf(grid);
        }

        public void Unplug(IPluggable connection, Grid grid)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }

            grid.Unplug(connection);
        }

        public bool IsConnected(IPluggable connection)
        {
            Grid grid;
            IsPluggedIn(connection, out grid);
            return grid != null && grid.HasAnyProducer();
        }

        public bool HasPower(IPluggable connection)
        {
            Grid grid;
            IsPluggedIn(connection, out grid);
            return grid != null && grid.IsOperating;
        }

        public float GetEfficiency(IPluggable connection)
        {
            float efficiency = 0f;
            Grid grid;
            IsPluggedIn(connection, out grid);
            if (grid != null)
            {
                efficiency = grid.Efficiency;
            }

            return efficiency;
        }

        public void Update(float deltaTime)
        {
            secondsPassed += deltaTime;
            if (secondsPassed < secondsToTick)
            {
                return;
            }

            secondsPassed = 0.0f;
            Tick();
        }

        public void RemoveGrid(Grid grid)
        {
            powerGrids.Remove(grid);
        }

        private void Tick()
        {
            if (IsEmpty)
            {
                return;
            }

            foreach (Grid powerGrid in powerGrids)
            {
                powerGrid.Tick();
            }
        }
    }
}
