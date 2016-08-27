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

namespace Power
{
    public class Grid
    {
        private readonly HashSet<Connection> connections;

        public Grid()
        {
            connections = new HashSet<Connection>();
        }

        /// <summary>
        /// Power grid has enough power for all its connections.
        /// </summary>
        public bool IsOperating { get; private set; }

        /// <summary>
        /// No connections in this grid.
        /// </summary>
        public bool IsEmpty
        {
            get { return connections.Count == 0; }
        }

        public bool CanPlugIn(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return true;
        }

        public bool PlugIn(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!CanPlugIn(connection))
            {
                return false;
            }

            connections.Add(connection);
            return true;
        }

        public bool IsPluggedIn(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        public void Unplug(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            connections.Remove(connection);
        }

        public void Update()
        {
            float currentPowerLevel = 0.0f;
            foreach (Connection connection in connections)
            {
                if (connection.IsPowerProducer)
                {
                    currentPowerLevel += connection.OutputRate;
                }

                if (connection.IsPowerConsumer)
                {
                    currentPowerLevel -= connection.InputRate;
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
            foreach (Connection connection in connections.Where(connection => connection.IsPowerAccumulator && !connection.IsFull))
            {
                if (currentPowerLevel - connection.InputRate < 0.0f)
                {
                    continue;
                }

                currentPowerLevel -= connection.InputRate;
                connection.AccumulatedPower += connection.InputRate;
            }
        }

        private void DischargeAccumulators(ref float currentPowerLevel)
        {
            if (currentPowerLevel +
                connections.Where(connection => connection.IsPowerAccumulator && !connection.IsEmpty).Sum(powerRelated => powerRelated.OutputRate) < 0)
            {
                return;
            }

            foreach (Connection connection in connections.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty))
            {
                currentPowerLevel += connection.OutputRate;
                connection.AccumulatedPower -= connection.OutputRate;
                if (currentPowerLevel >= 0.0f)
                {
                    break;
                }
            }
        }
    }
}
