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
    public const float K = 0.5f;

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
    /// The 2D Direction that is reflected 3D wise.
    /// Just makes it more clear what I do.
    /// </summary>
    private enum Direction
    {
        NW = 1,
        N = 2,
        NE = 3,
        W = 4,
        O = 5,
        E = 6,
        SW = 7,
        S = 8,
        SE = 9
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
            for (int z = 0; z < world.Depth; z++)
            {
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
                            DetermineIfPolygonal(world.GetTileAt(source.X + x, source.Y + i, source.Z + z), potentialHeat * effect * deltaTime, startingEnergy, source);
                        }

                        // Checking if we are at map limit
                        // Could just have a null check in determine if polygonal for speed improvements
                        if (source.X + x >= 0 && source.X + x < world.Width && source.Y - i >= 0 && source.Y - i < world.Height)
                        {
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
    }

    private List<Tile> GetMinMaxTiles(Tile tile, Tile source)
    {
        // If we are greater than + 1, lower - 1, equal +- 0 (exactly what CompareTo gives us :D)
        int zLevel = tile.Z.CompareTo(source.Z);

        // Just uses comparative operators (Compare to)
        Direction direction = (Direction)((tile.Y.CompareTo(source.Y) + 2) * (tile.X.CompareTo(source.X) + 2));

        List<Tile> tiles = new List<Tile>();
        Tile t;

        switch (direction)
        {
            case Direction.NW:
                // W
                t = world.GetTileAt(tile.X - 1, tile.Y, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // NW
                t = world.GetTileAt(tile.X - 1, tile.Y + 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // N
                t = world.GetTileAt(tile.X, tile.Y + 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                break;
            case Direction.N:
                for (int i = -1; i <= 1; i++)
                {
                    t = world.GetTileAt(tile.X + i, tile.Y + 1, tile.Z + zLevel);

                    if (t != null)
                    {
                        tiles.Add(t);
                    }
                }

                break;
            case Direction.NE:
                // E
                t = world.GetTileAt(tile.X + 1, tile.Y, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // NE
                t = world.GetTileAt(tile.X + 1, tile.Y + 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // N
                t = world.GetTileAt(tile.X, tile.Y + 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                break;
            case Direction.W:
                for (int i = -1; i <= 1; i++)
                {
                    t = world.GetTileAt(tile.X - 1, tile.Y + i, tile.Z + zLevel);

                    if (t != null)
                    {
                        tiles.Add(t);
                    }
                }

                break;
            case Direction.O:
                t = world.GetTileAt(tile.X, tile.Y, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                break;
            case Direction.E:
                for (int i = -1; i <= 1; i++)
                {
                    t = world.GetTileAt(tile.X + 1, tile.Y + i, tile.Z + zLevel);

                    if (t != null)
                    {
                        tiles.Add(t);
                    }
                }

                break;
            case Direction.SW:
                // S
                t = world.GetTileAt(tile.X, tile.Y - 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // SW
                t = world.GetTileAt(tile.X - 1, tile.Y - 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // W
                t = world.GetTileAt(tile.X - 1, tile.Y, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                break;
            case Direction.S:
                for (int i = -1; i <= 1; i++)
                {
                    t = world.GetTileAt(tile.X + i, tile.Y - 1, tile.Z + zLevel);

                    if (t != null)
                    {
                        tiles.Add(t);
                    }
                }

                break;
            case Direction.SE:
                // S
                t = world.GetTileAt(tile.X, tile.Y - 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // SE
                t = world.GetTileAt(tile.X + 1, tile.Y - 1, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                // E
                t = world.GetTileAt(tile.X + 1, tile.Y, tile.Z + zLevel);

                if (t != null)
                {
                    tiles.Add(t);
                }

                break;
        }

        return tiles;
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

        if (tile == null || tile.Room == null || tile.Room.ID == 0)
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
            List<Tile> neighbours = GetMinMaxTiles(tile, centerLocation);

            // Get our effect on the index (based off our temperature, so the hotter the 'better' we stop heat mimicking real behaviour)
            float baseTemperature = tile.TemperatureUnit.TemperatureInKelvin / (E * Mathf.Abs(Mathf.Log(indexForTile)) * K);

            // If heat within limits
            if (HeatGreaterThanLimit(baseTemperature, tile.TemperatureUnit.TemperatureInKelvin > 0))
            {
                // Apply to neighbours
                for (int i = 0; i < neighbours.Count; i++)
                {
                    neighbours[i].ApplyTemperature(-baseTemperature / startingHeat, startingHeat);
                }
            }
        }

        // Perform the tile's equalisation method
        tile.EqualiseTemperature();
    }
}
