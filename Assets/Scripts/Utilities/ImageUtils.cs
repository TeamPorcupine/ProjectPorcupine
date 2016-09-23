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

public static class ImageUtils
{
    public static Vector3 SpritePivotOffset(Sprite sprite, float rotation = 0f)
    {
        Vector3 offset;

        if (Math.Abs(rotation) == 90 || Math.Abs(rotation) == 270)
        {
            offset = new Vector3((sprite.pivot.y / sprite.pixelsPerUnit) - 0.5f, (sprite.pivot.x / sprite.pixelsPerUnit) - 0.5f, 0);
        }
        else
        {
            offset = new Vector3((sprite.pivot.x / sprite.pixelsPerUnit) - 0.5f, (sprite.pivot.y / sprite.pixelsPerUnit) - 0.5f, 0);
        }

        return offset;
    }
}
