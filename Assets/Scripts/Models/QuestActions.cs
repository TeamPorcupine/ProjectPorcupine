using MoonSharp.Interpreter;

public class QuestActions
{
    private static QuestActions _Instance;

    private Script myLuaScript;

    public QuestActions(string rawLuaCode)
    {
        // Tell the LUA interpreter system to load all the classes
        // that we have marked as [MoonSharpUserData]
        UserData.RegisterAssembly();

        _Instance = this;

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
        object func = _Instance.myLuaScript.Globals[functionName];

        return _Instance.myLuaScript.Call(func, args);
    }
}