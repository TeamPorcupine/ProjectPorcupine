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
    static Dictionary<string, string> revModDirs;
    public static List<string> activeModDirs;
    static Dictionary<string, bool> activeModsWaiting;
    static List<string> activeModDirsWaiting;
    static List<string> nonSaving;

    public static void Load()
    {
        modDirs = new Dictionary<string, string>();
        JArray active;
        nonSaving = new List<string>();
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
            string[] y = mod.FullName.Split('\\');
            string name = y[y.Length - 1];
            if (File.Exists(modPath))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(modPath)));
                name = (string)modData["name"];
                bool noSave = (bool)(modData["doNotSave"] ?? false);
                if (noSave)
                {
                    nonSaving.Add(mod.FullName);
                }
            }
            activeMods.Add(name, active.Contains(name));
            modDirs.Add(name,mod.FullName);
            revModDirs.Add(mod.FullName, name);
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
            string[] y = mod.Split('\\');
            string name = y[y.Length-1];
            string desc = string.Empty;
            if (File.Exists(Path.Combine(mod, "mod.json")))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod, "mod.json"))));
                name = (string)modData["name"];
                desc = (string)modData["desc"] ?? string.Empty;
            }
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
            string[] y = mod.Split('\\');
            string name = y[y.Length-1];
            string desc = string.Empty;
            if (File.Exists(Path.Combine(mod, "mod.json")))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod, "mod.json"))));
                name = (string)modData["name"];
                desc = (string)modData["desc"] ?? string.Empty;
            }
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
        if (activeModDirsWaiting.Contains(modDirs[mod]) == false)
        {
            setEnabled(mod, true);
            return;
        }
        int i = activeModDirsWaiting.IndexOf(modDirs[mod]);
        if (i == activeModDirsWaiting.Count-1 && up < 0)
        {
            setEnabled(mod, false);
            return;
        }
        if ((i == 0 && up > 0) || up == 0)
        {
            return;
        }
        activeModDirsWaiting.Remove(modDirs[mod]);
        i -= up;
        i = Mathf.Clamp(i, 0, activeModDirs.Count-1);
        activeModDirsWaiting.Insert(i, modDirs[mod]);
    }
    public static void commit()
    {
        activeModDirs = activeModDirsWaiting;
        activeMods = activeModsWaiting;
    }
    public static void reset()
    {
        activeModDirsWaiting = activeModDirs;
        activeModsWaiting = activeMods;
    }
    public static JArray WriteJSON()
    {
        JArray output = new JArray();
        foreach (string mod in activeModDirs)
        {
            if(nonSaving.Contains(mod))
            {
                continue;
            }
            output.Add(revModDirs[mod]);
        }
        return output;
    }
    public static void Save()
    {
        StreamWriter sw = new StreamWriter(Path.Combine(Application.persistentDataPath, "ModSettings.json"));
        JsonWriter writer = new JsonTextWriter(sw);
        JObject ModJSON = new JObject();
        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Formatting = Formatting.Indented;
        ModJSON.Add("activeMods", WriteJSON());
        serializer.Serialize(writer, ModJSON);

        writer.Flush();
    }
    public static void DisableAll()
    {
        activeModDirs = new List<string>();
        foreach (string mod in activeMods.Keys)
        {
            activeMods[mod] = false;
        }
        reset();
    }
}
