#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;

/// <summary>
/// Displays the currently used memory and the currently allocated memory.
/// </summary>
public class MemoryPerformanceComponent : BasePerformanceComponent
{
    private TextPerformanceComponentUI component;

    public override int PriorityID()
    {
        return 5;
    }

    public override void Update()
    {
        component.ChangeText("Mem: " + ((Profiler.GetTotalReservedMemory() / 1024) / 1024) + "mb\nAlloc: " + ((Profiler.GetTotalAllocatedMemory() / 1024) / 1024) + "mb");
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string NameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }

    public override void Start(BasePerformanceComponentUI componentUI)
    {
        Profiler.enabled = true;
        component = (TextPerformanceComponentUI)componentUI;
        component.text.fontSize = 11;
        component.text.resizeTextForBestFit = true;
    }
}
