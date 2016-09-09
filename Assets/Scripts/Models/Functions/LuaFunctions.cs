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
using ProjectPorcupine.PowerNetwork;

public class LuaFunctions
{
    protected Script script;

    public LuaFunctions()
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        this.script = new Script();

        // If we want to be able to instantiate a new object of a class
        //   i.e. by doing    SomeClass.__new()
        // We need to make the base type visible.
        RegisterGlobal(typeof(Inventory));
        RegisterGlobal(typeof(Job));
        RegisterGlobal(typeof(ModUtils));
        RegisterGlobal(typeof(World));
        RegisterGlobal(typeof(WorldController));
        RegisterGlobal(typeof(Connection));
        RegisterGlobal(typeof(Scheduler.Scheduler));
        RegisterGlobal(typeof(Scheduler.ScheduledEvent));
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
    /// Loads the script from the specified text.
    /// </summary>
    /// <param name="text">The code text.</param>
    /// <param name="scriptName">The script name.</param>
    public bool LoadScript(string text, string scriptName)
    {
        try
        {
            script.DoString(text);
        }
        catch (SyntaxErrorException e)
        {
            Debug.ULogErrorChannel("Lua", "[" + scriptName + "] LUA Parse error: " + e.DecoratedMessage);
            return false;
        }

        return true;
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
    public void CallWithInstance(string[] functionNames, object instance, float deltaTime = 0f)
    {
        if (instance == null)
        {
            // These errors are about the lua code so putting them in the Lua channel.
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
