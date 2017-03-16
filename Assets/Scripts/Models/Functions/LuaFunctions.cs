#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using MoonSharp.Interpreter;
using ProjectPorcupine.PowerNetwork;

public class LuaFunctions : IFunctions
{
    protected Script script;
    private string scriptName;

    public LuaFunctions()
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        this.script = new Script();

        // Registering types
        UserData.RegisterType<UnityEngine.Vector3>();
        UserData.RegisterType<UnityEngine.Vector2>();
        UserData.RegisterType<UnityEngine.Vector4>();
        UserData.RegisterType<UnityEngine.UI.Text>();

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
        RegisterGlobal(typeof(ProjectPorcupine.Jobs.RequestedItem));
        RegisterGlobal(typeof(DeveloperConsole.DevConsole));
        RegisterGlobal(typeof(Settings));
    }

    public bool HasFunction(string name)
    {
        return name != null && script.Globals[name] != null;
    }

    /// <summary>
    /// Loads the script from the specified text.
    /// </summary>
    /// <param name="text">The code text.</param>
    /// <param name="scriptName">The script name.</param>
    public bool LoadScript(string text, string scriptName)
    {
        this.scriptName = scriptName;
        try
        {
            script.DoString(text);
        }
        catch (SyntaxErrorException e)
        {
            UnityDebugger.Debugger.LogError("Lua", "[" + scriptName + "] LUA Parse error: " + e.DecoratedMessage);
            return false;
        }

        return true;
    }

    public DynValue CallWithError(string functionName, params object[] args)
    {
        return Call(functionName, true, args);
    }

    public DynValue Call(string functionName, params object[] args)
    {
        return Call(functionName, false, args);
    }

    public T Call<T>(string functionName, params object[] args)
    {
        return Call(functionName, args).ToObject<T>();
    }

    /// <summary>
    /// Calls the specified lua functions with the specified instance.
    /// </summary>
    /// <param name="functionNames">Function names.</param>
    /// <param name="instance">An instance of the actions type.</param>
    /// <param name="deltaTime">Delta time.</param>
    public void CallWithInstance(string[] functionNames, object instance, params object[] parameters)
    {
        if (instance == null)
        {
            // These errors are about the lua code so putting them in the Lua channel.
            UnityDebugger.Debugger.LogError("Lua", "Instance is null, cannot call LUA function (something is fishy).");
        }

        foreach (string fn in functionNames)
        {
            if (fn == null)
            {
                UnityDebugger.Debugger.LogError("Lua", "'" + fn + "' is not a LUA function.");
                return;
            }

            object[] instanceAndParams = new object[parameters.Length + 1];
            instanceAndParams[0] = instance;
            parameters.CopyTo(instanceAndParams, 1);

            try
            {
                Call(fn, instanceAndParams);
            }
            catch (ScriptRuntimeException e)
            {
                UnityDebugger.Debugger.LogError("Lua", "[" + scriptName + "] LUA RunTime error: " + e.DecoratedMessage);
            }
        }
    }

    public void RegisterType(Type type)
    {
        RegisterGlobal(type);
    }

    /// <summary>
    /// Call the specified lua function with the specified args.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    private DynValue Call(string functionName, bool throwError, params object[] args)
    {
        object func = script.Globals[functionName];

        try
        {
            return script.Call(func, args);
        }
        catch (ScriptRuntimeException e)
        {
            UnityDebugger.Debugger.LogError("Lua", "[" + scriptName + "] LUA RunTime error: " + e.DecoratedMessage);
            return null;
        }
    }

    /// <summary>
    /// Registers a class as a global entity to use it inside of lua.
    /// </summary>
    /// <param name="type">Class typeof.</param>
    private void RegisterGlobal(Type type)
    {
        script.Globals[type.Name] = type;
    }
}
