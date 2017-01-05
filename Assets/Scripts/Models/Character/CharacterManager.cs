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
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Character manager that holds all the characters.
/// </summary>
[MoonSharpUserData]
public class CharacterManager : IEnumerable<Character>
{
    public List<Character> characters;

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
        TimeManager.Instance.RegisterFastUpdate(character);

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

    public JToken ToJson()
    {
        JArray charactersJson = new JArray();
        foreach (Character character in characters)
        {
            charactersJson.Add(character.ToJSon());
        }

        return charactersJson;
    }

    public void FromJson(JToken charactersToken)
    {
        if (charactersToken == null)
        {
            return;
        }

        JArray charactersJArray = (JArray)charactersToken;

        foreach (JToken characterToken in charactersJArray)
        {
            Character character;
            int x = (int)characterToken["X"];
            int y = (int)characterToken["Y"];
            int z = (int)characterToken["Z"];
            if (characterToken["Colors"] != null)
            {
                JToken colorToken = characterToken["Colors"];
                Color color = ColorUtilities.ParseColorFromString((string)colorToken["CharacterColor"][0], (string)colorToken["CharacterColor"][1], (string)colorToken["CharacterColor"][2]);
                Color colorUni = ColorUtilities.ParseColorFromString((string)colorToken["UniformColor"][0], (string)colorToken["UniformColor"][1], (string)colorToken["UniformColor"][2]);
                Color colorSkin = ColorUtilities.ParseColorFromString((string)colorToken["SkinColor"][0], (string)colorToken["SkinColor"][1], (string)colorToken["SkinColor"][2]);
                character = Create(World.Current.GetTileAt(x, y, z), color, colorUni, colorSkin);
            }
            else
            {
                character = Create(World.Current.GetTileAt(x, y, z));
            }

            // While it's not strictly necessary to use a foreach here, it *is* an array structure, so it should be treated as such
            if (characterToken["Inventories"] != null)
            {
                foreach (JToken inventoryToken in characterToken["Inventories"])
                {
                    Inventory inventory = new Inventory();
                    inventory.FromJson(inventoryToken);
                    World.Current.InventoryManager.PlaceInventory(character, inventory);
                }
            }

            character.name = (string)characterToken["Name"];
        }
    }
}
