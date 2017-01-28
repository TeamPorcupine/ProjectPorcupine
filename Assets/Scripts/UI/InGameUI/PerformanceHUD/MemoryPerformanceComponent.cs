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

#if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;
#endif

/// <summary>
/// Displays the currently used memory and the currently allocated memory.
/// </summary>
public class MemoryPerformanceComponent : BasePerformanceHUDElement
{
    public Text UITextElement { get; set; }

    public override void Update()
    {
        UITextElement.text = "Mem: " + ((Profiler.GetTotalReservedMemory() / 1024) / 1024) + "mb\nAlloc: " + ((Profiler.GetTotalAllocatedMemory() / 1024) / 1024) + "mb";
    }

    //public override string NameOfComponent()
    // {
    //     return "UI/TextPerformanceComponentUI";
    // }

    public override GameObject InitializeElement()
    {
        Profiler.enabled = true;
        UITextElement.fontSize = 11;
        UITextElement.resizeTextForBestFit = true;

        return null;
    }
}
