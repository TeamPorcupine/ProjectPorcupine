#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
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
    private int index = 2;

    private string[] columnRowOptions = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
    private int columnIndex = 0;
    private int rowIndex = 0;

    private bool isMultipleSprite = false;
    private bool showInstructions = false;
    private bool useCustomPivot = false;
    private bool textureLoaded = false;

    private float pivotX = 0.5f;
    private float pivotY = 0.5f;

    private string imageName = string.Empty;
    private string imageExt = string.Empty;

    private int spriteCount = 0;    

    private Texture2D myTexture = null;

    private Vector2 scrollPosition;
    private EditorWindow window;

    private enum Version
    {
        v1,
        v2
    }

    [MenuItem("Window/Sprite To XML")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SpriteToXML));
    }

    private int PixelsPerUnit()
    {
        return int.Parse(pixelPerUnitOptions[index]);
    }

    private void Awake()
    {
        window = GetWindow(typeof(SpriteToXML));
        window.minSize = new Vector2(460, 680);
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal("Box");

        switch (version)
        {
            case Version.v1:
                if (GUILayout.Button("Unity Based"))
                {
                    version = Version.v1;
                }

                if (GUILayout.Button("Settings Based", EditorStyles.miniButton))
                {
                    version = Version.v2;
                }

                break;

            case Version.v2:
                if (GUILayout.Button("Unity Based", EditorStyles.miniButton))
                {
                    version = Version.v1;
                }

                if (GUILayout.Button("Settings Based"))
                {
                    version = Version.v2;
                }

                break;
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
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Instructions", EditorStyles.boldLabel);            
            GUILayout.Label("1. Sprite must be in 'Resources/Editor/SpriteToXML'.");
            GUILayout.Label("2. Edit your sprite in Unity's sprite editor as normal.");
            GUILayout.Label("3. Select the folder to output the sprite and XML.");
            GUILayout.Label("4. Press 'Export' button");
            GUILayout.Label("5. XML will be generated moved along with the sprite to the specified folder");
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();            
        }
        
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("Image must be in this folder: " + spriteToXmlPath);
        if (GUILayout.Button("Open Image Folder"))
        {
            if (Directory.Exists(spriteToXmlPath))
            {
                EditorUtility.RevealInFinder(spriteToXmlPath);
            }
            else
            {
                Directory.CreateDirectory(spriteToXmlPath);
                EditorUtility.RevealInFinder(spriteToXmlPath);
            }            
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

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
                UnityDebugger.Debugger.LogError("SpriteToXML", "Place only one sprite in 'Resources/Editor/SpriteToXML'");
                return;
            }

            ExportSprites();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("Current Path: " + outputDirPath);
        if (GUILayout.Button("Open Output Folder"))
        {
            EditorUtility.RevealInFinder(outputDirPath);
        }

        EditorGUILayout.EndVertical();
    }

    private void ExportSprites()
    {
        UnityDebugger.Debugger.Log("SpriteToXML", "Files saved to: " + outputDirPath);

        foreach (string fn in filesInDir)
        {
            UnityDebugger.Debugger.Log("SpriteToXML", "files in dir: " + fn);
        }   
            
        foreach (Texture2D t in images)
        {
            UnityDebugger.Debugger.Log("SpriteToXML", "Filename: " + t.name);            
        }

        WriteXml();        
    }   

    private void WriteXml()
    {
        if (outputDirPath == string.Empty)
        {
            UnityDebugger.Debugger.LogError("SpriteToXML", "Please select a folder");
            return;
        }
        
        for (int i = 0; i < images.Length; i++)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = false
            };

            XmlWriter writer = XmlWriter.Create(Path.Combine(outputDirPath, images[i].name + ".xml"), xmlWriterSettings);

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
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".png.meta", outputDirPath + "/" + images[i].name + ".meta");
                }
                else if (s.Contains(".jpg"))
                {
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".jpg", outputDirPath + "/" + images[i].name + ".jpg");
                    FileUtil.MoveFileOrDirectory(Application.dataPath + "/Resources/Editor/SpriteToXML/" + images[i].name + ".jpg.meta", outputDirPath + "/" + images[i].name + ".meta");
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
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Instructions", EditorStyles.boldLabel);
            GUILayout.Label("1. Select the image you want an XML for ");
            GUILayout.Label("2. Specify the rows and columns of the image ");
            GUILayout.Label("3. Select the folder to output the sprite and xml");
            GUILayout.Label("4. Press 'Export' button");
            GUILayout.Label("5. XML will generate and sprite will be moved to the specified folder");
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Select Image"))
        {
            inputDirPath = EditorUtility.OpenFilePanelWithFilters("Select file to generate a XML for", inputDirPath, new string[] { "Image files", "png,jpg,jpeg" });
            if (File.Exists(inputDirPath))
            {
                LoadImage(inputDirPath);
            }
        }

        if (GUILayout.Button("Set Output Folder"))
        {
            outputDirPath = EditorUtility.OpenFolderPanel("Select folder to save XML", outputDirPath, string.Empty);
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();        

        GUILayout.Label("Settings:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");        
        GUILayout.BeginHorizontal();

        GUILayout.Label("Pixels Per Unit:", EditorStyles.boldLabel);
        index = EditorGUILayout.Popup(index, pixelPerUnitOptions);

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();       
        GUILayout.BeginHorizontal();

        isMultipleSprite = EditorGUILayout.ToggleLeft("Is this a Multiple Sprite image?", isMultipleSprite);
        useCustomPivot = EditorGUILayout.ToggleLeft("Use a custom pivot?", useCustomPivot);

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();        

        if (isMultipleSprite)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Select Rows/Columns:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Rows:", EditorStyles.boldLabel);
            rowIndex = EditorGUILayout.Popup(rowIndex, columnRowOptions);

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Columns:", EditorStyles.boldLabel);
            columnIndex = EditorGUILayout.Popup(columnIndex, columnRowOptions);

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.Space();            
        }
        
        if (useCustomPivot)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Custom Pivot Settings:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal("Box");
            pivotX = Mathf.Clamp01(EditorGUILayout.FloatField("Pivot X", pivotX));
            pivotY = Mathf.Clamp01(EditorGUILayout.FloatField("Pivot Y", pivotY));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        if (textureLoaded == true)
        {            
            GUILayout.Label(imageName + " Preview:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(446), GUILayout.Height(210));
            GUILayout.Label(myTexture);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }                 

        if (textureLoaded == true && outputDirPath != string.Empty)
        {
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Export " + imageName + ".xml"))
            {
                if ((columnIndex == 0 && rowIndex == 0) && isMultipleSprite == true)
                {
                    EditorUtility.DisplayDialog("Select proper Row/Column count", "Please select more than 1 Row/Column!", "OK");
                    UnityDebugger.Debugger.LogError("SpriteToXML", "Please select more than 1 Row/Column");
                }
                else
                {
                    GenerateXML(inputDirPath, myTexture);
                }
            }
        }

        if (outputDirPath != string.Empty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Current Path: " + outputDirPath);

            if (GUILayout.Button("Open Output Folder"))
            {
                EditorUtility.RevealInFinder(outputDirPath);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void LoadImage(string filePath)
    {        
        // Load the file into a texture.
        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

        // Create some kind of dummy instance of Texture2D.
        // LoadImage will correctly resize the texture based on the image file.
        Texture2D imageTexture = new Texture2D(2, 2);
        myTexture = new Texture2D(2, 2);

        // Image was successfully loaded.
        if (imageTexture.LoadImage(imageBytes))
        {
            myTexture = imageTexture;
            imageName = Path.GetFileNameWithoutExtension(filePath);
            imageExt = Path.GetExtension(filePath);
            UnityDebugger.Debugger.Log("SpriteToXML", imageName + " Loaded");
            textureLoaded = true;            
        }        
    }

    private void GenerateXML(string filePath, Texture2D imageTexture)
    {
        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t",
            NewLineOnAttributes = false
        };

        XmlWriter writer = XmlWriter.Create(Path.Combine(outputDirPath, imageName + ".xml"), xmlWriterSettings);

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

        MoveImage(filePath);
        ResetForm();        
    }

    private void SingleSpriteXML(XmlWriter writer, Texture2D imageTexture)
    {        
            writer.WriteStartElement("Sprite");
            writer.WriteAttributeString("name", imageName);
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

    private void MoveImage(string filePath)
    {
        string destPath = Path.Combine(outputDirPath, imageName + imageExt);
            
        if (File.Exists(destPath))
        {
            try
            {
                File.Replace(filePath, destPath, destPath + ".bak");
                UnityDebugger.Debugger.LogWarning("SpriteToXML", "Image already exsists, backing old one up to: " + destPath + ".bak");
            }
            catch (Exception ex)
            {
                UnityDebugger.Debugger.LogWarning("SpriteToXML", ex.Message + " - " + imageName + imageExt + " not moved.");
                EditorUtility.DisplayDialog(imageName + imageExt + " not moved.", "The original and output directories cannot be the same!" + "\n\n" + "XML was still generated.", "OK");
            }
        }
        else
        {
            File.Move(filePath, destPath);            
            UnityDebugger.Debugger.Log("SpriteToXML", "Image moved to: " + destPath);
        }

        if (File.Exists(filePath + ".meta") && File.Exists(destPath + ".meta"))
        {
            try
            {
                File.Replace(filePath + ".meta", destPath + ".meta", destPath + ".meta.bak");
            }
            catch (Exception ex)
            {
                UnityDebugger.Debugger.LogWarning("SpriteToXML", ex.Message + " - " + imageName + imageExt + ".meta not moved.");
            }            
        }
        else
        {
            File.Move(filePath + ".meta", destPath + ".meta");
        }
    }

    private void ResetForm()
    {
        textureLoaded = false;
        spriteCount = 0;
        inputDirPath = imageExt = imageName = string.Empty;
        myTexture = null;
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }
}
