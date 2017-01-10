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

/// <summary>
/// A tile by tile temperature management system.
/// It utilises a generalised formula for temperature diffusion and conduction (credit: Braedon Wooding)
/// Formula is: H -= startingEnergy / (e * abs(log(index)) * k), 
/// where index is the thermal conductivity index in format W/m.K (watts per Kelvin metre).
/// </summary>
[MoonSharpUserData]
public class Temperature
{
    /// <summary>
    /// The thermal conductivity of air (within the range).
    /// </summary>
    public const float DefaultThermalConductivity = 0.024f;

    /// <summary>
    /// The thermal conductivity of a vacuum.
    /// This won't effect the square in which the vacuum is located but rather its neighbours => resulting in a space vacuum effect.
    /// Value of 10,000,000 (10 million).
    /// </summary>
    public const float VacuumThermalConductivity = 10000000f;

    /// <summary>
    /// The value E, but as a float.
    /// </summary>
    public const float E = (float)Math.E;

    /// <summary>
    /// A constant that changes the value of the result,
    /// a smaller k will result in a smaller heat potential => less heat dispersion.
    /// </summary>
    public const float K = 1;

    /// <summary>
    /// Cache maps of heat, these don't have to be regenerated ever.
    /// </summary>
    private static Dictionary<float, float[]> temperatureMap = new Dictionary<float, float[]>();

    /// <summary>
    /// The world this system is attached to.
    /// </summary>
    private World world;

    /// <summary>
    /// Create a new temperature management system that is attached to world.
    /// </summary>
    /// <param name="world"> The world to attach to. </param>
    public Temperature(World world)
    {
        this.world = world;
    }

    /// <summary>
    /// When world changes (and this should change too), update our world.
    /// </summary>
    /// <param name="world"> The new world to attach too. </param>
    public void OnWorldChange(World world)
    {
        this.world = world;
    }

    /// <summary>
    /// Interface to temperature model, returns temperature at x, y, z.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns> Temperature at x,y,z.</returns>
    /// <remarks> Just does a get tile at then returns the temperature unit. </remarks>
    public TemperatureUnit GetTemperatureUnit(int x, int y, int z)
    {
        return world.GetTileAt(x, y, z).TemperatureUnit;
    }

    /// <summary>
    /// Checks bounds for heat.
    /// WHILE kelvin can't be 0, we can have a negative temperature applied.
    /// I.e. - 100 K from room, therefore we need to check based on that ruleset.
    /// </summary>
    /// <param name="heat"> The heat in question. </param>
    /// <param name="positive"> Was the initial heat positive or negative. </param>
    /// <returns> True if the heat is greater than the limits else it returns false. </returns>
    /// <remarks> 
    /// Yes if the heat is negative its less than the limit, 
    /// but in abstract terms its 'greater' than the limit.
    /// </remarks>
    public bool HeatGreaterThanLimit(float heat, bool positive)
    {
        if (positive)
        {
            return heat > 0.1;
        }
        else
        {
            return heat < -0.1;
        }
    }

    /// <summary>
    /// Create a temperature spread at the location provided.
    /// </summary>
    /// <param name="location"> The location to begin at. </param>
    /// <param name="value"> The potential heat to disperse. </param>
    /// <remarks>
    /// Utilises a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it maps out a circular map (using the caches),
    /// then it performs a sub generalisation for polygonal areas (that are different indexes).
    /// </remarks>
    public void ProduceTemperatureAtLocation(Vector3 location, float value, float deltaTime)
    {
        ProduceTemperatureAtTile(world.GetTileAt((int)location.x, (int)location.y, (int)location.z), value, deltaTime);
    }

    /// <summary>
    /// Create a temperature spread at the furniture provided.
    /// </summary>
    /// <param name="furniture"> The furniture item to begin at. </param>
    /// <param name="value"> The potential heat to disperse. </param>
    /// <remarks>
    /// Utilises a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it maps out a circular map (using the caches),
    /// then it performs a sub generalisation for polygonal areas (that are different indexes).
    /// </remarks>
    public void ProduceTemperatureAtFurniture(Furniture furniture, float value, float deltaTime)
    {
        ProduceTemperatureAtTile(furniture.Tile, value, deltaTime);
    }

    /// <summary>
    /// Create a temperature spread at the source tile provided.
    /// </summary>
    /// <param name="source"> The source tile to begin at. </param>
    /// <param name="value"> The potential heat to disperse. </param>
    /// <param name="deltaTime"> How much time has passed since the last frame used to create a result that is a product of potential heat and deltaTime. </param>
    /// <remarks>
    /// Utilises a majoritively circular sub polygonal generalisation to perform the temperature spread.
    /// Essentially it maps out a circular map (using the caches),
    /// then it performs a sub generalisation for polygonal areas (that are different indexes).
    /// </remarks>
    public void ProduceTemperatureAtTile(Tile source, float value, float deltaTime)
    {
        float startingEnergy = value;
        float effect = 1;

        // If a cache doesn't exist generate one.
        if (temperatureMap.ContainsKey(value) == false)
        {
            List<float> premap = new List<float>();

            float potentialHeat = value;

            // While the heat is above/below the min/max limit do the mapping
            while (HeatGreaterThanLimit(potentialHeat, value >= 0))
            {
                // Add the previous heat then work out the next layer.
                premap.Add(potentialHeat);
                potentialHeat -= startingEnergy / (E * Mathf.Abs(Mathf.Log(DefaultThermalConductivity)) * K);
            }

            temperatureMap[value] = premap.ToArray();
        }

        // Perform the majoritively circular generalisation
        // We work backwards cause it means we can also do a equalisation operation without needing another loop
        for (int i = temperatureMap[value].Length - 1; i >= 0; i--)
        {
            // The current potential heat
            float potentialHeat = temperatureMap[value][i];

            // If we are at center then we do this case so it doesn't spread to neighbours 
            // since if you have heat exiting a heater you don't want that heater to block heater from getting to neighbours
            if (i == 0)
            {
                world.GetTileAt(source.X, source.Y, source.Z).ApplyTemperature(potentialHeat * effect * deltaTime, startingEnergy);

                // No need to continue since we are at end
                break;
            }

            // Cycle across
            // This creates does the outside of each layer that is does the top bottom left and right sides.
            // So it always does x and does y at the very left and very right
            for (int x = -i; x <= i; x++)
            {
                if (x == -i || x == i)
                {
                    // If we are at an end then cycle downwards
                    for (int y = -i; y <= i; y++)
                    {
                        // Checking if we are at map limit
                        // Could just have a null check in determine if polygonal for speed improvements
                        if (source.X + x >= 0 && source.X + x < world.Width && source.Y + y >= 0 && source.Y + y < world.Height)
                        {
                            int z = 0;
                            DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y + y, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                        }
                    }
                }
                else
                {
                    // If we aren't at map limit then do top and bottom.
                    // Checking if we are at map limit
                    // Could just have a null check in determine if polygonal for speed improvements
                    if (source.X + x >= 0 && source.X + x < world.Width && source.Y + i >= 0 && source.Y + i < world.Height)
                    {
                        int z = 0;
                        DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y + i, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                    }

                    // Checking if we are at map limit
                    // Could just have a null check in determine if polygonal for speed improvements
                    if (source.X + x >= 0 && source.X + x < world.Width && source.Y - i >= 0 && source.Y - i < world.Height)
                    {
                        int z = 0;
                        DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y - i, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                    }
                }
            }

            // If our tile is beyond the map border then stop
            if (((i / 2) + source.X > world.Width && source.X - (i / 2) <= 0) || ((i / 2) + source.Y > world.Height && source.Y - (i / 2) <= 0))
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns a mapped float from the supplied x, y and center location.
    /// 0 = None, 1 = N, 1.5 = NE, 2 = E, 2.5 = SE, 3 = S, 3.5 = SW, 4 = W, 4.5 = NW.
    /// </summary>
    /// <param name="centerLocation"> The location to create the relative location from. </param>
    /// <param name="tile"> The tile that is relative to the center location. </param>
    private float GetLocationFloatFromXY(Tile tile, Tile centerLocation)
    {
        int x = tile.X;
        int y = tile.Y;

        if (x == y)
        {
            // If x == y then it either has to be top right corner or bottom left corner
            return x == 0 ? 0 : (x > centerLocation.X ? 1.5f : 3.5f);
        }
        else if (Math.Abs(x) == Math.Abs(y))
        {
            // If the |x| == |y| and x != y, then it has to be top left or bottom right corner.
            return x > centerLocation.X ? 2.5f : 4.5f;
        }
        else
        {
            // X != Y so it has to be one of the 4 cordinal directions
            if (x == 0 || Math.Abs(x) < Math.Abs(y))
            {
                // If x == 0 and y can't equal 0, then it has to be top or bottom
                // Or x < y (same conditional essentially, just the x == 0 is faster and happens in 1/2 of the cases)
                return y > centerLocation.Y ? 1 : 3;
            }
            else
            {
                // If x != 0 and |x| > |y| then x has to be either left or right
                return x > centerLocation.X ? 2 : 4;
            }
        }
    }

    /// <summary>
    /// Get all the relative numbers based of the relative location.
    /// Relative neighbours are the min - max of a certain relative location.
    /// </summary>
    /// <param name="posRelativeToSource"> The relative location <seealso cref="GetLocationFloatFromXY(Tile, Tile)"/>. </param>
    /// <param name="tile"> The tile to get the relative neighbours from. </param>
    /// <returns> A list of all relative neighbours. </returns>
    /// <example>
    /// That is if we have the following example x is tile, y is relative neighbours, c is the center tile that x is relative to
    /// - - - y
    /// - c x y
    /// - - - y
    /// Or
    /// - y y
    /// - x y
    /// c - -.
    /// </example>
    private List<Tile> GetRelativeNeighbours(float posRelativeToSource, Tile tile)
    {
        // 0 is our 'null' case basically
        if (posRelativeToSource == 0)
        {
            return new List<Tile>();
        }

        List<Tile> locations = new List<Tile>();

        float max, min;

        // Due to the fact the numbers don't wrap around we have to do these two 'checks'
        // Could be switch but you don't gain much and its kinda less readable
        if (posRelativeToSource == 1)
        {
            // If we are at 1 (N) then do NW and NE
            max = 1.5f;
            min = 4.5f;
        }
        else if (posRelativeToSource == 4.5f)
        {
            // If we are at 4.5 (NW) do W and E
            max = 1;
            min = 4;
        }
        else
        {
            // Else the rest will work via this simple method
            max = posRelativeToSource + 0.5f;
            min = posRelativeToSource - 0.5f;
        }

        // Do the numbered direction of 'center', left, and right
        locations.Add(GetTileFromRelativeDirection(posRelativeToSource, tile));
        locations.Add(GetTileFromRelativeDirection(min, tile));
        locations.Add(GetTileFromRelativeDirection(max, tile));

        return locations;
    }

    /// <summary>
    /// Works out the x, y, and z coordinates
    /// Z can just be the x and y one up and one below.
    /// </summary>
    /// <param name="posRelativeToSource"> The position that we are at relative to the source. <seealso cref="GetLocationFloatFromXY(Tile, Tile)"/>.</param>
    /// <param name="tile"> The tile that is relative to source. </param>
    /// <returns></returns>
    private Tile GetTileFromRelativeDirection(float posRelativeToSource, Tile tile)
    {
        // Our coordinates
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
            // Has to be either 1, 1.5, 3, 3.5
            y = posRelativeToSource == 3 ? -1 : 1;

            if (posRelativeToSource % 2 != 1)
            {
                x = posRelativeToSource == 3.5 ? -1 : 1;
            }
        }

        // Return tile
        return world.GetTileAt(tile.X + x, tile.Y + y, tile.Z);
    }

    /// <summary>
    /// Perform the sub polygonal generalisation method.
    /// That is perform a sub generalisation method that is polygonal in nature.
    /// A tile only needs to perform this generalisation if its index is different to the <see cref="DefaultThermalConductivity"/> index.
    /// It will also effect from its relative neighbours since by definition of this method they will have been already computed so we are safe to change them.
    /// </summary>
    /// <param name="location"> 0 = None, 1 = N, 1.5 = NE, 2 = E, 2.5 = SE, 3 = S, 3.5 = SW, 4 = W, 4.5 = NW. </param>
    /// <param name="centerLocation"> The center location in which this heat is dispersing from. </param>
    /// <param name="tile"> The relative tile to center location that has an index different from <see cref="DefaultThermalConductivity"/>. </param>
    /// <param name="potentialHeat"> The potential heat of this layer. </param>
    /// <param name="startingHeat"> The heat we began at. </param>
    private void DetermineIfPolygonal(Tile tile, float potentialHeat, float startingHeat, Tile centerLocation)
    {
        float indexForTile;

        if (tile == null || tile.Room == null || tile.Room.ID == -1)
        {
            // If we are at 'void' this is the thermal value of vacuum/space/void.
            indexForTile = VacuumThermalConductivity;
        }
        else if (tile.Furniture == null)
        {
            // If no furniture then air
            // Maybe later tiles can have a thermal index too but not for now
            indexForTile = DefaultThermalConductivity;
        }
        else
        {
            // If furniture exists then just use that index.
            indexForTile = tile.Furniture.ThermalConductivityIndex;
        }

        // Apply the potential heat to the polygonal flagged tile
        tile.ApplyTemperature(potentialHeat, startingHeat);

        // If the index is different then the default thermal conductivity value then perform our neighbour generalisation
        if (indexForTile != DefaultThermalConductivity)
        {
            // Get effect relative neighbours
            float position = GetLocationFloatFromXY(tile, centerLocation);
            List<Tile> neighbours = GetRelativeNeighbours(position, tile);

            // Get our effect on the index (based off our temperature, so the hotter the 'better' we stop heat mimicking real behaviour)
            float baseTemperature = tile.TemperatureUnit.TemperatureInKelvin / (E * Mathf.Abs(Mathf.Log(indexForTile)) * K);

            // If heat within limits
            if (HeatGreaterThanLimit(baseTemperature, tile.TemperatureUnit.TemperatureInKelvin > 0))
            {
                // Apply to neighbours
                for (int i = 0; i < neighbours.Count; i++)
                {
                    neighbours[i].ApplyTemperature(-baseTemperature, startingHeat);
                }
            }
        }

        // Perform the tile's equalisation method
        tile.EqualiseTemperature();
    }
}
