#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using UnityEngine;

public class TimeManager
{
    private static TimeManager instance;

    private float gameTickPerSecond = 5;

    // Current position in that array.
    private int timeScalePosition = 2;

    // An array of possible time multipliers.
    private float[] possibleTimeScales = new float[6] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeManager"/> class.
    /// </summary>
    public TimeManager()
    {
        instance = this;
        TimeScale = 1f;
        TotalDeltaTime = 0f;
        IsPaused = false;
        
        KeyboardManager.Instance.RegisterInputAction("SetSpeed1", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(2));
        KeyboardManager.Instance.RegisterInputAction("SetSpeed2", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(3));
        KeyboardManager.Instance.RegisterInputAction("SetSpeed3", KeyboardMappedInputType.KeyUp, () => SetTimeScalePosition(4));
        KeyboardManager.Instance.RegisterInputAction("DecreaseSpeed", KeyboardMappedInputType.KeyUp, DecreaseTimeScale);
        KeyboardManager.Instance.RegisterInputAction("IncreaseSpeed", KeyboardMappedInputType.KeyUp, IncreaseTimeScale);
    }

    /// <summary>
    /// Systems that update every frame.
    /// </summary>
    public event Action<float> EveryFrame;

    /// <summary>
    /// Systems that update every frame not in Modal.
    /// </summary>
    public event Action<float> EveryFrameNotModal;

    /// <summary>
    /// Systems that update every frame while unpaused.
    /// </summary>
    public event Action<float> EveryFrameUnpaused;

    /// <summary>
    /// Systems that update at fixed frequency.
    /// </summary>
    public event Action<float> FixedFrequency;

    /// <summary>
    /// Systems that update at fixed frequency while unpaused.
    /// </summary>
    public event Action<float> FixedFrequencyUnpaused;

    /// <summary>
    /// Gets the TimeManager instance.
    /// </summary>
    /// <value>The TimeManager instance.</value>
    public static TimeManager Instance
    {
        get
        {
            if (instance == null)
            {
                new TimeManager();
            }

            return instance;
        }
    }

    /// <summary>
    /// Gets the game time tick delay.
    /// </summary>
    /// <value>The game time tick delay.</value>
    public float GameTickDelay
    {
        get { return 1f / gameTickPerSecond; }
    }

    /// <summary>
    /// Gets the total delta time.
    /// </summary>
    /// <value>The total delta time.</value>
    public float TotalDeltaTime { get; private set; }

    /// <summary>
    /// Multiplier of Time.deltaTime.
    /// </summary>
    /// <value>The time scale.</value>
    public float TimeScale { get; private set; }

    /// <summary>
    /// Returns true if the game is paused.
    /// </summary>
    /// <value><c>true</c> if this game is paused; otherwise, <c>false</c>.</value>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Update the total time and invoke the required events.
    /// </summary>
    /// <param name="time">Time since last frame.</param>
    public void Update(float time)
    {
        float deltaTime = time * TimeScale;

        // Systems that update every frame.
        InvokeEvent(EveryFrame, time);

        // Systems that update every frame not in Modal.
        if (WorldController.Instance.IsModal == false)
        {
            InvokeEvent(EveryFrameNotModal, time);
        }

        // Systems that update every frame while unpaused.
        if (WorldController.Instance.IsPaused == false)
        {
            InvokeEvent(EveryFrameUnpaused, deltaTime);
        }

        // Systems that update at fixed frequency.
        if (TotalDeltaTime >= GameTickDelay)
        {
            InvokeEvent(FixedFrequency, TotalDeltaTime);

            // Systems that update at fixed frequency when not paused.
            if (WorldController.Instance.IsPaused == false)
            {
                InvokeEvent(FixedFrequencyUnpaused, TotalDeltaTime);
            }

            TotalDeltaTime = 0;
        }

        TotalDeltaTime += deltaTime;
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
            TimeScale = possibleTimeScales[newTimeScalePosition];
            Debug.ULogChannel("Game speed", "Game speed set to " + TimeScale + "x");
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
    /// Destroy this instance.
    /// </summary>
    public void Destroy()
    {
        instance = null;
    }

    /// <summary>
    /// Invokes the given event action.
    /// </summary>
    /// <param name="eventAction">The event action.</param>
    /// <param name="time">The delta time.</param>
    private void InvokeEvent(Action<float> eventAction, float time)
    {
        if (eventAction != null)
        {
            eventAction(time);
        }
    }
}
