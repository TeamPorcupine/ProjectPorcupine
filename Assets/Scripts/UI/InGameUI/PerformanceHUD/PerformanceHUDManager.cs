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

public class PerformanceHUDManager : MonoBehaviour
{
    // The current mode
    private static PerformanceComponentGroup currentGroup;

    // Root Object
    private static GameObject rootObject;

    void Awake()
    {
        PerformanceComponentGroups.groups = new PerformanceComponentGroup[] {
            PerformanceComponentGroups.none,
            PerformanceComponentGroups.basic,
            PerformanceComponentGroups.extended,
            PerformanceComponentGroups.verbose
        };
    }

    // Use this for initialization
    void Start()
    {
        if (rootObject == null)
        {
            rootObject = transform.GetChild(0).gameObject;
        }

        //TODO: REMOVE TESTING VALUE
        int groupSetting = Settings.GetSetting("DialogBoxSettings_performanceGroup", 1);

        // Just a guard statement essentially
        if (PerformanceComponentGroups.groups.Length > groupSetting)
        {
            Debug.ULogChannel("Performance", "The current channel was set to index: " + groupSetting);
            currentGroup = PerformanceComponentGroups.groups[groupSetting];

            // Order by ascending using Linq
            if (currentGroup.groupID != -1)
                currentGroup.groupElements = currentGroup.groupElements.OrderBy(c => c.priorityID()).ToArray();
        }
        else if (groupSetting > 0 && PerformanceComponentGroups.groups.Length > 0)
        {
            Debug.ULogErrorChannel("Performance", "Index out of range: Current group is set to 0" + groupSetting);
        }
        else
        {
            Debug.ULogErrorChannel("Performance", "Array Empty: The PerformanceComponentGroups.groups array is empty");
            currentGroup = PerformanceComponentGroups.none;
        }

        DirtyUI();
    }

    // Update is called once per frame
    void Update()
    {
        // Could be simplified?
        if (currentGroup.groupID == -1)
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
        // Could be improved
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
        if (currentGroup.groupID != -1)
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
