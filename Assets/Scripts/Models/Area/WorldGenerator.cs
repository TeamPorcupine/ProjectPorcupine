#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class WorldGenerator
{
    private static WorldGenerator instance;
    
    private TileType asteroidFloorType = null;

    private int startAreaWidth = 0;
    private int startAreaHeight = 0;
    private int startAreaCenterX = 0;
    private int startAreaCenterY = 0;
    private int[,] startAreaTiles = new int[0, 0];
    private string[,] startAreaFurnitures = new string[0, 0];

    private AsteroidInfo asteroidInfo;

    private WorldGenerator()
    {
    }

    public static WorldGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WorldGenerator();
            }

            return instance;
        }
    }

    public void Generate(World world, int seed)
    {
        asteroidFloorType = TileType.Empty; 

        ReadXML();
        Random.InitState(seed);
        int width = world.Width;
        int height = world.Height;
        int depth = world.Depth;
        int offsetX = Random.Range(0, 10000);
        int offsetY = Random.Range(0, 10000);

        int minEdgeDistance = 5;
        
        int sumOfAllWeightedChances = asteroidInfo.Resources.Select(x => x.WeightedChance).Sum();

        for (int x = 0; x < startAreaWidth; x++)
        {
            for (int y = 0; y < startAreaHeight; y++)
            {
                int worldX = (width / 2) - startAreaCenterX + x;
                int worldY = (height / 2) + startAreaCenterY - y;

                Tile tile = world.GetTileAt(worldX, worldY, 0);
                tile.SetTileType(PrototypeManager.TileType[startAreaTiles[x, y]]);
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
                    float noiseValue = Mathf.PerlinNoise(
                        (x + offsetX) / (width * asteroidInfo.NoiseScale * scaleZ),
                        (y + offsetY) / (height * asteroidInfo.NoiseScale * scaleZ));
                    if (noiseValue >= asteroidInfo.NoiseThreshhold && !IsStartArea(x, y, world))
                    {
                        Tile tile = world.GetTileAt(x, y, z);

                        if (tile.X < minEdgeDistance || tile.Y < minEdgeDistance ||
                              World.Current.Width - tile.X <= minEdgeDistance ||
                              World.Current.Height - tile.Y <= minEdgeDistance)
                        {
                            continue;
                        }

                        tile.SetTileType(asteroidFloorType);

                        world.FurnitureManager.PlaceFurniture("astro_wall", tile, false);

                        if (Random.value <= asteroidInfo.ResourceChance && tile.Furniture.Name == "Rock Wall")
                        {
                            if (asteroidInfo.Resources.Count > 0)
                            {
                                int currentweight = 0;
                                int randomweight = Random.Range(0, sumOfAllWeightedChances);

                                for (int i = 0; i < asteroidInfo.Resources.Count; i++)
                                {
                                    Resource inv = asteroidInfo.Resources[i];

                                    int weight = inv.WeightedChance; 
                                    currentweight += weight;

                                    if (randomweight <= currentweight)
                                    {
                                        tile.Furniture.Deconstruct();

                                        Furniture oreWall = PrototypeManager.Furniture.Get("astro_wall").Clone();
                                        oreWall.Parameters["ore_type"].SetValue(inv.Type);
                                        oreWall.Parameters["source_type"].SetValue(inv.Source);
                                        
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

    private bool IsStartArea(int x, int y, World world)
    {
        int boundX = (world.Width / 2) - startAreaCenterX;
        int boundY = (world.Height / 2) + startAreaCenterY;

        if (x >= boundX && x < (boundX + startAreaWidth) && y >= (boundY - startAreaHeight) && y < boundY)
        {
            return true;
        }

        return false;
    }

    private void ReadXML()
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

    private void ReadXmlAsteroid(XmlReader reader)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(AsteroidInfo));
        asteroidInfo = (AsteroidInfo)serializer.Deserialize(reader);
    }

    private void ReadXmlStartArea(XmlReader reader)
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
    
    [System.Serializable]
    [XmlRoot("Asteroid")]
    public class AsteroidInfo
    {
        [XmlElement("NoiseScale")]
        public float NoiseScale { get; set; }

        [XmlElement("NoiseThreshhold")]
        public float NoiseThreshhold { get; set; }

        [XmlElement("ResourceChance")]
        public float ResourceChance { get; set; }

        public List<Resource> Resources { get; set; }
    }

    [System.Serializable]
    public class Resource
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("min")]
        public int Min { get; set; }

        [XmlAttribute("max")]
        public int Max { get; set; }

        [XmlAttribute("weightedChance")]
        public int WeightedChance { get; set; }
    }
}
