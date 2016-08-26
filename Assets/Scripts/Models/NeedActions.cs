#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using MoonSharp.Interpreter;

public class NeedActions
{
    private static NeedActions _Instance;

    private Script myLuaScript;

    public static NeedActions Instance
    {
        get { return _Instance ?? (_Instance = new NeedActions()); }
    }

    private NeedActions()
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        myLuaScript = new Script();

        // If we want to be able to instantiate a new object of a class
        //   i.e. by doing    SomeClass.__new()
        // We need to make the base type visible.
        myLuaScript.Globals["Inventory"] = typeof(Inventory);
        myLuaScript.Globals["Job"] = typeof(Job);
        myLuaScript.Globals["ModUtils"] = typeof(ModUtils);
        // Also to access statics/globals
        myLuaScript.Globals["World"] = typeof(World);
    }

    public static void AddScript(string rawLuaCode)
    {
        Instance.myLuaScript.DoString(rawLuaCode);
    }
    
    public static void CallFunctionsWithNeed(string[] functionNames, Need need, float deltaTime)
    {
        foreach (string fn in functionNames)
        {
            object func = Instance.myLuaScript.Globals[fn];

            if (func == null)
            {
                Debug.LogError("'" + fn + "' is not a LUA function.");
                return;
            }

            DynValue result = Instance.myLuaScript.Call(func, need, deltaTime);

            if (result.Type == DataType.String)
            {
                Debug.Log(result.String);
            }
        }
    }

    public static DynValue CallFunction(string functionName, params object[] args)
    {
        object func = Instance.myLuaScript.Globals[functionName];

        return Instance.myLuaScript.Call(func, args);
    }
    public static void RegisterGlobal(System.Type type)
    {
        Instance.myLuaScript.Globals[type.Name] = type;
    }
}
