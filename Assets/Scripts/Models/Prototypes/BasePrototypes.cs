#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasePrototypes<T>
{
    protected Dictionary<string, T> prototypes;

    public BasePrototypes()
    {
        prototypes = new Dictionary<string, T>();
    }

    /// <summary>
    /// Gets the prototype keys.
    /// </summary>
    /// <value>The prototype keys.</value>
    public Dictionary<string, T>.KeyCollection Keys
    {
        get
        {
            return prototypes.Keys;
        }
    }

    /// <summary>
    /// Gets the prototype Values.
    /// </summary>
    /// <value>The prototype values.</value>
    public List<T> Values
    {
        get
        {
            return prototypes.Values.ToList();
        }
    }

    /// <summary>
    /// Gets the prototypes count.
    /// </summary>
    /// <value>The prototypes count.</value>
    public int Count
    {
        get
        {
            return prototypes.Count;
        }
    }

    /// <summary>
    /// Determines whether there is a prototype with the specified type.
    /// </summary>
    /// <returns><c>true</c> if there is a prototype with the specified type; otherwise, <c>false</c>.</returns>
    /// <param name="protoType">The prototype type.</param>
    public bool HasPrototype(string type)
    {
        return prototypes.ContainsKey(type);
    }

    /// <summary>
    /// Returns the prototype with the specified type.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="protoType">The prototype type.</param>
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
    /// <param name="index">The prototype index.</param>
    public T GetPrototype(int index)
    {
        return prototypes.ElementAt(index).Value;
    }

    /// <summary>
    /// Adds the given prototype.
    /// </summary>
    /// <param name="type">The prototype type.</param>
    /// <param name="proto">The prototype instance.</param>
    public void SetPrototype(string type, T proto)
    {
        prototypes[type] = proto;
    }
}
