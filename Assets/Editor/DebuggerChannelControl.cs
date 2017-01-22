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
using UnityEditor;
using UnityEngine;

public class DebuggerChannelControl : EditorWindow, IHasCustomMenu
{ 
    private EditorWindow window;
    private bool allState;
    private bool allPreveState;
    private ChannelSettingsSO channelSettings;
    private Vector2 scrollViewVector = Vector2.down;
    private string channelSettingsPath = "Assets/Resources/ChannelSettings.asset";

    [MenuItem("Window/Debugger Channel Control")]
    public static void ShowWindow()
    {
        GetWindow(typeof(DebuggerChannelControl));
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(
            new GUIContent("Reload"),
            false,
            new GenericMenu.MenuFunction(() =>
            {
                AssetDatabase.DeleteAsset(channelSettingsPath);
                Repaint();
            }));
    }

    private void Awake()
    {
        channelSettings = Resources.Load<ChannelSettingsSO>("ChannelSettings");
        if (channelSettings == null)
        {
            channelSettings = ScriptableObject.CreateInstance<ChannelSettingsSO>();
            AssetDatabase.CreateAsset(channelSettings, channelSettingsPath);
            AssetDatabase.SaveAssets();
        }

        allState = channelSettings.DefaultState;
        allPreveState = allState;
    }

    private void OnGUI()
    {
        bool dirtySettings = false;
        scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
        EditorGUILayout.BeginVertical("Box");
        bool allStateChanged = false;
        allState = GUILayout.Toggle(allState, "All");

        if (allState != allPreveState)
        {
            allPreveState = allState;
            allStateChanged = true;
            channelSettings.DefaultState = allState;
            dirtySettings = true;
        }

        if (UnityDebugger.Debugger.Channels != null)
        {
            Dictionary<string, bool> toggleReturns = new Dictionary<string, bool>();
            foreach (string channelName in UnityDebugger.Debugger.Channels.Keys.AsEnumerable())
            {
                toggleReturns.Add(channelName, GUILayout.Toggle(UnityDebugger.Debugger.Channels[channelName], channelName));
                if (allStateChanged)
                {
                    toggleReturns[channelName] = allState;
                }
            }

            foreach (string channelName in toggleReturns.Keys.ToList())
            {
                UnityDebugger.Debugger.Channels[channelName] = toggleReturns[channelName];

                if (channelSettings == null)
                {
                    // We're in a weird state with no channelSettings, just bail 'til we get it back.
                    return;
                }

                if (!channelSettings.ChannelState.ContainsKey(channelName) && toggleReturns.ContainsKey(channelName))
                {
                    bool theValueIWant = toggleReturns[channelName];
                    channelSettings.ChannelState.Add(channelName, theValueIWant);
                    dirtySettings = true;
                }
                else if (channelSettings.ChannelState[channelName] != toggleReturns[channelName])
                {
                    channelSettings.ChannelState[channelName] = toggleReturns[channelName];
                    dirtySettings = true;
                }
            }
        }

        if (dirtySettings)
        {
            if (channelSettings == null)
            {
                Awake();
            }
            
            EditorUtility.SetDirty(channelSettings);
        }

        EditorGUILayout.EndVertical();
        GUILayout.EndScrollView();
    }    
}
