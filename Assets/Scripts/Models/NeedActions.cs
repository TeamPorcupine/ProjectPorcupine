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
using UnityEngine;

public class NeedActions
{
    static NeedActions()
    {
        LoadScript();
    }

    public static void LoadScript()
    {
        string luaFilePath = Path.Combine(Application.streamingAssetsPath, "LUA");
        luaFilePath = Path.Combine(luaFilePath, "Need.lua");

        LuaUtilities.LoadScriptFromFile(luaFilePath);
    }

    public static void LoadModsScripts(DirectoryInfo[] mods)
    {
        foreach (DirectoryInfo mod in mods)
        {
            string luaModFile = Path.Combine(mod.FullName, "Need.lua");
            if (File.Exists(luaModFile))
            {
                LuaUtilities.LoadScriptFromFile(luaModFile);
            }
        }
    }

    public static void CallFunctionsWithNeed(string[] functionNames, Need need, float deltaTime)
    {
        foreach (string fn in functionNames)
        {
            DynValue result = LuaUtilities.CallFunction(fn, need, deltaTime);

            if (result.Type == DataType.String)
            {
                Debug.ULogChannel("NeedActions", "Lua response:", result.String);
            }
        }
    }
}
