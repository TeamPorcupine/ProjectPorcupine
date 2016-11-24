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

    private bool allState = false;
    private bool allPreveState = false;
    private bool needToChangeDefaultState = false;

    [MenuItem("Window/Debugger Channel Control")]
    public static void ShowWindow()
    {
        GetWindow(typeof(DebuggerChannelControl));
    }

    private void Awake()
    {
        window = GetWindow(typeof(DebuggerChannelControl));
        window.minSize = new Vector2(460, 100);
        EditorApplication.playmodeStateChanged += OnPlayStateChanged;
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal("Box");
        bool allStateChanged = false;
        allState = GUILayout.Toggle(allState, "All");

        UnityDebugger.Debugger.DefaultState = allState;

        if (allState != allPreveState)
        {
            allPreveState = allState;
            allStateChanged = true;
        }

        if (needToChangeDefaultState) 
        {
//            if (UnityDebugger.Debugger.Channels != null)
//            {
                UnityDebugger.Debugger.DefaultState = allState;
                needToChangeDefaultState = false;
//            }

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
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    public void OnPlayStateChanged()
    {
        Debug.LogWarning("This Happens");
        EditorApplication.playmodeStateChanged += OnPlayStateChanged;
            UnityDebugger.Debugger.DefaultState = allState;
            needToChangeDefaultState = true;
    }
}
