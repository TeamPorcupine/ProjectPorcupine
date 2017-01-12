#region License
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
    private static List<string> activeModDirs;
    private static List<string> activeModDirsRev;
    private static List<string> activeModDirsWaiting;
    private static List<string> nonSaving;
    private static Transform uiParent;

    public static Dictionary<string, string> ModDirs { get; private set; }

    public static Dictionary<string, string> RevModDirs { get; private set; }

    public static bool Loaded { get; private set; }

    public static List<string> ActiveModDirs
    {
        get
        {
            return activeModDirs;
        }

        set
        {
            activeModDirs = value;
            activeModDirsRev = value;
            activeModDirsRev.Reverse();
        }
    }

    public static List<string> ActiveModDirsRev
    {
        get
        {
            return activeModDirsRev;
        }
    }

    public static string ListToString(List<string> l) // TODO: Find a better place for this
    {
        string s = string.Empty;
        foreach (string st in l)
        {
            s = s + ", " + st;
        }
        
        s = s.TrimStart(' ', ',');
        return s;
    }

    public static void Load()
    {
        if (ModDirs != null)
        {
            return;
        }

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
        
        ActiveModDirs = new List<string>();
        foreach (DirectoryInfo mod in ModsManager.GetModsFiles())
        {
            string modPath = Path.Combine(mod.FullName, "mod.json");
            string[] y = mod.FullName.Split('\\');
            string name = y[y.Length - 1];
            if (File.Exists(modPath))
            {
                JObject modData = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(modPath)));
                name = (string)modData["name"];
                if (RevModDirs.ContainsKey(name))
                {
                    continue;
                }

                bool noSave = (bool)(modData["doNotSave"] ?? false);
                if (noSave)
                {
                    nonSaving.Add(name);
                }
            }
            else if (RevModDirs.ContainsKey(name))
            {
                continue;
            }

            ModDirs.Add(name, mod.FullName);
            RevModDirs.Add(mod.FullName, name);
        }

        if (ActiveModDirs == null)
        {
            return;
        }

        for (int i = 0; i < active.Count; i++)
        {
            JToken activeMod = active[i];
            if (ModDirs.ContainsKey((string)activeMod))
            {
                ActiveModDirs.Add(ModDirs[(string)activeMod]);
            }
        }

        activeModDirsWaiting = ActiveModDirs;
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

        GameObject prefab = (GameObject)Resources.Load("UI/ModMenu/Mod");
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

        if (enabled && (ActiveModDirs.Contains(ModDirs[mod]) == false))
        {
            ActiveModDirs.Add(ModDirs[mod]);
        }
        else if (ActiveModDirs.Contains(ModDirs[mod]))
        {
            ActiveModDirs.Remove(ModDirs[mod]);
        }

        UnityDebugger.Debugger.Log((enabled ? "Enabled" : "Disabled") + " mod " + mod);
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
        i = Mathf.Clamp(i, 0, ActiveModDirs.Count);
        activeModDirsWaiting.Insert(i, ModDirs[mod]);
        DisplaySettings(uiParent);
    }

    public static void Commit(bool save = false)
    {
        ActiveModDirs = activeModDirsWaiting;
        if (save)
        {
            SceneController.Instance.LoadMainMenu();
        }
    }

    public static void Reset()
    {
        activeModDirsWaiting = ActiveModDirs;
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
            foreach (string mod in ActiveModDirs)
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

        SceneController.Instance.LoadMainMenu();
    }

    public static void DisableAll()
    {
        ActiveModDirs = new List<string>();
        Reset();
        DisplaySettings();
    }

    public static void ReportModsLoaded()
    {
        loadedMods = new List<string>();
        foreach (string mod in ActiveModDirs)
        {
            loadedMods.Add(RevModDirs[mod]);
        }
    }

    public static void CheckMods(string worldFile, DialogBoxLoadGame dblg)
    {
        StreamReader s = File.OpenText(worldFile);
        JObject modSettings = (JObject)JToken.ReadFrom(new JsonTextReader(s));
        JArray active = (JArray)modSettings["ActiveMods"];
        s.Close();
        List<string> inactive = new List<string>();
        if (active == null)
        {
            dblg.LoadWorld(worldFile);
            return;
        }

        for (int i = 0; i < active.Count; i++)
        {
            if (ActiveModDirs.Contains(ModDirs[(string)active[i]]) == false)
            {
                inactive.Add((string)active[i]);
            }
        }

        if (inactive.Count > 0)
        {
            DialogBoxPromptOrInfo check;

            if (WorldController.Instance != null)
            {
                check = WorldController.Instance.dialogBoxManager.dialogBoxPromptOrInfo;
            }
            else if (MainMenuController.Instance != null)
            {
                check = MainMenuController.Instance.dialogBoxManager.dialogBoxPromptOrInfo;
            }
            else
            {
                dblg.LoadWorld(worldFile);
                return;
            }

            check.SetPrompt("prompt_load_mods", ListToString(inactive));
            check.SetButtons(new DialogBoxResult[] { DialogBoxResult.Yes, DialogBoxResult.No, DialogBoxResult.Cancel });
            check.Closed =
                () =>
                {
                    switch (check.Result)
                    {
                        case DialogBoxResult.Yes:
                            for (int y = 0; y < active.Count; y++)
                            {
                                SetEnabled((string)active[y], true);
                            }

                            GameController.Instance.soundController.OnButtonSFX();
                            dblg.LoadWorld(worldFile);
                            break;
                        case DialogBoxResult.No:
                            GameController.Instance.soundController.OnButtonSFX();
                            dblg.LoadWorld(worldFile);
                            break;
                        case DialogBoxResult.Cancel:
                            GameController.Instance.soundController.OnButtonSFX();
                            break;
                    }
                };

            check.ShowDialog();
            return;
        }

        dblg.LoadWorld(worldFile);
    }
}
