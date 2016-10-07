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
using ProjectPorcupine.Buildable.Components;

namespace ProjectPorcupine.PowerNetwork
{
    public class Grid
    {
        private readonly HashSet<PowerConnection> connections;

        public Grid()
        {
            connections = new HashSet<PowerConnection>();
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

        public bool CanPlugIn(PowerConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return true;
        }

        public bool PlugIn(PowerConnection connection)
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

        public bool IsPluggedIn(PowerConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        public void Unplug(PowerConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            connections.Remove(connection);
        }

        public void Tick()
        {
            float currentPowerLevel = 0.0f;
            foreach (PowerConnection connection in connections)
            {
                if (connection.IsPowerProducer)
                {
                    currentPowerLevel += connection.Provides.Rate;
                }

                if (connection.IsPowerConsumer)
                {
                    currentPowerLevel -= connection.Requires.Rate;
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
            foreach (PowerConnection connection in connections.Where(connection => connection.IsPowerAccumulator && !connection.IsFull))
            {
                float inputRate = connection.AccumulatedPower + connection.Requires.Rate > connection.Provides.Capacity ?
                    connection.Provides.Capacity - connection.AccumulatedPower :
                    connection.Requires.Rate;

                if (currentPowerLevel - inputRate < 0.0f)
                {
                    inputRate = currentPowerLevel;
                }

                currentPowerLevel -= inputRate;
                connection.AccumulatedPower += inputRate;

                if (currentPowerLevel.IsZero())
                {
                    break;
                }
            }
        }

        private void DischargeAccumulators(ref float currentPowerLevel)
        {
            float possibleOutput = connections.Where(connection => connection.IsPowerAccumulator && !connection.IsEmpty)
                .Sum(connection => GetOutputRate(connection));

            if (currentPowerLevel + possibleOutput < 0)
            {
                return;
            }

            foreach (PowerConnection connection in connections.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty))
            {
                float outputRate = connection.Provides.Rate > Math.Abs(currentPowerLevel) ? Math.Abs(currentPowerLevel) : connection.Provides.Rate;
                outputRate = GetOutputRate(connection, outputRate);

                currentPowerLevel += outputRate;
                connection.AccumulatedPower -= outputRate;
                if (currentPowerLevel >= 0.0f)
                {
                    break;
                }
            }
        }

        private float GetOutputRate(PowerConnection connection, float outputRate = 0.0f)
        {
            if (outputRate.IsZero())
            {
                outputRate = connection.Provides.Rate;
            }

            return connection.AccumulatedPower - outputRate < 0.0f ? connection.AccumulatedPower : outputRate;
        }
    }
}
