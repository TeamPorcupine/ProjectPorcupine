#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public static class ModUtils
{
    public static float Clamp01(float value) 
    {
        return Mathf.Clamp01(value); 
    }

    public static void Log(object obj) 
    {
        Debug.Log(obj);
    }

    public static void LogWarning(object obj) 
    {
        Debug.LogWarning(obj);
    }

    public static void LogError(object obj) 
    {
        Debug.LogError(obj);
    }
}
