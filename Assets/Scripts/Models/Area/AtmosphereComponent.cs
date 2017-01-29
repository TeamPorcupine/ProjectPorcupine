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
    private Dictionary<string, float> gasses;

    public AtmosphereComponent()
    {
        TotalGas = 0;
        gasses = new Dictionary<string, float>();
        ThermalEnergy = 0;
    }

    public float TotalGas { get; private set; }

    public float ThermalEnergy { get; private set; }

    #region gas
    /// <summary>
    /// Gets the total gas stored in this component.
    /// </summary>
    /// <returns>The total gas amount.</returns>
    public float GetGasAmount()
    {
        return TotalGas;
    }

    /// <summary>
    /// Gets the amount of this gas stored in this component.
    /// </summary>
    /// <returns>The amount of this gas stored in the component.</returns>
    /// <param name="gasName">Gas you want the pressure of.</param>
    public float GetGasAmount(string gasName)
    {
        return gasses.ContainsKey(gasName) ? gasses[gasName] : 0;
    }

    /// <summary>
    /// Gets the fraction of the gas present that is this type of gas.
    /// </summary>
    /// <returns>The fraction of this type of gas.</returns>
    /// <param name="gasName">Name of the gas.</param>
    public float GetGasFraction(string gasName)
    {
        return TotalGas > 0 ? GetGasAmount(gasName) / TotalGas : 0.0f;
    }

    /// <summary>
    /// Get the names of gasses present in this component.
    /// </summary>
    /// <returns>The names of gasses present.</returns>
    public string[] GetGasNames()
    {
        return gasses.Keys.ToArray();
    }

    /// <summary>
    /// Sets the amount of gas of this type to the value. Temperature will stay constant.
    /// </summary>
    /// <param name="gasName">The name of the gas whose value is set.</param>
    /// <param name="amount">The amount of gas to set it to.</param>
    public void SetGas(string gasName, float value)
    {
        float delta = value - GetGasAmount(gasName);
        ThermalEnergy += delta * GetTemperature();
        ChangeGas(gasName, delta);
        ////UnityDebugger.Debugger.Log("Atmosphere", "Setting " + gasName + ". New value is " + GetGasAmount(gasName));
    }

    /// <summary>
    /// Creates gas of a determined type and temperature out of nowhere. This should only be used when there is no source for the gas. Otherwise use MoveGasTo.
    /// </summary>
    /// <param name="gasName">Name of the gas to create.</param>
    /// <param name="GetGasAmount">Amount of gas to create.</param>
    /// <param name="temperature">Temperature of the gas.</param>
    public void CreateGas(string gasName, float amount, float temperature)
    {
        if (amount < 0 || temperature < 0)
        {
            UnityDebugger.Debugger.LogError("CreateGas -- Amount or temperature can not be negative: " + amount + ", " + temperature);
            return;
        }

        ChangeGas(gasName, amount);

        ThermalEnergy += amount * temperature;
    }

    /// <summary>
    /// Destroys gas evenly. This should only be used when there is no destination for the gas. Otherwise use MoveGasTo.
    /// </summary>
    /// <param name="amount">Amount to destroy.</param>
    public void DestroyGas(float amount)
    {
        if (amount < 0)
        {
            UnityDebugger.Debugger.LogError("DestroyGas -- Amount can not be negative: " + amount);
            return;
        }

        amount = Mathf.Min(TotalGas, amount);
        foreach (var gasName in GetGasNames())
        {
            ChangeGas(gasName, -amount * GetGasFraction(gasName));
        }

        ThermalEnergy -= amount * GetTemperature();
    }

    /// <summary>
    /// Destroys gas of this type. This should only be used when there is no destination for the gas. Otherwise use MoveGasTo.
    /// </summary>
    /// <param name="gasName">Name of gas to destroy.</param>
    /// <param name="amount">Amount to destroy.</param>
    public void DestroyGas(string gasName, float amount)
    {
        if (amount < 0)
        {
            UnityDebugger.Debugger.LogError("DestroyGas -- Amount can not be negative: " + amount);
            return;
        }

        amount = Mathf.Min(GetGasAmount(gasName), amount);

        ThermalEnergy -= amount * GetTemperature();
        ChangeGas(gasName, -amount);
    }

    /// <summary>
    /// Moves gas to another atmosphere. Thermal energy is transferred accordingly.
    /// </summary>
    /// <param name="destination">Destination of the gas.</param>
    /// <param name="amount">Amount of gas to move.</param>
    public void MoveGasTo(AtmosphereComponent destination, float amount)
    {
        if (destination == null)
        {
            UnityDebugger.Debugger.LogError("MoveGasTo -- Destination can not be null");
            return;
        }

        if (amount < 0 || float.IsNaN(amount))
        {
            UnityDebugger.Debugger.LogError("MoveGasTo -- Amount can not be negative: " + amount);
            return;
        }

        amount = Mathf.Min(this.TotalGas, amount);

        float thermalDelta = amount * this.GetTemperature();
        this.ThermalEnergy -= thermalDelta;
        destination.ThermalEnergy += thermalDelta;

        foreach (var gasName in this.GetGasNames())
        {
            float partialAmount = amount * GetGasFraction(gasName);
            this.ChangeGas(gasName, -partialAmount);
            destination.ChangeGas(gasName, partialAmount);
        }
    }

    public void MoveGasTo(AtmosphereComponent destination, string gasName, float amount)
    {
        if (amount < 0)
        {
            UnityDebugger.Debugger.LogError("MoveGasTo -- Amount can not be negative: " + amount);
            return;
        }

        amount = Mathf.Min(this.GetGasAmount(gasName), amount);

        float thermalDelta = amount * this.GetTemperature();
        this.ThermalEnergy -= thermalDelta;
        destination.ThermalEnergy += thermalDelta;

        this.ChangeGas(gasName, -amount);
        destination.ChangeGas(gasName, amount);
    }
    #endregion

    #region temperature
    /// <summary>
    /// Gets the temperature.
    /// </summary>
    /// <returns>The temperature.</returns>
    public float GetTemperature()
    {
        float t = TotalGas.IsZero() ? 0.0f : ThermalEnergy / TotalGas;
        if (float.IsNaN(t))
        {
            UnityDebugger.Debugger.Log("Atmosphere", "NaN result. Total gas = " + TotalGas + ". Energy = " + ThermalEnergy + ".");
        }

        return TotalGas.IsZero() ? 0.0f : ThermalEnergy / TotalGas;
    }

    /// <summary>
    /// Sets the temperature.
    /// </summary>
    /// <param name="temperature">Temperature.</param>
    public void SetTemperature(float temperature)
    {
        ThermalEnergy = TotalGas * temperature;
    }

    /// <summary>
    /// Changes the energy.
    /// </summary>
    /// <param name="amount">The amount of energy added or removed from the total.</param>
    public void ChangeEnergy(float amount)
    {
        ThermalEnergy += Mathf.Max(-ThermalEnergy, amount);
    }
    #endregion

    private void ChangeGas(string gasName, float amount)
    {
        if (gasses.ContainsKey(gasName) == false)
        {
            gasses[gasName] = 0;
        }

        if (gasses[gasName] <= -amount)
        {
            TotalGas -= gasses[gasName];
            gasses.Remove(gasName);
        }
        else
        {
            TotalGas += amount;
            gasses[gasName] += amount;
        }
    }
}