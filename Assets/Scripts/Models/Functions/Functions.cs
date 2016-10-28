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

public class Functions
{
    private const string ModFunctionsLogChannel = "ModScript";
        
    public Functions()
    {
        FunctionsSets = new HashSet<IFunctions>();
    }

    public enum Type
    {
        Lua,
        CSharp
    }
    
    public HashSet<IFunctions> FunctionsSets { get; private set; }

    public bool HasFunction(string name)
    {
        return GetFunctions(name) != null;
    }

    public bool LoadScript(string text, string scriptName, Type type)
    {
        bool result = false;
        if (type == Type.Lua)
        {
            LuaFunctions luaFunctions = new LuaFunctions();

            if (luaFunctions.LoadScript(text, scriptName))
            {
                FunctionsSets.Add(luaFunctions);
            }
        }
        else
        {
            CSharpFunctions netFunctions = new CSharpFunctions();
            if (netFunctions.LoadScript(text, scriptName))
            {
                FunctionsSets.Add(netFunctions);
            }
        }

        return result;
    }

    public DynValue Call(string functionName, params object[] args)
    {
        IFunctions functions = GetFunctions(functionName);
        if (functions != null)
        {
            return functions.Call(functionName, args);
        }
        else
        {
            Debug.ULogChannel(ModFunctionsLogChannel, "'" + functionName + "' is not a LUA nor CSharp function!");
            return null;
        }
    }

    public T Call<T>(string functionName, params object[] args)
    {
        IFunctions functions = GetFunctions(functionName);
        if (functions != null)
        {
            return functions.Call<T>(functionName, args);
        }
        else
        {
            Debug.ULogChannel(ModFunctionsLogChannel, "'" + functionName + "' is not a LUA nor CSharp function!");
            return default(T);
        }
    }

    public void CallWithInstance(string[] functionNames, object instance, params object[] parameters)
    {
        foreach (string fn in functionNames)
        {
            if (fn == null)
            {
                Debug.ULogErrorChannel(ModFunctionsLogChannel, "'" + fn + "'  is not a LUA nor CSharp function!");
                return;
            }

            DynValue result;
            object[] instanceAndParams = new object[parameters.Length + 1];
            instanceAndParams[0] = instance;
            parameters.CopyTo(instanceAndParams, 1);

            result = Call(fn, instanceAndParams);

            if (result != null && result.Type == DataType.String)
            {
                Debug.ULogErrorChannel(ModFunctionsLogChannel, result.String);
            }
        }
    }

    public void RegisterType(System.Type type)
    {
        foreach (IFunctions functionsSet in FunctionsSets)
        {
            functionsSet.RegisterType(type);
        }
    }

    private IFunctions GetFunctions(string name)
    {
        foreach (IFunctions functionsSet in FunctionsSets)
        {
            if (functionsSet.HasFunction(name))
            {
                return functionsSet;
            }
        }

        return null;
    }
}