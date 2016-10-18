#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

/// <summary>
/// All the groups/modes for the PerformanceHUD
/// </summary>
public static class PerformanceComponentGroups
{
    /// <summary>
    /// All groups
    /// </summary>
    public static PerformanceComponentGroup[] groups;

    //NULL FORM
    /// <summary>
    /// No HUD
    /// </summary>
    public static readonly PerformanceComponentGroup none = new PerformanceComponentGroup(new BasePerformanceComponent[] { }, true, "No FPS");
    /// <summary>
    /// Just FPS
    /// </summary>
    public static readonly PerformanceComponentGroup basic = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent() }, false, "Basic");
    /// <summary>
    /// FPS + AVG FPS + Range FPS
    /// </summary>
    public static readonly PerformanceComponentGroup extended = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent() }, false, "Extended");
    /// <summary>
    /// FPS + AVG FPS + Range FPS + Memory
    /// </summary>
    public static readonly PerformanceComponentGroup verbose = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent(), new MemoryPerformanceComponent() }, false, "Verbose");
}

/// <summary>
/// A performance group of UI elements
/// </summary>
public sealed class PerformanceComponentGroup
{
    /// <summary>
    /// All the UI Elements
    /// </summary>
    public BasePerformanceComponent[] groupElements;
    /// <summary>
    /// If true disable UI
    /// </summary>
    public bool disableUI;
    /// <summary>
    /// The name to display as option
    /// </summary>
    public string groupName;

    public PerformanceComponentGroup(BasePerformanceComponent[] elements, bool disableUI, string name)
    {
        groupElements = elements;
        this.disableUI = disableUI;
        groupName = name;
    }
}
