#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using Scheduler;

public class ActionsManager
{
    public ActionsManager()
    {
        Furniture = new Actions<Furniture>("Furniture.lua");
        Need = new Actions<Need>("Need.lua");
        GameEvent = new Actions<GameEvent>("GameEvent.lua");
        TileType = new Actions<TileType>("Tile.lua");
        Quest = new Actions<Quest>("Quest.lua");
        ScheduledEvent = new Actions<ScheduledEvent>("ScheduledEvent.lua");
    }

    public static Actions<Furniture> Furniture
    {
        get;
        protected set;
    }

    public static Actions<Need> Need
    {
        get;
        protected set;
    }

    public static Actions<GameEvent> GameEvent
    {
        get;
        protected set;
    }

    public static Actions<TileType> TileType
    {
        get;
        protected set;
    }

    public static Actions<Quest> Quest
    {
        get;
        protected set;
    }

    public static Actions<ScheduledEvent> ScheduledEvent
    {
        get;
        protected set;
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
