#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

/// <summary>
/// All the groups/modes for the PerformanceHUD.
/// </summary>
public static class PerformanceComponentGroups
{
    /// <summary>
    /// A group with no elements (empty HUD).
    /// </summary>
    public static readonly PerformanceComponentGroup None = new PerformanceComponentGroup(new BasePerformanceComponent[] { }, true, "No FPS");

    /// <summary>
    /// A group with just an FPS Counter.
    /// </summary>
    public static readonly PerformanceComponentGroup Basic = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent() }, false, "Basic");

    /// <summary>
    /// A group with elements: FPS, AVG FPS, and Range FPS.
    /// </summary>           
    public static readonly PerformanceComponentGroup Extended = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent() }, false, "Extended");

    /// <summary>
    /// A group with elements: FPS, AVG FPS, Range FPS, and Memory.
    /// </summary>
    public static readonly PerformanceComponentGroup Verbose = new PerformanceComponentGroup(new BasePerformanceComponent[] { new FPSPerformanceComponent(), new FPSRangePerformanceComponent(), new FPSAveragePerformanceComponent(), new MemoryPerformanceComponent() }, false, "Verbose");

    /// <summary>
    /// All groups in a single array.
    /// </summary>
    public static PerformanceComponentGroup[] groups;
}
