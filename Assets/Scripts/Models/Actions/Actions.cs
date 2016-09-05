#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using MoonSharp.Interpreter;
using UnityEngine;

public class Actions<T>
{
    protected string fileName;
    protected Script script;

    public Actions(string fileName)
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        this.fileName = fileName;
        this.script = new Script();

        // If we want to be able to instantiate a new object of a class
        //   i.e. by doing    SomeClass.__new()
        // We need to make the base type visible.
        RegisterGlobal(typeof(Inventory));
        RegisterGlobal(typeof(Job));
        RegisterGlobal(typeof(ModUtils));
        RegisterGlobal(typeof(World));
        RegisterGlobal(typeof(WorldController));
        RegisterGlobal(typeof(Power.Connection));
    }

    /// <summary>
    /// Loads the base and the mods scripts.
    /// </summary>
    /// <param name="mods">The mods directories.</param>
    public void LoadScripts(DirectoryInfo[] mods)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = Path.Combine(filePath, fileName);
        if (File.Exists(filePath))
        {
            string text = File.ReadAllText(filePath);
            LoadScriptFromText(text);
        }

        foreach (DirectoryInfo mod in mods)
        {
            filePath = Path.Combine(mod.FullName, fileName);
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                LoadScriptFromText(text);
            }
        }
    }

    /// <summary>
    /// Loads the script from the specified text.
    /// </summary>
    /// <param name="text">The code text.</param>
    public void LoadScriptFromText(string text)
    {
        try
        {
            script.DoString(text);
        }
        catch (MoonSharp.Interpreter.SyntaxErrorException e)
        {
            Debug.ULogErrorChannel("Lua", "[" + fileName + "] LUA Parse error: " + e.DecoratedMessage);
        }
    }

    /// <summary>
    /// Registers a class as a global entity to use it inside of lua.
    /// </summary>
    /// <param name="type">Class typeof.</param>
    public void RegisterGlobal(Type type)
    {
        script.Globals[type.Name] = type;
    }

    /// <summary>
    /// Call the specified lua function with the specified args.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public DynValue Call(string functionName, params object[] args)
    {
        object func = script.Globals[functionName];

        if (func == null)
        {
            Debug.ULogChannel("Lua", "'" + functionName + "' is not a LUA function!");
            return null;
        }

        try
        {
            return script.Call(func, args);
        }
        catch (ScriptRuntimeException e)
        {
            Debug.ULogErrorChannel("Lua", e.DecoratedMessage);
            return null;
        }
    }

    /// <summary>
    /// Calls the specified lua functions with the specified instance.
    /// </summary>
    /// <param name="functionNames">Function names.</param>
    /// <param name="instance">An instance of the actions type.</param>
    /// <param name="deltaTime">Delta time.</param>
    public void CallWithInstance(string[] functionNames, T instance, float deltaTime = 0f)
    {
        if (instance == null)
        {
            // These errors are about the lua code so putting themin the Lua channel.
            Debug.ULogErrorChannel("Lua", "Instance is null, cannot call LUA function (something is fishy).");
        }

        foreach (string fn in functionNames)
        {
            if (fn == null)
            {
                Debug.ULogErrorChannel("Lua", "'" + fn + "' is not a LUA function.");
                return;
            }

            DynValue result;
            if (deltaTime != 0f)
            {
                result = Call(fn, instance, deltaTime);
            }
            else
            {
                result = Call(fn, instance);
            }

            if (result.Type == DataType.String)
            {
                Debug.ULogErrorChannel("Lua", result.String);
            }
        }
    }
}
