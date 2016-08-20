#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CharacterSpriteController : MonoBehaviour
{

    Dictionary<Character, GameObject> characterGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.cbCharacterCreated += OnCharacterCreated;

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character c in world.characters)
        {
            OnCharacterCreated(c);
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    public void OnCharacterCreated(Character c)
    {
        // Logger.Log("OnCharacterCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject char_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        characterGameObjectMap.Add(c, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(c.X, c.Y, 0);
        char_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.current.GetSprite("Character", "p1_front");
        sr.sortingLayerName = "Characters";

        // Add the inventory sprite onto the character
        GameObject inv_go = new GameObject("Inventory");
        SpriteRenderer inv_sr = inv_go.AddComponent<SpriteRenderer>();
        inv_sr.sortingOrder = 1;
        inv_sr.sortingLayerName = "Characters";
        inv_go.transform.SetParent(char_go.transform);
        inv_go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);// Config needs to be added to XML
        inv_go.transform.localPosition = new Vector3(0,-0.37f,0); // Config needs to be added to XML

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        c.cbCharacterChanged += OnCharacterChanged;

        // Adds a random name to the Character
        TextAsset names = Resources.Load("names") as TextAsset;
        string[] lines = Regex.Split( names.text, "\r\n" );
        c.name = lines[Random.Range(0, lines.Length-1)];


    }

    void OnCharacterChanged(Character c)
    {
        //Logger.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.
        SpriteRenderer inv_sr = characterGameObjectMap[c].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (c.inventory != null)
        {
            inv_sr.sprite = SpriteManager.current.GetSprite("Inventory", c.inventory.GetName());
        }
        else
        {
            inv_sr.sprite = null;
        }


        if (characterGameObjectMap.ContainsKey(c) == false)
        {
            Logger.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        //Logger.Log(furn_go);
        //Logger.Log(furn_go.GetComponent<SpriteRenderer>());

        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

        char_go.transform.position = new Vector3(c.X, c.Y, 0);
    }


	
}
