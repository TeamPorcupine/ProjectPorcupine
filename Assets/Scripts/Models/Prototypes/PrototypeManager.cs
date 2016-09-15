#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using Scheduler;

/// <summary>
/// A class that holds the Prototype Maps of each entity that requires it.
/// </summary>
public class PrototypeManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrototypeManager"/> class.
    /// </summary>
    public PrototypeManager()
    {
        Inventory = new PrototypeMap<InventoryCommon>("Inventories", "Inventory");
        FurnitureJob = new PrototypeMap<Job>();
        Furniture = new PrototypeMap<Furniture>("Furnitures", "Furniture");
        Need = new PrototypeMap<Need>("Needs", "Need");
        Trader = new PrototypeMap<TraderPrototype>("Traders", "Trader");
        Quest = new PrototypeMap<Quest>("Quests", "Quest");
        Stat = new PrototypeMap<Stat>("Stats", "Stat");
        SchedulerEvent = new PrototypeMap<ScheduledEvent>("Events", "Event");
    }

    /// <summary>
    /// Gets the furniture job prototype map.
    /// </summary>
    /// <value>The furniture job prototype map.</value>
    public static PrototypeMap<Job> FurnitureJob { get; private set; }

    /// <summary>
    /// Gets the furniture prototype map.
    /// </summary>
    /// <value>The furniture prototype map.</value>
    public static PrototypeMap<Furniture> Furniture { get; private set; }

    /// <summary>
    /// Gets the inventory prototype map.
    /// </summary>
    /// <value>The inventory prototype map.</value>
    public static PrototypeMap<InventoryCommon> Inventory { get; private set; }

    /// <summary>
    /// Gets the need prototype map.
    /// </summary>
    /// <value>The need prototype map.</value>
    public static PrototypeMap<Need> Need { get; private set; }

    /// <summary>
    /// Gets the trader prototype map.
    /// </summary>
    /// <value>The trader prototype map.</value>
    public static PrototypeMap<TraderPrototype> Trader { get; private set; }

    /// <summary>
    /// Gets the quest prototype map.
    /// </summary>
    /// <value>The quest prototype map.</value>
    public static PrototypeMap<Quest> Quest { get; private set; }

    /// <summary>
    /// Gets the stat prototype map.
    /// </summary>
    /// <value>The stat prototype map.</value>
    public static PrototypeMap<Stat> Stat { get; private set; }

    /// <summary>
    /// Gets the scheduler event prototype map.
    /// </summary>
    /// <value>The scheduler event prototype map.</value>
    public static PrototypeMap<ScheduledEvent> SchedulerEvent { get; private set; }
}
