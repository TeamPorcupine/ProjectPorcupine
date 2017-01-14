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
using System.Xml.Serialization;
using NUnit.Framework;

public class WorldGeneratorTest
{
    [Test]
    public void TestAsteroidSerialization()
    {
        WorldGenerator.AsteroidInfo asteroidInfo = new WorldGenerator.AsteroidInfo()
        {
            AsteroidSize = 10,
            AsteroidDensity = 0.25f,
            ResourceChance = 0.15f,
            Resources = new List<WorldGenerator.Resource>()
                {
                    new WorldGenerator.Resource()
                    {
                        Type = "Raw Iron",
                        Source = "iron_ore",
                        Min = 5,
                        Max = 15,
                        WeightedChance = 50
                    }
                }
        };

        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(WorldGenerator.AsteroidInfo));

        serializer.Serialize(writer, asteroidInfo);

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("Asteroid.xml", writer.ToString());

        // deserialize
        WorldGenerator.AsteroidInfo deserializedAsteroidInfo = (WorldGenerator.AsteroidInfo)serializer.Deserialize(sr);

        Assert.NotNull(deserializedAsteroidInfo);
    }
}