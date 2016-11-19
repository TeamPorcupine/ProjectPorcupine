using UnityEngine;
using System.Collections;
using DeveloperConsole;

public static class CommandFunctions
{
    public static void SetTimeStamp(string values)
    {
        bool result;
        if (bool.TryParse(values, out result))
        {
            CommandSettings.ShowTimeStamp = result;
            DevConsole.Log("Change successful :D", "green");
        }
        else
        {
            DevConsole.LogError("Value not a boolean");
        }
    }
}