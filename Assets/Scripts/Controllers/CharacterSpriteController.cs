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

public enum SpriteSwapRedColor
{
    UNIFORMCOLOR = 151,
    UNIFORMCOLORLIGHT = 201,
    UNIFORMCOLORDARK = 101
}

public class CharacterSpriteController
{

    Dictionary<Character, GameObject> characterGameObjectMap;

    World world;
    GameObject characterParent;

    Color[] SwapSpriteColors;
   
    // Use this for initialization
    public CharacterSpriteController(World currentWorld)
    {
        world = currentWorld;
        characterParent = new GameObject("Characters");

        // prepare swap texture for shader
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;
        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }
        colorSwapTex.Apply();
        SwapSpriteColors = new Color[colorSwapTex.width];

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
        
        sr.material = GetMaterial(c);
        
        c.animation = new CharacterAnimation(c, sr);

        // Change colors on the texture
        // Grab the first sprite, and copy the texture from that
        //Texture2D newTexture = CopyTexture2D(SpriteManager.current.GetSprite("Character", "p2_idle_south").texture, c.GetCharacterColor());
        Texture2D newTexture = SpriteManager.current.GetSprite("Character", "tp2_idle_south").texture;
        // load all character sprites and replace the textures with the colorized version
        Sprite[] sprites = 
            {
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_idle_south")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_idle_east")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_idle_north")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_east_01")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_east_02")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_north_01")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_north_02")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_south_01")),
                ReplaceSpriteTexture(newTexture, SpriteManager.current.GetSprite("Character", "tp2_walk_south_02"))
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
        
        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        c.cbCharacterChanged += OnCharacterChanged;        
    }

    // Create material with color-swapping texture for the shader
    private Material GetMaterial(Character c)
    {
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;
        // Reset texture
        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }
        colorSwapTex.Apply();

        // Define the swapping colors
        // Only the red color value from SpriteSwapRedColor is used to compare
        Color newColorLight = Color.Lerp(c.GetCharacterColor(), ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newColorDark = Color.Lerp(c.GetCharacterColor(), ColorFromIntRGB(0, 0, 0), 0.5f);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLOR, c.GetCharacterColor());
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORLIGHT, newColorLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORDARK, newColorDark);
        colorSwapTex.Apply();
        
        Material SwapMaterial = new Material(Resources.Load<Material>("Shaders/ColorSwap"));
        Shader SwapShader = Resources.Load<Shader>("Shaders/Sprites-ColorSwap");
        
        SwapMaterial.shader = SwapShader;
        SwapMaterial.SetTexture("_SwapTex", colorSwapTex);
        SwapMaterial.SetFloat("Pixel snap", 1);
        
        return SwapMaterial;
    }

    private Texture2D SwapColor(Texture2D tex, SpriteSwapRedColor index, Color color)
    {
        SwapSpriteColors[(int)index] = color;
        tex.SetPixel((int)index, 0, color);
        return tex;
    }

    // Replace sprite texture with the colorized version
    private Sprite ReplaceSpriteTexture(Texture2D newTexture, Sprite sprite)
    {        
        //Sprite s = Sprite.Create(newTexture, sprite.textureRect, new Vector2(0.5f, 0.3f), sprite.pixelsPerUnit);
        return sprite;
    }
    
    /*
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
    */

    void OnCharacterChanged(Character c)
    {
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

        // TODO: When we have a helmetless spritesheet, use this check to switch spritesheet on the character
        /*
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
        */

        char_go.transform.position = new Vector3(c.X, c.Y, 0);
    }

    // helper function for shader replacement colors
    public static Color ColorFromIntRGB(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
    }
}
