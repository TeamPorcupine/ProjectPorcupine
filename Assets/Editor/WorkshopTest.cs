#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

public class WorkshopTest
{
    [Test]
    public void TestSerialization()
    {
        FurnitureWorkshop fi = new FurnitureWorkshop();        
        fi.PossibleProductions = new System.Collections.Generic.List<FurnitureWorkshop.ProductionChain>();
        var chain1 = new FurnitureWorkshop.ProductionChain()
            {
            Name = "Iron smelting", ProcessingTime = 4.0f
            };
        chain1.Input = new System.Collections.Generic.List<FurnitureWorkshop.Item>();
        chain1.Input.Add(new FurnitureWorkshop.Item()
            {
            ObjectType = "Raw Iron", Amount = 3, SlotPosX = 0, SlotPosY = 0
            });
        chain1.Output = new System.Collections.Generic.List<FurnitureWorkshop.Item>();
        chain1.Output.Add(new FurnitureWorkshop.Item()
            {
            ObjectType = "Steel Plate", Amount = 3, SlotPosX = 2, SlotPosY = 0
            });

        fi.PossibleProductions.Add(chain1);

        var chain2 = new FurnitureWorkshop.ProductionChain()
            {
            Name = "Copper smelting", ProcessingTime = 3.0f
        };
        chain2.Input = new System.Collections.Generic.List<FurnitureWorkshop.Item>();
        chain2.Input.Add(new FurnitureWorkshop.Item()
            {
            ObjectType = "Raw Copper", Amount = 3, SlotPosX = 0, SlotPosY = 0
            });
        chain2.Output = new System.Collections.Generic.List<FurnitureWorkshop.Item>();
        chain2.Output.Add(new FurnitureWorkshop.Item()
            {
            ObjectType = "Copper Wire", Amount = 6, SlotPosX = 2, SlotPosY = 0
            });

        fi.PossibleProductions.Add(chain2);

        FileStream fs = new FileStream("Workshop.xml", FileMode.Create);
        TextWriter writer = new StreamWriter(fs, new UTF8Encoding());

        XmlSerializer serializer = new XmlSerializer(typeof(FurnitureWorkshop));
        serializer.Serialize(writer, fi);
        writer.Close();

        FileStream fr = new FileStream("Workshop.xml", FileMode.Open);
        var dfi = (FurnitureWorkshop)serializer.Deserialize(fr);

        Assert.NotNull(dfi);
        Assert.AreEqual("Raw Iron", dfi.PossibleProductions[0].Input[0].ObjectType);
    }
}
