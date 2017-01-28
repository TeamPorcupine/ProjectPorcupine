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
/// Displays current FPS (over 0.5s period).
/// </summary>
public class FPSPerformanceComponent : BasePerformanceHUDElement
{
    private const float FPSMeasurePeriod = 0.5f;

    private const string GreenDisplay = "<color=lime>{0}</color>";
    private const string RedDisplay = "<color=red>{0}</color>";
    private const string YellowDisplay = "<color=yellow>{0}</color>";

    private int fpsAccumulator = 0;
    private float fpsNextPeriod = 0;
    private int currentFps;

    // public override string NameOfComponent()
    //  {
    //       return "UI/TextPerformanceComponentUI";
    //  }

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
                //UITextElement.text = string.Format(GreenDisplay, currentFps);
            }
            else if (currentFps >= 30 && currentFps <= 55)
            {
                // Less preferable but playable
                //UITextElement.text = string.Format(YellowDisplay, currentFps);
            }
            else
            {
                // Too low, most likely due to an error or major slowdown
                //UITextElement.text = string.Format(RedDisplay, currentFps);
            }
        }
    }

    public override GameObject InitializeElement()
    {
        //UITextElement.fontSize = 20;

        fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

        return null;
    }
}
