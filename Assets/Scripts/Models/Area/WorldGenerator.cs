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

        int width = world.Width;
        int height = world.Height;
        int depth = world.Depth;

        ReadXML(world);
        world.ResizeWorld(width, height, depth);

        Random.InitState(seed);
        int offsetX = Random.Range(0, 10000);
        int offsetY = Random.Range(0, 10000);
        
        int sumOfAllWeightedChances = asteroidInfo.Resources.Select(x => x.WeightedChance).Sum();

        if (SceneController.GenerateAsteroids)
        {
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
                        
                        Tile tile = world.GetTileAt(x, y, z);
                        if (noiseValue >= asteroidInfo.NoiseThreshhold && tile.Room != null && tile.Room.ID == 0)
                        {
                            tile.SetTileType(asteroidFloorType);

                            world.FurnitureManager.PlaceFurniture("astro_wall", tile, false);

                            if (Random.value <= asteroidInfo.ResourceChance && tile.Furniture.Name == "Rock Wall")
                            {
                                if (asteroidInfo.Resources.Count > 0)
                                {
                                    int currentWeight = 0;
                                    int randomWeight = Random.Range(0, sumOfAllWeightedChances);

                                    for (int i = 0; i < asteroidInfo.Resources.Count; i++)
                                    {
                                        Resource inv = asteroidInfo.Resources[i];

                                        int weight = inv.WeightedChance; 
                                        currentWeight += weight;

                                        if (randomWeight <= currentWeight)
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
    }

    private static void ReadXmlWallet(XmlReader reader, World world)
    {
        XmlReader wallet = reader.ReadSubtree();

        while (wallet.Read())
        {
            if (wallet.Name == "Currency")
            {
                world.Wallet.AddCurrency(
                    wallet.GetAttribute("name"),
                    float.Parse(wallet.GetAttribute("startingBalance")));
            }
        }
    }

    private void ReadXML(World world)
    {
        // Setup XML Reader
        // Optimally, this would use GameController.Instance.GeneratorBasePath(), but that apparently may not exist at this point.
        // TODO: Investigate either a way to ensure GameController exists at this time or another place to reliably store the base path, that is accessible
        // both in _World and MainMenu scenes
        string filePath = System.IO.Path.Combine(System.IO.Path.Combine(Application.streamingAssetsPath, "WorldGen"), SceneController.GeneratorFile);
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
                    UnityDebugger.Debugger.LogError("WorldGenerator", "Error reading WorldGenerator/Asteroid" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                UnityDebugger.Debugger.LogError("WorldGenerator", "Did not find a 'Asteroid' element in the WorldGenerator definition file.");
            }

            if (reader.ReadToNextSibling("StartArea"))
            {
                try
                {
                    string startAreaFileName = reader.GetAttribute("file");
                    string startAreaFilePath = Path.Combine(Application.streamingAssetsPath, Path.Combine("WorldGen", startAreaFileName));
                    ReadStartArea(startAreaFilePath, world);
                }
                catch (System.Exception e)
                {
                    UnityDebugger.Debugger.LogError("WorldGenerator", "Error reading WorldGenerator/StartArea" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                UnityDebugger.Debugger.LogError("WorldGenerator", "Did not find a 'StartArea' element in the WorldGenerator definition file.");
            }

            if (reader.ReadToNextSibling("Wallet"))
            {
                try
                {
                    ReadXmlWallet(reader, world);
                }
                catch (System.Exception e)
                {
                    UnityDebugger.Debugger.LogError("WorldGenerator", "Error reading WorldGenerator/Wallet" + System.Environment.NewLine + "Exception: " + e.Message + System.Environment.NewLine + "StackTrace: " + e.StackTrace);
                }
            }
            else
            {
                UnityDebugger.Debugger.LogError("WorldGenerator", "Did not find a 'Wallet' element in the WorldGenerator definition file.");
            }
        }
        else
        {
            UnityDebugger.Debugger.LogError("WorldGenerator", "Did not find a 'WorldGenerator' element in the WorldGenerator definition file.");
        }
    }

    private void ReadXmlAsteroid(XmlReader reader)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(AsteroidInfo));
        asteroidInfo = (AsteroidInfo)serializer.Deserialize(reader);
    }

    private void ReadStartArea(string startAreaFilePath, World world)
    {
        world.ReadJson(startAreaFilePath);
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
