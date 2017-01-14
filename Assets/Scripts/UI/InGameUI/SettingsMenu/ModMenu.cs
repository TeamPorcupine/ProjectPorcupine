﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class ModMenu
{
    public static List<string> loadedMods;
    public static List<string> activeModDirs;
    private static List<string> activeModDirsWaiting;
    private static List<string> nonSaving;
    private static Transform uiParent;

    public static Dictionary<string, string> ModDirs { get; private set; }

    public static Dictionary<string, string> RevModDirs { get; private set; }

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        ModDirs = new Dictionary<string, string>();
        RevModDirs = new Dictionary<string, string>();
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

            ModDirs.Add(name, mod.FullName);
            RevModDirs.Add(mod.FullName, name);
        }

        for (int i = 0; i < active.Count; i++)
        {
            JToken activeMod = active[i];
            if (ModDirs.ContainsKey((string)activeMod))
            {
                activeModDirs.Add(ModDirs[(string)activeMod]);
            }
        }

        activeModDirsWaiting = activeModDirs;
    }

    public static void DisplaySettings()
    {
        DisplaySettings(uiParent);
    }

    public static void DisplaySettings(Transform parent)
    {
        Loaded = false;
        while (parent.childCount > 0)
        {
            Transform c = parent.GetChild(0);
            c.SetParent(null);
            UnityEngine.Object.Destroy(c.gameObject);
        }

        GameObject prefab = (GameObject)Resources.Load("Prefab/DialogBoxPrefabs/Mod");
        foreach (string mod in activeModDirsWaiting)
        {
            string[] y = mod.Split('\\');
            string name = y[y.Length - 1];
            string desc = string.Empty;
            if (File.Exists(Path.Combine(mod, "mod.json")))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod, "mod.json"))));
                name = (string)modData["name"];
                desc = (string)modData["desc"] ?? string.Empty;
            }

            GameObject m = (GameObject)GameObject.Instantiate(prefab, parent);
            m.transform.FindChild("Title").GetComponent<Text>().text = name;
            m.transform.FindChild("Description").GetComponent<Text>().text = desc;
            m.transform.FindChild("Toggle").GetComponent<Toggle>().isOn = true;
            m.name = name;
        }

        foreach (string mod in ModDirs.Values)
        {
            if (activeModDirsWaiting.Contains(mod))
            {
                continue;
            }

            string[] y = mod.Split('\\');
            string name = y[y.Length - 1];
            string desc = string.Empty;
            if (File.Exists(Path.Combine(mod, "mod.json")))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(mod, "mod.json"))));
                name = (string)modData["name"];
                desc = (string)modData["desc"] ?? string.Empty;
            }

            GameObject m = (GameObject)GameObject.Instantiate(prefab, parent);
            m.transform.FindChild("Title").GetComponent<Text>().text = name;
            m.transform.FindChild("Description").GetComponent<Text>().text = desc;
            m.transform.FindChild("Toggle").GetComponent<Toggle>().isOn = false;
            m.name = name;
        }

        parent.GetComponent<AutomaticVerticalSize>().AdjustSize();
        parent.parent.GetComponent<RectTransform>().sizeDelta = parent.GetComponent<RectTransform>().sizeDelta;
        parent.parent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        parent.parent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        uiParent = parent;
        Loaded = true;
    }

    public static void SetEnabled(string mod, bool enabled)
    {
        if (string.IsNullOrEmpty(mod))
        {
            return;
        }

        if (ModDirs.ContainsKey(mod) == false)
        {
            return;
        }

        if (enabled && (activeModDirs.Contains(ModDirs[mod]) == false))
        {
            activeModDirs.Add(ModDirs[mod]);
        }
        else if (activeModDirs.Contains(ModDirs[mod]))
        {
            activeModDirs.Remove(ModDirs[mod]);
        }
    }

    public static void ReorderMod(string mod, int up)
    {
        if (string.IsNullOrEmpty(mod))
        {
            return;
        }

        if (activeModDirsWaiting.Contains(ModDirs[mod]) == false)
        {
            SetEnabled(mod, true);
            return;
        }

        int i = activeModDirsWaiting.IndexOf(ModDirs[mod]);
        if (i == activeModDirsWaiting.Count - 1 && up < 0)
        {
            SetEnabled(mod, false);
            return;
        }

        if ((i == 0 && up > 0) || up == 0)
        {
            return;
        }

        activeModDirsWaiting.Remove(ModDirs[mod]);
        i -= up;
        i = Mathf.Clamp(i, 0, activeModDirs.Count);
        activeModDirsWaiting.Insert(i, ModDirs[mod]);
        DisplaySettings(uiParent);
    }

    public static void Commit()
    {
        activeModDirs = activeModDirsWaiting;
    }

    public static void Reset()
    {
        activeModDirsWaiting = activeModDirs;
    }

    public static JArray WriteJSON(bool forSave)
    {
        JArray output = new JArray();
        if (forSave)
        {
            foreach (string mod in loadedMods)
            {
                if (nonSaving.Contains(mod))
                {
                    continue;
                }

                output.Add(mod);
            }
        }
        else
        {
            foreach (string mod in activeModDirs)
            {
                output.Add(RevModDirs[mod]);
            }
        }

        return output;
    }

    public static void Save()
    {
        JObject modJson = new JObject();
        modJson.Add("activeMods", WriteJSON(false));
        string jsonData = JsonConvert.SerializeObject(modJson, Formatting.Indented);

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
        Reset();
    }

    public static void ReportModsLoaded()
    {
        loadedMods = new List<string>();
        foreach (string mod in activeModDirs)
        {
            loadedMods.Add(RevModDirs[mod]);
        }
    }
}
