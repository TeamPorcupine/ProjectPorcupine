using UnityEngine;
using System.Collections;

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
