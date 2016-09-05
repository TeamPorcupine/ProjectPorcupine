#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;

public class ActionsManager
{
    private static Dictionary<string, Actions> actions;

    public ActionsManager()
    {
        actions = new Dictionary<string, Actions>();

        actions.Add("Furniture", new Actions("Furniture.lua"));
        actions.Add("Need", new Actions("Need.lua"));
        actions.Add("GameEvent", new Actions("GameEvent.lua"));
        actions.Add("TileType", new Actions("Tile.lua"));
        actions.Add("Quest", new Actions("Quest.lua"));
        actions.Add("ScheduledEvent", new Actions("ScheduledEvent.lua"));
    }

    /// <summary>
    /// Gets the furniture actions.
    /// </summary>
    /// <value>The furniture actions.</value>
    public static Actions Furniture
    {
        get { return actions["Furniture"]; }
    }

    /// <summary>
    /// Gets the need actions.
    /// </summary>
    /// <value>The need actions.</value>
    public static Actions Need
    {
        get { return actions["Need"]; }
    }

    /// <summary>
    /// Gets the game event actions.
    /// </summary>
    /// <value>The game event actions.</value>
    public static Actions GameEvent
    {
        get { return actions["GameEvent"]; }
    }

    /// <summary>
    /// Gets the tile type actions.
    /// </summary>
    /// <value>The tile type actions.</value>
    public static Actions TileType
    {
        get { return actions["TileType"]; }
    }

    /// <summary>
    /// Gets the quest actions.
    /// </summary>
    /// <value>The quest actions.</value>
    public static Actions Quest
    {
        get { return actions["Quest"]; }
    }

    /// <summary>
    /// Gets the scheduled event actions.
    /// </summary>
    /// <value>The scheduled event actions.</value>
    public static Actions ScheduledEvent
    {
        get { return actions["ScheduledEvent"]; }
    }

    /// <summary>
    /// Get the Actions with the specified name.
    /// </summary>
    /// <param name="name">The actions key.</param>
    public static Actions Get(string name)
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
