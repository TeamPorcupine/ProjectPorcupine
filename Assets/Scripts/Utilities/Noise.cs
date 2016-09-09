using UnityEngine;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,,] GeneratePerlinNoiseMap(int mapWidth, int mapHeight, int mapDepth, int seed, float noiseScale, int octaves, float persistance, float lacunarity)
    {
        return GeneratePerlinNoiseMap(mapWidth, mapHeight, mapDepth, seed, noiseScale, octaves, persistance, lacunarity, Vector2.zero, NormalizeMode.Local);
    }

    public static float[,,] GeneratePerlinNoiseMap(int mapWidth, int mapHeight, int mapDepth, int seed, float noiseScale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,,] noiseMap = new float[mapWidth, mapHeight, mapDepth];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (noiseScale <= 0)
        {
            noiseScale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int z = 0; z < mapDepth; z++)
        {        
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseScale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseScale * frequency;                        

                        float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * 2) - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y, z] = noiseHeight;
                }
            }
        }

        for (int z = 0; z < mapDepth; z++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (normalizeMode == NormalizeMode.Local)
                    {
                        noiseMap[x, y, z] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y, z]);
                    }
                    else
                    {
                        float normalizedHeight = (noiseMap[x, y, z] + 1) / (maxPossibleHeight / 0.9f);
                        noiseMap[x, y, z] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }
        }

        return noiseMap;
    }
}
