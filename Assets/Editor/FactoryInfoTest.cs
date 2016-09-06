using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

public class FactoryInfoTest
{
    [Test]
    public void TestSerialization()
    {
        FactoryInfo fi = new FactoryInfo();        
        fi.PossibleProductions = new System.Collections.Generic.List<FactoryInfo.ProductionChain>();
        var chain1 = new FactoryInfo.ProductionChain()
            { Name="Iron smelting", ProcessingTime = 4.0f };
        chain1.Input = new System.Collections.Generic.List<FactoryInfo.Item>();
        chain1.Input.Add(new FactoryInfo.Item()
            { ObjectType = "Raw Iron", Amount = 3, SlotPosX = 0, SlotPosY= 0 });
        chain1.Output = new System.Collections.Generic.List<FactoryInfo.Item>();
        chain1.Output.Add(new FactoryInfo.Item()
            { ObjectType = "Steel Plate", Amount = 3, SlotPosX = 2, SlotPosY = 0 });

        fi.PossibleProductions.Add(chain1);

        var chain2 = new FactoryInfo.ProductionChain()
            { Name = "Copper smelting", ProcessingTime = 3.0f };
        chain2.Input = new System.Collections.Generic.List<FactoryInfo.Item>();
        chain2.Input.Add(new FactoryInfo.Item()
            { ObjectType = "Raw Copper", Amount = 3, SlotPosX = 0, SlotPosY = 0 });
        chain2.Output = new System.Collections.Generic.List<FactoryInfo.Item>();
        chain2.Output.Add(new FactoryInfo.Item()
            { ObjectType = "Copper Wire", Amount = 6, SlotPosX = 2, SlotPosY = 0 });

        fi.PossibleProductions.Add(chain2);

        FileStream fs = new FileStream("FactoryInfo.xml", FileMode.Create);
        TextWriter writer = new StreamWriter(fs, new UTF8Encoding());

        XmlSerializer serializer = new XmlSerializer(typeof(FactoryInfo));
        serializer.Serialize(writer, fi);
        writer.Close();

        FileStream fr = new FileStream("FactoryInfo.xml", FileMode.Open);
        var dfi = (FactoryInfo)serializer.Deserialize(fr);

        Assert.NotNull(dfi);
        Assert.AreEqual("Raw Iron", dfi.PossibleProductions[0].Input[0].ObjectType);
    }
}
