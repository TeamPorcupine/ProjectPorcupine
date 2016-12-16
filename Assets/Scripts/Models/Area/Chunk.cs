#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;

public struct Chunk
{
    /// <summary>
    /// Min x to Max x.
    /// </summary>
    public Vector2 xRange;

    /// <summary>
    /// Min y to Max y.
    /// </summary>
    public Vector2 yRange;

    /// <summary>
    /// Our list of furnitures.
    /// </summary>
    public HashSet<Furniture> furnitures;

    /// <summary>
    /// Are we visible.
    /// </summary>
    public bool visible;

    /// <summary>
    /// Standard constructor.  Presumes that mininum is ALWAYS 0.
    /// </summary>
    /// <param name="xRange"> The .x representing smallest point, .y representing largest point. </param>
    /// <param name="yRange"> The .x representing smallest point, .y representing largest point. </param>
    /// <param name="clampX"> The maximum x value (0 representing no maximum value). </param>
    /// <param name="clampY"> The maximum y value (0 representing no maximum value). </param>
    public Chunk(Vector2 xRange, Vector2 yRange, int clampX = 0, int clampY = 0)
    {
        this.xRange = xRange;
        this.yRange = yRange;

        if (clampX != 0)
        {
            this.xRange.x = Mathf.Clamp(this.xRange.x, 0, clampX);
            this.xRange.y = Mathf.Clamp(this.xRange.y, 0, clampX);
        }

        if (clampY != 0)
        {
            this.yRange.x = Mathf.Clamp(this.yRange.x, 0, clampY);
            this.yRange.y = Mathf.Clamp(this.yRange.y, 0, clampY);
        }

        this.furnitures = new HashSet<Furniture>();
        this.visible = false;
    }

    /// <summary>
    /// Will run if visible.
    /// </summary>
    public void EveryFrameUpdate(float deltaTime)
    {
        if (visible)
        {
            Furniture[] tempFunctions = new Furniture[furnitures.Count];
            furnitures.CopyTo(tempFunctions);

            foreach (Furniture furniture in tempFunctions)
            {
                furniture.EveryFrameUpdate(deltaTime);
            }
        }
    }

    /// <summary>
    /// Will run fixed freq. if visible else every frame update.
    /// </summary>
    public void TickFixedFrequency(float deltaTime)
    {
        Furniture[] tempFunctions = new Furniture[furnitures.Count];
        furnitures.CopyTo(tempFunctions);

        foreach (Furniture furniture in tempFunctions)
        {
            if (visible)
            {
                furniture.FixedFrequencyUpdate(deltaTime);
            }
            else
            {
                furniture.EveryFrameUpdate(deltaTime);
            }
        }
    }

    /// <summary>
    /// Bounds checker, that checks max and min to see if the two points intersect :D.
    /// </summary>
    /// <param name="xBoundsRange"> The .x representing smallest point, .y representing largest point. </param>
    /// <param name="yBoundsRange"> The .x representing smallest point, .y representing largest point. </param>
    /// <returns> If ranges intersect then it returns true else false. </returns>
    public bool BoundsIntersect(Vector2 xBoundsRange, Vector2 yBoundsRange, bool setVisible)
    {
        // X Checker
        if ((this.xRange.x >= xBoundsRange.x && this.xRange.x <= xBoundsRange.y)
            || (this.xRange.y >= xBoundsRange.x && this.xRange.y <= xBoundsRange.y))
        {
            // Y Checker
            if ((this.yRange.x >= yBoundsRange.x && this.yRange.x <= yBoundsRange.y)
                || (this.yRange.y >= yBoundsRange.x && this.yRange.y <= yBoundsRange.y))
            {
                if (setVisible)
                {
                    this.visible = true;
                }

                return true;
            }
        }

        if (setVisible)
        {
            this.visible = false;
        }

        return false;
    }

    /// <summary>
    /// Is the point in this bounds.
    /// </summary>
    /// <param name="point"> The point in question. </param>
    /// <returns> If the point is in these bounds it returns true else false. </returns>
    public bool PointInBounds(Vector2 point)
    {
        // X Checker
        if (point.x >= this.xRange.x && point.x <= this.xRange.y)
        {
            // Y Checker
            if (point.y >= this.yRange.x && point.y <= this.xRange.y)
            {
                return true;
            }
        }

        return false;
    }
}