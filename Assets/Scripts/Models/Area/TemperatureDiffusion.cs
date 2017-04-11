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
public class TemperatureDiffusion
{
    private Dictionary<Room, Dictionary<Room, float>> diffusion;
    private List<Furniture> sinksAndSources;
    private bool recomputeOnNextUpdate;

    /// <summary>
    /// Create and Initialize arrays with default values.
    /// </summary>
    public TemperatureDiffusion()
    {
        RecomputeDiffusion();
        sinksAndSources = new List<Furniture>();

        World.Current.FurnitureManager.Created += OnFurnitureCreated;
        foreach (Furniture furn in World.Current.FurnitureManager)
        {
            if (furn.RoomEnclosure)
            {
                furn.Removed += OnFurnitureRemoved;
            }
        }
    }

    /// <summary>
    /// If needed, progress physics.
    /// </summary>
    public void Update(float deltaTime)
    {
        UpdateTemperature(deltaTime);
    }

    public void RegisterSinkOrSource(Furniture provider)
    {
        if (sinksAndSources.Contains(provider) == false)
        {
            sinksAndSources.Add(provider);
            ////Debug.Log("Registered sources: " + sinksAndSources.Count);
        }
    }

    public void DeregisterSinkOrSource(Furniture provider)
    {
        if (sinksAndSources.Contains(provider))
        {
            sinksAndSources.Remove(provider);
            ////Debug.Log("Registered sources: " + sinksAndSources.Count);
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
        Room room = World.Current.GetTileAt(x, y, z).Room;
        return room == null ? 0 : room.Atmosphere.GetTemperature();
    }

    public float GetTemperatureInC(int x, int y, int z)
    {
        return GetTemperature(x, y, z) - 273.15f;
    }

    public float GetTemperatureInF(int x, int y, int z)
    {
        return (GetTemperature(x, y, z) * 1.8f) - 459.67f;
    }

    /*
    /// <summary>
    /// Sets the temperature at (x,y,z) to a value.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <returns>Temperature to set at x,y,z.</returns>
    public void SetTemperature(Room room, float value)
    {
        thermalEnergy[room] = value * room.TileCount;
    }

    /// <summary>
    /// Changes the temperature at (x,y,z) by an amount.
    /// </summary>
    /// <param name="x">X coordinates.</param>
    /// <param name="y">Y coordinates.</param>
    /// <param name="z">Z coordinates.</param>
    /// <param name="incr">Temperature to increase at x,y, z.</param>
    public void ChangeEnergy(Room room, float energy)
    {
        if (thermalEnergy.ContainsKey(room) == false)
        {
            thermalEnergy[room] = energy;
        }
        else
        {
            thermalEnergy[room] += energy;
        }
    }
    */

    public void Resize()
    {
        RecomputeDiffusion();
        sinksAndSources = new List<Furniture>();
    }

    private void OnFurnitureCreated(Furniture furn)
    {
        if (furn.RoomEnclosure)
        {
            furn.Removed += OnFurnitureRemoved;
            recomputeOnNextUpdate = true;
        }
    }

    private void OnFurnitureRemoved(Furniture furn)
    {
        recomputeOnNextUpdate = true;
    }

    private void RecomputeDiffusion()
    {
        recomputeOnNextUpdate = false;

        InitDiffusionMap();

        for (int x = 0; x < World.Current.Width; x++)
        {
            for (int y = 0; y < World.Current.Height; y++)
            {
                for (int z = 0; z < World.Current.Depth; z++)
                {
                    Tile tile = World.Current.GetTileAt(x, y, z);

                    if (tile.Furniture != null && tile.Furniture.RoomEnclosure)
                    {
                        Tile[] neighbours = tile.GetNeighbours(true, false, true);

                        AddDiffusionFromSource(tile.Furniture, tile.North(), neighbours[5], tile.South(), neighbours[6]);
                        AddDiffusionFromSource(tile.Furniture, tile.East(), neighbours[6], tile.West(), neighbours[7]);
                        AddDiffusionFromSource(tile.Furniture, tile.South(), neighbours[7], tile.North(), neighbours[4]);
                        AddDiffusionFromSource(tile.Furniture, tile.West(), neighbours[4], tile.East(), neighbours[5]);
                    }
                }
            }
        }

        foreach (var r1 in diffusion.Keys)
        {
            foreach (var r2 in diffusion[r1].Keys)
            {
                ////Debug.Log(r1.ID + " -> " + r2.ID + " = " + diffusion[r1][r2]);
            }
        }
    }

    private void InitDiffusionMap()
    {
        diffusion = new Dictionary<Room, Dictionary<Room, float>>();
        foreach (Room room in World.Current.RoomManager)
        {
            diffusion[room] = new Dictionary<Room, float>();
        }
    }

    private void AddDiffusionFromSource(Furniture wall, Tile source, Tile left, Tile middle, Tile right)
    {
        float diffusivity = wall.Parameters["thermal_diffusivity"].ToFloat(0);
        if (AreTilesInDifferentRooms(source, left))
        {
            AddDiffusionFromTo(source.Room, left.Room, (left.Room.IsOutsideRoom() ? 0.01f : 0.25f) * diffusivity);
        }

        if (AreTilesInDifferentRooms(source, middle))
        {
            AddDiffusionFromTo(source.Room, middle.Room, (middle.Room.IsOutsideRoom() ? 0.02f : 0.5f) * diffusivity);
        }

        if (AreTilesInDifferentRooms(source, right))
        {
            AddDiffusionFromTo(source.Room, right.Room, (right.Room.IsOutsideRoom() ? 0.01f : 0.25f) * diffusivity);
        }
    }

    private bool AreTilesInDifferentRooms(Tile t1, Tile t2)
    {
        return t1 != null && t2 != null &&
            t1.Room != null && t2.Room != null &&
            t1.Room.Equals(t2.Room) == false;
    }

    private void AddDiffusionFromTo(Room r1, Room r2, float value)
    {
        if (diffusion[r1].ContainsKey(r2) == false)
        {
            diffusion[r1][r2] = value;
        }
        else
        {
            diffusion[r1][r2] += value;
        }
    }

    private void UpdateTemperature(float deltaTime)
    {
        if (recomputeOnNextUpdate)
        {
            RecomputeDiffusion();
        }

        foreach (var furn in sinksAndSources)
        {
            GenerateHeatFromFurniture(furn, deltaTime);
        }

        foreach (var r1 in diffusion.Keys)
        {
            foreach (var r2 in diffusion[r1].Keys)
            {
                float temperatureDifference = r1.Atmosphere.GetTemperature() - r2.Atmosphere.GetTemperature();
                if (temperatureDifference > 0)
                {
                    float energyTransfer = diffusion[r1][r2] * temperatureDifference * Mathf.Sqrt(r1.GetGasPressure()) * Mathf.Sqrt(r2.GetGasPressure()) * deltaTime;
                    r1.Atmosphere.ChangeEnergy(-energyTransfer);
                    r2.Atmosphere.ChangeEnergy(energyTransfer);
                }
            }
        }
    }

    private void GenerateHeatFromFurniture(Furniture furniture, float deltaTime)
    {
        if (furniture.Tile.Room.IsOutsideRoom() == true)
        {
            return;
        }

        Tile tile = furniture.Tile;
        float pressure = tile.Room.GetGasPressure();
        float efficiency = ModUtils.Clamp01(pressure / furniture.Parameters["pressure_threshold"].ToFloat());
        float energyChangePerSecond = furniture.Parameters["base_heating"].ToFloat() * efficiency;
        float energyChange = energyChangePerSecond * deltaTime;

        UnityDebugger.Debugger.Log("Atmosphere", "Generating heat: " + furniture.Type + " = " + energyChangePerSecond + "(" + pressure + " -> " + efficiency + "%)");

        tile.Room.Atmosphere.ChangeEnergy(energyChange);
    }
}
