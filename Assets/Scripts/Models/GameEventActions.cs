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

public class GameEventActions
{
    private static GameEventActions instance;

    private Script myLuaScript;

    public GameEventActions(string luaFile)
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        LuaUtilities.LoadScriptFromFile(luaFile);
    }

    public static void CallFunctionsWithEvent(string[] functionNames, GameEvent gameEvent)
    {
        foreach (string fn in functionNames)
        {
            DynValue result = LuaUtilities.CallFunction(fn, gameEvent);
            
            if (result.Type == DataType.String)
            {
                Debug.ULogErrorChannel("Lua", result.String);
            }
        }
    }

    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        WorldController.Instance.World.PlaceFurniture(theJob.JobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJob = null;
    }
}
