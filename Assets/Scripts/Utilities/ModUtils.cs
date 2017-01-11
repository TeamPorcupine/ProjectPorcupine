#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using MoonSharp.Interpreter;
using ProjectPorcupine.Buildable.Components;
using ProjectPorcupine.PowerNetwork;
using UnityEngine;

[MoonSharpUserData]
public static class ModUtils
{
    private static string defaultLogChannel = "ModUtility";

    public static Vector2 LUAVector2(float x, float y)
    {
        return new Vector2(x, y);
    }

    public static Vector3 LUAVector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    public static Vector3 LUAVector4(float x, float y, float z, float w)
    {
        return new Vector4(x, y, z, w);
    }

    /// <summary>
    /// Returns true if the Tile Exists at that point.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool GetTileAt(Vector3 pos, out Tile t)
    {
        // Get reference to world
        World world;
        if (GetCurrentWorld(out world))
        {
            t = world.GetTileAt((int)pos.x, (int)pos.y, (int)pos.z);
            return t != null;
        }
        else
        {
            t = null;
            return false;
        }
    }

    /// <summary>
    /// Returns true if World.Current is real.
    /// </summary>
    public static bool GetCurrentWorld(out World world)
    {
        world = World.Current;

        return world != null;
    }

    public static float Clamp01(float value)
    {
        return Mathf.Clamp01(value);
    }

    public static int FloorToInt(float value)
    {
        return Mathf.FloorToInt(value);
    }

    public static float Round(float value, int digits)
    {
        return (float)System.Math.Round((double)value, digits);
    }

    public static void Log(object obj)
    {
        UnityDebugger.Debugger.Log(defaultLogChannel, obj);
    }

    public static void LogWarning(object obj)
    {
        UnityDebugger.Debugger.LogWarning(defaultLogChannel, obj);
    }

    public static void LogError(object obj)
    {
        UnityDebugger.Debugger.LogError(defaultLogChannel, obj);
    }

    public static void ULogChannel(string channel, string message)
    {
        UnityDebugger.Debugger.Log(channel, message);
    }

    public static void ULogWarningChannel(string channel, string message)
    {
        UnityDebugger.Debugger.LogWarning(channel, message);
    }

    public static void ULogErrorChannel(string channel, string message)
    {
        UnityDebugger.Debugger.LogError(channel, message);
    }

    public static void ULog(string message)
    {
        UnityDebugger.Debugger.Log(defaultLogChannel, message);
    }

    public static void ULogWarning(string message)
    {
        UnityDebugger.Debugger.LogWarning(defaultLogChannel, message);
    }

    public static void ULogError(string message)
    {
        UnityDebugger.Debugger.LogError(defaultLogChannel, message);
    }

    public static float Clamp(float value, float min, float max)
    {
        return value.Clamp(min, max);
    }

    public static int Min(int a, int b)
    {
        return Mathf.Min(a, b);
    }

    public static int Max(int a, int b)
    {
        return Mathf.Max(a, b);
    }

    public static IPluggable GetPlugablePowerConnectionForTile(Tile tile)
    {
        if (tile != null && tile.Furniture != null)
        {
            return tile.Furniture.GetComponent<PowerConnection>("PowerConnection");
        }

        return null;
    }
}
