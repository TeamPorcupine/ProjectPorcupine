using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

/// <summary>
/// A  Temperature management system. Temperature is stored at each tile and evolved using
/// https://en.wikipedia.org/wiki/Heat_equation
/// </summary>
[MoonSharpUserData]
public class Temperature
{
    /// <summary>
    /// All heaters and refrigerators shoul register an Action here
    /// TODO: find elegant way to modify this through LUA
    /// </summary>
    public Action sinksAndSources;

    /// <summary>
    /// How often doe the physics update
    /// </summary>
    public float updateInterval = 0.01f;

    /// <summary>
    /// Internal only variables
    /// </summary>
    float[][] temperature;
    float[] thermalDiffusivity;

    /// <summary>
    /// Default value assigned to thermalDIffusivity at "empty" tile.
    /// </summary>
    static public float defaultThermalDiffusivity = 1f;

    // Internal stuff
    /// <summary>
    /// Size of map
    /// </summary>
    int xSize, ySize;
    /// <summary>
    /// Time since last update
    /// </summary>
    float elapsed = 0f;
    /// <summary>
    /// We switch between two "states" of temperatrue, because we reuqire a tempoerary array containing the old value
    /// </summary>
    int offset = 0;

    /// <summary>
    /// Create and Initialize arrays with default values.
    /// </summary>
    /// <param name="xS">x size of world</param>
    /// <param name="yS">y size of world</param>
    public Temperature(int xS, int yS)
    {
        xSize = xS;
        ySize = yS;
        temperature = new float[2][]
        {
            new float[xSize*ySize],
            new float[xSize*ySize],
        };
        thermalDiffusivity = new float[xSize * ySize];

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int index = GetIndex(x, y);
                temperature[0][index] = 0f;
                thermalDiffusivity[index] = 1f;
            }
        }

        // TODO: remove, dummy heater at x=50, y=50, with power=1000
        sinksAndSources += () => { SetTemperature(50, 50, 300); };
        
    }

    /// <summary>
    /// If needed, progress physics
    /// </summary>
    public void Update()
    {
        // Progress physical time (should me linked to TIme.dt at some point)
        elapsed += Time.deltaTime;

        if (elapsed >= updateInterval)
        {
            ProgressTemperature();
            //BuildMatrix();
            updateInterval = Mathf.Infinity;
            elapsed = 0;
        }
    }

    /// <summary>
    /// Public interface to temperature model, returns temperature at x,y
    /// </summary>
    /// <param name="x">y coord</param>
    /// <param name="y">y coord</param>
    /// <returns>temperature at x,y</returns>
    public float GetTemperature(int x, int y)
    {
        return temperature[offset][GetIndex(x, y)];
    }

    /// <summary>
    /// Public interface to setting temperature, set temperature at (x,y) to temp
    /// </summary>
    /// <param name="x">y coord</param>
    /// <param name="y">y coord</param>
    /// <param name="temp">temeprature to set at x,y</param>
    public void SetTemperature(int x, int y, float temp)
    {
        temperature[offset][GetIndex(x, y)] = temp;
    }

    /// <summary>
    /// Public interface to thermal diffusivity model. Each tile has a value (say alpha) that
    /// tells  how the heat flows into that tile. Lower value means heat flows much slower (like trough a wall)
    /// while a value of 1 means the temperature "moves" faster. Think of it as a kind of isolation factor.
    /// TODO: walls should set the coefficient to 0.1?
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns>thermal diffusivity alpha at x,y</returns>
    public float GetThermalDiffusivity(int x, int y)
    {
        return thermalDiffusivity[GetIndex(x, y)];
    }

    /// <summary>
    /// Public interface to thermal diffusivity model. Set the value of thermal diffusivity at x,y to coeff
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="coeff">thermal diffusifity to set at x,y</param>
    public void SetThermalDiffusivity(int x, int y, float coeff)
    {
        thermalDiffusivity[GetIndex(x, y)] = coeff;
    }

    /// <summary>
    /// Internal indexing of array
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns>Actual index for array access</returns>
    int GetIndex(int x, int y)
    {
        return y * xSize + x;
    }

    /// <summary>
    /// Evolve the temperature model. Loops over all tiles!
    /// </summary>
    void ProgressTemperature()
    {
        //Debug.Log("Updating temperature!");
        // TODO: Compute temperature sources
        // foreach(Tile tile in sinkAndSources)
        // GetTileAt(55, 55).temp_old = 1000;
        if (sinksAndSources != null) sinksAndSources();

        // Store references
        float[] temp_curr = temperature[1 - offset];
        float[] temp_old = temperature[offset];

        // Compute a constant:
        // delta.Time * magic_coefficient * 0.5 (avg for thermalDiffusivity)
        float C = 0.3f * 0.5f;
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int index = GetIndex(x, y);
                int index_up = GetIndex(x, y+1);
                int index_down = GetIndex(x, y-1);
                int index_left = GetIndex(x-1, y);
                int index_right = GetIndex(x+1, y);

                // Update temperature using finite difference and forward method:
                // U^{n+1} = U^n + dt*(\Div alpha \Grad U^n)

                temp_curr[index] = temp_old[index];

                // TODO: if empty space, set temperature to 0
                if (WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0)).room == null)
                {
                    temp_curr[index] = 0f;
                }
                if (x > 0)
                {
                    temp_curr[index] +=
                        C * (thermalDiffusivity[index] + thermalDiffusivity[index_left]) *
                        (temp_old[index_left] - temp_old[index]);
                }
                if (y > 0)
                {
                    temp_curr[index] +=
                        C * (thermalDiffusivity[index] + thermalDiffusivity[index_down]) *
                        (temp_old[index_down] - temp_old[index]);
                }
                if (x < xSize - 1)
                {
                    temp_curr[index] +=
                        C * (thermalDiffusivity[index] + thermalDiffusivity[index_right]) *
                        (temp_old[index_right] - temp_old[index]);
                }
                if (y < ySize - 1)
                {
                    temp_curr[index] +=
                        C * (thermalDiffusivity[index] + thermalDiffusivity[index_up]) *
                        (temp_old[index_up] - temp_old[index]);
                }
            }
        }
        // Swap variable order
        offset = 1 - offset;

    }

    void BuildMatrix()
    {
        //OfIndexed(int rows, int columns, IEnumerable < Tuple < int, int, double >> enumerable);
        List<MathNet.Numerics.Tuple<int, int, float>> tuples = new List<MathNet.Numerics.Tuple<int, int, float>>();

        float C = 0.3f * 0.5f;
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int index = GetIndex(x, y);
                int index_up = GetIndex(x, y + 1);
                int index_down = GetIndex(x, y - 1);
                int index_left = GetIndex(x - 1, y);
                int index_right = GetIndex(x + 1, y);

                // Update temperature using finite difference and forward method:
                // U^{n+1} = U^n + dt*(\Div alpha \Grad U^n)



                // TODO: if empty space, set temperature to 0
                if (WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0)).room == null)
                {
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, 0f));
                }
                else
                {
                }
                tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, 1f));
                if (x > 0)
                {
                    float D = C * (thermalDiffusivity[index] + thermalDiffusivity[index_left]);
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index_left, D));
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, -D));
                }
                if (y > 0)
                {
                    float D = C * (thermalDiffusivity[index] + thermalDiffusivity[index_down]);
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index_down, D));
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, -D));
                }
                if (x < xSize - 1)
                {
                    float D = C * (thermalDiffusivity[index] + thermalDiffusivity[index_right]);
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index_right, D));
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, -D));
                }
                if (y < ySize - 1)
                {
                    float D = C * (thermalDiffusivity[index] + thermalDiffusivity[index_up]);
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index_up, D));
                    tuples.Add(new MathNet.Numerics.Tuple<int, int, float>(index, index, -D));
                }
            }
        }

        SparseMatrix Mat = SparseMatrix.OfIndexed(xSize * ySize, xSize * ySize, tuples);

        MathNet.Numerics.LinearAlgebra.Single.Solvers.BiCgStab bcg = new MathNet.Numerics.LinearAlgebra.Single.Solvers.BiCgStab();

        Vector<float> v = Vector.Build.DenseOfArray(temperature[0]);
        Vector<float> w = Vector.Build.DenseOfArray(temperature[1]);


        ////MathNet.Numerics.LinearAlgebra.Single.Solvers.ILU0Preconditioner ilu = new MathNet.Numerics.LinearAlgebra.Single.Solvers.ILU0Preconditioner();
        MathNet.Numerics.LinearAlgebra.Single.Solvers.DiagonalPreconditioner ilu = new MathNet.Numerics.LinearAlgebra.Single.Solvers.DiagonalPreconditioner();

        //Mat.Multiply(v);

        MathNet.Numerics.LinearAlgebra.Solvers.Iterator<float> it = new MathNet.Numerics.LinearAlgebra.Solvers.Iterator<float>();
        w = Mat.SolveIterative(v, bcg, it, ilu);
        //bcg.Solve(Mat, v, w, it, ilu);

    }
}
