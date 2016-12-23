#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

/// <summary>
/// Sprite Manager isn't responsible for actually creating GameObjects.
/// That is going to be the job of the individual ________SpriteController scripts.
/// Our job is simply to load all sprites from disk and keep the organized.
/// </summary>
public class SpriteManager
{
    // A sprite image with a "ph_" as a prefix will be loaded as a placeholder if the normal spite image is missing.
    // This is used to easily identity spires that needs improvement.
    private const string PlaceHolderPrefix = "ph_";

    private static Texture2D noResourceTexture;

    private static Dictionary<string, Sprite> sprites;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteManager"/> class.
    /// </summary>
    public SpriteManager()
    {
        sprites = new Dictionary<string, Sprite>();

        CreateNoTexture();
    }

    /// <summary>
    /// Gets the sprite for the given category and name.
    /// </summary>
    /// <returns>The sprite.</returns>
    /// <param name="categoryName">Category name.</param>
    /// <param name="spriteName">Sprite name.</param>
    public static Sprite GetSprite(string categoryName, string spriteName)
    {
        Sprite sprite = null;

        string spriteNamePlaceHolder = categoryName + "/" + PlaceHolderPrefix + spriteName;
        spriteName = categoryName + "/" + spriteName;

        if (sprites.ContainsKey(spriteName))
        {
            sprite = sprites[spriteName];
        }
        else if (sprites.ContainsKey(spriteNamePlaceHolder))
        {
            sprite = sprites[spriteNamePlaceHolder];
        }
        else
        {
            sprite = Sprite.Create(noResourceTexture, new Rect(Vector2.zero, new Vector3(32, 32)), new Vector2(0.5f, 0.5f), 32);
            UnityDebugger.Debugger.LogWarningFormat("SpriteManager", "No sprite: {0}, using fallback sprite.", spriteName);
        }

        return sprite;
    }

    /// <summary>
    /// Gets a random sprite from a category.
    /// </summary>
    /// <returns>The sprite.</returns>
    /// <param name="categoryName">Category name.</param>
    public static Sprite GetRandomSprite(string categoryName)
    {
        Sprite sprite = null;

        Dictionary<string, Sprite> spritesFromCategory = sprites.Where(p => p.Key.StartsWith(categoryName)).ToDictionary(p => p.Key, p => p.Value);

        if (spritesFromCategory.Count > 0)
        {
            System.Random rand = new System.Random();
            sprite = spritesFromCategory.ElementAt(rand.Next(0, spritesFromCategory.Count)).Value;
        }

        return sprite;
    }

    /// <summary>
    /// Determines if there is a sprite with the specified category and name.
    /// </summary>
    /// <returns><c>true</c> if there is a sprite with the specified category and name; otherwise, <c>false</c>.</returns>
    /// <param name="categoryName">Category name.</param>
    /// <param name="spriteName">Sprite name.</param>
    public static bool HasSprite(string categoryName, string spriteName)
    {
        string spriteNamePlaceHolder = categoryName + "/" + PlaceHolderPrefix + spriteName;
        spriteName = categoryName + "/" + spriteName;
        return sprites.ContainsKey(spriteName) || sprites.ContainsKey(spriteNamePlaceHolder);
    }

    /// <summary>
    /// Loads the sprites from the given directory path.
    /// </summary>
    /// <param name="directoryPath">Directory path.</param>
    public static void LoadSpriteFiles(string directoryPath)
    {
        // First, we're going to see if we have any more sub-directories,
        // if so -- call LoadSpritesFromDirectory on that.
        string[] subDirectories = Directory.GetDirectories(directoryPath);
        foreach (string subDirectory in subDirectories)
        {
            LoadSpriteFiles(subDirectory);
        }

        string[] filesInDir = Directory.GetFiles(directoryPath);
        foreach (string fileName in filesInDir)
        {
            // Is this an image file?
            // Unity's LoadImage seems to support only png and jpg
            // NOTE: We **could** try to check file extensions, but why not just
            // have Unity **attemp** to load the image, and if it doesn't work,
            // then I guess it wasn't an image! An advantage of this, is that we
            // don't have to worry about oddball filenames, nor do we have to worry
            // about what happens if Unity adds support for more image format
            // or drops support for existing ones.
            string spriteCategory = new DirectoryInfo(directoryPath).Name;

            LoadImage(spriteCategory, fileName);
        }
    }

    /// <summary>
    /// Loads a single image from the given filePath, if possible.
    /// </summary>
    /// <param name="spriteCategory">Sprite category.</param>
    /// <param name="filePath">File path.</param>
    private static void LoadImage(string spriteCategory, string filePath)
    {
        // TODO:  LoadImage is returning TRUE for things like .meta and .xml files.  What??!
        //      So as a temporary fix, let's just bail if we have something we KNOW should not
        //      be an image.
        if (filePath.Contains(".xml") || filePath.Contains(".meta") || filePath.Contains(".db"))
        {
            return;
        }

        // Load the file into a texture
        byte[] imageBytes = File.ReadAllBytes(filePath);

        // Create some kind of dummy instance of Texture2D
        Texture2D imageTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        // LoadImage will correctly resize the texture based on the image file
        if (imageTexture.LoadImage(imageBytes))
        {
            // Image was successfully loaded.
            imageTexture.filterMode = FilterMode.Point;

            // So let's see if there's a matching XML file for this image.
            string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
            string basePath = Path.GetDirectoryName(filePath);

            // NOTE: The extension must be in lower case!
            string xmlPath = Path.Combine(basePath, baseSpriteName + ".xml");

            if (File.Exists(xmlPath))
            {
                string xmlText = File.ReadAllText(xmlPath);

                // Loop through the xml file finding all the <sprite> tags
                // and calling LoadSprite once for each of them.
                XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

                // Set our cursor on the first Sprite we find.
                if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                {
                    do
                    {
                        ReadSpriteFromXml(spriteCategory, reader, imageTexture);
                    }
                    while (reader.ReadToNextSibling("Sprite"));
                }
                else
                {
                    UnityDebugger.Debugger.LogError("SpriteManager", "Could not find a <Sprites> tag.");
                    return;
                }
            }
            else
            {
                // File couldn't be read, probably because it doesn't exist
                // so we'll just assume the whole image is one sprite with pixelPerUnit = 64
                LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), 64, new Vector2(0.5f, 0.5f));
            }

            // Attempt to load/parse the XML file to get information on the sprite(s)
        }

        // Else, the file wasn't actually a image file, so just move on.
    }

    /// <summary>
    /// Reads the sprite from xml for the image.
    /// </summary>
    /// <param name="spriteCategory">Sprite category.</param>
    /// <param name="reader">The Xml Reader.</param>
    /// <param name="imageTexture">Image texture.</param>
    private static void ReadSpriteFromXml(string spriteCategory, XmlReader reader, Texture2D imageTexture)
    {
        string name = reader.GetAttribute("name");
        int x = int.Parse(reader.GetAttribute("x"));
        int y = int.Parse(reader.GetAttribute("y"));
        int w = int.Parse(reader.GetAttribute("w"));
        int h = int.Parse(reader.GetAttribute("h"));

        float pivotX = ReadPivot(reader, "pivotX");
        float pivotY = ReadPivot(reader, "pivotY");

        int pixelPerUnit = int.Parse(reader.GetAttribute("pixelPerUnit"));

        LoadSprite(spriteCategory, name, imageTexture, new Rect(x * pixelPerUnit, y * pixelPerUnit, w * pixelPerUnit, h * pixelPerUnit), pixelPerUnit, new Vector2(pivotX, pivotY));
    }

    /// <summary>
    /// Reads the x or y pivot from the XML reader.
    /// </summary>
    /// <returns>The pivot.</returns>
    /// <param name="reader">The Xml Reader.</param>
    /// <param name="pivotName">The pivot attribute name.</param>
    private static float ReadPivot(XmlReader reader, string pivotName)
    {
        string pivotAttribute = reader.GetAttribute(pivotName);
        float pivot;
        if (float.TryParse(pivotAttribute, out pivot) == false)
        {
            // If pivot point didn't exist default to 0.5f
            pivot = 0.5f;
        }

        // Clamp pivot between 0..1
        pivot = Mathf.Clamp01(pivot);

        return pivot;
    }

    /// <summary>
    /// Creates and stores the sprite.
    /// </summary>
    /// <param name="spriteCategory">Sprite category.</param>
    /// <param name="spriteName">Sprite name.</param>
    /// <param name="imageTexture">Image texture.</param>
    /// <param name="spriteCoordinates">Sprite coordinates.</param>
    /// <param name="pixelsPerUnit">Pixels per unit.</param>
    /// <param name="pivotPoint">Pivot point.</param>
    private static void LoadSprite(string spriteCategory, string spriteName, Texture2D imageTexture, Rect spriteCoordinates, int pixelsPerUnit, Vector2 pivotPoint)
    {
        spriteName = spriteCategory + "/" + spriteName;

        Sprite s = Sprite.Create(imageTexture, spriteCoordinates, pivotPoint, pixelsPerUnit);

        sprites[spriteName] = s;
    }

    /// <summary>
    /// Creates the no resource texture.
    /// </summary>
    private void CreateNoTexture()
    {
        // Generate a 32x32 magenta image
        noResourceTexture = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        Color32[] pixels = noResourceTexture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(255, 0, 255, 255);
        }

        noResourceTexture.SetPixels32(pixels);
        noResourceTexture.Apply();
    }
}
