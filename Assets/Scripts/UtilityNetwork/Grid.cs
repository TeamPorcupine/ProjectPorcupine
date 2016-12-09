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
        private readonly HashSet<IPluggable> connections;

        public Grid()
        {
            connections = new HashSet<IPluggable>();
            UtilityType = string.Empty;
            SubType = string.Empty;
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

        /// <summary>
        /// Gets the number of connections to this grid.
        /// </summary>
        public int ConnectionCount
        {
            get 
            {
                return connections.Count; 
            }
        }

        public string UtilityType { get; private set; }

        public string SubType { get; private set; }

        /// <summary>
        /// Determines whether the connection can plug into this grid.
        /// </summary>
        /// <returns><c>true</c> if the connection can plug into this grid; otherwise, <c>false</c>.</returns>
        public bool CanPlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (UtilityType != string.Empty && UtilityType != connection.UtilityType)
            {
                UnityDebugger.Debugger.LogWarning("UtilityType isn't null and doesn't match, no plugin");
                return false;
            }

            if (SubType != string.Empty && connection.SubType != string.Empty && SubType != connection.SubType)
            {
                UnityDebugger.Debugger.LogWarning("Neither SubType is empty, and they don't match, no plugin");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Plugs the IPluggable into this grid.
        /// </summary>
        /// <returns><c>true</c>, if in was plugged, <c>false</c> otherwise.</returns>
        /// <param name="connection">IPluggable to be plugged in.</param>
        public bool PlugIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!CanPlugIn(connection))
            {
                UnityDebugger.Debugger.LogWarning("Can't Plugin");
                return false;
            }

            if (UtilityType == string.Empty)
            {
                UtilityType = connection.UtilityType;
            }

            if (SubType == string.Empty)
            {
                SubType = connection.SubType;
            }
            else if (connection.SubType == string.Empty)
            {
                connection.SubType = SubType;
            }

            connections.Add(connection);
            return true;
        }

        /// <summary>
        /// Determines whether the connection is plugged into this Grid.
        /// </summary>
        /// <returns><c>true</c> if the connection is plugged into this Grid; otherwise, <c>false</c>.</returns>
        public bool IsPluggedIn(IPluggable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        /// <summary>
        /// Unplug the specified IPluggable from this Grid.
        /// </summary>
        /// <param name="connection">IPluggable to be unplugged.</param>
        public void Unplug(IPluggable connection)
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
            foreach (IPluggable connection in connections)
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

        /// <summary>
        /// Merge the specified Grid with this Grid.
        /// </summary>
        /// <param name="otherGrid">Other grid to be merged.</param>
        public void Merge(Grid otherGrid)
        {
            connections.UnionWith(otherGrid.connections);
        }

        /// <summary>
        /// Split this Grid into multiple grids.
        /// </summary>
        public void Split()
        {
            IPluggable[] tempConnections = (IPluggable[])connections.ToArray().Clone();
            connections.Clear();
            foreach (IPluggable connection in tempConnections)
            {
                connection.Reconnect();
            }
        }

        private void ChargeAccumulators(ref float currentPowerLevel)
        {
            foreach (IPluggable connection in connections.Where(connection => connection.IsAccumulator && !connection.IsFull))
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

            foreach (IPluggable connection in connections.Where(powerRelated => powerRelated.IsAccumulator && !powerRelated.IsEmpty))
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

        private float GetOutputRate(IPluggable connection, float outputRate = 0.0f)
        {
            if (outputRate.IsZero())
            {
                outputRate = connection.OutputRate;
            }

            return connection.AccumulatedAmount - outputRate < 0.0f ? connection.AccumulatedAmount : outputRate;
        }
    }
}
