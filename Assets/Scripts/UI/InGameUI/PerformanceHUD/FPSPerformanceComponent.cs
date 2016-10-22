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
/// Displays current FPS (over 0.5s period).
/// </summary>
public class FPSPerformanceComponent : BasePerformanceComponent
{
    private const float FPSMeasurePeriod = 0.5f;
    private const string GreenDisplay = "<color=lime>{0}</color>";
    private const string RedDisplay = "<color=red>{0}</color>";
    private const string YellowDisplay = "<color=yellow>{0}</color>";
    private int fpsAccumulator = 0;
    private float fpsNextPeriod = 0;
    private int currentFps;

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

            // Colour Changing
            if (currentFps > 55)
            {
                // A good area to be at
                component.ChangeText(string.Format(GreenDisplay, currentFps));
            }
            else if (currentFps >= 30 && currentFps <= 55)
            {
                // Less preferable but playable
                component.ChangeText(string.Format(YellowDisplay, currentFps));
            }
            else
            {
                // Too low, most likely due to an error or major slowdown
                component.ChangeText(string.Format(RedDisplay, currentFps));
            }
        }
    }

    public override void Start(BasePerformanceComponentUI componentUI)
    {
        component = (TextPerformanceComponentUI)componentUI;
        component.text.fontSize = 20;

        fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;
    }
}
