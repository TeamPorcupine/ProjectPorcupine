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
using UnityEngine;

/// <summary>
/// Character name manager that holds all the possible names and tries to give a random and unused name everytime is requested.
/// </summary>
public class CharacterNameManager
{
    private static Queue<string> unusedNames;
    private static Queue<string> usedNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterNameManager"/> class.
    /// </summary>
    public CharacterNameManager()
    {
        unusedNames = new Queue<string>();
        usedNames = new Queue<string>();
    }

    /// <summary>
    /// Load new names to use for Characters.
    /// </summary>
    /// <param name="nameStrings">An array of name strings.</param>
    public static void LoadNames(string[] nameStrings)
    {
        // Randomize the given strings
        List<string> namesList = nameStrings.OrderBy(c => Random.Range(0f, 1f)).ToList();

        // Add all the names to the unused queue in the random order
        foreach (string name in namesList)
        {
            unusedNames.Enqueue(name);
        }
    }

    /// <summary>
    /// Returns a randomly chosen name, prioritizing names which have not been used yet.
    /// </summary>
    /// <returns>A randomly chosen name.</returns>
    public static string GetNewName()
    {
        string name = unusedNames.Dequeue();
        usedNames.Enqueue(name);

        // We run out of used names, lets start using the used ones
        if (unusedNames.Count == 0)
        {
            unusedNames = new Queue<string>(usedNames);
            usedNames = new Queue<string>();
        }

        return name;
    }
}
