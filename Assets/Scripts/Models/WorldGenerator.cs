using UnityEngine;
using System.Collections;

public class WorldGenerator {

    public static int startAreaSize = 3;

    public static TileType asteroid_floor_type = TileType.Floor;
    public static float asteroid_noise_scale = 0.15f;
    public static float asteroid_noise_break = 0.75f;

    public static void Generate(World world){
        int width = world.Width;
        int height = world.Height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = Mathf.PerlinNoise(x / (width * asteroid_noise_scale), y / (height * asteroid_noise_scale));
                if(val >= asteroid_noise_break)
                {
                    Tile t = world.GetTileAt(x, y);
                    t.Type = asteroid_floor_type;

                    if (Random.value > 0.65f)
                    {
                        int stackSize = Random.Range(5, 25);
                        Inventory inv = Inventory.New("Steel Plate", 50, stackSize);
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
                t.Type = asteroid_floor_type;

                if(x == -startAreaSize || x == startAreaSize || y == -startAreaSize || y == startAreaSize){
                    if (x == 0 && y == startAreaSize)
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
