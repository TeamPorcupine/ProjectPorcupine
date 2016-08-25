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
    public FurnitureActions()
    {
        // TODO: This should be moved to a more logical location
        LuaUtilities.RegisterGlobal(typeof(Inventory));
        LuaUtilities.RegisterGlobal(typeof(Job));
        LuaUtilities.RegisterGlobal(typeof(ModUtils));
        LuaUtilities.RegisterGlobal(typeof(World));
    }

    public static void CallFunctionsWithFurniture(string[] functionNames, Furniture furn, float deltaTime)
    {
        foreach (string fn in functionNames)
        {
            DynValue result = LuaUtilities.CallFunction(fn, furn, deltaTime);
            
            if (result.Type == DataType.String)
            {
                Debug.Log(result.String);
            }
        }
    }
    
    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        WorldController.Instance.world.PlaceFurniture(theJob.jobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJob = null;
    }
}
