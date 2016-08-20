﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Temperature
{

    public Action sinksAndSources;

    public float updateInterval = 1f;

    // 
    float[][] _temperature;
    float[] _thermalDiffusivity;

    // Internal stuff
    int xSize, ySize;
    float elapsed = 0f;
    int offset = 0;

    public Temperature(int xS, int yS)
    {
        xSize = xS;
        ySize = yS;
        _temperature = new float[2][]
        {
            new float[xSize*ySize],
            new float[xSize*ySize],
        };
        _thermalDiffusivity = new float[xSize * ySize];

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int index = GetIndex(x, y);
                _temperature[0][index] = 0f;
                _thermalDiffusivity[index] = 1f;
            }
        }

        sinksAndSources += () => { SetTemperature(50, 50, 1000); };
    }

    public void Update()
    {
        // Progress physical time (should me linked to TIme.dt at some point)
        elapsed += Time.deltaTime;

        if (elapsed >= updateInterval)
        {
            ProgressTemperature();
            elapsed = 0;
        }
    }

    public float GetTemperature(int x, int y)
    {
        return _temperature[offset][GetIndex(x, y)];
    }

    public void SetTemperature(int x, int y, float temp)
    {
        _temperature[offset][GetIndex(x, y)] = temp;
    }

    public float GetThermalDiffusivity(int x, int y)
    {
        return _thermalDiffusivity[GetIndex(x, y)];
    }

    public void SetThermalDiffusivity(int x, int y, float coeff)
    {
        _thermalDiffusivity[GetIndex(x, y)] = coeff;
    }

    int GetIndex(int x, int y)
    {
        return y * xSize + x;
    }

    void ProgressTemperature()
    {
        Debug.Log("Updating temperature!");
        // TODO: Compute temperature sources
        // foreach(Tile tile in sinkAndSources)
        // GetTileAt(55, 55).temp_old = 1000;
        if (sinksAndSources != null) sinksAndSources();

        float[] temp_curr = _temperature[1 - offset];
        float[] temp_old = _temperature[offset];

        // delta.Time * magic_coefficient * 0.5 (avg for thermalDiffusivity)
        float C = 0.1f * 0.5f;
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int index = GetIndex(x, y);
                int index_up = GetIndex(x, y+1);
                int index_down = GetIndex(x, y-1);
                int index_left = GetIndex(x-1, y);
                int index_right = GetIndex(x+1, y);

                temp_curr[index] = temp_old[index];

                // TODO: if empty space, set temperature to 0
                //if (WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0)).room == null)
                //{
                //    temp_curr[index] = 0f;
                //}
                if (x > 0)
                {
                    temp_curr[index] +=
                        C * (_thermalDiffusivity[index] + _thermalDiffusivity[index_left]) *
                        (temp_old[index_left] - temp_old[index]);
                }
                if (y > 0)
                {
                    temp_curr[index] +=
                        C * (_thermalDiffusivity[index] + _thermalDiffusivity[index_down]) *
                        (temp_old[index_down] - temp_old[index]);
                }
                if (x < xSize - 1)
                {
                    temp_curr[index] +=
                        C * (_thermalDiffusivity[index] + _thermalDiffusivity[index_right]) *
                        (temp_old[index_right] - temp_old[index]);
                }
                if (y < ySize - 1)
                {
                    temp_curr[index] +=
                        C * (_thermalDiffusivity[index] + _thermalDiffusivity[index_up]) *
                        (temp_old[index_up] - temp_old[index]);
                }
            }
        }
        // Swap variable order
        offset = 1 - offset;

    }


}
