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

        /// <summary>
        /// Determines whether the connection can plug into this grid.
        /// </summary>
        /// <returns><c>true</c> if the connection can plug into this grid; otherwise, <c>false</c>.</returns>
        public bool CanPlugIn(IPlugable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return true;
        }

        /// <summary>
        /// Plugs the IPlugable into this grid.
        /// </summary>
        /// <returns><c>true</c>, if in was plugged, <c>false</c> otherwise.</returns>
        /// <param name="connection">IPlugable to be plugged in.</param>
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

        /// <summary>
        /// Determines whether the connection is plugged into this Grid.
        /// </summary>
        /// <returns><c>true</c> if the connection is plugged into this Grid; otherwise, <c>false</c>.</returns>
        public bool IsPluggedIn(IPlugable connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        /// <summary>
        /// Unplug the specified IPlugable from this Grid.
        /// </summary>
        /// <param name="connection">IPlugable to be unplugged.</param>
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

        /// <summary>
        /// Gets the ID for this grid within the PowerNetwork.
        /// </summary>
        /// <returns>The ID number.</returns>
        public int GetId()
        {
            return World.Current.PowerNetwork.FindId(this);
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
            IPlugable[] tempConnections = (IPlugable[])connections.ToArray().Clone();
            connections.Clear();
            foreach (IPlugable connection in tempConnections)
            {
                connection.Reconnect();
            }
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
