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
using UnityEngine;

[MoonSharpUserData]
public class TemperatureUnit
{
    public TemperatureUnit(float kelvin)
    {
        this.temperatureInKelvin = kelvin;
    }

    public void UpdateTemperature(float newTemp)
    {
        temperatureInKelvin = newTemp;
        temperatureInKelvin = Mathf.Clamp(temperatureInKelvin, 0, temperatureInKelvin);
    }

    public void IncreaseTemperature(float amount)
    {
        temperatureInKelvin += amount;
        temperatureInKelvin = Mathf.Clamp(temperatureInKelvin, 0, temperatureInKelvin);
    }

    public override string ToString()
    {
        return "K: " + temperatureInKelvin + " C: " + temperatureInCelsius + " F: " + temperatureInFarenheit;
    }

    public float temperatureInKelvin;

    public float temperatureInCelsius { get { return temperatureInKelvin - 273.15f; } }

    public float temperatureInFarenheit { get { return temperatureInKelvin * 1.8f - 459.67f; } }
}

/// <summary>
/// A temperature management system
/// Braedon Wooding's Implementation using a general formula (built by himself)
/// The formula is: H -= startingEnergy / (e * | log(index) | * k), 
/// where index is the thermal conductivity index in format W/m.K
/// Note to me: We also take into account a few other things
/// Needs better description!!
/// </summary>
[MoonSharpUserData]
public class Temperature
{
    /// <summary>
    /// Taken straight from the real world value.  Then fine tuned a little
    /// </summary>
    public const float defaultThermalConductivity = 0.024f;

    public const float E = (float)Math.E;

    /// <summary>
    /// Our magical constant.
    /// </summary>
    public static float k = 1;

    /// <summary>
    /// How often does the system update?
    /// It uses this with a mixture of delta time
    /// Can be removed?
    /// </summary>
    public float updateInterval = 0.5f;

    /// <summary>
    /// The world this system is attached to
    /// </summary>
    private World world;

    /// <summary>
    /// The size of the map, just so I don't need to constantly go through world
    /// </summary>
    private int sizeX, sizeY, sizeZ;

    private Dictionary<float, float[]> temperatureMap = new Dictionary<float, float[]>();

    /// <summary>
    /// Create and Initialize arrays with default values.
    /// </summary>
    public Temperature(World world)
    {
        sizeX = world.Width;
        sizeY = world.Height;
        sizeZ = world.Depth;
        temperatureMap = new Dictionary<float, float[]>();
    }

    /// <summary>
    /// Public interface to temperature model, returns temperature at x, y.
    /// Just hooks up to the world anyway.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Temperature at x,y,z.</returns>
    public TemperatureUnit GetTemperatureUnit(int x, int y, int z)
    {
        return world.GetTileAt(x, y, z).TemperatureUnit;
    }

    /// <summary>
    /// Checks wether temperature is admissible, if not warns you and returns false.
    /// </summary>
    /// <param name="temp">Wanted temperature.</param>
    /// <returns>True if temperature is within bounds, else false.</returns>
    public bool IsWithinTemperatureBounds(float temp)
    {
        if (temp >= 0 && temp < Mathf.Infinity)
        {
            return true;
        }
        else
        {
            // string.format not needed with UberLogger.
            UnityDebugger.Debugger.LogWarningFormat("Temperature", "Yep, something is wrong with your temperature: {0}.", temp);
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
            UnityDebugger.Debugger.LogWarningFormat("Temperature", "Trying to set a thermal diffusivity that may break the world: {0}.", thermal_diff);
            return false;
        }
    }

    public void Resize(World world)
    {
        this.world = world;
        sizeX = world.Width;
        sizeY = world.Height;
        sizeZ = world.Depth;
    }

    /// <summary>
    /// Produce a temperature spread at the tile provided.
    /// </summary>
    /// <param name="source"> The source tile to begin at. </param>
    /// <param name="value"> The potential heat to disperse. </param>
    /// <remarks>   
    /// Uses a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it presumes that most follow the default thermal conductivity index then if it doesn't it deals with it seperately
    /// So it looks like a circle generalisation spread (but in reality its more of a polygonal generalisation).
    /// </remarks>
    public void ProduceTemperatureAtTile(Tile source, float value, float deltaTime)
    {
        float startingEnergy = value;
        float effect = 1;

        if (temperatureMap.ContainsKey(value) == false)
        {
            // Generate the map
            List<float> premap = new List<float>();

            float potentialHeat = value;
            while (potentialHeat > 0.1)
            {
                potentialHeat -= startingEnergy / (E * Mathf.Abs(Mathf.Log(defaultThermalConductivity)) * k);
                premap.Add(potentialHeat);
            }

            temperatureMap[value] = premap.ToArray();
        }

        // Ways to improve:
        // 1) Iterate max of x (length of the temperature) and max of y (again length of the temperature), indexing based on variable
        // 2) Do polygonal's slightly differently
        for (int i = temperatureMap[value].Length - 1; i >= 0; i--)
        {
            float potentialHeat = temperatureMap[value][i];

            for (int x = -i; x <= i; x++)
            {
                if (x == -i || x == i)
                {
                    for (int y = -i; y <= i; y++)
                    {
                        // Could have it so you have special values that x / y has to equal for this to equal false
                        if (source.X + x >= 0 && source.X + x < sizeX && source.Y + y >= 0 && source.Y + y < sizeY)
                        {
                            int z = 0;
                            DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y + y, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                        }
                    }
                }
                else
                {
                    if (source.X + x >= 0 && source.X + x < sizeX && source.Y + i >= 0 && source.Y + i < sizeY)
                    {
                        int z = 0;
                        DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y + i, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                    }

                    if (source.X + x >= 0 && source.X + x < sizeX && source.Y - i >= 0 && source.Y - i < sizeY)
                    {
                        int z = 0;
                        DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y - i, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                    }
                }
            }

            if (i / 2 + source.X > sizeX && source.X - i / 2 <= 0 || i / 2 + source.Y > sizeY && source.Y - i / 2 <= 0)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns a mapped float from the supplied x, y and center location.
    /// 0 = None, 1 = N, 1.5 = NE, 2 = E, 2.5 = SE, 3 = S, 3.5 = SW, 4 = W, 4.5 = NW
    /// </summary>
    private float GetLocationFloatFromXY(Tile tile, Tile centerLocation)
    {
        int x = tile.X;
        int y = tile.Y;

        if (x == y)
        {
            return x == 0 ? 0 : (x > centerLocation.X ? 1.5f : 3.5f);
        }
        else if (Math.Abs(x) == Math.Abs(y))
        {
            // Opposite corners
            return x > centerLocation.X ? 2.5f : 4.5f;
        }
        else
        {
            // X != Y
            if (x == 0 || Math.Abs(x) < Math.Abs(y))
            {
                return y > centerLocation.Y ? 1 : 3;
            }
            else
            {
                return x > centerLocation.X ? 2 : 4;
            }
        }
    }

    private Tile[] GetNeighbours(float posRelativeToSource, Tile tile)
    {
        if (posRelativeToSource == 0)
        {
            return new Tile[0];
        }

        List<Tile> locations = new List<Tile>();

        float max, min;

        if (posRelativeToSource == 1)
        {
            max = 1.5f;
            min = 4.5f;
        }
        else if (posRelativeToSource == 4.5f)
        {
            max = 1;
            min = 4;
        }
        else
        {
            max = posRelativeToSource + 0.5f;
            min = posRelativeToSource - 0.5f;
        }

        // Do the numbered direction of 'center', left, and right
        locations.Add(GetTileFromRelativeDirection(posRelativeToSource, tile));
        locations.Add(GetTileFromRelativeDirection(min, tile));
        locations.Add(GetTileFromRelativeDirection(max, tile));

        return locations.ToArray();
    }

    private Tile GetTileFromRelativeDirection(float posRelativeToSource, Tile tile)
    {
        int intPosRelativeToSource = (int)posRelativeToSource;
        int x = 0;
        int y = 0;

        if (intPosRelativeToSource % 2 == 0)
        {
            // Then it has to be either 2, 2.5, 4, 4.5
            x = intPosRelativeToSource == 4 ? -1 : 1;

            if (posRelativeToSource % 2 != 0)
            {
                y = posRelativeToSource == 4.5f ? 1 : -1;
            }
        }
        else if (intPosRelativeToSource % 2 == 1)
        {
            y = posRelativeToSource == 3 ? -1 : 1;


            if (posRelativeToSource % 2 != 1)
            {
                x = posRelativeToSource == 3.5 ? -1 : 1;
            }
        }

        return world.GetTileAt(tile.X + x, tile.Y + y, tile.Z);
    }

    /// <summary>
    /// If the tile isn't a polygonal then just apply normally else then do a little math on it
    /// </summary>
    /// <param name="location"> 0 = None, 1 = N, 1.5 = NE, 2 = E, 2.5 = SE, 3 = S, 3.5 = SW, 4 = W, 4.5 = NW </param>
    private void DetermineIfPolygonal(Tile tile, float potentialHeat, float startingHeat, Tile centerLocation)
    {
        float thermalValue;

        if (tile == null || tile.Room == null || tile.Room.ID == -1)
        {
            thermalValue = 10000000;
        }
        else if (tile.Furniture == null)
        {
            thermalValue = defaultThermalConductivity;
        }
        else
        {
            thermalValue = tile.Furniture.ThermalConductivityIndex;
        }

        tile.ApplyTemperature(potentialHeat, startingHeat);

        if (tile.Furniture != null && thermalValue != defaultThermalConductivity)
        {
            float position = GetLocationFloatFromXY(tile, centerLocation);

            float baseTemperature;

            baseTemperature = (tile.TemperatureUnit.temperatureInKelvin) / (E * Mathf.Abs(Mathf.Log(thermalValue)) * k);

            Tile[] neighbours = GetNeighbours(position, tile);

            for (int i = 0; i < neighbours.Length; i++)
            {
                neighbours[i].ApplyTemperature(-baseTemperature, startingHeat);
            }
        }
    }

    /// <summary>
    /// Handle all the tiles that don't match the 'default thermal conductivity index'.
    /// Essentially the tiles that turn it from a circle to a polygon.
    /// </summary>
    /// <param name="polyognalTiles"></param>
    private void HandlePolygonal(Tile flaggedTile, float potentialHeat, float deltaTime, float effect)
    {

    }

    /// <summary>
    /// Produce a temperature spread at the location provided.
    /// </summary>
    /// <param name="location"> The location to begin at. </param>
    /// <param name="value"> The potential heat to disperse. </param>
    /// <remarks>   
    /// Uses a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it presumes that most follow the default thermal conductivity index then if it doesn't it deals with it seperately
    /// So it looks like a circle generalisation spread (but in reality its more of a polygonal generalisation).
    /// </remarks>
    public void ProduceTemperatureAtLocation(Vector3 location, float value, float deltaTime)
    {
        ProduceTemperatureAtTile(world.GetTileAt((int)location.x, (int)location.y, (int)location.z), value, deltaTime);
    }

    /// <summary>
    /// Produce a temperature spread at the temperature provided.
    /// </summary>
    /// <param name="furniture"> The furniture item to begin at. </param>
    /// <param name="value"> The potential heat dispersed. </param>
    /// <remarks>   
    /// Uses a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it presumes that most follow the default thermal conductivity index then if it doesn't it deals with it seperately
    /// So it looks like a circle generalisation spread (but in reality its more of a polygonal generalisation).
    /// </remarks>
    public void ProduceTemperatureAtFurniture(Furniture furniture, float value, float deltaTime)
    {
        ProduceTemperatureAtTile(furniture.Tile, value, deltaTime);
    }
}
