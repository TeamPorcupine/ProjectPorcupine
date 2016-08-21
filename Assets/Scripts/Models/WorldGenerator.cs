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
using System.Xml;
using System.IO;

public class WorldGenerator
{

    public static int startAreaSize = 3;

    public const TileType asteroidFloorType = TileType.Floor;
    public static float asteroidNoiseScale = 0.2f;
    public static float asteroidNoiseThreshhold = 0.75f;
    public static float asteroidRessourceChance = 0.85f;
    public static int asteroidRessourceMin = 5;
    public static int asteroidRessourceMax = 15;
    public static Inventory[] ressources;

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

                    if (Random.value >= asteroidRessourceChance)
                    {
                        int stackSize = Random.Range(asteroidRessourceMin, asteroidRessourceMax);

                        if (ressources.Length > 0)
                        {
                            int currentchance = 0;
                            int randomchance = Random.Range(0, 100);
                            foreach (Inventory i in ressources)
                            {
                                int chance = i.stackSize; // In stacksize the chance was cached
                                currentchance += chance;

                                if (randomchance <= currentchance)
                                {
                                    if (stackSize > i.maxStackSize)
                                        stackSize = i.maxStackSize;

                                    world.inventoryManager.PlaceInventory(t, new Inventory(i.objectType, i.maxStackSize, stackSize));
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
                int xPos = width / 2 + x;
                int yPos = height / 2 + y;

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
                            case "RessourceChance":
                                reader.Read();
                                asteroidRessourceChance = asteroid.ReadContentAsFloat();
                                break;
                            case "RessourceMin":
                                reader.Read();
                                asteroidRessourceMin = asteroid.ReadContentAsInt();
                                break;
                            case "RessourceMax":
                                reader.Read();
                                asteroidRessourceMax = asteroid.ReadContentAsInt();
                                break;
                            case "Ressources":
                                XmlReader res_reader = reader.ReadSubtree();

                                System.Collections.Generic.List<Inventory> res = new System.Collections.Generic.List<Inventory>();

                                while (res_reader.Read())
                                {
                                    if (res_reader.Name == "Ressource")
                                    {
                                        res.Add(new Inventory(
                                                res_reader.GetAttribute("objectType"),
                                                int.Parse(res_reader.GetAttribute("maxStack")),
                                                (int)(float.Parse(res_reader.GetAttribute("chance")) * 100)
                                            ));
                                    }
                                }

                                ressources = res.ToArray();

                                break;
                        }
                    }
                }
                catch(System.Exception e)
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