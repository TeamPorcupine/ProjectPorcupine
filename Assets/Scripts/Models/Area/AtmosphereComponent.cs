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
using System.Threading;
using MoonSharp.Interpreter;
using ProjectPorcupine.Rooms;
using UnityEngine;

[MoonSharpUserData]
public class AtmosphereComponent
{
    private float totalGas, totalDeltaGas;
    private Dictionary<string, float> gasses, gassesDelta;

    private float thermalEnergy;

    public AtmosphereComponent()
    {
        totalGas = 0;
        gasses = new Dictionary<string, float>();
        gassesDelta = new Dictionary<string, float>();
    }

    #region gas
    /// <summary>
    /// Gets the gas amount.
    /// </summary>
    /// <returns>The gas amount.</returns>
    public float GetGasAmount() {}

    /// <summary>
    /// Gets the gas amount.
    /// </summary>
    /// <returns>The gas amount.</returns>
    /// <param name="gasName">Gas name.</param>
    public float GetGasAmount(string gasName) {}

    /// <summary>
    /// Gets the gas delta.
    /// </summary>
    /// <returns>The gas delta.</returns>
    public float GetGasDelta() {}

    /// <summary>
    /// Gets the gas delta.
    /// </summary>
    /// <returns>The gas delta.</returns>
    /// <param name="gasName">Gas name.</param>
    public float GetGasDelta(string gasName) {}

    /// <summary>
    /// Creates the gas.
    /// </summary>
    /// <param name="gasName">Gas name.</param>
    /// <param name="GetGasAmount">Get gas amount.</param>
    /// <param name="temperature">Temperature.</param>
    public void CreateGas(string gasName, float GetGasAmount, float temperature) {}

    /// <summary>
    /// Moves the gas to.
    /// </summary>
    /// <param name="destination">Destination.</param>
    /// <param name="amount">Amount.</param>
    public void MoveGasTo(AtmosphereComponent destination, float amount) {}
    #endregion

    #region temperature
    /// <summary>
    /// Gets the temperature.
    /// </summary>
    /// <returns>The temperature.</returns>
    public float GetTemperature() {}

    /// <summary>
    /// Sets the temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    public void SetTemperature(float temperature) {}

    /// <summary>
    /// Changes the energy.
    /// </summary>
    /// <param name="amount">Amount.</param>
    public void ChangeEnergy(float amount) {}
    #endregion
}