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

/// <summary>
/// This is just a nicer wrapper of chunks allowing you to say 
/// assign this furniture to some chunk,
/// or getting it to build chunks based on size of map.
/// </summary>
public class ChunkManager
{
    // Has to be an array since we care about order.
    public readonly Chunk[] Chunks;
    public readonly bool Initalized;

    private readonly int amountOfHorizontalChunks;
    private readonly int amountOfVerticalChunks;

    private readonly int sizeX;
    private readonly int sizeY;

    /// <summary>
    /// Generate a new set of chunks from the range.  Taking a percentage and incrementing.
    /// </summary>
    /// <param name="mapRange"> The map range, with .x representing the X Width, and .y representing the Y Height. </param>
    /// <param name="percentage"> The percentage to take should be divided by 100. </param>
    public ChunkManager(Vector2 mapRange, float percentage = .1f)
    {
        this.Initalized = true;

        // What we want to do is split up the map range into smaller segments
        // So we will just take 20%, nicely at least at a time
        // When we round up it should mean like one less chunk not a big deal.
        sizeX = Mathf.CeilToInt(mapRange.x * percentage);
        sizeY = Mathf.CeilToInt(mapRange.y * percentage);

        amountOfHorizontalChunks = Mathf.CeilToInt(1f / percentage);
        amountOfVerticalChunks = Mathf.CeilToInt(1f / percentage);

        List<Chunk> variableChunks = new List<Chunk>();

        for (int x = 0; x < amountOfHorizontalChunks; x++)
        {
            for (int y = 0; y < amountOfVerticalChunks; y++)
            {
                // This will check that its not a redundant chunk if it is then ignore that chunk
                // Technically we are fine with a chunk where its just a single square with min and max being that square.
                if (x * sizeX < mapRange.x && x * sizeY < mapRange.y)
                {
                    variableChunks.Add(new Chunk(new Vector2(x * sizeX, sizeX * (x + 1)), new Vector2(x * sizeY, sizeY * (x + 1)), (int)mapRange.x - 1, (int)mapRange.y - 1));
                }
            }
        }

        Chunks = variableChunks.ToArray();
    }

    public ChunkManager()
    {
        this.Initalized = false;
        Chunks = new Chunk[] { };
    }

    public void TickFixedFrequency(float deltaTime)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].TickFixedFrequency(deltaTime);
        }
    }

    public void TickEveryFrame(float deltaTime)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].EveryFrameUpdate(deltaTime);
        }
    }

    /// <summary>
    /// Actives/Disables chunks depending on visiblity.
    /// Essentially a helper function.
    /// </summary>
    public void CheckBounds(Bounds bounds)
    {
        CheckBounds(new Vector2(bounds.min.x, bounds.max.x), new Vector2(bounds.min.y, bounds.max.y));
    }

    /// <summary>
    /// Actives/Disables chunks depending on visiblity.
    /// The real function that does the checking based on the ranges.
    /// </summary>
    public void CheckBounds(Vector2 xRange, Vector2 yRange)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].BoundsIntersect(xRange, yRange, true);
        }
    }

    public int GetApproximateFurnitureChunkPosition(Furniture furniture)
    {
        Vector2 furniturePos = furniture.Tile.Vector3;

        // Do a little math :P
        // May fail so this is just an optimisation of sorts.
        // We are finding our 'x' coordinate then going down by the width to get to our 'y' coordinate
        // We minus 1 from y since technically the first position is 0 instead of 1 (weird thing).
        return Mathf.CeilToInt(furniturePos.x / sizeX) + (sizeX * (Mathf.CeilToInt(furniturePos.y / sizeY) - 1));
    }

    /// <summary>
    /// Changes the state of a furniture, by either adding or removing it.
    /// </summary>
    /// <param name="add"> If true then add, else remove. </param>
    /// <returns> True if it can find a chunk with its position else false. </returns>
    public bool ChangeFurniture(Furniture furniture, bool add)
    {
        int expectedChunkPosition = GetApproximateFurnitureChunkPosition(furniture);

        if (expectedChunkPosition < Chunks.Length)
        {
            Chunk chunk = Chunks[expectedChunkPosition];

            // Just our double checker
            if (chunk.PointInBounds(furniture.Tile.Vector3))
            {
                if (add)
                {
                    chunk.furnitures.Add(furniture);
                }
                else if (chunk.furnitures.Contains(furniture))
                {
                    chunk.furnitures.Remove(furniture);
                }

                return true;
            }
        }

        // Now we have to do the backup choice :(
        // We will do this either if we can't find a chunk position OR if that chunk position is wrong (we'll do a double check)
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunk chunk = Chunks[i];

            if (chunk.furnitures.Contains(furniture))
            {
                if (add)
                {
                    chunk.furnitures.Add(furniture);
                }
                else if (chunk.furnitures.Contains(furniture))
                {
                    chunk.furnitures.Remove(furniture);
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes furniture from chunk if it sits in the range.
    /// Helper function for ChangeFurniture.
    /// </summary>
    /// <returns> True if it could find it in the chunks else it'll return false. </returns>
    public bool RemoveFurniture(Furniture furniture)
    {
        return ChangeFurniture(furniture, false);
    }

    /// <summary>
    /// Adds furniture to one of the chunks if they sit in the range.
    /// Helper function for ChangeFurniture.
    /// </summary>
    /// <returns> True if it could find a chunk else it'll return false. </returns>
    public bool AddFurniture(Furniture furniture)
    {
        return ChangeFurniture(furniture, true);
    }
}
