#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


public class BasePrototypes<T>
{

    protected Dictionary<string, T> prototypes;


    public BasePrototypes()
    {
        prototypes = new Dictionary<string, T>();
    }



    /// <summary>
    /// Determines whether there is a prototype with the specified type.
    /// </summary>
    /// <returns><c>true</c> if there is a prototype with the specified type; otherwise, <c>false</c>.</returns>
    /// <param name="type">Type.</param>
    public bool HasPrototype(string type)
    {
        return prototypes.ContainsKey(type);
    }

    /// <summary>
    /// Returns the prototype with the specified type.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="type">Type.</param>
    public T GetPrototype(string type)
    {
        if (HasPrototype(type))
        {
            return prototypes[type];
        }
        return default(T);
    }

    /// <summary>
    /// Returns the prototype at the specified index.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="index">Index.</param>
    public T GetPrototype(int index)
    {
        return prototypes.ElementAt(index).Value;
    }

    /// <summary>
    /// Adds the given prototype.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="proto">Proto.</param>
    public void SetPrototype(string type, T proto)
    {
        prototypes[type] = proto;
    }



    /// <summary>
    /// Returns the prototype Keys.
    /// </summary>
    public Dictionary<string, T>.KeyCollection Keys
    {
        get
        {
            return prototypes.Keys;
        }
    }

    /// <summary>
    /// Returns the prototype Keys.
    /// </summary>
    public List<T> Values
    {
        get
        {
            return prototypes.Values.ToList();
        }
    }

    /// <summary>
    /// Returns the amount of prototypes.
    /// </summary>
    public int Count
    {
        get
        {
            return prototypes.Count;
        }
    }
}
