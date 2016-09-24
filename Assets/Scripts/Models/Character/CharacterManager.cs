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
using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// Character manager that holds all the characters.
/// </summary>
[MoonSharpUserData]
public class CharacterManager : IEnumerable<Character>
{
    private List<Character> characters;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterManager"/> class.
    /// </summary>
    public CharacterManager()
    {
        characters = new List<Character>();
    }

    /// <summary>
    /// Occurs when a character is created.
    /// </summary>
    public event Action<Character> Created;

    /// <summary>
    /// Create a Character in the specified tile.
    /// </summary>
    /// <param name="tile">The tile where the Character is placed.</param>
    public Character Create(Tile tile)
    {
        return Create(tile, ColorUtilities.RandomColor(), ColorUtilities.RandomGrayColor(), ColorUtilities.RandomSkinColor());
    }

    /// <summary>
    /// Create a Character in the specified tile, with the specified color, uniform color and skin color.
    /// </summary>
    /// <param name="tile">The tile where the Character is placed.</param>
    /// <param name="color">The uniform strip color.</param>
    /// <param name="uniformColor">The uniform color.</param>
    /// <param name="skinColor">The skin color.</param>
    public Character Create(Tile tile, Color color, Color uniformColor, Color skinColor)
    {
        Character character = new Character(tile, color, uniformColor, skinColor);

        character.name = CharacterNameManager.GetNewName();
        characters.Add(character);

        if (Created != null)
        {
            Created(character);
        }

        return character;
    }

    /// <summary>
    /// A function to return the Character object from the character's name.
    /// </summary>
    /// <param name="name">The name of the character.</param>
    /// <returns>The character with that name.</returns>
    public Character GetFromName(string name)
    {
        foreach (Character character in characters)
        {
            if (character.name == name)
            {
                return character;
            }
        }

        return null;
    }

    /// <summary>
    /// Calls the update function of each character with the given delta time.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void Update(float deltaTime)
    {
        // Change from a foreach due to the collection being modified while its being looped through
        for (int i = characters.Count - 1; i >= 0; i--)
        {
            characters[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// Gets the characters enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator GetEnumerator()
    {
        return characters.GetEnumerator();
    }

    /// <summary>
    /// Gets each character.
    /// </summary>
    /// <returns>Each character.</returns>
    IEnumerator<Character> IEnumerable<Character>.GetEnumerator()
    {
        foreach (Character character in characters)
        {
            yield return character;
        }
    }

    /// <summary>
    /// Writes the Characters to the XML.
    /// </summary>
    /// <param name="writer">The XML Writer.</param>
    public void WriteXml(XmlWriter writer)
    {
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
