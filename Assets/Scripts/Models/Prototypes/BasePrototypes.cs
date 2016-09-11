#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;

public class BasePrototypes<T>
{
    protected readonly Dictionary<string, T> Prototypes;

    public BasePrototypes()
    {
        Prototypes = new Dictionary<string, T>();
    }

    /// <summary>
    /// Gets the prototype keys.
    /// </summary>
    /// <value>The prototype keys.</value>
    public Dictionary<string, T>.KeyCollection Keys
    {
        get
        {
            return Prototypes.Keys;
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
            return Prototypes.Values.ToList();
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
            return Prototypes.Count;
        }
    }

    /// <summary>
    /// Returns the prototype at the specified index.
    /// </summary>
    /// <param name="index">The prototype index.</param>
    /// <returns>The prototype.</returns>
    public T this[int index]
    {
        get
        {
            return Prototypes.ElementAt(index).Value;
        }
    }

    /// <summary>
    /// Determines whether there is a prototype with the specified type.
    /// </summary>
    /// <returns><c>true</c> if there is a prototype with the specified type; otherwise, <c>false</c>.</returns>
    /// <param name="type">The prototype type.</param>
    public bool Has(string type)
    {
        return Prototypes.ContainsKey(type);
    }

    /// <summary>
    /// Returns the prototype with the specified type.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="type">The prototype type.</param>
    public T Get(string type)
    {
        if (Has(type))
        {
            return Prototypes[type];
        }

        return default(T);
    }

    /// <summary>
    /// Adds the given prototype. If the protptype exists it is overwirten.
    /// </summary>
    /// <param name="type">The prototype type.</param>
    /// <param name="proto">The prototype instance.</param>
    public void Set(string type, T proto)
    {
        Prototypes[type] = proto;
    }

    /// <summary>
    /// Add the given prototype. If a prototype of the given type is already registered, overwrite the old one while logging a warning.
    /// </summary>
    /// <param name="type">The prototype type.</param>
    /// <param name="proto">The prototype instance.</param>
    public void Add(string type, T proto)
    {
        if (Has(type))
        {
            Debug.ULogWarningChannel("BasePrototypes<T>.Add", "Trying to register a prototype of type '{0}' which already exists. Overwriting.", type);
        }

        Set(type, proto);
    }
}
