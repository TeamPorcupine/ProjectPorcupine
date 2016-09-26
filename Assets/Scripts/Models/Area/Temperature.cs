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
using System.Threading;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// A  Temperature management system. Temperature is stored at each tile and evolved using
/// https://en.wikipedia.org/wiki/Heat_equation.
/// </summary>
[MoonSharpUserData]
public class Temperature 
{
    /// <summary>
    /// Default value assigned to thermalDIffusivity at "empty" tile.
    /// DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING: MUST BE BETWEEN 0 and 1.
    /// </summary>
    public static float defaultThermalDiffusivity = 1f;

    /// <summary>
    /// How often doe the physics update.
    /// </summary>
    public float updateInterval = 0.1f;

    /// <summary>
    /// All heaters and refrigerators shoul register themselves here using the public interface.
    /// </summary>
    private Dictionary<Furniture, Action<float>> sinksAndSources;

    /// <summary>
    /// Internal only variables.
    /// </summary>
    private float[][] temperature;
    private float[] thermalDiffusivity;

    /// <summary>
    /// Size of map.
    /// </summary>
    private int sizeX = World.Current.Width;
    private int sizeY = World.Current.Height;
    private int sizeZ = World.Current.Depth;

    /// <summary>
    /// Time since last update.
    /// </summary>
    private float elapsed = 0f;

    /// <summary>
    /// We switch between two "states" of temperatrue, because we reuqire a tempoerary array containing the old value.
    /// </summary>
    private int offset = 0;

    /// <summary>
    /// Create and Initialize arrays with default values.
    /// </summary>
    public Temperature() 
    {
        temperature = new float[2][]
        {
            new float[sizeX * sizeY * sizeZ],
            new float[sizeX * sizeY * sizeZ],
        };
        thermalDiffusivity = new float[sizeX * sizeY * sizeZ];
        for (int z = 0; z < sizeZ; z++) 
        {
            for (int y = 0; y < sizeY; y++) 
            {
                for (int x = 0; x < sizeX; x++) 
                {
                    int index = GetIndex(x, y, z);
                    temperature[0][index] = 0f;
                    thermalDiffusivity[index] = 1f;
                }
            }
        }

        sinksAndSources = new Dictionary<Furniture, Action<float>>();
    }

    /// <summary>
    /// If needed, progress physics.
    /// </summary>
    public void Update() 
    {
        // Progress physical time (should me linked to TIme.dt at some point)
        elapsed += updateInterval;

        if (elapsed >= updateInterval) 
        {
            ProgressTemperature(updateInterval);
            elapsed = elapsed - updateInterval;
        }
    }

    public void RegisterSinkOrSource(Furniture provider) 
    {
        // TODO: This need to be implemented.
        sinksAndSources[provider] = (float deltaTime) =>
        {
            provider.EventActions.Trigger("OnUpdateTemperature", provider, deltaTime);
        };
    }

    public void DeregisterSinkOrSource(Furniture provider) 
    {
        if (sinksAndSources.ContainsKey(provider)) 
        {
            sinksAndSources.Remove(provider);
        }
    }

    /// <summary>
    /// Public interface to temperature model, returns temperature at x, y.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Temperature at x,y,z.</returns>
    public float GetTemperature(int x, int y, int z) 
    {
        return temperature[offset][GetIndex(x, y, z)];
    }

    /// <summary>
    /// Public interface to setting temperature, set temperature at (x,y) to temp.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Temperature to set at x,y,z.</returns>
    public void SetTemperature(int x, int y, int z, float temp) 
    {
        if (IsWithinTemperatureBounds(temp)) 
        {
            temperature[offset][GetIndex(x, y, z)] = temp;
        }
    }

    /// <summary>
    /// Public interface to changing the temperature, increases temperature at (x,y) by incr.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <param name="incr">Temperature to increase at x,y, z.</param>
    public void ChangeTemperature(int x, int y, int z, float incr) 
    {
        if (IsWithinTemperatureBounds(temperature[offset][GetIndex(x, y, z)] + incr)) 
        {
            temperature[offset][GetIndex(x, y, z)] += incr;
        }
    }

    /// <summary>
    /// Public interface to thermal diffusivity model. Each tile has a value (say alpha) that
    /// tells  how the heat flows into that tile. Lower value means heat flows much slower (like trough a wall)
    /// while a value of 1 means the temperature "moves" faster. Think of it as a kind of isolation factor.
    /// TODO: Walls should set the coefficient to 0.1.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Thermal diffusivity alpha at x,y,z.</returns>
    public float GetThermalDiffusivity(int x, int y, int z) 
    {
        return thermalDiffusivity[GetIndex(x, y, z)];
    }

    /// <summary>
    /// Public interface to thermal diffusivity model. Set the value of thermal diffusivity at x,y to coeff.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <param name="coeff">Thermal diffusivity to set at x,y, z.</param>
    public void SetThermalDiffusivity(int x, int y, int z, float coeff) 
    {
        if (IsWithinThermalDiffusivityBounds(coeff)) 
        {
            thermalDiffusivity[GetIndex(x, y, z)] = coeff;
        }
    }

    /// <summary>
    /// Public interface to thermal diffusivity model. Change the value of thermal diffusivity at x,y by incr.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <param name="incr">Thermal diffusifity to increase at x,y,z.</param>
    public void ChangeThermalDiffusivity(int x, int y, int z, float incr) 
    {
        if (IsWithinThermalDiffusivityBounds(thermalDiffusivity[GetIndex(x, y, z)] + incr)) 
        {
            thermalDiffusivity[GetIndex(x, y, z)] += incr;
        }
    }

    /// <summary>
    /// Checks wether temperature is admissible, if not warns you and returns false.
    /// </summary>
    /// <param name="temp">Wanted temperature.</param>
    /// <returns>True if temperature is ok, false and a formal complaint if it's not ok.</returns>
    public bool IsWithinTemperatureBounds(float temp) 
    {
        if (temp >= 0 && temp < Mathf.Infinity) 
        {
                return true;
        }
        else 
        {
            // string.format not needed with UberLogger.
            Debug.ULogWarningChannel("Temperature", "Yep, something is wrong with your temperature: {0}.", temp);
            return false;
        }
    }

    /// <summary>
    /// Checks wether thermal diffusivity is admissible, if not warns you and returns false.
    /// </summary>
    /// <param name="thermal_diff">Wanted thermal diff.</param>
    /// <returns>True if thermal diff. is ok, false and a formal complaint if it's not ok.</returns>
    public bool IsWithinThermalDiffusivityBounds(float thermal_diff) 
    {
        if (thermal_diff >= 0 && thermal_diff <= 1) 
        {
            return true;
        }
        else 
        {
            Debug.ULogWarningChannel("Temperature", "Trying to set a thermal diffusivity that may break the world: {0}.", thermal_diff);
            return false;
        }
    }

    /// <summary>
    /// Internal indexing of array.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Actual index for array access.</returns>
    private int GetIndex(int x, int y, int z) 
    {
        return (z * sizeX * sizeY) + (y * sizeX) + x;
    }

    /// <summary>
    /// Evolve the temperature model. Loops over all tiles.
    /// </summary>
    private void ProgressTemperature(float deltaT) 
    {
        // TODO: Compute temperature sources.
        if (sinksAndSources != null) 
        {
            foreach (Action<float> act in sinksAndSources.Values) 
            {
                act(deltaT);
            }
        }

        Thread thread = new Thread(ForwardTemp);
        thread.Start();
    }

    /// <summary>
    /// Update temperature using a forward method.
    /// </summary>
    private void ForwardTemp() 
    {
        // Store references.
        float[] temp_curr = temperature[1 - offset];
        float[] temp_old = temperature[offset];

        // Compute a constant:
        // delta.Time * magic_coefficient * 0.5 (avg for thermalDiffusivity).
        // Make sure c is always between 0 and 0.5*0.25 (not included) or things will blow up
        // in your face.
        float c = 0.23f * 0.5f;

        // Calculates for all tiles.     
        for (int z = 0; z < sizeZ; z++) 
        {
            for (int y = 0; y < sizeY; y++) 
            {
                for (int x = 0; x < sizeX; x++) 
                {
                    int index = GetIndex(x, y, z);
                    int index_N = GetIndex(x, y + 1, z);
                    int index_S = GetIndex(x, y - 1, z);
                    int index_W = GetIndex(x - 1, y, z);
                    int index_E = GetIndex(x + 1, y, z);
                    int index_above = GetIndex(x, y, z + 1);
                    int index_below = GetIndex(x, y, z - 1);
                        
                    // Update temperature using finite difference and forward method:
                    // U^{n+1} = U^n + dt*(\Div alpha \Grad U^n).
                    temp_curr[index] = temp_old[index];

                    // If empty space, set temperature to 0.                 
                    if (WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, z)).Room == null) 
                    { 
                        temp_old[index] = 0f;
                    }

                    if (x > 0) 
                    {
                        temp_curr[index] +=
                            c * Mathf.Min(thermalDiffusivity[index], thermalDiffusivity[index_W]) *
                            (temp_old[index_W] - temp_old[index]);
                    }

                    if (y > 0) 
                    {
                        temp_curr[index] +=
                            c * Mathf.Min(thermalDiffusivity[index], thermalDiffusivity[index_S]) *
                            (temp_old[index_S] - temp_old[index]);
                    }

                    if (x < sizeX - 1) 
                    {
                        temp_curr[index] +=
                            c * Mathf.Min(thermalDiffusivity[index], thermalDiffusivity[index_E]) *
                            (temp_old[index_E] - temp_old[index]);
                    }

                    if (y < sizeY - 1) 
                    {
                        temp_curr[index] +=
                            c * Mathf.Min(thermalDiffusivity[index], thermalDiffusivity[index_N]) *
                            (temp_old[index_N] - temp_old[index]);
                    }

                    if (z > 0) 
                    {
                        temp_curr[index] += c * 0.5f * (temp_old[index_below] - temp_old[index]);
                    }

                    if (z < sizeZ - 1) 
                    {
                        temp_curr[index] += c * 0.5f * (temp_old[index_above] - temp_old[index]);
                    }                   
                }
            }
        }

        // Swap variable order
        offset = 1 - offset;
    }
}
