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

/// <summary>
/// Displays current FPS (over 0.5s period)
/// </summary>
public class FPSPerformanceComponent : BasePerformanceComponent
{
    private const float FPSMeasurePeriod = 0.5f;
    private const string Display = "{0} FPS";
    private int fpsAccumulator = 0;
    private float fpsNextPeriod = 0;
    private int currentFps;

    private TextPerformanceComponentUI component;

    public override int priorityID()
    {
        //By default will be first component shown
        return 0;
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string nameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }

    // The shown FPS will be 0 for the first second until it ticks over correctly
    public override void Update()
    {
        // measure average frames per second
        fpsAccumulator++;
        if (Time.realtimeSinceStartup > fpsNextPeriod)
        {
            currentFps = (int)(fpsAccumulator / FPSMeasurePeriod);
            fpsAccumulator = 0;

            fpsNextPeriod += FPSMeasurePeriod;

            component.changeText(string.Format(Display, currentFps));
        }
    }

    public override void Start(BasePerformanceComponentUI UIComponent)
    {
        component = (TextPerformanceComponentUI)UIComponent;

        fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;
    }
}
