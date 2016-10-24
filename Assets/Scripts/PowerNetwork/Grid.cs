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
    public class Grid
    {
        private readonly HashSet<IPlugable> connections;

        public Grid()
        {
            connections = new HashSet<IPlugable>();
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

        public bool CanPlugIn(IPlugable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return true;
        }

        public bool PlugIn(IPlugable connection)
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

        public bool IsPluggedIn(IPlugable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        public void Unplug(IPlugable connection)
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
            foreach (IPlugable connection in connections)
            {
                if (connection.IsProducer)
                {
                    currentPowerLevel += connection.OutputRate;
                }

                if (connection.IsConsumer)
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
            foreach (IPlugable connection in connections.Where(connection => connection.IsAccumulator && !connection.IsFull))
            {
                float inputRate = connection.AccumulatedAmount + connection.InputRate > connection.AccumulatorCapacity ?
                    connection.AccumulatorCapacity - connection.AccumulatedAmount :
                    connection.InputRate;

                if (currentPowerLevel - inputRate < 0.0f)
                {
                    inputRate = currentPowerLevel;
                }

                currentPowerLevel -= inputRate;
                connection.AccumulatedAmount += inputRate;

                if (currentPowerLevel.IsZero())
                {
                    break;
                }
            }
        }

        private void DischargeAccumulators(ref float currentPowerLevel)
        {
            float possibleOutput = connections.Where(connection => connection.IsAccumulator && !connection.IsEmpty)
                .Sum(connection => GetOutputRate(connection));

            if (currentPowerLevel + possibleOutput < 0)
            {
                return;
            }

            foreach (IPlugable connection in connections.Where(powerRelated => powerRelated.IsAccumulator && !powerRelated.IsEmpty))
            {
                float outputRate = connection.OutputRate > Math.Abs(currentPowerLevel) ? Math.Abs(currentPowerLevel) : connection.OutputRate;
                outputRate = GetOutputRate(connection, outputRate);

                currentPowerLevel += outputRate;
                connection.AccumulatedAmount -= outputRate;
                if (currentPowerLevel >= 0.0f)
                {
                    break;
                }
            }
        }

        private float GetOutputRate(IPlugable connection, float outputRate = 0.0f)
        {
            if (outputRate.IsZero())
            {
                outputRate = connection.OutputRate;
            }

            return connection.AccumulatedAmount - outputRate < 0.0f ? connection.AccumulatedAmount : outputRate;
        }
    }
}
