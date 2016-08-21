using System;
using System.IO;

public class ModsManager
{
    DirectoryInfo[] mods;

    public ModsManager(string dataPath)
    {
        // Read the Furniture.xml files from Mods directory
        string modsPath = System.IO.Path.Combine(dataPath, "Mods");
        DirectoryInfo modsDir = new DirectoryInfo(modsPath);
        mods = modsDir.GetDirectories();
    }

    public DirectoryInfo[] GetMods() {
        return mods;
    }
}

