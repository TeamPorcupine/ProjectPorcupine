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
using System.IO;
using System.Xml;

public class SpriteManager : MonoBehaviour
{

    // Sprite Manager isn't responsible for actually creating GameObjects.
    // That is going to be the job of the individual ________SpriteController scripts.
    // Our job is simply to load all sprites from disk and keep the organized.

    static public SpriteManager current;

    public static Texture2D noRescourceTexture;

    Dictionary<string, Sprite> sprites;

    void Awake()
    {
        if (noRescourceTexture == null)
        {
            //Generate a 32x32 magenta image
            noRescourceTexture = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            Color32[] pixels = noRescourceTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 0, 255, 255);
            }
            noRescourceTexture.SetPixels32(pixels);
            noRescourceTexture.Apply();
        }
    }

    // Use this for initialization
    void OnEnable()
    {
        current = this;

        LoadSprites();
    }

    void LoadSprites()
    {
        sprites = new Dictionary<string, Sprite>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Images");
        string modsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Mods");
        //filePath = System.IO.Path.Combine( Application.streamingAssetsPath, "CursorCircle.png" );

        //LoadSprite("CursorCircle", filePath);

        LoadSpritesFromDirectory(filePath);

        DirectoryInfo[] mods = WorldController.Instance.modsManager.GetMods();
        foreach (DirectoryInfo mod in mods)
        {
            string modImagesPath = Path.Combine(mod.FullName, "Images");
            if (Directory.Exists(modImagesPath))
            {
                LoadSpritesFromDirectory(modImagesPath);
            }
        }
    }

    void LoadSpritesFromDirectory(string filePath)
    {
        Logger.Log("LoadSpritesFromDirectory: " + filePath);
        // First, we're going to see if we have any more sub-directories,
        // if so -- call LoadSpritesFromDirectory on that.

        string[] subDirs = Directory.GetDirectories(filePath);
        foreach (string sd in subDirs)
        {
            LoadSpritesFromDirectory(sd);
        }

        string[] filesInDir = Directory.GetFiles(filePath);
        foreach (string fn in filesInDir)
        {
            // Is this an image file?
            // Unity's LoadImage seems to support only png and jpg
            // NOTE: We **could** try to check file extensions, but why not just
            // have Unity **attemp** to load the image, and if it doesn't work,
            // then I guess it wasn't an image! An advantage of this, is that we
            // don't have to worry about oddball filenames, nor do we have to worry
            // about what happens if Unity adds support for more image format
            // or drops support for existing ones.

            string spriteCategory = new DirectoryInfo(filePath).Name;

            LoadImage(spriteCategory, fn);
        }

    }

    void LoadImage(string spriteCategory, string filePath)
    {
        //Logger.Log("LoadImage: " + filePath);

        // TODO:  LoadImage is returning TRUE for things like .meta and .xml files.  What??!
        //		So as a temporary fix, let's just bail if we have something we KNOW should not
        //  	be an image.
        if (filePath.Contains(".xml") || filePath.Contains(".meta") || filePath.Contains(".db"))
        {
            return;
        }

        // Load the file into a texture
        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

        Texture2D imageTexture = new Texture2D(2, 2);	// Create some kind of dummy instance of Texture2D
        // LoadImage will correctly resize the texture based on the image file


        if (imageTexture.LoadImage(imageBytes))
        {

            // Image was successfully loaded.
            // So let's see if there's a matching XML file for this image.
            string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
            string basePath = Path.GetDirectoryName(filePath);

            // NOTE: The extension must be in lower case!
            string xmlPath = System.IO.Path.Combine(basePath, baseSpriteName + ".xml");

            if (System.IO.File.Exists(xmlPath))
            {
                string xmlText = System.IO.File.ReadAllText(xmlPath);
                // TODO: Loop through the xml file finding all the <sprite> tags
                // and calling LoadSprite once for each of them.
                
                XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

                // Set our cursor on the first Sprite we find.
                if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                {
                    do
                    {
                        ReadSpriteFromXml(spriteCategory, reader, imageTexture);
                    } while(reader.ReadToNextSibling("Sprite"));
                }
                else
                {
                    Logger.LogError("Could not find a <Sprites> tag.");
                    return;
                }

            }
            else
            {
                // File couldn't be read, probably because it doesn't exist
                // so we'll just assume the whole image is one sprite with pixelPerUnit = 32
                LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), 32, new Vector2(0.5f, 0.5f));

            }

            // Attempt to load/parse the XML file to get information on the sprite(s)

        }
			
        // else, the file wasn't actually a image file, so just move on.

    }

    void ReadSpriteFromXml(string spriteCategory, XmlReader reader, Texture2D imageTexture)
    {
        //Logger.Log("ReadSpriteFromXml");
        string name = reader.GetAttribute("name");
        int x = int.Parse(reader.GetAttribute("x"));
        int y = int.Parse(reader.GetAttribute("y"));
        int w = int.Parse(reader.GetAttribute("w"));
        int h = int.Parse(reader.GetAttribute("h"));

        //Try read the pivot point
        string pivotXAttribute = reader.GetAttribute("pivotX");
        float pivotX;
        if (float.TryParse(pivotXAttribute, out pivotX) == false)
        {
            //If pivot point didn't exist default to 0.5f
            pivotX = 0.5f;
        }

        //Clamp pivot between 0..1
        pivotX = Mathf.Clamp01(pivotX);

        //Try read the pivot point
        string pivotYAttribute = reader.GetAttribute("pivotY");
        float pivotY;
        if (float.TryParse(pivotYAttribute, out pivotY) == false)
        {
            //If pivot point didn't exist default to 0.5f
            pivotY = 0.5f;
        }

        //Clamp pivot between 0..1
        pivotY = Mathf.Clamp01(pivotY);
        
        int pixelPerUnit = int.Parse(reader.GetAttribute("pixelPerUnit"));

        LoadSprite(spriteCategory, name, imageTexture, new Rect(x, y, w, h), pixelPerUnit, new Vector2(pivotX, pivotY));
    }

    void LoadSprite(string spriteCategory, string spriteName, Texture2D imageTexture, Rect spriteCoordinates, int pixelsPerUnit, Vector2 pivotPoint)
    {
        spriteName = spriteCategory + "/" + spriteName;
        //Logger.Log("LoadSprite: " + spriteName);

        Sprite s = Sprite.Create(imageTexture, spriteCoordinates, pivotPoint, pixelsPerUnit);

        sprites[spriteName] = s;
    }

    public Sprite GetSprite(string categoryName, string spriteName)
    {
        //Logger.Log(spriteName);

        spriteName = categoryName + "/" + spriteName;

        if (sprites.ContainsKey(spriteName) == false)
        {
            //Logger.LogError("No sprite with name: " + spriteName);
            
            //Return a magenta image
            return Sprite.Create(noRescourceTexture, new Rect(Vector2.zero, new Vector3(32, 32)), new Vector2(0.5f, 0.5f), 32);
        }

        return sprites[spriteName];
    }
}
