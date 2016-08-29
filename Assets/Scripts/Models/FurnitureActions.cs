#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.IO;
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
        LuaUtilities.RegisterGlobal(typeof(Power.Connection));

        LoadScripts();
    }

    public static void LoadScripts()
    {
        string luaFilePath = Path.Combine(Application.streamingAssetsPath, "LUA");
        luaFilePath = Path.Combine(luaFilePath, "Furniture.lua");
        LuaUtilities.LoadScriptFromFile(luaFilePath);
    }

    public static void LoadModsScripts(DirectoryInfo[] mods)
    {
        foreach (DirectoryInfo mod in mods)
        {
            string luaModFile = Path.Combine(mod.FullName, "Furniture.lua");
            if (File.Exists(luaModFile))
            {
                LuaUtilities.LoadScriptFromFile(luaModFile);
            }
        }
    }

    public static void CallFunctionsWithFurniture<T>(string[] functionNames, T furn, float deltaTime)
    {
        if (furn == null)
        {
            // These errors are about the lua code so putting themin the Lua channel.
            Debug.ULogErrorChannel("Lua", "Furn is null, cannot call LUA function (something is fishy).");
        }

        foreach (string fn in functionNames)
        {
            if (fn == null)
            {
                Debug.ULogErrorChannel("Lua", "'" + fn + "' is not a LUA function.");
                return;
            }
            
            DynValue result = LuaUtilities.CallFunction(fn, furn, deltaTime);
            
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
