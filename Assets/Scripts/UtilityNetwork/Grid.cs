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
using UnityEngine;

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
            Efficiency = 1f;
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

        public float Efficiency { get; private set; }

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
                UnityDebugger.Debugger.LogWarning("Grid", "UtilityType isn't null and doesn't match, no plugin");
                return false;
            }

            if (SubType != string.Empty && connection.SubType != string.Empty && SubType != connection.SubType)
            {
                UnityDebugger.Debugger.LogWarning("Grid", "Neither SubType is empty, and they don't match, no plugin");
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
                UnityDebugger.Debugger.LogWarning("Grid", "Can't Plugin");
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

        public bool HasAnyProducer()
        {
            foreach (IPluggable connection in connections)
            {
                if (connection.IsProducer)
                {
                    return true;
                }
            }

            return false;
        }

        public void Tick()
        {
            float producers = 0f;
            float producersVarying = 0f;
            float producersStable = 0;
            float consumersVarying = 0f;
            float consumersStable = 0;
            
            foreach (IPluggable connection in connections)
            {
                if (connection.IsProducer && connection.AllRequirementsFulfilled)
                {
                    producersStable += connection.OutputRate;
                }

                if (connection.IsConsumer && connection.AllRequirementsFulfilled)
                {
                    if (connection.InputCanVary)
                    {
                        consumersVarying += connection.InputRate;
                    }
                    else
                    {
                        consumersStable += connection.InputRate;
                    }
                }
            }
            
            float currentLevel = producersStable - consumersVarying - consumersStable;

            if (producersStable > 0f && currentLevel.IsZero())
            {
                IsOperating = true;
                return;
            }

            // if there is power shortage, check if you can get power from 'on demand' producers
            if (currentLevel < 0.0f)
            {
                float curLevelWithVaryingOutput = currentLevel;

                // here check if we can plug in some varying output
                // can't use connection.IsProducer as it's hooked on IsRunning already
                foreach (IPluggable connection in connections)
                {
                    if (connection.OutputCanVary) 
                    {
                        connection.OutputIsNeeded = true;
                    }
                }
            }
            else
            {
                // there is more power than needed, check if you can shut down some 'on demand' producers
                float curLevelWithVaryingOutput = currentLevel;
                foreach (IPluggable connection in connections)
                {
                    if (connection.IsProducer && connection.OutputCanVary && connection.OutputIsNeeded && connection.AllRequirementsFulfilled)
                    {
                        curLevelWithVaryingOutput -= connection.OutputRate;
                        if (curLevelWithVaryingOutput >= 0)
                        {
                            connection.OutputIsNeeded = false;
                        }
                    }
                }
            }

            if (currentLevel > 0.0f)
            {
                FillStorage(ref currentLevel);
            }
            else
            {                
                EmptyStorage(ref currentLevel);
            }

            producers = producersStable + producersVarying;

            Efficiency = 1f;

            // calculate immediate efficiency
            if (currentLevel < 0 && consumersVarying > 0)
            {
                float curLevelWithoutVary = currentLevel + consumersVarying;

                // if there is enough power when discarting varying consumers, calculate efficiency for them 
                if (curLevelWithoutVary > 0)
                {
                    float efficiency = curLevelWithoutVary / consumersVarying;

                    Efficiency = Mathf.Clamp(efficiency, 0, 1f);
                    currentLevel = 0f;
                }
                else
                {
                    Efficiency = 0f;
                }
            }
            
            // Efficiency == 1f condition prevents flickering of machines without varying input
            IsOperating = currentLevel >= 0.0f && Efficiency == 1f;
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

        private void FillStorage(ref float currentLevel)
        {
            foreach (IPluggable connection in connections.Where(connection => connection.IsStorage && !connection.IsFull))
            {
                float inputRate = connection.StoredAmount + connection.InputRate > connection.StorageCapacity ?
                    connection.StorageCapacity - connection.StoredAmount :
                    connection.InputRate;

                if (currentLevel - inputRate < 0.0f)
                {
                    inputRate = currentLevel;
                }

                currentLevel -= inputRate;
                connection.StoredAmount += inputRate;

                if (currentLevel.IsZero())
                {
                    break;
                }
            }
        }

        private void EmptyStorage(ref float currentLevel)
        {
            float possibleOutput = connections.Where(connection => connection.IsStorage && !connection.IsEmpty)
                .Sum(connection => GetOutputRate(connection));

            if (currentLevel + possibleOutput < 0)
            {
                return;
            }

            foreach (IPluggable connection in connections.Where(utilityConnection => utilityConnection.IsStorage && !utilityConnection.IsEmpty))
            {
                float outputRate = connection.OutputRate > Math.Abs(currentLevel) ? Math.Abs(currentLevel) : connection.OutputRate;
                outputRate = GetOutputRate(connection, outputRate);

                currentLevel += outputRate;
                connection.StoredAmount -= outputRate;
                if (currentLevel >= 0.0f)
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

            return connection.StoredAmount - outputRate < 0.0f ? connection.StoredAmount : outputRate;
        }
    }
}
