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
        LoadFunctionScripts("Furniture", "Furniture.lua");
        LoadFunctionScripts("Need", "Need.lua");
        LoadFunctionScripts("GameEvent", "GameEvent.lua");
        LoadFunctionScripts("TileType", "Tiles.lua");
        LoadFunctionScripts("Quest", "Quest.lua");
        LoadFunctionScripts("ScheduledEvent", "ScheduledEvent.lua");

        PrototypeManager.Furniture.LoadPrototypes(mods);
        PrototypeManager.Inventory.LoadPrototypes(mods);
        PrototypeManager.Need.LoadPrototypes(mods);
        PrototypeManager.Trader.LoadPrototypes(mods);
        PrototypeManager.SchedulerEvent.LoadPrototypes(mods);
        PrototypeManager.Stat.LoadPrototypes(mods);
        PrototypeManager.Quest.LoadPrototypes(mods);
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }

    /// <summary>
    /// Loads the base and the mods scripts.
    /// </summary>
    /// <param name="mods">The mods directories.</param>
    /// <param name="fileName">The file name.</param>
    private void LoadFunctionScripts(string functionsName, string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = Path.Combine(filePath, fileName);
        if (File.Exists(filePath))
        {
            string text = File.ReadAllText(filePath);
            FunctionsManager.Get(functionsName).LoadScript(text, functionsName);
        }

        foreach (DirectoryInfo mod in mods)
        {
            filePath = Path.Combine(mod.FullName, fileName);
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                FunctionsManager.Get(functionsName).LoadScript(text, functionsName);
            }
        }
    }
}
