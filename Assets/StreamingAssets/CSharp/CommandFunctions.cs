using System.Collections;
using DeveloperConsole;

public static class CommandFunctions
{
    /// <summary>
    /// To prevent C# Modding side of commands from getting lost
    /// </summary>
    public static void SetTimeStamp(bool on)
    {
        CommandSettings.ShowTimeStamp = on;
        DevConsole.Log("Change successful :D", "green");
    }
}