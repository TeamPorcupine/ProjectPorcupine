#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.RemoteDebugger;
using MoonSharp.RemoteDebugger.Network;
using UnityEngine;

public class FurnitureActions
{
    private static FurnitureActions _Instance;

    private Script myLuaScript;

    public FurnitureActions()
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        _Instance = this;

        myLuaScript = new Script();

        // If we want to be able to instantiate a new object of a class
        //   i.e. by doing    SomeClass.__new()
        // We need to make the base type visible.
        myLuaScript.Globals["Inventory"] = typeof(Inventory);
        myLuaScript.Globals["Job"] = typeof(Job);

        // Also to access statics/globals
        myLuaScript.Globals["World"] = typeof(World);
    }

    public static void addScript(string rawLuaCode)
    {
        _Instance.myLuaScript.DoString(rawLuaCode);
    }

    public static void CallFunctionsWithFurniture(string[] functionNames, Furniture furn, float deltaTime)
    {
        foreach (string fn in functionNames)
        {
            object func = _Instance.myLuaScript.Globals[fn];

            if (func == null)
            {
                Logger.LogError("'" + fn + "' is not a LUA function.");
                return;
            }

            DynValue result = _Instance.myLuaScript.Call(func, furn, deltaTime);

            if (result.Type == DataType.String)
            {
                Logger.Log(result.String);
            }
        }
    }

    public static DynValue CallFunction(string functionName, params object[] args)
    {
        object func = _Instance.myLuaScript.Globals[functionName];

        return _Instance.myLuaScript.Call(func, args);
    }

    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        WorldController.Instance.world.PlaceFurniture(theJob.jobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.pendingBuildJob = null;
    }
}
