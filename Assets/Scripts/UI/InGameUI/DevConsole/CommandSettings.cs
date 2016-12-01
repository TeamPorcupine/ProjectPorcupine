#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using DeveloperConsole;

public static class CommandSettings
{
    /// <summary>
    /// The current font size.
    /// </summary>
    public static int FontSize
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_consoleFontSize", 15);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_consoleFontSize", value);
            Settings.SaveSettings();

            DevConsole.DirtySettings();
        }
    }

    public static int ScrollingSensitivity
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_scrollSensitivity", 6);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_scrollSensitivity", value);
            Settings.SaveSettings();

            DevConsole.DirtySettings();
        }
    }

    /// <summary>
    /// Show a time stamp at the end of every command.
    /// </summary>
    public static bool ShowTimeStamp
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_timeStampToggle", true);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_timeStampToggle", value);
            Settings.SaveSettings();
        }
    }

    /// <summary>
    /// Activate the developer console.
    /// </summary>
    public static bool DeveloperConsoleToggle
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_devConsoleToggle", true);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_devConsoleToggle", value);
            Settings.SaveSettings();
        }
    }

    /// <summary>
    /// The current performance hud mode.
    /// </summary>
    public static int PerformanceHUDMode
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_performanceGroup", 1);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_performanceGroup", value);
            Settings.SaveSettings();
        }
    }

    /// <summary>
    /// Is developer mode toggled.
    /// </summary>
    public static bool DeveloperModeToggle
    {
        get
        {
            return Settings.GetSetting("DialogBoxSettingsDevConsole_developerModeToggle", false);
        }

        set
        {
            Settings.SetSetting("DialogBoxSettingsDevConsole_developerModeToggle", value);
            Settings.SaveSettings();
        }
    }
}