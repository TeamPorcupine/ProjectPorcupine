#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;

public class FunctionsManager
{
    private static Dictionary<string, LuaFunctions> actions;

    public FunctionsManager()
    {
        actions = new Dictionary<string, LuaFunctions>();

        actions.Add("Furniture", new LuaFunctions());
        actions.Add("Need", new LuaFunctions());
        actions.Add("GameEvent", new LuaFunctions());
        actions.Add("TileType", new LuaFunctions());
        actions.Add("Quest", new LuaFunctions());
        actions.Add("ScheduledEvent", new LuaFunctions());
    }

    /// <summary>
    /// Gets the furniture Lua Functions.
    /// </summary>
    /// <value>The furniture Lua Functions.</value>
    public static LuaFunctions Furniture
    {
        get { return actions["Furniture"]; }
    }

    /// <summary>
    /// Gets the need Lua Functions.
    /// </summary>
    /// <value>The need actions.</value>
    public static LuaFunctions Need
    {
        get { return actions["Need"]; }
    }

    /// <summary>
    /// Gets the game event Lua Functions.
    /// </summary>
    /// <value>The game event Lua Functions.</value>
    public static LuaFunctions GameEvent
    {
        get { return actions["GameEvent"]; }
    }

    /// <summary>
    /// Gets the tile type Lua Functions.
    /// </summary>
    /// <value>The tile type Lua Functions.</value>
    public static LuaFunctions TileType
    {
        get { return actions["TileType"]; }
    }

    /// <summary>
    /// Gets the quest Lua Functions.
    /// </summary>
    /// <value>The quest Lua Functions.</value>
    public static LuaFunctions Quest
    {
        get { return actions["Quest"]; }
    }

    /// <summary>
    /// Gets the scheduled event Lua Functions.
    /// </summary>
    /// <value>The scheduled event Lua Functions.</value>
    public static LuaFunctions ScheduledEvent
    {
        get { return actions["ScheduledEvent"]; }
    }

    /// <summary>
    /// Get the Lua Functions for the specified name.
    /// </summary>
    /// <param name="name">The functions key.</param>
    public static LuaFunctions Get(string name)
    {
        if (actions.ContainsKey(name))
        {
            return actions[name];
        }

        return null;
    }

    // TODO: Move this function to a better place
    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        WorldController.Instance.World.PlaceFurniture(theJob.JobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.PendingBuildJob = null;
    }
}
