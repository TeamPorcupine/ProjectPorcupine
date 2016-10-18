#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the PerformanceHUD (new FPS counter)
/// </summary>
public class PerformanceHUDManager : MonoBehaviour
{
    /// <summary>
    /// The current group/mode to display
    /// </summary>
    private static PerformanceComponentGroup currentGroup;

    /// <summary>
    /// The root object for the HUD
    /// </summary>
    private static GameObject rootObject;

    // Setup the groups
    void Awake()
    {
        PerformanceComponentGroups.groups = new PerformanceComponentGroup[] {
            PerformanceComponentGroups.none,
            PerformanceComponentGroups.basic,
            PerformanceComponentGroups.extended,
            PerformanceComponentGroups.verbose
        };
    }

    void Start()
    {
        // Root should already exist just grab child
        if (rootObject == null)
        {
            rootObject = transform.GetChild(0).gameObject;
        }

        int groupSetting = Settings.GetSetting("DialogBoxSettings_performanceGroup", 1);

        // Just a guard statement essentially
        if (PerformanceComponentGroups.groups.Length > groupSetting)
        {
            Debug.ULogChannel("Performance", "The current channel was set to index: " + groupSetting);
            currentGroup = PerformanceComponentGroups.groups[groupSetting];

            // Order by ascending using Linq
            if (currentGroup.disableUI == true)
                currentGroup.groupElements = currentGroup.groupElements.OrderBy(c => c.priorityID()).ToArray();
        }
        // If so then just set to first option (normally none)
        else if (groupSetting > 0 && PerformanceComponentGroups.groups.Length > 0)
        {
            Debug.ULogErrorChannel("Performance", "Index out of range: Current group is set to 0" + groupSetting);
        }
        // Else set to none (none is a readonly so it should always exist)
        else
        {
            Debug.ULogErrorChannel("Performance", "Array Empty: The PerformanceComponentGroups.groups array is empty");
            currentGroup = PerformanceComponentGroups.none;
        }

        // Setup UI
        DirtyUI();
    }

    void Update()
    {
        // If we are -1 then its none so disable UI
        if (currentGroup.disableUI == true)
        {
            //Disable self
            gameObject.SetActive(false);

            return;
        }

        // Update UI
        // Shouldn't be slower and makes more contextual sense
        foreach (BasePerformanceComponent element in currentGroup.groupElements)
        {
            if (element != null)
            {
                element.Update();
            }
        }
    }

    /// <summary>
    /// Clean and Re-Draw
    /// Can be static cause it shouldn't matter
    /// </summary>
    public static void DirtyUI()
    {
        // Could be improved but its fine
        rootObject.transform.parent.gameObject.SetActive(true);
        // Clear
        foreach (Transform child in rootObject.transform)
        {
            Destroy(child.gameObject);
        }

        // Draw
        // Get new Performance Mode/Group
        currentGroup = PerformanceComponentGroups.groups[Settings.GetSetting("DialogBoxSettings_performanceGroup", 1)];
        // Order by ascending using Linq
        if (currentGroup.disableUI == true)
            currentGroup.groupElements = currentGroup.groupElements.OrderBy(c => c.priorityID()).ToArray();

        // Draw and Begin UI Functionality
        foreach (BasePerformanceComponent element in currentGroup.groupElements)
        {
            BasePerformanceComponentUI go = ((GameObject)Instantiate(Resources.Load(element.nameOfComponent()))).GetComponent<BasePerformanceComponentUI>();
            go.gameObject.transform.SetParent(rootObject.transform);

            element.Start(go);
        }
    }
}
