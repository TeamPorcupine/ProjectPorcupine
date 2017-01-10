#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;

// Misc.
public static partial class SettingsKeyHolder
{
    public static int WorldWidth
    {
        get
        {
            return Settings.GetSetting("worldWidth", 101);
        }

        set
        {
            Settings.SetSetting("worldWidth", value);
        }
    }

    public static int WorldHeight
    {
        get
        {
            return Settings.GetSetting("worldHeight", 101);
        }

        set
        {
            Settings.SetSetting("worldHeight", value);
        }
    }

    public static int ZoomLerp
    {
        get
        {
            return Settings.GetSetting("ZoomLerp", 10);
        }

        set
        {
            Settings.SetSetting("ZoomLerp", value);
        }
    }

    public static int ZoomSensitivity
    {
        get
        {
            return Settings.GetSetting("ZoomSensitivity", 3);
        }

        set
        {
            Settings.SetSetting("ZoomSensitivity", value);
        }
    }
}

// General
public static partial class SettingsKeyHolder
{
    public static string Language
    {
        get
        {
            return Settings.GetSetting("general_localization_language", "en_US");
        }

        set
        {
            Settings.SetSetting("general_localization_language", value);
        }
    }

    public static bool AutoUpdate
    {
        get
        {
            return Settings.GetSetting("general_localization_autoUpdate", true);
        }

        set
        {
            Settings.SetSetting("general_localization_autoUpdate", value);
        }
    }

    public static int Interval
    {
        get
        {
            return Settings.GetSetting("general_autosave_interval", 10);
        }

        set
        {
            Settings.SetSetting("general_autosave_interval", value);
        }
    }

    public static int MaxFiles
    {
        get
        {
            return Settings.GetSetting("general_autosave_maxFiles", 5);
        }

        set
        {
            Settings.SetSetting("general_autosave_maxFiles", value);
        }
    }
}

// Sound
public static partial class SettingsKeyHolder
{
    public static float Master
    {
        get
        {
            return Settings.GetSetting("sound_volume_master", 1);
        }

        set
        {
            Settings.SetSetting("sound_volume_master", value);
        }
    }

    public static float Music
    {
        get
        {
            return Settings.GetSetting("sound_volume_music", 1);
        }

        set
        {
            Settings.SetSetting("sound_volume_music", value);
        }
    }

    public static float Game
    {
        get
        {
            return Settings.GetSetting("sound_volume_game", 1);
        }

        set
        {
            Settings.SetSetting("sound_volume_game", value);
        }
    }

    public static float Alerts
    {
        get
        {
            return Settings.GetSetting("sound_volume_alerts", 1);
        }

        set
        {
            Settings.SetSetting("sound_volume_alerts", value);
        }
    }

    public static float UI
    {
        get
        {
            return Settings.GetSetting("sound_volume_ui", 1);
        }

        set
        {
            Settings.SetSetting("sound_volume_ui", value);
        }
    }

    // SettingsMenu TODO: Maybe make it a struct?  Depends on how we interpret this.
    public static int Device
    {
        get
        {
            return Settings.GetSetting("sound_advanced_device", 0);
        }

        set
        {
            Settings.SetSetting("sound_advanced_device", value);
        }
    }

    public static bool Locational
    {
        get
        {
            return Settings.GetSetting("sound_advanced_locational", true);
        }

        set
        {
            Settings.SetSetting("sound_advanced_locational", value);
        }
    }
}

// Video
public static partial class SettingsKeyHolder
{
    public static int UISkin
    {
        get
        {
            return Settings.GetSetting("video_general_uiSkin", 0);
        }

        set
        {
            Settings.SetSetting("video_general_uiSkin", value);
        }
    }

    public static int Quality
    {
        get
        {
            return Settings.GetSetting("video_general_quality", 2);
        }

        set
        {
            Settings.SetSetting("video_general_quality", value);
        }
    }

    public static int Vsync
    {
        get
        {
            return Settings.GetSetting("video_general_vsync", 0);
        }

        set
        {
            Settings.SetSetting("video_general_vsync", value);
        }
    }

    public static bool SoftParticles
    {
        get
        {
            return Settings.GetSetting("video_advanced_particles", true);
        }

        set
        {
            Settings.SetSetting("video_advanced_particles", value);
        }
    }

    public static int Shadows
    {
        get
        {
            return Settings.GetSetting("video_advanced_shadows", 0);
        }

        set
        {
            Settings.SetSetting("video_advanced_shadows", value);
        }
    }

    public static int AnisotropicFiltering
    {
        get
        {
            return Settings.GetSetting("video_advanced_anisotropicFiltering", 2);
        }

        set
        {
            Settings.SetSetting("video_advanced_anisotropicFiltering", value);
        }
    }

    public static int AA
    {
        get
        {
            return Settings.GetSetting("video_advanced_aa", 0);
        }

        set
        {
            Settings.SetSetting("video_advanced_aa", value);
        }
    }

    public static bool Fullscreen
    {
        get
        {
            return Settings.GetSetting("video_window_mode", true);
        }

        set
        {
            Settings.SetSetting("video_window_mode", value);
        }
    }

    public static int Resolution
    {
        get
        {
            return Settings.GetSetting("video_window_resoultion", 0);
        }

        set
        {
            Settings.SetSetting("video_window_resoultion", value);
        }
    }
}

public static partial class SettingsKeyHolder
{
    public static bool DeveloperMode
    {
        get
        {
            return Settings.GetSetting("developer_general_developerMode", true);
        }

        set
        {
            Settings.SetSetting("developer_general_developerMode", value);
        }
    }

    public static bool LoggingLevel
    {
        get
        {
            return Settings.GetSetting("developer_general_loggingLevel", false);
        }

        set
        {
            Settings.SetSetting("developer_general_loggingLevel", value);
        }
    }

    public static int ScrollSensitivity
    {
        get
        {
            return Settings.GetSetting("developer_console_scrollSensitivity", 6);
        }

        set
        {
            Settings.SetSetting("developer_console_scrollSensitivity", value);
        }
    }

    public static bool EnableDevConsole
    {
        get
        {
            return Settings.GetSetting("developer_console_enableDevConsole", true);
        }

        set
        {
            Settings.SetSetting("developer_console_enableDevConsole", value);
        }
    }

    public static bool TimeStamps
    {
        get
        {
            return Settings.GetSetting("developer_console_timeStamp", true);
        }

        set
        {
            Settings.SetSetting("developer_console_timeStamp", value);
        }
    }

    public static int FontSize
    {
        get
        {
            return Settings.GetSetting("developer_console_fontSize", 15);
        }

        set
        {
            Settings.SetSetting("developer_console_fontSize", value);
        }
    }

    public static int PerformanceHUD
    {
        get
        {
            return Settings.GetSetting("developer_utilities_performanceHUD", 1);
        }

        set
        {
            Settings.SetSetting("developer_utilities_performanceHUD", value);
        }
    }
}