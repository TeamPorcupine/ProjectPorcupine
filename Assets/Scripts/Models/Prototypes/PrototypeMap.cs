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
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

/// <summary>
/// A class that holds prototypes to be used later.
/// </summary>
public class PrototypeMap<T> where T : IPrototypable, new()
{
    private readonly Dictionary<string, T> prototypes;
    private string listTag;
    private string elementTag;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrototypeMap`1"/> class.
    /// </summary>
    public PrototypeMap()
    {
        this.prototypes = new Dictionary<string, T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrototypeMap`1"/> class.
    /// </summary>
    /// <param name="listTag">Name used for the XML tag that holds all the prototypes.</param>
    /// <param name="elementTag">Name used for the XML tag that hold each prototype.</param>
    public PrototypeMap(string listTag, string elementTag)
    {
        this.prototypes = new Dictionary<string, T>();
        this.listTag = listTag;
        this.elementTag = elementTag;
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
    /// Returns the prototype at the specified index.
    /// </summary>
    /// <param name="index">The prototype index.</param>
    /// <returns>The prototype.</returns>
    public T this[int index]
    {
        get
        {
            return prototypes.ElementAt(index).Value;
        }
    }

    /// <summary>
    /// Determines whether there is a prototype with the specified type.
    /// </summary>
    /// <returns><c>true</c> if there is a prototype with the specified type; otherwise, <c>false</c>.</returns>
    /// <param name="type">The prototype type.</param>
    public bool Has(string type)
    {
        return prototypes.ContainsKey(type);
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
            return prototypes[type];
        }

        return default(T);
    }

    /// <summary>
    /// Adds the given prototype. If the protptype exists it is overwirten.
    /// </summary>
    /// <param name="proto">The prototype instance.</param>
    public void Set(T proto)
    {
        prototypes[proto.Type] = proto;
    }

    /// <summary>
    /// Add the given prototype. If a prototype of the given type is already registered, overwrite the old one while logging a warning.
    /// </summary>
    /// <param name="proto">The prototype instance.</param>
    public void Add(T proto)
    {
        if (Has(proto.Type))
        {
            UnityDebugger.Debugger.LogWarningFormat("PrototypeMap", "Trying to register a prototype of type '{0}' which already exists. Overwriting.", proto.Type);
        }

        Set(proto);
    }

    /// <summary>
    /// Loads all the prototypes from the specified text.
    /// </summary>
    /// <param name="xmlText">Xml text to parse.</param>
    public void LoadPrototypes(string xmlText)
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

        if (reader.ReadToDescendant(listTag))
        {
            if (reader.ReadToDescendant(elementTag))
            {
                do
                {
                    LoadPrototype(reader);
                }
                while (reader.ReadToNextSibling(elementTag));
            }
            else
            {
                UnityDebugger.Debugger.LogError("PrototypeMap", "The '" + listTag + "' prototype definition file doesn't have any '" + elementTag + "' elements.");
            }
        }
        else
        {
            UnityDebugger.Debugger.LogError("PrototypeMap", "Did not find a '" + listTag + "' element in the prototype definition file.");
        }
    }

    /// <summary>
    /// Loads a single prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    private void LoadPrototype(XmlReader reader)
    {
        T prototype = new T();
        try
        {
            prototype.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            // Leaving this for Unitys console because UberLogger doesn't show multiline messages correctly.
            UnityDebugger.Debugger.LogError("PrototypeMap", "Error reading '" + elementTag + "' prototype for: " + listTag + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
        }

        Set(prototype);
    }
}
