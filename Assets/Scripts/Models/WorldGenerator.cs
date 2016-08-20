#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;

public class WorldGenerator {

    public const int startAreaSize = 3;

    public const TileType asteroidFloorType = TileType.Floor;
    public const float asteroidNoiseScale = 0.2f;
    public const float asteroidNoiseThreshhold = 0.75f;
    public const float asteroidRessourceChance = 0.85f;
    public const int asteroidRessourceMin = 5;
    public const int asteroidRessourceMax = 15;

    public static void Generate(World world, int seed){
        Random.InitState(seed);
        int width = world.Width;
        int height = world.Height;

        int xOffset = Random.Range(0, 10000);
        int yOffset = Random.Range(0, 10000);
      
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = Mathf.PerlinNoise((x + xOffset) / (width * asteroidNoiseScale), (y + yOffset) / (height * asteroidNoiseScale));
                if(val >= asteroidNoiseThreshhold)
                {
                    Tile t = world.GetTileAt(x, y);
                    t.Type = asteroidFloorType;

                    if (Random.value >= asteroidRessourceChance)
                    {
                        int stackSize = Random.Range(asteroidRessourceMin, asteroidRessourceMax);
                        Inventory inv = Inventory.New("Raw Iron", 50, stackSize);
                        world.inventoryManager.PlaceInventory(t, inv);
                    }
                }
            }
        }
            
        for (int x = -startAreaSize; x <= startAreaSize; x++)
        {
            for (int y = -startAreaSize; y <= startAreaSize; y++)
            {
                int xPos = width / 2 + x;
                int yPos = height / 2 + y;

                Tile t = world.GetTileAt(xPos, yPos);
                t.Type = TileType.Floor;

                if(x == -startAreaSize || x == startAreaSize || y == -startAreaSize || y == startAreaSize)
                {
                    if (x == 0 && y == -startAreaSize)
                    {
                        world.PlaceFurniture("Door", t, true);
                        continue;
                    }

                    world.PlaceFurniture("furn_SteelWall", t, true);
                }
            }
        }
    }
}
