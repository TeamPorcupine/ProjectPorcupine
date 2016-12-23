using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ModMenu {
    public static Dictionary<string, bool> activeMods { get; private set; }
    static Dictionary<string,string> modDirs;
    public static List<string> activeModDirs;
    static Dictionary<string, bool> activeModsWaiting;
    static List<string> activeModDirsWaiting;

    public static void Load()
    {
        modDirs = new Dictionary<string, string>();
        JArray active;
        if (File.Exists(Path.Combine(Application.persistentDataPath, "ModSettings.json")))
        {
            JObject modSettings = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(Application.persistentDataPath, "ModSettings.json"))));
            active = (JArray)modSettings["activeMods"];
        }
        else
        {
            active = new JArray();
        }
        activeModDirs = new List<string>();
        foreach (DirectoryInfo mod in ModsManager.GetModsFiles())
        {
            string modPath = Path.Combine(mod.FullName, "mod.json");
            JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(modPath)));
            string name = (string)modData["name"];
            activeMods.Add(name, active.Contains(name));
            modDirs.Add(name,mod.FullName);
        }
        foreach (JObject activeMod in active)
        {
            if (activeMods[(string) activeMod])
            {
                activeModDirs.Add(modDirs[(string)activeMod]);
            }
        }
        activeModDirsWaiting = activeModDirs;
        activeModsWaiting = activeMods;
	}
    public static void DisplaySettings(Transform parent)
    {
        Load();
        GameObject prefab = (GameObject)Resources.Load("Prefab/DialogBoxPrefabs/Mod");
        foreach (string mod in activeModDirsWaiting)
        {
            JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod,"mod.json"))));
            string name = (string)modData["name"];
            string desc = (string)modData["desc"];
            GameObject m = (GameObject)Object.Instantiate(prefab, parent);
            m.transform.FindChild("Title").GetComponent<Text>().text = name;
            m.transform.FindChild("Description").GetComponent<Text>().text = desc;
            m.transform.FindChild("Toggle").GetComponent<Toggle>().isOn = true;
        }
        foreach (string mod in modDirs.Values)
        {
            if (activeModDirsWaiting.Contains(mod))
            {
                continue;
            }
            JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod, "mod.json"))));
            string name = (string)modData["name"];
            string desc = (string)modData["desc"];
            GameObject m = (GameObject)Object.Instantiate(prefab, parent);
            m.transform.FindChild("Title").GetComponent<Text>().text = name;
            m.transform.FindChild("Description").GetComponent<Text>().text = desc;
            m.transform.FindChild("Toggle").GetComponent<Toggle>().isOn = false;
        }
    }
    public static void setEnabled(string mod,bool enabled)
    {
        activeMods[mod] = enabled;
        if (enabled)
        {
            activeModDirs.Add(modDirs[mod]);
        }
        else
        {
            activeModDirs.Remove(modDirs[mod]);
        }
    }
    public static void reorderMod(string mod,int up)
    {
        if (activeModDirs.Contains(modDirs[mod]) == false)
        {
            setEnabled(mod, true);
            return;
        }
        int i = activeModDirs.IndexOf(modDirs[mod]);
        if (i == activeModDirs.Count-1 && up < 0)
        {
            setEnabled(mod, false);
            return;
        }
        if ((i == 0 && up > 0) || up == 0)
        {
            return;
        }
        activeModDirs.Remove(modDirs[mod]);
        i -= up;
        i = Mathf.Clamp(i, 0, activeModDirs.Count-1);
        activeModDirs.Insert(i, modDirs[mod]);
    }
}
