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
using UnityEditor;
using UnityEngine;

public class DebuggerChannelControl : EditorWindow
{
    private EditorWindow window;
    private HashSet<string> Channels;

    [MenuItem("Window/Debugger Channel Control")]
    public static void ShowWindow()
    {
        GetWindow(typeof(DebuggerChannelControl));
    }

    private void Awake()
    {
        window = GetWindow(typeof(DebuggerChannelControl));
        window.minSize = new Vector2(460, 100);
        Channels = new HashSet<string>();
        Channels.Add("Pathfinder");
        Channels.Add("SpriteManager");
        Channels.Add("More");
        Channels.Add("More1");
        Channels.Add("More2");
        Channels.Add("More3");
    }

    private void OnGUI()
    {
//        GUILayout.BeginHorizontal("Box");
        GUILayout.Toggle(true, "All");

        foreach (string channelName in Channels)
        {
            GUILayout.Toggle(true, channelName);
        }
//        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
}
