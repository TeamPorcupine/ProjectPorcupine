#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

/// <summary>
/// Different mathematical calculations. 
/// </summary>
public static class ColorUtilities
{
    /// <summary>
    /// Returns a Color from RGB values.
    /// </summary>
    public static Color ColorFromIntRGB(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
    }

    public static Color ParseColorFromString(string r, string g, string b)
    {
        return new Color(float.Parse(r), float.Parse(g), float.Parse(b), 1.0f);
    }

    public static Color RandomColor()
    {
        return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1.0f);
    }

    public static Color RandomGrayColor()
    {
        float shade = UnityEngine.Random.Range(0f, 1f);
        return new Color(shade, shade, shade, 1.0f);
    }

    public static Color RandomSkinColor()
    {
        // default skincolors to pick at random
        Color[] skinColors = new Color[]
        {
            ColorUtilities.ColorFromIntRGB(245, 217, 203),
            ColorUtilities.ColorFromIntRGB(237, 191, 167),
            ColorUtilities.ColorFromIntRGB(211, 142, 111),
            ColorUtilities.ColorFromIntRGB(234, 183, 138),
            ColorUtilities.ColorFromIntRGB(197, 132, 92),
            ColorUtilities.ColorFromIntRGB(88, 59, 43)
        };
        return skinColors[UnityEngine.Random.Range(0, skinColors.Length - 1)];
    }
}
