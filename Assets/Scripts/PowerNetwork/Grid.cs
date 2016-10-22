﻿#region License
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

        /// <summary>
        /// Gets the connections to this grid.
        /// </summary>
        public HashSet<Connection> Connections
        {
            get 
            {
                return connections; 
            }
        }

        /// <summary>
        /// Determines whether the connection can plug into this grid.
        /// </summary>
        /// <returns><c>true</c> if the connection can plug into this grid; otherwise, <c>false</c>.</returns>
        public bool CanPlugIn(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return true;
        }

        /// <summary>
        /// Plugs the Connection into this grid.
        /// </summary>
        /// <returns><c>true</c>, if in was plugged, <c>false</c> otherwise.</returns>
        /// <param name="connection">Connection to be plugged in.</param>
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

        /// <summary>
        /// Determines whether the connection is plugged into this Grid.
        /// </summary>
        /// <returns><c>true</c> if the connection is plugged into this Grid; otherwise, <c>false</c>.</returns>
        public bool IsPluggedIn(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return connections.Contains(connection);
        }

        /// <summary>
        /// Unplug the specified connection from this Grid.
        /// </summary>
        /// <param name="connection">Connection to be unplugged.</param>
        public void Unplug(Connection connection)
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
            Connection[] tempConnections = (Connection[])connections.ToArray().Clone();
            connections.Clear();
            foreach (Connection connection in tempConnections)
            {
                connection.Reconnect();
            }
        }

        private void ChargeAccumulators(ref float currentPowerLevel)
        {
            foreach (Connection connection in connections.Where(connection => connection.IsPowerAccumulator && !connection.IsFull))
            {
                float inputRate = connection.AccumulatedPower + connection.InputRate > connection.Capacity ?
                    connection.Capacity - connection.AccumulatedPower :
                    connection.InputRate;

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

            foreach (Connection connection in connections.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty))
            {
                float outputRate = connection.OutputRate > Math.Abs(currentPowerLevel) ? Math.Abs(currentPowerLevel) : connection.OutputRate;
                outputRate = GetOutputRate(connection, outputRate);

                currentPowerLevel += outputRate;
                connection.AccumulatedPower -= outputRate;
                if (currentPowerLevel >= 0.0f)
                {
                    break;
                }
            }
        }

        private float GetOutputRate(Connection connection, float outputRate = 0.0f)
        {
            if (outputRate.IsZero())
            {
                outputRate = connection.OutputRate;
            }

            return connection.AccumulatedPower - outputRate < 0.0f ? connection.AccumulatedPower : outputRate;
        }
    }
}
