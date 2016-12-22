using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ModMenu {
    public static Dictionary<string, bool> activeMods { get; private set; }
    static List<string> modDirs;

	public static void Load ()
    {
        modDirs = new List<string>();
        JObject modSettings = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(Application.persistentDataPath, "ModSettings.json"))));
        JArray active = (JArray)modSettings["activeMods"];
        foreach (DirectoryInfo mod in ModsManager.GetModsFiles())
        {
            string modPath = Path.Combine(mod.FullName, "mod.json");
            modDirs.Add(modPath);
            JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(modPath)));
            string name = (string)modData["name"];
            activeMods.Add(name, active.Contains(name));
        }
	}
    public static void DisplaySettings(Transform parent)
    {
        Load();
        foreach (string mod in modDirs)
        {
            JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(mod)));
            string name = (string)modData["name"];
            string desc = (string)modData["desc"];
        }
    }
}
