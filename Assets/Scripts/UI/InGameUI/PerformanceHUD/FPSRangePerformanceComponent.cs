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
/// Just measures the max and min range for FPS.
/// </summary>
public class FPSRangePerformanceComponent : BasePerformanceComponent
{
    private const float FPSMeasurePeriod = 0.5f;
    private const string Display = "Min: {0}\nMax: {1}";
    private int fpsAccumulator = 0;
    private float fpsNextPeriod = 0;

    private int currentFps;
    private int lowestFps = 60;
    private int highestFps = 60;

    private TextPerformanceComponentUI component;

    public override int PriorityID()
    {
        // By default will be first component shown
        return 0;
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string NameOfComponent()
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

            if (currentFps < lowestFps)
            {
                lowestFps = currentFps;
            }
            else if (currentFps > highestFps)
            {
                highestFps = currentFps;
            }

            component.ChangeText(string.Format(Display, lowestFps, highestFps));
        }
    }

    public override void Start(BasePerformanceComponentUI componentUI)
    {
        component = (TextPerformanceComponentUI)componentUI;

        fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

        component.text.fontSize = 12;
    }
}