#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using MoonSharp.Interpreter;

public class QuestActions
{
    private static QuestActions instance;

    private Script myLuaScript;

    public QuestActions(string rawLuaCode)
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        instance = this;

        myLuaScript = new Script();

        // If we want to be able to instantiate a new object of a class
        //   i.e. by doing    SomeClass.__new()
        // We need to make the base type visible.
        myLuaScript.Globals["Inventory"] = typeof(Inventory);
        myLuaScript.Globals["Quest"] = typeof(Quest);
        myLuaScript.Globals["QuestGoal"] = typeof(QuestGoal);
        myLuaScript.Globals["QuestReward"] = typeof(QuestReward);
        myLuaScript.Globals["ModUtils"] = typeof(ModUtils);

        // Also to access statics/globals
        myLuaScript.Globals["World"] = typeof(World);

        myLuaScript.DoString(rawLuaCode);
    }

    public static DynValue CallFunction(string functionName, params object[] args)
    {
        object func = instance.myLuaScript.Globals[functionName];

        return instance.myLuaScript.Call(func, args);
    }
}