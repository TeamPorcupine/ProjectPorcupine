#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

public class FurnitureWorkshopTest
{
    [Test]
    public void TestSerialization()
    {
        FurnitureWorkshop fi = new FurnitureWorkshop();        
        fi.PossibleProductions = new System.Collections.Generic.List<FurnitureWorkshop.ProductionChain>();
        FurnitureWorkshop.ProductionChain chain1 = new FurnitureWorkshop.ProductionChain()
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

        FurnitureWorkshop.ProductionChain chain2 = new FurnitureWorkshop.ProductionChain()
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

        fi.UsedAnimation = new FurnitureWorkshop.UsedAnimations()
        {
            Idle = "idle",
            Running = "running"
        };
        
        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(FurnitureWorkshop));
        serializer.Serialize(writer, fi);        

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("Workshop.xml", writer.ToString());
        
        // deserialize
        FurnitureWorkshop dfi = (FurnitureWorkshop)serializer.Deserialize(sr);
        
        Assert.NotNull(dfi);
        Assert.AreEqual("Raw Iron", dfi.PossibleProductions[0].Input[0].ObjectType);
        Assert.AreEqual("running", dfi.UsedAnimation.Running);
    }
}
