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

public class CharacterSpriteController
{

    Dictionary<Character, GameObject> characterGameObjectMap;

    World world;
    GameObject characterParent;

    // Use this for initialization
    public CharacterSpriteController(World currentWorld)
    {
        world = currentWorld;
        characterParent = new GameObject("Characters");
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
        // Debug.Log("OnCharacterCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject char_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        characterGameObjectMap.Add(c, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(c.X, c.Y, 0);
        char_go.transform.SetParent(characterParent.transform, true);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();        
        sr.sortingLayerName = "Characters";
        
        c.animation = new CharacterAnimation(c, sr);
        
        // load all character sprites and change colors
        Sprite[] sprites = 
            {
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_idle_south"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_idle_east"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_idle_north"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_east_01"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_east_02"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_north_01"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_north_02"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_south_01"), c.GetCharacterColor()),
                ReplaceSpriteColor(SpriteManager.current.GetSprite("Character", "p3_walk_south_02"), c.GetCharacterColor())
            };
        c.animation.SetSprites(sprites);
        
        // Add the inventory sprite onto the character
        GameObject inv_go = new GameObject("Inventory");
        SpriteRenderer inv_sr = inv_go.AddComponent<SpriteRenderer>();
        inv_sr.sortingOrder = 1;
        inv_sr.sortingLayerName = "Characters";
        inv_go.transform.SetParent(char_go.transform);
        inv_go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);// Config needs to be added to XML
        inv_go.transform.localPosition = new Vector3(0,-0.37f,0); // Config needs to be added to XML

        // Add the reflection of the character's helmet
        GameObject helmet_go = new GameObject ("HelmetGlass");
        SpriteRenderer helmet_sr = helmet_go.AddComponent<SpriteRenderer>();
        helmet_sr.sortingOrder = 1;
        //helmet_sr.sprite = SpriteManager.current.GetSprite("Character", "p2_helmet");
        helmet_sr.sortingLayerName = "Characters";
        helmet_go.transform.SetParent (char_go.transform);
        helmet_go.transform.localPosition = new Vector3(0,0,0);

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        c.cbCharacterChanged += OnCharacterChanged;
        
    }

    // change relevant pixels in sprite and return a colorized version.
    private Sprite ReplaceSpriteColor(Sprite sprite, Color32 newColor)
    {
        Texture2D newTexture = CopyTexture2D(sprite.texture, newColor);
        Sprite s = Sprite.Create(newTexture, sprite.textureRect, new Vector2(0.5f, 0.3f), sprite.pixelsPerUnit);
        return s;
    }

    private Texture2D CopyTexture2D(Texture2D fromTexture, Color32 newColor)
    {
        Texture2D texture = new Texture2D(fromTexture.width, fromTexture.height);
        texture.filterMode = fromTexture.filterMode;
        texture.wrapMode = fromTexture.wrapMode;

        Color[] pixelColors = fromTexture.GetPixels(0, 0, fromTexture.width, fromTexture.height);

        Color32 fromColorMain = new Color32(255, 0, 170, 255);
        Color32 fromColorLight = new Color32(255, 128, 213, 255);
        Color32 fromColorDark = new Color32(128, 0, 85, 255);

        Color newColorLight = Color32.Lerp(newColor, new Color32(255, 255, 255, 255), 0.5f);
        Color newColorDark = Color32.Lerp(newColor, new Color32(0, 0, 0, 255), 0.5f);
        
        int y = 0;
        while (y < pixelColors.Length)
        {
            if (pixelColors[y] == fromColorMain)
            {
                pixelColors[y] = newColor;                
            }
            else if (pixelColors[y] == fromColorLight)
            {
                pixelColors[y] = newColorLight;
            }
            else if (pixelColors[y] == fromColorDark)
            {
                pixelColors[y] = newColorDark;
            }
            ++y;
        }

        texture.SetPixels(pixelColors);
        texture.Apply();
        return texture;
    }

    void OnCharacterChanged(Character c)
    {
        //Debug.Log("OnFurnitureChanged");
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
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        if (c.CurrTile.Room != null)
        {
            if (c.CurrTile.Room.GetGasAmount ("O2") <= 0.5f && char_go.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled == false)
            {
                char_go.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
            }
            else if(c.CurrTile.Room.GetGasAmount ("O2") >= 0.5f && char_go.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled == true)
            {
                char_go.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
            }
        }


        char_go.transform.position = new Vector3(c.X, c.Y, 0);
    }


	
}
