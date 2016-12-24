#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class GameEvent : IPrototypable
{
    private List<string> preconditions;

    private List<string> executionActions;

    private bool executed;
    private float timer;
    private int repeats;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent"/> class.
    /// Used to create the Prototype.
    /// </summary>
    public GameEvent()
    {
        Repeat = false;
        MaxRepeats = 0;
        preconditions = new List<string>();
        executionActions = new List<string>();
        timer = 0;
        repeats = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent"/> class.
    /// Copy constructor use Clone.
    /// </summary>
    /// <param name="other">Other game event.</param>
    private GameEvent(GameEvent other)
    {
        Name = other.Name;
        Repeat = other.Repeat;
        MaxRepeats = other.MaxRepeats;
        preconditions = new List<string>(other.preconditions);
        executionActions = new List<string>(other.executionActions);
        timer = 0;
        repeats = 0;
    }

    /// <summary>
    /// Gets the type of the game event. Used for the Prototype and is equal to the name.
    /// </summary>
    /// <value>The type of the game event.</value>
    public string Type
    {
        get { return Name; }
    }

    /// <summary>
    /// Gets the name of the game event.
    /// </summary>
    /// <value>The name of the game event.</value>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets  a value indicating whether this <see cref="GameEvent"/> is repeats.
    /// </summary>
    /// <value><c>true</c> if the game event repeats; otherwise, <c>false</c>.</value>
    public bool Repeat { get; protected set; }

    /// <summary>
    /// Gets the max amount of repeats.
    /// </summary>
    /// <value>The max amount of repeats.</value>
    public int MaxRepeats { get; protected set; }

    /// <summary>
    /// Gets the timer.
    /// </summary>
    /// <value>The game event timer.</value>
    public float Timer
    {
        get { return timer; }
    }

    /// <summary>
    /// Update the game event.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void Update(float deltaTime)
    {
        int conditionsMet = 0;
        foreach (string precondition in preconditions)
        {
            // Call lua precondition it should return 1 if met otherwise 0
            conditionsMet += (int)FunctionsManager.GameEvent.Call(precondition, this, deltaTime).Number;
        }

        if (conditionsMet >= preconditions.Count && executed == false && (MaxRepeats <= 0 || repeats < MaxRepeats))
        {
            repeats++;
            Execute();
        }
    }

    /// <summary>
    /// Adds the given time to the timer.
    /// </summary>
    /// <param name="time">The time to add.</param>
    public void AddTimer(float time)
    {
        timer += time;
    }

    /// <summary>
    /// Resets the game event timer.
    /// </summary>
    public void ResetTimer()
    {
        timer = 0;
    }

    /// <summary>
    /// Execute the actions of this game event.
    /// </summary>
    public void Execute()
    {
        if (executionActions != null)
        {
            // Execute Lua code like in Furniture ( FurnitureActions ) 
            FunctionsManager.GameEvent.CallWithInstance(executionActions, this);
        }

        if (!Repeat)
        {
            executed = true;
        }
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    public GameEvent Clone()
    {
        return new GameEvent(this);
    }

    /// <summary>
    /// Reads the prototype from the specified XML reader.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    public void ReadXmlPrototype(XmlReader reader)
    {
        Name = reader.GetAttribute("Name");

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Repeats":
                    MaxRepeats = int.Parse(reader.GetAttribute("MaxRepeats"));
                    Repeat = true;
                    break;
                case "Precondition":
                    preconditions.Add(reader.GetAttribute("FunctionName"));
                    break;
                case "OnExecute":
                    executionActions.Add(reader.GetAttribute("FunctionName"));
                    break;
            }
        }
    }
}
