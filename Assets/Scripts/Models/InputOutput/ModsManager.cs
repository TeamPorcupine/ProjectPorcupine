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
using UnityEngine;

public class ModsManager
{
    private DirectoryInfo[] mods;

    public ModsManager()
    {
        mods = GetModsFiles();

        LoadSharedFiles();

        if (SceneController.Instance.IsAtIntroScene())
        {
            LoadIntroFiles();
        }
        else if (SceneController.Instance.IsAtMainScene())
        {
            LoadMainSceneFiles();
        }
    }

    /// <summary>
    /// Return directory info of the mod folder.
    /// </summary>
    public static DirectoryInfo[] GetModsFiles()
    {
        DirectoryInfo modsDir = new DirectoryInfo(GetPathToModsFolder());
        return modsDir.GetDirectories();
    }

    /// <summary>
    /// Loads the script file in the given location.
    /// </summary>
    /// <param name="file">The file name.</param>
    /// <param name="functionsName">The functions name.</param>
    public void LoadFunctionsInFile(FileInfo file, string functionsName)
    {
        LoadTextFile(
            file.DirectoryName,
            file.Name,
            (filePath) =>
            {
                StreamReader reader = new StreamReader(file.OpenRead());
                string text = reader.ReadToEnd();
                FunctionsManager.Get(functionsName).LoadScript(text, functionsName, file.Extension == ".lua" ? Functions.Type.Lua : Functions.Type.CSharp);
            });
    }

    /// <summary>
    /// Return the path to the mod folder.
    /// </summary>
    private static string GetPathToModsFolder()
    {
        return System.IO.Path.Combine(System.IO.Path.Combine(Application.streamingAssetsPath, "Data"), "Mods");
    }

    private void LoadMainSceneFiles()
    {
        LoadFunctions("Furniture.lua", "Furniture");
        LoadFunctions("Utility.lua", "Utility");
        LoadFunctions("RoomBehavior.lua", "RoomBehavior");
        LoadFunctions("Need.lua", "Need");
        LoadFunctions("GameEvent.lua", "GameEvent");
        LoadFunctions("Tiles.lua", "TileType");
        LoadFunctions("Quest.lua", "Quest");
        LoadFunctions("ScheduledEvent.lua", "ScheduledEvent");
        LoadFunctions("Overlay.lua", "Overlay");

        LoadFunctions("FurnitureFunctions.cs", "Furniture");
        LoadFunctions("OverlayFunctions.cs", "Overlay");

        LoadPrototypes("Tiles.xml", PrototypeManager.TileType.LoadPrototypes);
        LoadPrototypes("Furniture.xml", PrototypeManager.Furniture.LoadPrototypes);
        LoadPrototypes("Utility.xml", PrototypeManager.Utility.LoadPrototypes);
        LoadPrototypes("RoomBehavior.xml", (text) => PrototypeManager.RoomBehavior.LoadPrototypes(text));
        LoadPrototypes("Inventory.xml", PrototypeManager.Inventory.LoadPrototypes);
        LoadPrototypes("Need.xml", PrototypeManager.Need.LoadPrototypes);
        LoadPrototypes("Trader.xml", PrototypeManager.Trader.LoadPrototypes);
        LoadPrototypes("Currency.xml", PrototypeManager.Currency.LoadPrototypes);
        LoadPrototypes("GameEvents.xml", PrototypeManager.GameEvent.LoadPrototypes);
        LoadPrototypes("ScheduledEvents.xml", PrototypeManager.ScheduledEvent.LoadPrototypes);
        LoadPrototypes("Stats.xml", PrototypeManager.Stat.LoadPrototypes);
        LoadPrototypes("Quest.xml", PrototypeManager.Quest.LoadPrototypes);
        LoadPrototypes("Headlines.xml", PrototypeManager.Headline.LoadPrototypes);
        LoadPrototypes("Overlay.xml", PrototypeManager.Overlay.LoadPrototypes);
        LoadPrototypes("Ships.xml", PrototypeManager.Ship.LoadPrototypes);

        LoadCharacterNames("CharacterNames.txt");

        LoadDirectoryAssets("Images", SpriteManager.LoadSpriteFiles);
        LoadDirectoryAssets("Audio", AudioManager.LoadAudioFiles);
    }

    private void LoadIntroFiles()
    {
        LoadDirectoryAssets("MainMenu/Images", SpriteManager.LoadSpriteFiles);
        LoadDirectoryAssets("MainMenu/Audio", AudioManager.LoadAudioFiles);
    }

    private void LoadSharedFiles()
    {
        LoadDirectoryAssets("Shared/Images", SpriteManager.LoadSpriteFiles);
        LoadDirectoryAssets("Shared/Audio", AudioManager.LoadAudioFiles);

        LoadFunctions("CommandFunctions.cs", "DevConsole");
        LoadFunctions("ConsoleCommands.lua", "DevConsole");

        LoadPrototypes("ConsoleCommands.xml", PrototypeManager.DevConsole.LoadPrototypes);
        LoadPrototypes("SettingsTemplate.xml", PrototypeManager.SettingsCategories.LoadPrototypes);
        LoadPrototypes("PerformanceHUDTemplate.xml", PrototypeManager.PerformanceHUD.LoadPrototypes);

        LoadFunctions("SettingsMenuFunctions.cs", "SettingsMenu");
        LoadFunctions("SettingsMenuCommands.lua", "SettingsMenu");

        LoadFunctions("PerformanceHUDFunctions.cs", "PerformanceHUD");
        LoadFunctions("PerfanceHUDCommands.lua", "PerformanceHUD");
    }

    /// <summary>
    /// Loads all the functions using the given file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="functionsName">The functions name.</param>
    private void LoadFunctions(string fileName, string functionsName)
    {
        string ext = Path.GetExtension(fileName);
        string folder = "LUA";
        Functions.Type scriptType = Functions.Type.Lua;

        if (string.Compare(".cs", ext, true) == 0)
        {
            folder = "CSharp";
            scriptType = Functions.Type.CSharp;
        }

        LoadTextFile(
            folder,
            fileName,
            (filePath) =>
            {
                if (File.Exists(filePath))
                {
                    string text = File.ReadAllText(filePath);
                    FunctionsManager.Get(functionsName).LoadScript(text, functionsName, scriptType);
                }
                else
                {
                    UnityDebugger.Debugger.LogError(folder == "CSharp" ? "CSharp" : "LUA", "file " + filePath + " not found");
                }
            });
    }

    /// <summary>
    /// Loads all the protoypes using the given file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="prototypesLoader">Called to handle the prototypes loading.</param>
    private void LoadPrototypes(string fileName, Action<string> prototypesLoader)
    {
        LoadTextFile(
            "Data",
            fileName,
            (filePath) =>
            {
                string text = File.ReadAllText(filePath);
                prototypesLoader(text);
            });
    }

    /// <summary>
    /// Loads all the character names from the given file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    private void LoadCharacterNames(string fileName)
    {
        LoadTextFile(
            "Data",
            fileName,
            (filePath) =>
            {
                string[] lines = File.ReadAllLines(filePath);
                CharacterNameManager.LoadNames(lines);
            });
    }

    /// <summary>
    /// Loads the given file from the given folder in the base and inside the mods and
    /// calls the Action with the file path.
    /// </summary>
    /// <param name="directoryName">Directory name.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="readText">Called to handle the text reading and actual loading.</param>
    private void LoadTextFile(string directoryName, string fileName, Action<string> readText)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, directoryName);
        filePath = Path.Combine(filePath, fileName);
        if (File.Exists(filePath))
        {
            readText(filePath);
        }

        foreach (DirectoryInfo mod in mods)
        {
            filePath = Path.Combine(mod.FullName, fileName);
            if (File.Exists(filePath))
            {
                readText(filePath);
            }
        }
    }

    /// <summary>
    /// Loads the all the assets from the given directory.
    /// </summary>
    /// <param name="directoryName">Directory name.</param>
    /// <param name="readDirectory">Called to handle the loading of each file in the given directory.</param>
    private void LoadDirectoryAssets(string directoryName, Action<string> readDirectory)
    {
        string directoryPath = Path.Combine(Application.streamingAssetsPath, directoryName);
        if (Directory.Exists(directoryPath))
        {
            readDirectory(directoryPath);
        }

        foreach (DirectoryInfo mod in mods)
        {
            directoryPath = Path.Combine(mod.FullName, directoryName);
            if (Directory.Exists(directoryPath))
            {
                readDirectory(directoryPath);
            }
        }
    }
}
