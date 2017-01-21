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
    UNIFORMCOLORVERYLIGHT = 155,
    UNIFORMCOLORLIGHT = 132,
    UNIFORMCOLOR = 105,
    UNIFORMCOLORDARK = 86,
    UNIFORMCOLORVERYDARK = 68,
    UNIFORMSTRIPECOLORVERYLIGHT = 194,
    UNIFORMSTRIPECOLORLIGHT = 180,
    UNIFORMSTRIPECOLOR = 163,
    UNIFORMSTRIPECOLORDARK = 152,
    UNIFORMSTRIPECOLOVVERYRDARK = 141,
    HAIRCOLOR = 152,
    HAIRCOLORLIGHT = 202,
    HAIRCOLORDARK = 102,
    SKINCOLOR = 238,
    SKINCOLORDARK = 217,
    SKINCOLORVERYDARK = 143
}

public class CharacterSpriteController : BaseSpriteController<Character>
{
    private Color[] swapSpriteColors;

    // Use this for initialization
    public CharacterSpriteController(World world) : base(world, "Characters")
    {        
        // prepare swap texture for shader
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;
        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }

        colorSwapTex.Apply();
        swapSpriteColors = new Color[colorSwapTex.width];

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.CharacterManager.Created += OnCreated;

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character character in world.CharacterManager)
        {
            OnCreated(character);
        }
    }   

    public override void RemoveAll()
    {
        world.CharacterManager.Created -= OnCreated;

        foreach (Character c in world.CharacterManager)
        {
            c.OnCharacterChanged -= OnChanged; 
        }

        base.RemoveAll();
    }

    protected override void OnCreated(Character character)
    {
        // This creates a new GameObject and adds it to our scene.
        GameObject char_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(character, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.X, character.Y, character.Z);
        char_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";

        // Add material with color replacement shader, and generate color replacement texture
        sr.material = GetMaterial(character);
        character.animation = new Animation.CharacterAnimation(character, sr);

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
        character.OnCharacterChanged += OnChanged;
    }

    protected override void OnChanged(Character character)
    {
        // Make sure the furniture's graphics are correct.
        GameObject inventoryGo = objectGameObjectMap[character].transform.GetChild(0).gameObject;
        SpriteRenderer inventorySR = inventoryGo.GetComponent<SpriteRenderer>();

        // Important to set the characters SortOrder first.
        int charSortOrder = character.animation.SetAndGetSortOrder();
        if (character.inventory != null)
        {
            inventorySR = InventorySpriteController.SetSprite(inventoryGo, character.inventory);
            inventorySR.sortingOrder = charSortOrder + 1;
        }
        else
        {
            inventorySR.sprite = null;
            if (inventoryGo.transform.childCount > 0)
            {
                // Inventory should only ever have one child to destroy.
                GameObject.Destroy(inventoryGo.transform.GetChild(0).gameObject);
            }
        }

        if (objectGameObjectMap.ContainsKey(character) == false)
        {
            UnityDebugger.Debugger.LogError("CharacterSpriteController", "OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = objectGameObjectMap[character];

        char_go.transform.position = new Vector3(character.X, character.Y, character.Z);
    }

    protected override void OnRemoved(Character character)
    {
        character.OnCharacterChanged -= OnChanged;
        GameObject char_go = objectGameObjectMap[character];
        objectGameObjectMap.Remove(character);
        GameObject.Destroy(char_go);
    }

    // Add material with color replacement shader, and generate color replacement texture
    private Material GetMaterial(Character character)
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

        // Define the swapping colors. Add white to highlights and black to shadows        
        Color newUniformStripeColorLight = Color.Lerp(character.GetCharacterColor(), ColorUtilities.ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newUniformStripeColorVeryLight = Color.Lerp(newUniformStripeColorLight, ColorUtilities.ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newUniformStripeColorDark = Color.Lerp(character.GetCharacterColor(), ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.5f);
        Color newUniformStripeColorVeryDark = Color.Lerp(newUniformStripeColorDark, ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.5f);
        Color newSkinColor = character.GetCharacterSkinColor();
        Color newSkinColorDark = Color.Lerp(newSkinColor, ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.2f);
        Color newSkinColorVeryDark = Color.Lerp(newSkinColorDark, ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.5f);
        Color newUniformColor = character.GetCharacterUniformColor();
        Color newUniformColorLight = Color.Lerp(newUniformColor, ColorUtilities.ColorFromIntRGB(255, 255, 255), 0.2f);
        Color newUniformColorVeryLight = Color.Lerp(newUniformColorLight, ColorUtilities.ColorFromIntRGB(255, 255, 255), 0.5f);
        Color newUniformColorDark = Color.Lerp(newUniformColor, ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.2f);
        Color newUniformColorVeryDark = Color.Lerp(newUniformColorDark, ColorUtilities.ColorFromIntRGB(0, 0, 0), 0.5f);

        // add the colors to the texture
        // TODO: Do something similar for HAIRCOLOR, when we have a character with visible hair
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLOR, newUniformColor);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORLIGHT, newUniformColorLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORVERYLIGHT, newUniformColorVeryLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORDARK, newUniformColorDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMCOLORVERYDARK, newUniformColorVeryDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLOR, character.GetCharacterColor());
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLORLIGHT, newUniformStripeColorLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLORVERYLIGHT, newUniformStripeColorVeryLight);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLORDARK, newUniformStripeColorDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.UNIFORMSTRIPECOLOVVERYRDARK, newUniformStripeColorVeryDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.SKINCOLOR, newSkinColor);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.SKINCOLORDARK, newSkinColorDark);
        colorSwapTex = SwapColor(colorSwapTex, SpriteSwapRedColor.SKINCOLORVERYDARK, newSkinColorVeryDark);
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
}
