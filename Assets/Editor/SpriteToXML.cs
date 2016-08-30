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
    private string spriteToXmlPath = Application.dataPath + "/Resources/Editor/SpriteToXML/";
    private string outputDirPath = string.Empty;
    private string inputDirPath = string.Empty;

    private Texture2D[] images;
    private Texture2D image;
    private Sprite[] sprites;
    private string[] filesInDir;

    private Version version = Version.v1;

    private string[] pixelPerUnitOptions = new string[] { "16", "32", "64", "128", "256", "512", "1024" };
    private int index = 1;

    private string[] columnRowOptions = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
    private int columnIndex = 0;
    private int rowIndex = 0;

    private bool isMultipleSprite = false;
    private bool showInstructions = false;
    private bool useCustomPivot = false;

    private float pivotX = 0.5f;
    private float pivotY = 0.5f;

    private string imageName;
    private int spriteCount = 0;

    private enum Version
    {
        v1,
        v2
    }

    [MenuItem("Window/Sprite Sheet To XML")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SpriteToXML));
    }

    private int PixelsPerUnit()
    {
        return int.Parse(pixelPerUnitOptions[index]);
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Version 0.1"))
        {
            version = Version.v1;            
        }

        if (GUILayout.Button("Version 0.2"))
        {
            version = Version.v2;
        }

        GUILayout.EndHorizontal();
        showInstructions = EditorGUILayout.ToggleLeft("Instructions", showInstructions);
        EditorGUILayout.Space();

        switch (version)
        {
            case Version.v1:
                ShowVersion1();
                break;
            case Version.v2:
                ShowVersion2();
                break;
        }        
    }

    private void ShowVersion1()
    {        
        GUILayout.Label("Unity Sprite Editor Based - Only 1 sprite at a time", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (showInstructions)
        {
            GUILayout.Label("Instructions", EditorStyles.boldLabel);            
            GUILayout.Label("1. Sprite must be in 'Resources/Editor/SpriteToXML'.");
            GUILayout.Label("2. Edit your sprite in Unity's sprite editor as normal.");
            GUILayout.Label("3. Select the folder to output the sprite and XML.");
            GUILayout.Label("4. Press 'Export' button");
            GUILayout.Label("5. XML will be generated moved along with the sprite to the specified folder");
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Set Output Folder"))
        {
            outputDirPath = EditorUtility.OpenFolderPanel("Select folder to save XML", outputDirPath, string.Empty);
        }

        if (GUILayout.Button("Export Sprite to XML"))
        {
            images = Resources.LoadAll<Texture2D>("Editor/SpriteToXML");
            sprites = Resources.LoadAll<Sprite>("Editor/SpriteToXML");
            
            filesInDir = Directory.GetFiles(spriteToXmlPath);

            if (images.Length > 1)
            {
                Debug.LogError("Place only one sprite in 'Resources/Editor/SpriteToXML'");
                return;
            }

            ExportSprites();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Current Path: " + outputDirPath);
        if (GUILayout.Button("Open Output Folder"))
        {
            EditorUtility.RevealInFinder(outputDirPath);
        }
    }

    private void ExportSprites()
    {
        Debug.Log("Files saved to: " + outputDirPath);

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
        if (outputDirPath == string.Empty)
        {
            Debug.LogError("SpriteToXML :: Please select a folder");
            return;
        }
        
        for (int i = 0; i < images.Length; i++)
        {
            XmlWriter writer = XmlWriter.Create(Path.Combine(outputDirPath, images[i].name + ".xml"));

            writer.WriteStartDocument();
            writer.WriteStartElement("Sprites");

            foreach (Sprite s in sprites)
            {
                writer.WriteStartElement("Sprite");
                writer.WriteAttributeString("name", s.name);
                writer.WriteAttributeString("x", (s.rect.x / s.pixelsPerUnit).ToString());
                writer.WriteAttributeString("y", (s.rect.y / s.pixelsPerUnit).ToString());
                writer.WriteAttributeString("w", (s.rect.width / s.pixelsPerUnit).ToString());
                writer.WriteAttributeString("h", (s.rect.height / s.pixelsPerUnit).ToString());
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
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".png", outputDirPath + "/" + images[i].name + ".png");
                }
                else if (s.Contains(".jpg"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".jpg", outputDirPath + "/" + images[i].name + ".jpg");
                }
                else if (s.Contains(".meta"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".meta", outputDirPath + "/" + images[i].name + ".meta");
                }
                else
                {
                    continue;
                }
            }                       
        }
    }

    private void ShowVersion2()
    {
        GUILayout.Label("Generates XML based on image and settings selected", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (showInstructions)
        {
            GUILayout.Label("Instructions", EditorStyles.boldLabel);
            GUILayout.Label("1. Select the image you want an XML for ");
            GUILayout.Label("2. Specify the rows and columns of the image ");
            GUILayout.Label("3. Select the folder to output the sprite and xml");
            GUILayout.Label("4. Press 'Export' button");
            GUILayout.Label("5. XML will generate and sprite will be moved to the specified folder");
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Select Image"))
        {
            inputDirPath = EditorUtility.OpenFilePanel("Select file to generate a XML for", inputDirPath, string.Empty);
        }

        if (GUILayout.Button("Set Output Folder"))
        {
            outputDirPath = EditorUtility.OpenFolderPanel("Select folder to save XML", outputDirPath, string.Empty);
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Pixels Per Unit:", EditorStyles.boldLabel);
        index = EditorGUILayout.Popup(index, pixelPerUnitOptions);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        isMultipleSprite = EditorGUILayout.ToggleLeft("Is this a Multiple Sprite image?", isMultipleSprite);
        useCustomPivot = EditorGUILayout.ToggleLeft("Use a custom pivot?", useCustomPivot);

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (isMultipleSprite)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rows:", EditorStyles.boldLabel);
            rowIndex = EditorGUILayout.Popup(rowIndex, columnRowOptions);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Columns:", EditorStyles.boldLabel);
            columnIndex = EditorGUILayout.Popup(columnIndex, columnRowOptions);
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        if (useCustomPivot)
        {
            pivotX = EditorGUILayout.FloatField("Pivot X", pivotX);
            pivotY = EditorGUILayout.FloatField("Pivot Y", pivotY);
        }

        if (inputDirPath != string.Empty && outputDirPath != string.Empty)
        {
            if (GUILayout.Button("Export Sprite to XML"))
            {
                Debug.ULogChannel("SpriteToXML", "File Loaded: " + inputDirPath);

                if (inputDirPath.Contains(".png") || inputDirPath.Contains(".jpg") || inputDirPath.Contains(".jpeg"))
                {
                    if (columnIndex == 0 && rowIndex == 0 && isMultipleSprite == true)
                    {
                        Debug.ULogErrorChannel("SpriteToXML", "Please select more than 1 Row/Column");
                    }
                    else
                    {
                        LoadImage(inputDirPath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Select proper filetype", "You must select a PNG or JPG image!", "OK");
                    Debug.ULogErrorChannel("SpriteToXML", "Please select a PNG or JPG image");
                }
            }
        }

        if (outputDirPath != string.Empty)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Current Path: " + outputDirPath);
            if (GUILayout.Button("Open Output Folder"))
            {
                EditorUtility.RevealInFinder(outputDirPath);
            }
        }
    }

    private void LoadImage(string filePath)
    {
        ////Debug.Log("LoadImage: " + filePath);

        // Load the file into a texture.
        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

        // Create some kind of dummy instance of Texture2D.
        // LoadImage will correctly resize the texture based on the image file.
        Texture2D imageTexture = new Texture2D(2, 2);

        // Image was successfully loaded.
        if (imageTexture.LoadImage(imageBytes))
        {
            imageName = Path.GetFileNameWithoutExtension(filePath);            

            XmlWriter writer = XmlWriter.Create(Path.Combine(outputDirPath, imageName + ".xml"));

            writer.WriteStartDocument();
            writer.WriteStartElement("Sprites");

            switch (isMultipleSprite)
            {
                case false:
                    SingleSpriteXML(writer, imageTexture);
                    break;
                case true:
                    MultiSpriteXML(writer, imageTexture);
                    break;
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            spriteCount = 0;
            inputDirPath = string.Empty;
        }        
    }

    private void SingleSpriteXML(XmlWriter writer, Texture2D imageTexture)
    {
        foreach (Sprite s in sprites)
        {
            writer.WriteStartElement("Sprite");
            writer.WriteAttributeString("name", s.name);
            writer.WriteAttributeString("x", "0");
            writer.WriteAttributeString("y", "0");
            writer.WriteAttributeString("w", (imageTexture.width / PixelsPerUnit()).ToString());
            writer.WriteAttributeString("h", (imageTexture.height / PixelsPerUnit()).ToString());
            writer.WriteAttributeString("pixelPerUnit", PixelsPerUnit().ToString());

            if (useCustomPivot)
            {
                writer.WriteAttributeString("pivotX", pivotX.ToString());                
                writer.WriteAttributeString("pivotY", pivotY.ToString());
            }

            writer.WriteEndElement();
        }
    }

    private void MultiSpriteXML(XmlWriter writer, Texture2D imageTexture)
    {
        for (int y = int.Parse(columnRowOptions[rowIndex]) - 1; y > -1; y--)
        {
            for (int x = 0; x < int.Parse(columnRowOptions[columnIndex]); x++)                
            {
                writer.WriteStartElement("Sprite");
                writer.WriteAttributeString("name", imageName + "_" + spriteCount);
                writer.WriteAttributeString("x", (((imageTexture.width / int.Parse(columnRowOptions[columnIndex])) / PixelsPerUnit()) * x).ToString());
                writer.WriteAttributeString("y", (((imageTexture.height / int.Parse(columnRowOptions[rowIndex])) / PixelsPerUnit()) * y).ToString());
                writer.WriteAttributeString("w", ((imageTexture.width / int.Parse(columnRowOptions[columnIndex])) / PixelsPerUnit()).ToString());
                writer.WriteAttributeString("h", ((imageTexture.height / int.Parse(columnRowOptions[rowIndex])) / PixelsPerUnit()).ToString());
                writer.WriteAttributeString("pixelPerUnit", PixelsPerUnit().ToString());

                if (useCustomPivot)
                {
                    writer.WriteAttributeString("pivotX", pivotX.ToString());
                    writer.WriteAttributeString("pivotY", pivotY.ToString());
                }

                writer.WriteEndElement();

                spriteCount++;
            }
        }        
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }
}
