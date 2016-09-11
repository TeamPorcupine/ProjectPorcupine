#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;

public class TimeManager
{
    private static float gameTickPerSecond = 5;

    // Current position in that array.
    private int timeScalePosition = 2;

    // Multiplier of Time.deltaTime.
    private float timeScale = 1f;

    // An array of possible time multipliers.
    private float[] possibleTimeScales = new float[6] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };

    private float deltaTime = 0f;
    private float totalDeltaTime = 0f;

    public TimeManager()
    {
        KeyboardManager keyboardManager = KeyboardManager.Instance;
        keyboardManager.RegisterInputAction("SetSpeed1", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(2));
        keyboardManager.RegisterInputAction("SetSpeed2", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(3));
        keyboardManager.RegisterInputAction("SetSpeed3", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(4));
        keyboardManager.RegisterInputAction("DecreaseSpeed", KeyboardMappedInputType.KeyUp, DecreaseTimeScale);
        keyboardManager.RegisterInputAction("IncreaseSpeed", KeyboardMappedInputType.KeyUp, IncreaseTimeScale);
    }

    public static float GameTickDelay
    {
        get
        {
            return 1f / gameTickPerSecond;
        }
    }

    public float DeltaTime
    {
        get
        {
            return deltaTime;
        }
    }

    public float TotalDeltaTime
    {
        get
        {
            return totalDeltaTime;
        }
    }

    public float TimeScale
    {
        get
        {
            return timeScale;
        }
    }

    // Update is called once per frame
    public void Update()
    {
        deltaTime = Time.deltaTime * timeScale;
        totalDeltaTime += deltaTime;
    }

    /// <summary>
    /// Sets the speed of the game. Greater time scale position equals greater speed.
    /// </summary>
    /// <param name="newTimeScalePosition">New time scale position.</param>
    public void SetTimeScalePosition(int newTimeScalePosition)
    {
        if (newTimeScalePosition < possibleTimeScales.Length && newTimeScalePosition >= 0 && newTimeScalePosition != timeScalePosition)
        {
            timeScalePosition = newTimeScalePosition;
            timeScale = possibleTimeScales[newTimeScalePosition];
            Debug.ULogChannel("Game speed", "Game speed set to " + timeScale + "x");
        }
    }

    /// <summary>
    /// Increases the game speed by increasing the time scale by 1.
    /// </summary>
    public void IncreaseTimeScale()
    {
        SetTimeScalePosition(timeScalePosition + 1);
    }

    /// <summary>
    /// Decreases the game speed by decreasing the time scale by 1.
    /// </summary>
    public void DecreaseTimeScale()
    {
        SetTimeScalePosition(timeScalePosition - 1);
    }

    /// <summary>
    /// Resets the total delta time to 0.
    /// </summary>
    public void ResetTotalDeltaTime()
    {
        totalDeltaTime = 0;
    }
}
