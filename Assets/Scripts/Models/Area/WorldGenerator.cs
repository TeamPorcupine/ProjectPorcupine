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

    private int sumOfAllWeightedChances;

    private HashSet<Tile> currentAsteroid;

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

        sumOfAllWeightedChances = asteroidInfo.Resources.Select(x => x.WeightedChance).Sum();

        if (SceneController.GenerateAsteroids)
        {
            // To clarify this is the formula for an ellipsoid, taking the lesser of asteroid radius and world size in that dimension
            float averageAsteroidVolume = (4 / 3) * Mathf.PI * Mathf.Min(asteroidInfo.AsteroidSize, world.Height) * Mathf.Min(asteroidInfo.AsteroidSize, world.Width) * Mathf.Min(asteroidInfo.AsteroidSize, world.Depth);
            int numAsteroids = (int)((world.Height * world.Width * world.Depth) / averageAsteroidVolume * asteroidInfo.AsteroidDensity);
            numAsteroids = (int)(numAsteroids * Random.Range(.6f, 1.4f));

            List<Vector3> asteroidSeeds = GeneratePoints(numAsteroids, world);
            for (int asteroid = 0; asteroid < asteroidSeeds.Count; asteroid++)
            {
                GrowAsteroid(asteroidSeeds[asteroid], world);
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

    private List<Vector3> GeneratePoints(int numPoints, World world)
    {
        List<Vector3> finalPoints = new List<Vector3>();
        List<Vector3> workingPoints = new List<Vector3>();

        for (int i = 0; i < numPoints * 4; i++)
        {
            workingPoints.Add(new Vector3(Random.Range(0, world.Width), Random.Range(0, world.Height), Random.Range(0, world.Depth)));
        }

        for (int i = 0; i < numPoints; i++)
        {
            if (finalPoints.Count == 0)
            {
                int pointToAdd = Random.Range(0, workingPoints.Count);
                finalPoints.Add(workingPoints[pointToAdd]);
                workingPoints.RemoveAt(pointToAdd);
            }
            else
            {
                float currentFarthestTotalDistance = 0f;
                int currentFarthestPointIndex = 0;
                for (int j = 0; j < workingPoints.Count; j++)
                {
                    float closestDistance = Mathf.Infinity;
                    for (int k = 0; i < finalPoints.Count; k++)
                    {
                        float thisDistance = Vector3.Distance(finalPoints[k], workingPoints[j]);
                        if (thisDistance < closestDistance)
                        {
                            closestDistance = thisDistance;
                        }
                    }

                    if (closestDistance > currentFarthestTotalDistance)
                    {
                        currentFarthestTotalDistance = closestDistance;
                        currentFarthestPointIndex = j;
                    }
                }

                finalPoints.Add(workingPoints[currentFarthestPointIndex]);
                workingPoints.RemoveAt(currentFarthestPointIndex);
            }
        }

        return finalPoints;
    }

    private void GrowAsteroid(Vector3 location, World world)
    {
        currentAsteroid = new HashSet<Tile>();
        GrowAsteroid(world.GetTileAt((int)location.x, (int)location.y, (int)location.z));
        foreach (Tile tile in currentAsteroid)
        {
            PlaceAsteroidChunk(tile, world);
        }
    }

    private void GrowAsteroid(Tile tile, float depth = 0)
    {
        // This generates a range of sizes around the set size, a larger degre of variance leads to less consistent looking asteroids
        int minSize = asteroidInfo.AsteroidSize - 5;
        int maxSize = asteroidInfo.AsteroidSize + 5;
        if (tile != null)
        {
            currentAsteroid.Add(tile);
            foreach (Tile neighbor in tile.GetNeighbours(false, true))
            {
                if (depth < maxSize && !currentAsteroid.Contains(neighbor) && neighbor.Furniture == null && Random.value < (float)minSize / depth)
                {
                    GrowAsteroid(neighbor, depth + 1);
                }
            }
        }
    }

    private void PlaceAsteroidChunk(Tile tile, World world)
    {
        if (tile.Type != TileType.Empty)
        {
            return;
        }

        const int NeededNeighbors = 3;

        if (tile.GetNeighbours(true, true).Count(currentAsteroid.Contains) >= NeededNeighbors)
        {
            tile.SetTileType(asteroidFloorType);

            world.FurnitureManager.PlaceFurniture("astro_wall", tile, false);

            if (Random.value <= asteroidInfo.ResourceChance && tile.Furniture.Type == "astro_wall")
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
                            tile.Furniture.Parameters["ore_type"].SetValue(inv.Type);
                            tile.Furniture.Parameters["source_type"].SetValue(inv.Source);

                            break;
                        }
                    }
                }
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
        [XmlElement("AsteroidSize")]
        public int AsteroidSize { get; set; }

        [XmlElement("AsteroidDensity")]
        public float AsteroidDensity { get; set; }

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