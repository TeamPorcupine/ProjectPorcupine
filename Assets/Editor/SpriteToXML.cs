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
    private string[] filesInDir;

    [MenuItem("Window/Sprite Sheet To XML")]
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

            filesInDir = Directory.GetFiles(Application.dataPath + "/Resources/Editor/SpriteToXML/");

            if (images.Length > 1)
            {
                Debug.LogError("Place only one sprite in 'Resources/Editor/SpriteToXML'");
                return;
            }

            ExportSprites();
        }

     
        GUILayout.Label("Current Path: " + dirPath +  "");

        if (GUILayout.Button("Open Output Folder"))
        {
            EditorUtility.RevealInFinder(dirPath);
        }

    }    

    private void ExportSprites()
    {
        Debug.Log("Files saved to: " + dirPath);

        foreach (string fn in filesInDir)
        {
            Debug.Log("files in dir: " + fn);
        }   
            
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
            foreach (string s in filesInDir)
            {
                if (s.Contains(".png"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".png", dirPath + "/" + images[i].name + ".png");
                }
                else if (s.Contains(".jpg"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".jpg", dirPath + "/" + images[i].name + ".jpg");
                }
                else if (s.Contains(".meta"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".meta", dirPath + "/" + images[i].name + ".meta");
                }
                else
                {
                    continue;
                }
            }                       
        }
    }    
}
