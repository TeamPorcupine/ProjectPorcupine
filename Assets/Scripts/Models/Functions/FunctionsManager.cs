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
        actions.Add("Utility", new LuaFunctions());
        actions.Add("RoomBehavior", new LuaFunctions());
        actions.Add("Need", new LuaFunctions());
        actions.Add("GameEvent", new LuaFunctions());
        actions.Add("TileType", new LuaFunctions());
        actions.Add("Quest", new LuaFunctions());
        actions.Add("ScheduledEvent", new LuaFunctions());
        actions.Add("Overlay", new LuaFunctions());
    }

    /// <summary>
    /// Gets the furniture Lua Functions.
    /// </summary>
    /// <value>The furniture Lua Functions.</value>
    public static LuaFunctions Furniture
    {
        get { return Get("Furniture"); }
    }

    /// <summary>
    /// Gets the utility Lua Functions.
    /// </summary>
    /// <value>The utility Lua Functions.</value>
    public static LuaFunctions Utility
    {
        get { return Get("Utility"); }
    }

    /// <summary>
    /// Gets the RoomBehavior Lua Functions.
    /// </summary>
    /// <value>The RoomBehavior Lua Functions.</value>
    public static LuaFunctions RoomBehavior
    {
        get { return Get("RoomBehavior"); }
    }

    /// <summary>
    /// Gets the need Lua Functions.
    /// </summary>
    /// <value>The need actions.</value>
    public static LuaFunctions Need
    {
        get { return Get("Need"); }
    }

    /// <summary>
    /// Gets the game event Lua Functions.
    /// </summary>
    /// <value>The game event Lua Functions.</value>
    public static LuaFunctions GameEvent
    {
        get { return Get("GameEvent"); }
    }

    /// <summary>
    /// Gets the tile type Lua Functions.
    /// </summary>
    /// <value>The tile type Lua Functions.</value>
    public static LuaFunctions TileType
    {
        get { return Get("TileType"); }
    }

    /// <summary>
    /// Gets the quest Lua Functions.
    /// </summary>
    /// <value>The quest Lua Functions.</value>
    public static LuaFunctions Quest
    {
        get { return Get("Quest"); }
    }

    /// <summary>
    /// Gets the scheduled event Lua Functions.
    /// </summary>
    /// <value>The scheduled event Lua Functions.</value>
    public static LuaFunctions ScheduledEvent
    {
        get { return Get("ScheduledEvent"); }
    }

    /// <summary>
    /// Gets the overlay Lua Functions.
    /// </summary>
    /// <value>The overlay Lua Functions.</value>
    public static LuaFunctions Overlay
    {
        get { return Get("Overlay"); }
    }

    /// <summary>
    /// Get the Lua Functions for the specified name.
    /// </summary>
    /// <param name="name">The functions key.</param>
    public static LuaFunctions Get(string name)
    {
        if (actions == null)
        {
            return null;
        }

        if (actions.ContainsKey(name))
        {
            return actions[name];
        }

        return null;
    }
}
