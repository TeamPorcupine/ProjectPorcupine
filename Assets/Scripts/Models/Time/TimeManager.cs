#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
using System.Collections.Generic;
using System.Linq;


#endregion
using System;
using System.Collections;
using UnityEngine;

public class TimeManager
{
    private static TimeManager instance;

    private float gameTickPerSecond = 5;

    // An array of possible time multipliers.
    private float[] possibleTimeScales = new float[6] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };

    private Dictionary<IUpdatable, bool> fastUpdatables = new Dictionary<IUpdatable, bool>();
    private Dictionary<IUpdatable, bool> slowUpdatables = new Dictionary<IUpdatable, bool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeManager"/> class.
    /// </summary>
    public TimeManager()
    {
        instance = this;
        TimeScale = 1f;
        TotalDeltaTime = 0f;
        TimeScalePosition = 2;
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

    // Current position in that array.
    // Public so TimeScaleUpdater can easily get a position appropriate to an image.
    public int TimeScalePosition { get; private set; }

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
    /// Returns a copy of the time scale array.
    /// </summary>
    /// <returns> A non reference copy of the time scale array. </returns>
    public float[] GetTimeScaleArrayCopy()
    {
        return possibleTimeScales;
    }

    /// <summary>
    /// Update the total time and invoke the required events.
    /// </summary>
    /// <param name="time">Time since last frame.</param>
    public void Update(float time)
    {
//        if (Time.frameCount % 30 == 0)
//        {
//            System.GC.Collect();
//        }
        
        float deltaTime = time * TimeScale;

        // Systems that update every frame.
        InvokeEvent(EveryFrame, time);

        // Systems that update every frame not in Modal.
        if (GameController.Instance.IsModal == false)
        {
            InvokeEvent(EveryFrameNotModal, time);
        }

        // Systems that update every frame while unpaused.
        if (GameController.Instance.IsPaused == false)
        {
            InvokeEvent(EveryFrameUnpaused, deltaTime);
            ProcessUpdatables(deltaTime);
        }

        // Systems that update at fixed frequency.
        if (TotalDeltaTime >= GameTickDelay)
        {
            InvokeEvent(FixedFrequency, TotalDeltaTime);

            // Systems that update at fixed frequency when not paused.
            if (GameController.Instance.IsPaused == false)
            {
                InvokeEvent(FixedFrequencyUnpaused, TotalDeltaTime);
            }

            TotalDeltaTime = 0;
        }

        TotalDeltaTime += deltaTime;
    }

    private int updatableProgress = 0;
    private float[] accumulatedTime = new float[10];
    private int timePos = 0;

    public void ProcessUpdatables(float deltaTime)
    {
        Profiler.BeginSample("ProcessUpdatables");
        Profiler.BeginSample("fastUpdatables");
        IUpdatable[] updatablesCopy = new IUpdatable[fastUpdatables.Count];
        fastUpdatables.Keys.CopyTo(updatablesCopy, 0);
        for (int i = 0; i < fastUpdatables.Count; i++)
        {
            updatablesCopy[i].EveryFrameUpdate(deltaTime);
        }
        Profiler.EndSample();
        Profiler.BeginSample("slowUpdatables");

        accumulatedTime[timePos] = deltaTime;
        float accumulatedDeltaTime = accumulatedTime.Sum();

        int numToProcess = Mathf.CeilToInt((float)slowUpdatables.Count / 10);
        updatablesCopy = new IUpdatable[slowUpdatables.Count];
        slowUpdatables.Keys.CopyTo(updatablesCopy, 0);

        for (int i = updatableProgress; i < updatableProgress + numToProcess && i < slowUpdatables.Count; i++)
        {
            updatablesCopy[i].EveryFrameUpdate(accumulatedDeltaTime);
        }

        updatableProgress += numToProcess;

        timePos++;
        if (timePos >= accumulatedTime.Length)
        {
            timePos = 0;
        }
        updatablesCopy = null;
        Profiler.EndSample();
        Profiler.EndSample();
    }

    /// <summary>
    /// Calls the furnitures update function on every frame.
    /// The list needs to be copied temporarily in case furnitures are added or removed during the update.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
//    public void TickEveryFrame(float deltaTime)
//    {
//        Profiler.BeginSample("TEF");
//        List<Furniture> tempFurnituresVisible = new List<Furniture>(furnituresVisible);
//        foreach (Furniture furniture in tempFurnituresVisible)
//        {
//            furniture.EveryFrameUpdate(deltaTime);
//        }
//
//
//
//        // Update furniture outside of the camera view
//        List<Furniture> tempFurnituresInvisible = new List<Furniture>(furnituresInvisible);
//        //        int totalFurnCount = tempFurnituresInvisible.Count;
//
//        for (int i = invisibleFurnitureProgress; i < invisibleFurnitureProgress + invFurnToProcess && i < tempFurnituresInvisible.Count; i++)
//        {
//            tempFurnituresInvisible[i].EveryFrameUpdate(accumulatedDeltaTime);
//        }
//        invisibleFurnitureProgress += invFurnToProcess;
//
//        List<Furniture> tempFurnitures = new List<Furniture>(furnitures);
//        int furnToProcess = Mathf.CeilToInt((float)tempFurnitures.Count / 10);
//        for (int i = furnitureProgress; i < furnitureProgress + furnToProcess && i < tempFurnitures.Count; i++)
//        {
//            tempFurnitures[i].FixedFrequencyUpdate(accumulatedDeltaTime);
//        }
//        furnitureProgress += furnToProcess;
//
//        timePos++;
//        if (timePos >= accumulatedTime.Length)
//        {
//            timePos = 0;
//        }
//        Profiler.EndSample();
//    }

    /// <summary>
    /// Sets the speed of the game. Greater time scale position equals greater speed.
    /// </summary>
    /// <param name="newTimeScalePosition">New time scale position.</param>
    public void SetTimeScalePosition(int newTimeScalePosition)
    {
        if (newTimeScalePosition < possibleTimeScales.Length && newTimeScalePosition >= 0 && newTimeScalePosition != TimeScalePosition)
        {
            TimeScalePosition = newTimeScalePosition;
            TimeScale = possibleTimeScales[newTimeScalePosition];
            UnityDebugger.Debugger.Log("Game speed", "Game speed set to " + TimeScale + "x");
        }
    }

    /// <summary>
    /// Increases the game speed by increasing the time scale by 1.
    /// </summary>
    public void IncreaseTimeScale()
    {
        SetTimeScalePosition(TimeScalePosition + 1);
    }

    /// <summary>
    /// Decreases the game speed by decreasing the time scale by 1.
    /// </summary>
    public void DecreaseTimeScale()
    {
        SetTimeScalePosition(TimeScalePosition - 1);
    }

    /// <summary>
    /// Destroy this instance.
    /// </summary>
    public void Destroy()
    {
        instance = null;
    }

    public void RegisterFastUpdate(IUpdatable updatable)
    {
        if (!fastUpdatables.ContainsKey(updatable))
        {
            fastUpdatables.Add(updatable, true);
        }
    }

    public void UnregisterFastUpdate(IUpdatable updatable)
    {
        if (fastUpdatables.ContainsKey(updatable))
        {
            fastUpdatables.Remove(updatable);
        }
    }

    public void RegisterSlowUpdate(IUpdatable updatable)
    {
        if (!slowUpdatables.ContainsKey(updatable))
        {
            slowUpdatables.Add(updatable, true);
        }
    }

    public void UnregisterSlowUpdate(IUpdatable updatable)
    {
        if (slowUpdatables.ContainsKey(updatable))
        {
            slowUpdatables.Remove(updatable);
        }
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
