using UnityEngine;
using System.Collections;
using DeveloperConsole;

public static class CommandFunctions
{
    public static void SetTimeStamp(Vector3 on)
    {
        Debug.LogWarning(on);
        DevConsole.Log("Change successful :D", "green");
    }
}