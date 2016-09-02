#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildGame
{
    private const string BuildName = "ProjectPorcupine";

    public static void All()
    {
        OSX();
        Windows();
        Linux();
    }

    public static void Windows() 
    {
        Windows32();
        Windows64();
    }

    public static void Windows32()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/" + BuildName + "_Win32", BuildTarget.StandaloneWindows, BuildOptions.None);
    }

    public static void Windows64() 
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/" + BuildName + "_Win64", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static void OSX()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/" + BuildName + "_OSX", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
    }

    public static void Linux()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/" + BuildName + "_Linux", BuildTarget.StandaloneLinuxUniversal, BuildOptions.None);
    }

    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }
}

/*
to build from command line:
    mac:
        /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -logfile -projectPath path/to/ProjectPorcupine -executeMethod BuildGame.OSX
    windows: 
        "C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -logfile -projectPath path/to/ProjectPorcupine -executeMethod BuildGame.Windows
*/