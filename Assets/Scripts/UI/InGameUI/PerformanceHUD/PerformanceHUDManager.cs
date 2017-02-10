﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the PerformanceHUD (new FPS counter).
/// </summary>
public class PerformanceHUDManager : MonoBehaviour
{
    /// <summary>
    /// The current group/mode to display.
    /// </summary>
    public static Dictionary<PerformanceGroup, BasePerformanceHUDElement[]> allGroups;

    /// <summary>
    /// What group are we currently at.
    /// </summary>
    private static PerformanceGroup groupPointer;

    /// <summary>
    /// What current root are we at.
    /// </summary>
    private static int rootIndex = 0;

    /// <summary>
    /// All of our root objects.
    /// </summary>
    private static List<GameObject> rootObjects = new List<GameObject>();

    /// <summary>
    /// The root object for the HUD.
    /// </summary>
    private static GameObject RootObject
    {
        get
        {
            if (rootIndex < rootObjects.Count)
            {
                rootIndex++;
                return rootObjects[rootIndex - 1];
            }
            else if (rootIndex > 0)
            {
                rootIndex = 0;
                return rootObjects[rootIndex];
            }
            else
            {
                return null;
            }
        }

        set
        {
            rootObjects.Add(value);
        }
    }

    public static string[] GetNames()
    {
        return allGroups.Keys.Select(x => x.name).ToArray();
    }

    /// <summary>
    /// Clean and Re-Draw.
    /// </summary>
    public static void DirtyUI()
    {
        // Guard
        if (RootObject == null)
        {
            return;
        }

        // Could be improved but its fine
        RootObject.transform.parent.gameObject.SetActive(true);

        // Clear
        foreach (Transform child in RootObject.transform)
        {
            if (child.tag == "PerformanceUI")
            {
                Destroy(child.gameObject);
            }
        }

        groupPointer = allGroups.FirstOrDefault(x => x.Key.name == CommandSettings.PerformanceHUDMode).Key;

        // Set group
        if (groupPointer.name == null)
        {
            groupPointer = allGroups.First(x => x.Key.name == "none").Key;
        }

        // Draw and Begin UI Functionality
        foreach (BasePerformanceHUDElement elementName in allGroups[groupPointer])
        {
            GameObject go = elementName.InitializeElement();
            go.transform.SetParent(RootObject.transform);
            go.name = elementName.GetName();
        }
    }

    /// <summary>
    /// Assign variables, and hookup to API.
    /// </summary>
    private void Start()
    {
        TimeManager.Instance.EveryFrame += Instance_EveryFrame;

        // Root should already exist just grab child
        rootObjects = new List<GameObject>();
        foreach (Transform child in transform)
        {
            rootObjects.Add(child.gameObject);
        }

        // Load Settings
        allGroups = new Dictionary<PerformanceGroup, BasePerformanceHUDElement[]>();

        PerformanceGroup[] groups = PrototypeManager.PerformanceHUD.Values.SelectMany(x => x.groups).ToArray();

        allGroups.Add(new PerformanceGroup("none", new string[0], true), new BasePerformanceHUDElement[0]);

        for (int i = 0; i < groups.Length; i++)
        {
            BasePerformanceHUDElement[] elements = new BasePerformanceHUDElement[groups[i].elementNames.Length];

            for (int j = 0; j < groups[i].elementNames.Length; j++)
            {
                if (FunctionsManager.PerformanceHUD.HasFunction("Get" + groups[i].elementNames[j]))
                {
                    elements[j] = FunctionsManager.PerformanceHUD.Call("Get" + groups[i].elementNames[j]).ToObject<BasePerformanceHUDElement>();
                }
                else
                {
                    Debug.LogWarning("Get" + groups[i].elementNames[j] + "() Doesn't exist");
                }
            }

            allGroups.Add(groups[i], elements);
        }

        // Setup UI
        DirtyUI();
    }

    private void Instance_EveryFrame(float obj)
    {
        // If we are at group -1, or are already disabled then return
        if (gameObject.activeInHierarchy == false && groupPointer.disableUI == true)
        {
            // Disable self
            gameObject.SetActive(false);

            return;
        }

        // Update UI
        foreach (BasePerformanceHUDElement element in allGroups[groupPointer])
        {
            if (element != null)
            {
                element.Update();
            }
        }
    }
}
