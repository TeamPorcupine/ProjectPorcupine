#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.IO;
using MoonSharp.Interpreter;
using UnityEngine;

public class LuaUtilities 
{
    private static Script luaScript;

    static LuaUtilities()
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        luaScript = new Script();
    }

    public static DynValue CallFunction(string functionName, params object[] args)
    {
        object func = luaScript.Globals[functionName];

        if (func == null)
        {
            Debug.ULogChannel("Lua", "'" + functionName + "' is not a LUA function!");
            return null;
        }

        try
        {
            return luaScript.Call(func, args);
        }
        catch (ScriptRuntimeException e)
        {
            Debug.ULogErrorChannel("Lua", e.DecoratedMessage);
            return null;
        }
    }

    public static void LoadScriptFromFile(string filePath)
    {
        string luaCode = System.IO.File.ReadAllText(filePath);

        try
        {
        LuaUtilities.LoadScript(luaCode);
        }
        catch (MoonSharp.Interpreter.SyntaxErrorException e)
        {
            Debug.ULogErrorChannel("Lua", "[" + Path.GetFileName(filePath) + "] LUA Parse error: " + e.DecoratedMessage);
        }
    }

    // If we want to be able to instantiate a new object of a class
    //   i.e. by doing    SomeClass.__new()
    // We need to make the base type visible.
    public static void RegisterGlobal(System.Type type)
    {
        luaScript.Globals[type.Name] = type;
    }

    private static void LoadScript(string script)
    {
        luaScript.DoString(script);
    }
}
