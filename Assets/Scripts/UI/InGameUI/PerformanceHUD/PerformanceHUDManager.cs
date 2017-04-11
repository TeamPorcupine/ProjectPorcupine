#region License
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
    private static int columnRootIndex = 0;

    /// <summary>
    /// All of our root objects.
    /// </summary>
    private static List<GameObject> columnRootObjects = new List<GameObject>();

    public static string[] GetNames()
    {
        return allGroups.Keys.Select(x => x.name).ToArray();
    }

    /// <summary>
    /// Clean and Re-Draw.
    /// </summary>
    public static void DirtyUI()
    {
        // Clear
        foreach (Transform rootTransform in columnRootObjects.Select(x => x.transform))
        {
            foreach (Transform child in rootTransform)
            {
                Destroy(child.gameObject);
            }
        }

        groupPointer = allGroups.FirstOrDefault(x => x.Key.name == SettingsKeyHolder.PerformanceHUD).Key;

        // Set group
        if (groupPointer.name == null)
        {
            groupPointer = allGroups.First(x => x.Key.name == "none").Key;
        }

        // Draw and Begin UI Functionality
        foreach (BasePerformanceHUDElement elementName in allGroups[groupPointer])
        {
            Transform rootTransfer = GetColumnRootObject().transform;
            GameObject go = elementName.InitializeElement();
            go.transform.SetParent(rootTransfer);
            go.name = elementName.GetName();
        }
    }

    /// <summary>
    /// The root object for the HUD.
    /// </summary>
    private static GameObject GetColumnRootObject()
    {
        if (columnRootIndex < columnRootObjects.Count)
        {
            columnRootIndex++;
            return columnRootObjects[columnRootIndex - 1];
        }
        else if (columnRootIndex > 0)
        {
            columnRootIndex = 0;
            return columnRootObjects[columnRootIndex];
        }
        else if (columnRootObjects.Count == 0)
        {
            throw new System.Exception("Column Root Object Array is empty and the system wants an object");
        }
        else
        {
            columnRootIndex++;
            return GetColumnRootObject();
        }
    }

    /// <summary>
    /// Assign variables, and hookup to API.
    /// </summary>
    private void Start()
    {
        TimeManager.Instance.EveryFrame += Instance_EveryFrame;

        // Root should already exist just grab child
        columnRootObjects = new List<GameObject>();
        foreach (Transform child in transform)
        {
            columnRootObjects.Add(child.gameObject);
        }

        // Load Settings
        allGroups = new Dictionary<PerformanceGroup, BasePerformanceHUDElement[]>();

        PerformanceGroup[] groups = PrototypeManager.PerformanceHUD.Values.SelectMany(x => x.groups).ToArray();

        allGroups.Add(new PerformanceGroup("none", new string[0], new Parameter[0], true), new BasePerformanceHUDElement[0]);
        List<BasePerformanceHUDElement> elements = new List<BasePerformanceHUDElement>();

        // Convert the dictionary of specialised elements to a more generalised format
        for (int i = 0; i < groups.Length; i++)
        {
            for (int j = 0; j < groups[i].elementData.Length; j++)
            {
                if (FunctionsManager.PerformanceHUD.HasFunction("Get" + groups[i].elementData[j]))
                {
                    BasePerformanceHUDElement element = FunctionsManager.PerformanceHUD.Call("Get" + groups[i].elementData[j]).ToObject<BasePerformanceHUDElement>();
                    element.parameterData = groups[i].parameterData[j];
                    element.InitializeLUA();
                    elements.Add(element);
                }
                else
                {
                    Debug.LogWarning("Get" + groups[i] + groups[i].elementData[j] + "() Doesn't exist");
                }
            }

            allGroups.Add(groups[i], elements.ToArray());
            elements.Clear();
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
                element.UpdateLUA();
            }
        }
    }
}
