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

    public ModsManager(string dataPath)
    {
        string modsPath = System.IO.Path.Combine(dataPath, "Mods");
        DirectoryInfo modsDir = new DirectoryInfo(modsPath);
        mods = modsDir.GetDirectories();

        LoadFiles();
    }

    public void LoadFiles()
    {
        LoadFunctions("Furniture.lua", "Furniture");
        LoadFunctions("Utility.lua", "Utility");
        LoadFunctions("Need.lua", "Need");
        LoadFunctions("GameEvent.lua", "GameEvent");
        LoadFunctions("Tiles.lua", "TileType");
        LoadFunctions("Quest.lua", "Quest");
        LoadFunctions("ScheduledEvent.lua", "ScheduledEvent");

        LoadPrototypes("Tiles.xml", (text) => PrototypeManager.TileType.LoadPrototypes(text));
        LoadPrototypes("Furniture.xml", (text) => PrototypeManager.Furniture.LoadPrototypes(text));
        LoadPrototypes("Utility.xml", (text) => PrototypeManager.Utility.LoadPrototypes(text));
        LoadPrototypes("Inventory.xml", (text) => PrototypeManager.Inventory.LoadPrototypes(text));
        LoadPrototypes("Need.xml", (text) => PrototypeManager.Need.LoadPrototypes(text));
        LoadPrototypes("Trader.xml", (text) => PrototypeManager.Trader.LoadPrototypes(text));
        LoadPrototypes("Events.xml", (text) => PrototypeManager.SchedulerEvent.LoadPrototypes(text));
        LoadPrototypes("Stats.xml", (text) => PrototypeManager.Stat.LoadPrototypes(text));
        LoadPrototypes("Quest.xml", (text) => PrototypeManager.Quest.LoadPrototypes(text));
        LoadPrototypes("Headlines.xml", (text) => PrototypeManager.Headline.LoadPrototypes(text));

        LoadCharacterNames("CharacterNames.txt");

        LoadDirectoryAssets("Images", (path) => SpriteManager.LoadSpriteFiles(path));
        LoadDirectoryAssets("Audio", (path) => AudioManager.LoadAudioFiles(path));
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }

    /// <summary>
    /// Loads all the functions using the given file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="functionsName">The functions name.</param>
    private void LoadFunctions(string fileName, string functionsName)
    {
        LoadTextFile(
            "LUA",
            fileName,
            (filePath) =>
            {
                string text = File.ReadAllText(filePath);
                FunctionsManager.Get(functionsName).LoadScript(text, functionsName);
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
