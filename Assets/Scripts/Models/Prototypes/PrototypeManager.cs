#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ProjectPorcupine.Rooms;
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
        Inventory = new PrototypeMap<Inventory>("Inventories", "Inventory");
        TileType = new PrototypeMap<TileType>("Tiles", "Tile");
        Furniture = new PrototypeMap<Furniture>("Furnitures", "Furniture");
        Utility = new PrototypeMap<Utility>("Utilities", "Utility");
        RoomBehavior = new PrototypeMap<RoomBehavior>("RoomBehaviors", "RoomBehavior");
        Need = new PrototypeMap<Need>("Needs", "Need");
        Trader = new PrototypeMap<TraderPrototype>("Traders", "Trader");
        Currency = new PrototypeMap<Currency>("Currencies", "Currency");
        Quest = new PrototypeMap<Quest>("Quests", "Quest");
        Stat = new PrototypeMap<Stat>("Stats", "Stat");
        GameEvent = new PrototypeMap<GameEvent>("GameEvents", "GameEvent");
        ScheduledEvent = new PrototypeMap<ScheduledEvent>("ScheduledEvents", "ScheduledEvent");
        Headline = new PrototypeMap<Headline>("Headlines", "Headline");
        Overlay = new PrototypeMap<OverlayDescriptor>("Overlays", "Overlay");
        Ship = new PrototypeMap<Ship>("Ships", "Ship");
        DevConsole = new PrototypeMap<DeveloperConsole.CommandPrototype>("ConsoleCommands", "ConsoleCommand");
        SettingsCategories = new PrototypeMap<SettingsCategory>("Categories", "Category");
        PerformanceHUD = new PrototypeMap<PerformanceGroupReader>("ComponentGroups", "ComponentGroup");
    }

    /// <summary>
    /// Gets the tile type prototype map.
    /// </summary>
    /// <value>The furniture prototype map.</value>
    public static PrototypeMap<TileType> TileType { get; private set; }

    /// <summary>
    /// Gets the furniture prototype map.
    /// </summary>
    /// <value>The furniture prototype map.</value>
    public static PrototypeMap<Furniture> Furniture { get; private set; }

    /// <summary>
    /// Gets the utility prototype map.
    /// </summary>
    /// <value>The utility prototype map.</value>
    public static PrototypeMap<Utility> Utility { get; private set; }

    /// Gets the roomBehavior prototype map.
    /// </summary>
    /// <value>The roomBehavior prototype map.</value>
    public static PrototypeMap<RoomBehavior> RoomBehavior { get; private set; }

    /// <summary>
    /// Gets the inventory prototype map.
    /// </summary>
    /// <value>The inventory prototype map.</value>
    public static PrototypeMap<Inventory> Inventory { get; private set; }

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
    /// Gets the currency prototype map.
    /// </summary>
    /// <value>The currency prototype map.</value>
    public static PrototypeMap<Currency> Currency { get; private set; }

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
    /// Gets the game event prototype map.
    /// </summary>
    /// <value>The game event prototype map.</value>
    public static PrototypeMap<GameEvent> GameEvent { get; private set; }

    /// <summary>
    /// Gets the scheduled event prototype map.
    /// </summary>
    /// <value>The scheduled event prototype map.</value>
    public static PrototypeMap<ScheduledEvent> ScheduledEvent { get; private set; }

    /// <summary>
    /// Gets the headline prototype map.
    /// </summary>
    /// <value>The headline prototype map.</value>
    public static PrototypeMap<Headline> Headline { get; private set; }

    /// <summary>
    /// Gets the overlay prototype map.
    /// </summary>
    /// <value>The overlay prototype map.</value>
    public static PrototypeMap<OverlayDescriptor> Overlay { get; private set; }

    /// <summary>
    /// Gets the DevConsole prototype map.
    /// </summary>
    /// <value>The DevConsole prototype map.</value>
    public static PrototypeMap<DeveloperConsole.CommandPrototype> DevConsole { get; private set; }

    /// <summary>
    /// Gets the SettingsCategory prototype map.
    /// </summary>
    /// <value>The SettingsCategory prototype map.</value>
    public static PrototypeMap<SettingsCategory> SettingsCategories { get; private set; }

    /// <summary>
    /// Gets the PerformanceHUD prototype map.
    /// </summary>
    /// <value>The PerformanceHUD prototype map.</value>
    public static PrototypeMap<PerformanceGroupReader> PerformanceHUD { get; private set; }

    /// <summary>
    /// Gets the ship prototype map.
    /// </summary>
    /// <value>The ship prototype map.</value>
    public static PrototypeMap<Ship> Ship { get; private set; }
}
