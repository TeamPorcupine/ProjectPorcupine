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
        ActionsManager.Furniture.LoadModsScripts(mods);
        ActionsManager.Need.LoadModsScripts(mods);
        ActionsManager.GameEvent.LoadModsScripts(mods);
        ActionsManager.TileType.LoadModsScripts(mods);
        ActionsManager.Quest.LoadModsScripts(mods);
        ActionsManager.ScheduledEvent.LoadModsScripts(mods);

        PrototypeManager.Furniture.LoadModPrototypesFromFile(mods);
        PrototypeManager.Inventory.LoadModPrototypesFromFile(mods);
        PrototypeManager.Need.LoadModPrototypesFromFile(mods);
        PrototypeManager.Trader.LoadModPrototypesFromFile(mods);
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }
}
