#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the currently used memory and the currently allocated memory
/// </summary>
public class MemoryPerformanceComponent : BasePerformanceComponent
{
    private TextPerformanceComponentUI component;

    public override int priorityID()
    {
        return 5;
    }

    public override void Update()
    {
        component.changeText("Total: " + ((Profiler.GetTotalReservedMemory() / 1024) / 1024) + "mb\nAlloc: " + ((Profiler.GetTotalAllocatedMemory() / 1024) / 1024) + "mb");
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string nameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }

    public override void Start(BasePerformanceComponentUI UIComponent)
    {
        Profiler.enabled = true;
        component = (TextPerformanceComponentUI)UIComponent;
        component.text.fontSize = 12;
    }
}
