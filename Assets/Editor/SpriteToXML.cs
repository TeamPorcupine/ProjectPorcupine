#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class SpriteToXML : EditorWindow
{
    private const string Warning = "Only 1 .png sprite at a time";
    private const string Step1 = "1. Place sprite in 'Resources/Editor/SpriteToXML' ";
    private const string Step2 = "2. Edit your sprite in Unity's sprite editor as normal";    
    private const string Step3 = "3. Select the folder to output the sprite and xml";
    private const string Step4 = "4. Press export button";
    private const string Step5 = "5. Xml and sprite will be moved to specified folder";

    private string dirPath = Application.dataPath + "StreamingAssets/Images/";

    private Texture2D[] images;
    private Sprite[] sprites;

    [MenuItem("Window/Sprite Sheet To Xml")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SpriteToXML));
    }

    private void OnGUI()
    {
        GUILayout.Label("Instructions", EditorStyles.boldLabel);
        GUILayout.Label(Warning, EditorStyles.boldLabel);
        GUILayout.Label(Step1);        
        GUILayout.Label(Step2);
        GUILayout.Label(Step3);
        GUILayout.Label(Step4);
        GUILayout.Label(Step5);
        
        
        if (GUILayout.Button("Set Output Folder"))
        {
            dirPath = EditorUtility.OpenFolderPanel("Select folder to save XML", dirPath, string.Empty);
        }

        if (GUILayout.Button("Export Sprite to XML"))
        {

            images = Resources.LoadAll<Texture2D>("Editor/SpriteToXML");
            sprites = Resources.LoadAll<Sprite>("Editor/SpriteToXML");
            LoadSpritesFromDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/");


            if (images.Length > 1)
            {
                Debug.LogError("Place only one sprite in 'Resources/Editor/SpriteToXML'");
                return;
            }

            ExportSprites();
        }
    }    

    private void ExportSprites()
    {
        Debug.Log("Files saved to: " + dirPath);    
            
        foreach (Texture2D t in images)
        {
            Debug.Log("Filename: " + t.name);            
        }

        WriteXml();        
    }   

    private void WriteXml()
    {
        if (dirPath == string.Empty)
        {
            Debug.LogError("SpriteToXML :: Please select a folder");
            return;
        }
        
        for (int i = 0; i < images.Length; i++)
        {
            XmlWriter writer = XmlWriter.Create(Path.Combine(dirPath, images[i].name + ".xml"));

            writer.WriteStartDocument();
            writer.WriteStartElement("Sprites");

            foreach (Sprite s in sprites)
            {
                writer.WriteStartElement("Sprite");
                writer.WriteAttributeString("Name", s.name);
                writer.WriteAttributeString("x", s.rect.x.ToString());
                writer.WriteAttributeString("y", s.rect.y.ToString());
                writer.WriteAttributeString("w", s.rect.width.ToString());
                writer.WriteAttributeString("h", s.rect.height.ToString());
                writer.WriteAttributeString("pixelPerUnit", s.pixelsPerUnit.ToString());

                float pivotX = s.pivot.x / s.rect.width;
                writer.WriteAttributeString("pivotX", pivotX.ToString());

                float pivotY = s.pivot.y / s.rect.height;
                writer.WriteAttributeString("pivotY", pivotY.ToString());

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();            


            // Move the .png and meta file to the same directory as the xml.
            FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".png", dirPath + "/" + images[i].name + ".png");
            FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".meta", dirPath + "/" + images[i].name + ".meta");
        }
    }

    private void LoadSpritesFromDirectory(string filePath)
    {
        Debug.Log("LoadSpritesFromDirectory: " + filePath);
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
            Debug.Log(spriteCategory);
            //LoadImage(spriteCategory, fn);
        }

    }
}
