#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

/// <summary>
/// A performance group of UI elements.
/// </summary>
public sealed class PerformanceComponentGroup
{
    /// <summary>
    /// All the UI Elements.
    /// </summary>
    public BasePerformanceComponent[] groupElements;

    /// <summary>
    /// If true disable UI.
    /// </summary>
    public bool disableUI;

    /// <summary>
    /// The name to display as option.
    /// </summary>
    public string groupName;

    public PerformanceComponentGroup(BasePerformanceComponent[] elements, bool disableUI, string name)
    {
        groupElements = elements;
        this.disableUI = disableUI;
        groupName = name;
    }
}