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
    // Static to dirty UI (re draw)
    // Set to true so it'll draw it the first time
    public static bool dirty = true;

    // The current mode
    private PerformanceComponentGroup currentGroup;

    // Root Object
    public GameObject rootObject;

    // Use this for initialization
    void Start()
    {
        PerformanceComponentGroups.groups = new PerformanceComponentGroup[] {
            PerformanceComponentGroups.none,
            PerformanceComponentGroups.basic, PerformanceComponentGroups.standard,
            PerformanceComponentGroups.extended,
            PerformanceComponentGroups.verbose
        };

        if (rootObject == null)
        {
            rootObject = transform.GetChild(0).gameObject;
        }

        //TODO: REMOVE TESTING VALUE
        int groupSetting = 1; //Settings.GetSetting("DialogBoxSettings_performanceGroup", 3);

        // Just a guard statement essentially
        if (PerformanceComponentGroups.groups.Length > groupSetting)
        {
            Debug.ULogChannel("Performance", "The current channel was set to index: " + groupSetting);
            currentGroup = PerformanceComponentGroups.groups[groupSetting];

            // Order by ascending using Linq
            if (currentGroup.groupID != -1)
                currentGroup.groupElements = currentGroup.groupElements.OrderBy(c => c.priorityID()).ToArray();
        }
        else if (groupSetting > 0)
        {
            Debug.ULogErrorChannel("Performance", "Index out of range: The PerformanceComponentGroups.groups array is <= to " + groupSetting);
        }
        else
        {
            Debug.ULogErrorChannel("Performance", "Array Empty: The PerformanceComponentGroups.groups array is empty");
            currentGroup = PerformanceComponentGroups.none;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // POTENTIAL BUG, if for some reason doesn't tag dirty
        if (currentGroup.groupID == -1)
        {
            if (dirty)
                ClearUI();

            return;
        }

        if (dirty)
            DrawUI();

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

    void ClearUI()
    {
        //Clear UI
        foreach (Transform child in rootObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void DrawUI()
    {
        // Get new Performance Mode/Group
        currentGroup = PerformanceComponentGroups.groups[1];//Settings.GetSetting("DialogBoxSettings_performanceGroup", 1)];
        // Order by ascending using Linq
        if (currentGroup.groupID != -1)
            currentGroup.groupElements = currentGroup.groupElements.OrderBy(c => c.priorityID()).ToArray();

        // Draw and Begin UI Functionality
        foreach (BasePerformanceComponent element in currentGroup.groupElements)
        {
            if (element.UIComponent() == null)
            {
                BasePerformanceComponentUI go = ((GameObject)Instantiate(Resources.Load(element.nameOfComponent()))).GetComponent<BasePerformanceComponentUI>();
                go.gameObject.transform.SetParent(rootObject.transform);

                element.Start(go);
            }
            else
            {
                element.Start(element.UIComponent());
            }
        }

        dirty = false;
    }
}