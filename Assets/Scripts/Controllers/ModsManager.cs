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

        LoadPrototypes();
    }

    public void LoadPrototypes()
    {
        LoadFunctions("Furniture", "Furniture.lua");
        LoadFunctions("Need", "Need.lua");
        LoadFunctions("GameEvent", "GameEvent.lua");
        LoadFunctions("TileType", "Tiles.lua");
        LoadFunctions("Quest", "Quest.lua");
        LoadFunctions("ScheduledEvent", "ScheduledEvent.lua");

        PrototypeManager.Furniture.LoadPrototypes(mods);
        PrototypeManager.Inventory.LoadPrototypes(mods);
        PrototypeManager.Need.LoadPrototypes(mods);
        PrototypeManager.Trader.LoadPrototypes(mods);
        PrototypeManager.SchedulerEvent.LoadPrototypes(mods);
        PrototypeManager.Stat.LoadPrototypes(mods);
        PrototypeManager.Quest.LoadPrototypes(mods);

        LoadCharacterNames("CharacterNames.txt");
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }

    /// <summary>
    /// Loads all the functions from the given script.
    /// </summary>
    /// <param name="functionsName">The functions name.</param>
    /// <param name="fileName">The file name.</param>
    private void LoadFunctions(string functionsName, string fileName)
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
    /// <param name="folderName">Folder name.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="readText">Called to handle the text reading and actual loading.</param>
    private void LoadTextFile(string folderName, string fileName, Action<string> readText)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, folderName);
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
}
