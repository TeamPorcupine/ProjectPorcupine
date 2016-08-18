using UnityEngine;
using System.Collections;

public class WorldGenerator {

    public static TileType asteroid_floor_type = TileType.Floor;
    public static float asteroid_noise_scale = 0.15f;
    public static float asteroid_noise_break = 0.75f;

    public static void Generate(World world){
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                float val = Mathf.PerlinNoise(x / (world.Width * asteroid_noise_scale), y / (world.Height * asteroid_noise_scale));
                if(val >= asteroid_noise_break)
                {
                    world.GetTileAt(x, y).Type = asteroid_floor_type;
                }
            }
        }
    }
}
