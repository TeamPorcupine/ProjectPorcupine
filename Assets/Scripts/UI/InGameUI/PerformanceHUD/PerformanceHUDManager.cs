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

/// <summary>
/// Manages the PerformanceHUD (new FPS counter).
/// </summary>
public class PerformanceHUDManager : MonoBehaviour
{
    /// <summary>
    /// The current group/mode to display.
    /// </summary>
    private static PerformanceComponentGroup currentGroup;

    /// <summary>
    /// The root object for the HUD.
    /// </summary>
    private static GameObject rootObject;

    /// <summary>
    /// Clean and Re-Draw
    /// Can be static cause it shouldn't matter.
    /// </summary>
    public static void DirtyUI()
    {
        // Guard
        if (rootObject == null)
        {
            return;
        }

        // Could be improved but its fine
        rootObject.transform.parent.gameObject.SetActive(true);

        // Clear
        foreach (Transform child in rootObject.transform)
        {
            Destroy(child.gameObject);
        }

        // Draw
        // Get new Performance Mode/Group
        currentGroup = PerformanceComponentGroups.groups[CommandSettings.PerformanceHUDMode];

        // Draw and Begin UI Functionality
        foreach (BasePerformanceHUDElement element in currentGroup.groupElements)
        {
            //IPerformanceComponent go = ((GameObject)Instantiate(Resources.Load(element.NameOfComponent()))).GetComponent<BasePerformanceComponentUI>();
            //go.start();
        }
    }

    // Setup the groups
    private void Awake()
    {
        PerformanceComponentGroups.groups = new PerformanceComponentGroup[]
        {
            PerformanceComponentGroups.None,
            PerformanceComponentGroups.Basic,
            PerformanceComponentGroups.Extended,
            PerformanceComponentGroups.Verbose
        };
    }

    private void Start()
    {
        TimeManager.Instance.EveryFrame += Instance_EveryFrame;

        // Root should already exist just grab child
        if (rootObject == null)
        {
            rootObject = transform.GetChild(0).gameObject;
        }

        int groupSetting = CommandSettings.PerformanceHUDMode;

        // Just a guard statement essentially
        if (PerformanceComponentGroups.groups.Length > groupSetting)
        {
            UnityDebugger.Debugger.Log("Performance", "The current channel was set to index: " + groupSetting);
            currentGroup = PerformanceComponentGroups.groups[groupSetting];
        }
        else if (groupSetting > 0 && PerformanceComponentGroups.groups.Length > 0)
        {
            // If so then just set to first option (normally none)
            UnityDebugger.Debugger.LogError("Performance", "Index out of range: Current group is set to 0" + groupSetting);
        }
        else
        {
            // Else set to none (none is a readonly so it should always exist)
            UnityDebugger.Debugger.LogError("Performance", "Array Empty: The PerformanceComponentGroups.groups array is empty");
            currentGroup = PerformanceComponentGroups.None;
        }

        // Setup UI
        DirtyUI();
    }

    private void Instance_EveryFrame(float obj)
    {
        // If we are at group -1, or are already disabled then return
        if (gameObject.activeInHierarchy == false || currentGroup.disableUI == true)
        {
            // Disable self
            gameObject.SetActive(false);

            return;
        }

        // Update UI
        foreach (BasePerformanceHUDElement element in currentGroup.groupElements)
        {
            if (element != null)
            {
                element.Update();
            }
        }
    }
}
