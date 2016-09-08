#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using UnityEngine;

public interface IBuildable
{
    /// <summary>
    /// Gets the width of the furniture.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the furniture.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Checks whether the furniture has a certain tag.
    /// </summary>
    /// <param name="typeTag">Tag to check for.</param>
    /// <returns>True if furniture has specified tag.</returns>
    bool HasTypeTag(string typeTag);
}
