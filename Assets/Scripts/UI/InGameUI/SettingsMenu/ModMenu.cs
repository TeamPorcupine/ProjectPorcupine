using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ModMenu {
    static Dictionary<string,string> modDirs;
    static Dictionary<string, string> revModDirs;
    public static List<string> activeModDirs;
    static List<string> activeModDirsWaiting;
    static List<string> nonSaving;
    static Transform UIParent;

    public static void Load()
    {
        modDirs = new Dictionary<string, string>();
        revModDirs = new Dictionary<string, string>();
        JArray active;
        nonSaving = new List<string>();
        if (File.Exists(Path.Combine(Application.persistentDataPath, "ModSettings.json")))
        {
            StreamReader s = File.OpenText(Path.Combine(Application.persistentDataPath, "ModSettings.json"));
            JObject modSettings = (JObject)JToken.ReadFrom(new JsonTextReader(s));
            active = (JArray)modSettings["activeMods"];
            s.Close();
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
            modDirs.Add(name,mod.FullName);
            revModDirs.Add(mod.FullName, name);
        }
        for (int i = 0; i < active.Count; i++)
        {
            JToken activeMod = active[i];
            if (modDirs.ContainsKey((string)activeMod))
            {
                activeModDirs.Add(modDirs[(string)activeMod]);
            }
        }
        activeModDirsWaiting = activeModDirs;
	}
    public static void DisplaySettings(Transform parent)
    {
        while (parent.childCount > 0)
        {
            Transform c = parent.GetChild(0);
            c.SetParent(null);
            Object.Destroy(c.gameObject);
        }
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
            m.name = name;
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
            m.name = name;
        }
        parent.GetComponent<AutomaticVerticalSize>().AdjustSize();
        parent.parent.GetComponent<RectTransform>().sizeDelta = parent.GetComponent<RectTransform>().sizeDelta;
        parent.parent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        parent.parent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        UIParent = parent;
    }
    public static void setEnabled(string mod,bool enabled)
    {
        if (string.IsNullOrEmpty(mod))
        {
            return;
        }
        if (enabled)
        {
            activeModDirs.Add(modDirs[mod]);
        }
        else
        {
            activeModDirs.Remove(modDirs[mod]);
        }
        DisplaySettings(UIParent);
    }
    public static void reorderMod(string mod,int up)
    {
        if (string.IsNullOrEmpty(mod))
        {
            return;
        }
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
        i = Mathf.Clamp(i, 0, activeModDirs.Count);
        activeModDirsWaiting.Insert(i, modDirs[mod]);
        DisplaySettings(UIParent);
    }
    public static void commit()
    {
        activeModDirs = activeModDirsWaiting;
    }
    public static void reset()
    {
        activeModDirsWaiting = activeModDirs;
    }
    public static JArray WriteJSON(bool forSave)
    {
        JArray output = new JArray();
        foreach (string mod in activeModDirs)
        {
            if(nonSaving.Contains(mod) && forSave)
            {
                continue;
            }
            output.Add(revModDirs[mod]);
        }
        return output;
    }
    public static void Save()
    {
        JObject ModJson = new JObject();
        ModJson.Add("activeMods", WriteJSON(false));
        string jsonData = JsonConvert.SerializeObject(ModJson, Formatting.Indented);

        // Save the document.
        try
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Application.persistentDataPath, "ModSettings.json")))
            {
                writer.WriteLine(jsonData);
            }
        }
        catch (System.Exception e)
        {
            UnityDebugger.Debugger.LogWarning("Settings", "Settings could not be saved to " + Path.Combine(Application.persistentDataPath, "ModSettings.json"));
            UnityDebugger.Debugger.LogWarning("Settings", e.Message);
        }
    }
    public static void DisableAll()
    {
        activeModDirs = new List<string>();
        reset();
    }
}
