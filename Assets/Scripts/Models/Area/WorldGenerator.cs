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
    public static float asteroidResourceChance = 0.15f;
    public static Inventory[] resources;
    public static int[] resourceMin;
    public static int[] resourceMax;

    public static void Generate(World world, int seed)
    {
        AsteroidFloorType = PrototypeManager.TileType.Get("Floor");

        ReadXML();
        Random.InitState(seed);
        int width = world.Width;
        int height = world.Height;
        int depth = world.Depth;
        int offsetX = Random.Range(0, 10000);
        int offsetY = Random.Range(0, 10000);

        int minEdgeDistance = 5;

        int sumOfAllWeightedChances = 0;
        foreach (Inventory resource in resources)
        {
            sumOfAllWeightedChances += resource.StackSize;
        }

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (width / 2) - startAreaCenterX + x;
                int worldY = (height / 2) + startAreaCenterY - y;

                Tile tile = world.GetTileAt(worldX, worldY, 0);
                tile.Type = PrototypeManager.TileType[startAreaTiles[x, y]];
            }
        }

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (width / 2) - startAreaCenterX + x;
                int worldY = (height / 2) + startAreaCenterY - y;

                Tile tile = world.GetTileAt(worldX, worldY, 0);

                if (startAreaFurnitures[x, y] != null && startAreaFurnitures[x, y] != string.Empty)
                {
                    world.FurnitureManager.PlaceFurniture(startAreaFurnitures[x, y], tile, true);
                }
            }
        }

        for (int z = 0; z < depth; z++)
        {
            float scaleZ = Mathf.Lerp(1f, .5f, Mathf.Abs((depth / 2f) - z) / depth);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float noiseValue = Mathf.PerlinNoise((x + offsetX) / (width * asteroidNoiseScale * scaleZ), (y + offsetY) / (height * asteroidNoiseScale * scaleZ));
                    if (noiseValue >= asteroidNoiseThreshhold && !IsStartArea(x, y, world))
                    {
                        Tile tile = world.GetTileAt(x, y, z);

                        if (tile.X < minEdgeDistance || tile.Y < minEdgeDistance ||
                              World.Current.Width - tile.X <= minEdgeDistance ||
                              World.Current.Height - tile.Y <= minEdgeDistance)
                        {
                            continue;
                        }

                        tile.Type = AsteroidFloorType;

                        world.FurnitureManager.PlaceFurniture("astro_wall", tile, false);

                        if (Random.value <= asteroidResourceChance && tile.Furniture.Name == "Rock Wall")
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
                                        tile.Furniture.Deconstruct();

                                        Furniture oreWall = PrototypeManager.Furniture.Get("astro_wall").Clone();
                                        oreWall.Parameters["ore_type"].SetValue(inv.Type.ToString());

                                        switch (inv.Type)
                                        {
                                            case "Raw Iron":
                                                oreWall.Tint = new Color32(72, 209, 204, 255);
                                                break;
                                            case "Uranium":
                                                oreWall.Tint = new Color32(48, 128, 20, 255);
                                                break;
                                            case "Ice":
                                                oreWall.Tint = new Color32(202, 225, 255, 255);
                                                break;
                                            default:
                                                oreWall.Tint = Color.gray;
                                                break;
                                        }

                                        world.FurnitureManager.PlaceFurniture(oreWall, tile, false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
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
                    ReadXmlAsteroid(reader);
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
                    ReadXmlStartArea(reader);
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

            if (reader.ReadToNextSibling("Wallet"))
            {
                try
                {
                    ReadXmlWallet(reader);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error reading WorldGenerator/Wallet" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                Debug.ULogErrorChannel("WorldGenerator", "Did not find a 'Wallet' element in the WorldGenerator definition file.");
            }
        }
        else
        {
            Debug.ULogErrorChannel("WorldGenerator", "Did not find a 'WorldGenerator' element in the WorldGenerator definition file.");
        }
    }

    private static void ReadXmlAsteroid(XmlReader reader)
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
                                res_reader.GetAttribute("type"),
                                Mathf.CeilToInt(float.Parse(res_reader.GetAttribute("weightedChance"))),
                                int.Parse(res_reader.GetAttribute("maxStack"))));

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

    private static void ReadXmlStartArea(XmlReader reader)
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
                    XmlReader furnReader = reader.ReadSubtree();

                    startAreaFurnitures = new string[startAreaWidth, startAreaHeight];

                    while (furnReader.Read())
                    {
                        if (furnReader.Name == "Furniture")
                        {
                            int x = int.Parse(furnReader.GetAttribute("x"));
                            int y = int.Parse(furnReader.GetAttribute("y"));
                            startAreaFurnitures[x, y] = furnReader.GetAttribute("name");
                        }
                    }

                    break;
            }
        }
    }

    private static void ReadXmlWallet(XmlReader reader)
    {
        XmlReader wallet = reader.ReadSubtree();

        while (wallet.Read())
        {
            if (wallet.Name == "Currency")
            {
                World.Current.Wallet.AddCurrency(
                    wallet.GetAttribute("name"),
                    float.Parse(wallet.GetAttribute("startingBalance")));
            }
        }
    }
}
