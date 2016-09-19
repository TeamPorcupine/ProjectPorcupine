#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public static class ImageUtils
{
    public static Vector3 SpritePivotOffset(Sprite sprite)
    {
        return new Vector3((sprite.pivot.x / sprite.pixelsPerUnit) - 0.5f, (sprite.pivot.y / sprite.pixelsPerUnit) - 0.5f, 0);
    }

    // Used to adjust postion of the sprite when the spirte rotate to fit to the grid.
    public static Vector3 SpriteRotationOffset(Sprite sprite, float rotation)
    {
        Vector3 rotationOffset = new Vector3(0, 0, 0);
        if (sprite.rect.width != sprite.rect.height)
        {
            if (rotation == 90 || rotation == -90 || rotation == -270 || rotation == 270)
            {
                rotationOffset = new Vector3(0.5f, 0.5f, 0);
            }
        }

        return rotationOffset;
    }
}
