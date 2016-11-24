#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
    
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DebuggerChannelControl : EditorWindow
{
    private EditorWindow window;

    private bool allState;
    private bool allPreveState;
    private ChannelSettingsSO channelSettings;

    [MenuItem("Window/Debugger Channel Control")]
    public static void ShowWindow()
    {
        GetWindow(typeof(DebuggerChannelControl));
    }

    private void Awake()
    {
        channelSettings = Resources.Load<ChannelSettingsSO>("ChannelSettings");
        if(channelSettings == null)
        {
            channelSettings = ScriptableObject.CreateInstance<ChannelSettingsSO>();
            AssetDatabase.CreateAsset(channelSettings, "Assets/Resources/ChannelSettings.asset");
            AssetDatabase.SaveAssets();
        }

        allState = channelSettings.DefaultState;
        allPreveState = allState;
        window = GetWindow(typeof(DebuggerChannelControl));
        window.minSize = new Vector2(460, 100);
    }

    private void OnGUI()
    {
        bool allStateChanged = false;
        allState = GUILayout.Toggle(allState, "All");

        if (allState != allPreveState)
        {
            allPreveState = allState;
            allStateChanged = true;
            channelSettings.DefaultState = allState;
            EditorUtility.SetDirty(channelSettings);
        }

        if(UnityDebugger.Debugger.Channels != null)
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

            foreach (string channelName in toggleReturns.Keys.AsEnumerable())
            {
                UnityDebugger.Debugger.Channels[channelName] = toggleReturns[channelName];
//                if (!channelSettings.ChannelState.ContainsKey(channelName))
//                {
                if (!channelSettings.ChannelState.ContainsKey(channelName) || channelSettings.ChannelState[channelName] != toggleReturns[channelName])
                {
                    channelSettings.ChannelState[channelName] = toggleReturns[channelName];
                    EditorUtility.SetDirty(channelSettings);
                }
//                }
            }
        }

        EditorGUILayout.Space();
    }
}