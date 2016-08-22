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
    public const TileType AsteroidFloorType = TileType.Floor;

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
        ReadXML();
        Random.InitState(seed);
        int width = world.Width;
        int height = world.Height;

        int xOffset = Random.Range(0, 10000);
        int yOffset = Random.Range(0, 10000);

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (width / 2) - startAreaCenterX + x;
                int worldY = (height / 2) + startAreaCenterY - y;

                Tile tile = world.GetTileAt(worldX, worldY);
                tile.Type = (TileType)startAreaTiles[x, y];
            }
        }

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (width / 2) - startAreaCenterX + x;
                int worldY = (height / 2) + startAreaCenterY - y;

                Tile tile = world.GetTileAt(worldX, worldY);

                if (startAreaFurnitures[x, y] != null && startAreaFurnitures[x, y] != string.Empty)
                {
                    world.PlaceFurniture(startAreaFurnitures[x, y], tile, false);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = Mathf.PerlinNoise((x + xOffset) / (width * asteroidNoiseScale), (y + yOffset) / (height * asteroidNoiseScale));
                if (noiseValue >= asteroidNoiseThreshhold && !IsStartArea(x, y, world))
                {
                    Tile t = world.GetTileAt(x, y);
                    t.Type = AsteroidFloorType;

                    if (Random.value <= asteroidResourceChance && t.Furniture == null)
                    {
                        if (resources.Length > 0)
                        {
                            int currentchance = 0;
                            int randomchance = Random.Range(0, 100);

                            for (int i = 0; i < resources.Length; i++)
                            {
                                Inventory inv = resources[i];

                                int chance = inv.stackSize; // In stacksize the chance was cached
                                currentchance += chance;

                                if (randomchance <= currentchance)
                                {
                                    int stackSize = Random.Range(resourceMin[i], resourceMax[i]);

                                    if (stackSize > inv.maxStackSize)
                                    {
                                        stackSize = inv.maxStackSize;
                                    }

                                    world.inventoryManager.PlaceInventory(t, new Inventory(inv.objectType, inv.maxStackSize, stackSize));
                                    break;
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

        if (x >= boundX && x < boundX + startAreaWidth && y >= boundY && y < boundY - startAreaHeight)
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
                                                (int)(float.Parse(res_reader.GetAttribute("chance")) * 100)));

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
                    Debug.LogError("Error reading WorldGenerator/Asteroid" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                Debug.LogError("Did not find a 'Asteroid' element in the WorldGenerator definition file.");
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
                                    Debug.LogError("Error reading 'Tiles' array to short: " + splittedString.Length + " !");
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
                Debug.LogError("Did not find a 'StartArea' element in the WorldGenerator definition file.");
            }
        }
        else
        {
            Debug.LogError("Did not find a 'WorldGenerator' element in the WorldGenerator definition file.");
        }
    }
}