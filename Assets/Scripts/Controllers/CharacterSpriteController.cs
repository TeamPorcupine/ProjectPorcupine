#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;

public enum SpriteSwapRedColor
{
    UNIFORMCOLOR = 129,
    UNIFORMCOLORLIGHT = 199,
    UNIFORMCOLORDARK = 97,
    UNIFORMSTRIPECOLOR = 151,
    UNIFORMSTRIPECOLORLIGHT = 201,
    UNIFORMSTRIPECOLORDARK = 101,
    HAIRCOLOR = 152,
    HAIRCOLORLIGHT = 202,
    HAIRCOLORDARK = 102,
    SKINCOLOR = 244,
    SKINCOLORDARK = 229
}

public class CharacterSpriteController
{
    private Dictionary<Character, GameObject> characterGameObjectMap;

    private World world;
    private GameObject characterParent;

    private Color[] swapSpriteColors;
    private Color[] skinColors;

    // Use this for initialization
    public CharacterSpriteController(World currentWorld)
    {
        world = currentWorld;
        characterParent = new GameObject("Characters");

        // default skincolors to pick at random
        skinColors = new Color[]
        {
            ColorFromIntRGB(245, 217, 203),
            ColorFromIntRGB(237, 191, 167),
            ColorFromIntRGB(211, 142, 111),
            ColorFromIntRGB(234, 183, 138),
            ColorFromIntRGB(197, 132, 92),
            ColorFromIntRGB(88, 59, 43)
        };

        // prepare swap texture for shader
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;
        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }

        colorSwapTex.Apply();
        swapSpriteColors = new Color[colorSwapTex.width];

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.OnCharacterCreated += OnCharacterCreated;

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character c in world.characters)
        {
            OnCharacterCreated(c);
        }
    }

    // helper function for shader replacement colors
    public static Color ColorFromIntRGB(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
    }

    public void OnCharacterCreated(Character c)
    {
        // This creates a new GameObject and adds it to our scene.
        GameObject char_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        characterGameObjectMap.Add(c, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(c.X, c.Y, 0);
        char_go.transform.SetParent(characterParent.transform, true);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";

        // Add material with color replacement shader, and generate color replacement texture
        sr.material = GetMaterial(c);
        c.animation = new CharacterAnimation(c, sr);

        // Add the inventory sprite onto the character
        GameObject inv_go = new GameObject("Inventory");
        SpriteRenderer inv_sr = inv_go.AddComponent<SpriteRenderer>();
        inv_sr.sortingOrder = 1;
        inv_sr.sortingLayerName = "Characters";
        inv_go.transform.SetParent(char_go.transform);
        inv_go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // Config needs to be added to XML
        inv_go.transform.localPosition = new Vector3(0, -0.37f, 0); // Config needs to be added to XML

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        c.OnCharacterChanged += OnCharacterChanged;
    }

    // Add material with color replacement shader, and generate color replacement texture
    private Material GetMaterial(Character c)
    {
        // this 256x1 texture is transparent. Each pixel represents the red color value to replace.
        // if pixel 10 is not transparent, every color with r=10 will be replaced by the color of the pixel
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;

        // Reset texture
        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }

        colorSwapTex.Apply();

        // Define the swapping colors. Add white to hightlights and black to shadows        
        Color newColorLight = Color.Lerp(c.GetCharacterColor(), ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newColorDark = Color.Lerp(c.GetCharacterColor(), ColorFromIntRGB(0, 0, 0), 0.5f);
        Color newSkinColor = skinColors[UnityEngine.Random.Range(0, 5)];
        Color newSkinColorDark = Color.Lerp(newSkinColor, ColorFromIntRGB(0, 0, 0), 0.2f);
        int uniColor = UnityEngine.Random.Range(80, 230);
        Color newUniformColor = ColorFromIntRGB(uniColor, uniColor, uniColor);
        Color newUniformColorLight = Color.Lerp(newUniformColor, ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newUniformColorDark = Color.Lerp(newUniformColor, ColorFromIntRGB(0, 0, 0), 0.2f);

        // add the colors to the texture
        // TODO: Do something similar for HAIRCOLOR, when we have a character with visible hair
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLOR, newUniformColor);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORLIGHT, newUniformColorLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORDARK, newUniformColorDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLOR, c.GetCharacterColor());
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLORLIGHT, newColorLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLORDARK, newColorDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.SKINCOLOR, newSkinColor);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.SKINCOLORDARK, newSkinColorDark);
        colorSwapTex.Apply();

        // load material and shader
        Material swapMaterial = new Material(Resources.Load<Material>("Shaders/ColorSwap"));
        Shader swapShader = Resources.Load<Shader>("Shaders/Sprites-ColorSwap");
        swapMaterial.shader = swapShader;
        swapMaterial.SetTexture("_SwapTex", colorSwapTex);

        return swapMaterial;
    }

    private Texture2D SwapColor(Texture2D tex, SpriteSwapRedColor index, Color color)
    {
        swapSpriteColors[(int)index] = color;
        tex.SetPixel((int)index, 0, color);
        return tex;
    }

    private void OnCharacterChanged(Character c)
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
            Debug.ULogErrorChannel("CharacterSpriteController", "OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        char_go.transform.position = new Vector3(c.X, c.Y, 0);
    }
}
