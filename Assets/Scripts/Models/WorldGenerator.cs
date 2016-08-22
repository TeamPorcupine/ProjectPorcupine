#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;

public class WorldGenerator
{
    public const TileType asteroidFloorType = TileType.Floor;

    public static int startAreaSize = 3;

    public static float asteroidNoiseScale = 0.2f;
    public static float asteroidNoiseThreshhold = 0.75f;
    public static float asteroidResourceChance = 0.85f;
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = Mathf.PerlinNoise((x + xOffset) / (width * asteroidNoiseScale), (y + yOffset) / (height * asteroidNoiseScale));
                if (val >= asteroidNoiseThreshhold)
                {
                    Tile t = world.GetTileAt(x, y);
                    t.Type = asteroidFloorType;

                    if (Random.value >= asteroidResourceChance)
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

        for (int x = -startAreaSize; x <= startAreaSize; x++)
        {
            for (int y = -startAreaSize; y <= startAreaSize; y++)
            {
                int xPos = (width / 2) + x;
                int yPos = (height / 2) + y;

                Tile t = world.GetTileAt(xPos, yPos);
                t.Type = TileType.Floor;

                if (x == -startAreaSize || x == startAreaSize || y == -startAreaSize || y == startAreaSize)
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

    public static void ReadXML()
    {
        // Setup XML Reader
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "WorldGenerator.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

        if (reader.ReadToDescendant("WorldGenerator"))
        {
            startAreaSize = int.Parse(reader.GetAttribute("startAreaSize"));

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

                                System.Collections.Generic.List<Inventory> res = new System.Collections.Generic.List<Inventory>();
                                System.Collections.Generic.List<int> resMin = new System.Collections.Generic.List<int>();
                                System.Collections.Generic.List<int> resMax = new System.Collections.Generic.List<int>();

                                while (res_reader.Read())
                                {
                                    if (res_reader.Name == "Resource")
                                    {
                                        res.Add(new Inventory(
                                                res_reader.GetAttribute("objectType"),
                                                int.Parse(res_reader.GetAttribute("maxStack")),
                                                (int)(float.Parse(res_reader.GetAttribute("chance")) * 100)
                                            ));

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
                    Logger.LogError("Error reading WorldGenerator/Asteroid" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                Logger.LogError("Did not find a 'Asteroid' element in the WorldGenerator definition file.");
            }
        }
        else
        {
            Logger.LogError("Did not find a 'WorldGenerator' element in the WorldGenerator definition file.");
        }
    }
}