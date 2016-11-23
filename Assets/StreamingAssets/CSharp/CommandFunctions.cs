using System.Collections;
using DeveloperConsole;

public static class CommandFunctions
{
    // Just doing this so its an example
    // Tbh this is really quite cool, you can say any variable 
    // And it will adjust to that, including objects (as long as those objects exist in a assembly/namespace used commonly)
    public static void SetTimeStamp(bool on)
    {
        CommandSettings.ShowTimeStamp = on;
        DevConsole.Log("Change successful :D", "green");
    }
}