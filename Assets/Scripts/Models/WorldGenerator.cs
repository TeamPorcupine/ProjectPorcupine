#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using ProjectPorcupine.Noise;

public class WorldGenerator
{
    public static TileType AsteroidFloorType = null;

    public static int startAreaWidth = 0;
    public static int startAreaHeight = 0;
    public static int startAreaCenterX = 0;
    public static int startAreaCenterY = 0;
    public static int[,] startAreaTiles = new int[0, 0];
    public static string[,] startAreaFurnitures = new string[0, 0];

    public static float asteroidNoiseScale = 0.2f;
    public static float asteroidNoiseThreshhold = 0.75f;
    public static int asteroidNoiseOctaves = 8;
    public static float asteroidNoisePersistance = 8;
    public static float asteroidNoiseLacunarity = 0.5f;
    public static float asteroidResourceChance = 0.15f;
    public static Inventory[] resources;
    public static int[] resourceMin;
    public static int[] resourceMax;

    public static float[,,] noiseMap;
    private static bool useOtherNoiseSource = false;

    public static void Generate(int seed, bool useOtherWorldGen)
    {
        AsteroidFloorType = TileType.GetTileType("Floor");

        ReadXML();
        Random.InitState(seed);
        int width = World.Current.Width;
        int height = World.Current.Height;
        int depth = World.Current.Depth;
        useOtherNoiseSource = useOtherWorldGen;

        noiseMap = Noise.GetPerlinNoiseMap(width, height, depth, seed, asteroidNoiseScale, asteroidNoiseOctaves, asteroidNoisePersistance, asteroidNoiseLacunarity);       

        StartAreaCreation(width, height, depth);
        MapGeneration(width, height, depth);
    }

    public static bool IsStartArea(int x, int y, World world)
    {
        int boundX = (world.Width / 2) - startAreaCenterX;
        int boundY = (world.Height / 2) + startAreaCenterY;

        if (x >= boundX && x < (boundX + startAreaWidth) && y >= (boundY - startAreaHeight) && y < boundY)
        {
            return true;
        }

        return false;
    }

    public static void ReadXML()
    {
        // Setup XML Reader
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "WorldGenerator.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

        if (reader.ReadToDescendant("WorldGenerator"))
        {
            if (reader.ReadToDescendant("Asteroid"))
            {
                try
                {
                    XmlReader asteroid = reader.ReadSubtree();

                    while (asteroid.Read())
                    {
                        switch (asteroid.Name)
                        {
                            case "NoiseScale":
                                reader.Read();
                                asteroidNoiseScale = asteroid.ReadContentAsFloat();
                                break;
                            case "NoiseThreshhold":
                                reader.Read();
                                asteroidNoiseThreshhold = asteroid.ReadContentAsFloat();
                                break;
                            case "NoiseOctaves":
                                reader.Read();
                                asteroidNoiseOctaves = asteroid.ReadContentAsInt();
                                break;
                            case "NoisePersistance":
                                reader.Read();
                                asteroidNoisePersistance = asteroid.ReadContentAsFloat();
                                break;
                            case "NoiseLacunarity":
                                reader.Read();
                                asteroidNoiseLacunarity = asteroid.ReadContentAsFloat();
                                break;                            
                            case "ResourceChance":
                                reader.Read();
                                asteroidResourceChance = asteroid.ReadContentAsFloat();
                                break;
                            case "Resources":
                                XmlReader res_reader = reader.ReadSubtree();

                                List<Inventory> res = new List<Inventory>();
                                List<int> resMin = new List<int>();
                                List<int> resMax = new List<int>();

                                while (res_reader.Read())
                                {
                                    if (res_reader.Name == "Resource")
                                    {
                                        res.Add(new Inventory(
                                                res_reader.GetAttribute("objectType"),
                                                int.Parse(res_reader.GetAttribute("maxStack")),
                                                Mathf.CeilToInt(float.Parse(res_reader.GetAttribute("weightedChance")))));

                                        resMin.Add(int.Parse(res_reader.GetAttribute("min")));
                                        resMax.Add(int.Parse(res_reader.GetAttribute("max")));
                                    }
                                }

                                resources = res.ToArray();
                                resourceMin = resMin.ToArray();
                                resourceMax = resMax.ToArray();

                                break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    // Leaving this in because UberLogger doesn't handle multiline messages  
                    Debug.LogError("Error reading WorldGenerator/Asteroid" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                Debug.ULogErrorChannel("WorldGenerator", "Did not find a 'Asteroid' element in the WorldGenerator definition file.");
            }

            if (reader.ReadToNextSibling("StartArea"))
            {
                try
                {
                    startAreaWidth = int.Parse(reader.GetAttribute("width"));
                    startAreaHeight = int.Parse(reader.GetAttribute("height"));
                    startAreaCenterX = int.Parse(reader.GetAttribute("centerX"));
                    startAreaCenterY = int.Parse(reader.GetAttribute("centerY"));

                    startAreaTiles = new int[startAreaWidth, startAreaHeight];

                    XmlReader startArea = reader.ReadSubtree();

                    while (startArea.Read())
                    {
                        switch (startArea.Name)
                        {
                            case "Tiles":
                                reader.Read();
                                string tilesString = startArea.ReadContentAsString();
                                string[] splittedString = tilesString.Split(","[0]);

                                if (splittedString.Length < startAreaWidth * startAreaHeight)
                                {
                                    Debug.ULogErrorChannel("WorldGenerator", "Error reading 'Tiles' array to short: " + splittedString.Length + " !");
                                    break;
                                }

                                for (int x = 0; x < startAreaWidth; x++)
                                {
                                    for (int y = 0; y < startAreaHeight; y++)
                                    {
                                        startAreaTiles[x, y] = int.Parse(splittedString[x + (y * startAreaWidth)]);
                                    }
                                }

                                break; 
                            case "Furnitures":
                                XmlReader furn_reader = reader.ReadSubtree();

                                startAreaFurnitures = new string[startAreaWidth, startAreaHeight];

                                while (furn_reader.Read())
                                {
                                    if (furn_reader.Name == "Furniture")
                                    {
                                        int x = int.Parse(furn_reader.GetAttribute("x"));
                                        int y = int.Parse(furn_reader.GetAttribute("y"));
                                        startAreaFurnitures[x, y] = furn_reader.GetAttribute("name");
                                    }
                                }

                                break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error reading WorldGenerator/StartArea" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                Debug.ULogErrorChannel("WorldGenerator", "Did not find a 'StartArea' element in the WorldGenerator definition file.");
            }
        }
        else
        {
            Debug.ULogErrorChannel("WorldGenerator", "Did not find a 'WorldGenerator' element in the WorldGenerator definition file.");
        }
    }

    private static void MapGeneration(int worldWidth, int worldHeight, int worldDepth)
    {
        int offsetX = Random.Range(0, 10000);
        int offsetY = Random.Range(0, 10000);

        int minEdgeDistance = 5;
        float noiseValue;

        for (int z = 0; z < worldDepth; z++)
        {
            float scaleZ = Mathf.Lerp(1f, .5f, Mathf.Abs((worldDepth / 2f) - z) / worldDepth);
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (useOtherNoiseSource)
                    {
                        noiseValue = noiseMap[x, y, z];
                    }
                    else
                    {
                        noiseValue = Mathf.PerlinNoise((x + offsetX) / (worldWidth * asteroidNoiseScale * scaleZ), (y + offsetY) / (worldHeight * asteroidNoiseScale * scaleZ));
                    }
                    
                    if (noiseValue >= asteroidNoiseThreshhold && !IsStartArea(x, y, World.Current))
                    {
                        Tile tile = World.Current.GetTileAt(x, y, z);

                        if (tile.X < minEdgeDistance || tile.Y < minEdgeDistance ||
                              World.Current.Width - tile.X <= minEdgeDistance ||
                              World.Current.Height - tile.Y <= minEdgeDistance)
                        {
                            continue;
                        }

                        tile.Type = AsteroidFloorType;

                        SpawnResource(tile);
                    }
                }
            }
        }
    }
    
    private static void SpawnResource(Tile tile)
    {
        int sumOfAllWeightedChances = 0;
        foreach (Inventory resource in resources)
        {
            sumOfAllWeightedChances += resource.StackSize;
        }

        if (Random.value <= asteroidResourceChance && tile.Furniture == null)
        {
            if (resources.Length > 0)
            {
                int currentweight = 0;
                int randomweight = Random.Range(0, sumOfAllWeightedChances);

                for (int i = 0; i < resources.Length; i++)
                {
                    Inventory inv = resources[i];

                    int weight = inv.StackSize; // In stacksize the weight was cached
                    currentweight += weight;

                    if (randomweight <= currentweight)
                    {
                        if (inv.ObjectType == "Raw Iron" || inv.ObjectType == "Uranium")
                        {
                            Furniture mine = PrototypeManager.Furniture.Get("mine").Clone();
                            mine.Parameters["ore_type"].SetValue(inv.ObjectType.ToString());
                            World.Current.PlaceFurniture(mine, tile, false);
                            break;
                        }

                        int stackSize = Random.Range(resourceMin[i], resourceMax[i]);

                        if (stackSize > inv.MaxStackSize)
                        {
                            stackSize = inv.MaxStackSize;
                        }

                        World.Current.inventoryManager.PlaceInventory(tile, new Inventory(inv.ObjectType, inv.MaxStackSize, stackSize));
                        break;
                    }
                }
            }
        }
    }

    private static void StartAreaCreation(int worldWidth, int worldHeight, int worldDepth)
    {
        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (worldWidth / 2) - startAreaCenterX + x;
                int worldY = (worldHeight / 2) + startAreaCenterY - y;

                Tile tile = World.Current.GetTileAt(worldX, worldY, 0);
                tile.Type = TileType.LoadedTileTypes[startAreaTiles[x, y]];
            }
        }

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (worldWidth / 2) - startAreaCenterX + x;
                int worldY = (worldHeight / 2) + startAreaCenterY - y;

                Tile tile = World.Current.GetTileAt(worldX, worldY, 0);

                if (startAreaFurnitures[x, y] != null && startAreaFurnitures[x, y] != string.Empty)
                {
                    World.Current.PlaceFurniture(startAreaFurnitures[x, y], tile, true);
                }
            }
        }
    }
}
