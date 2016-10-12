#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using ProjectPorcupine.Buildable.Components;

public class BuildableComponentTest
{
    [Test]
    public void TestComponentCreation()
    {        
        string inputXML = @"<Component type='Workshop' >
        <ProductionChain name = 'Power Cell Pressing' processingTime = '5' >
         <Input>
             <Item objectType = 'Uranium' amount = '3' slotPosX = '0' slotPosY = '0' />
                    <Item objectType = 'Steel Plate' amount = '2' slotPosX = '1' slotPosY = '0' />
                       </Input>
                       <Output>
                           <Item objectType = 'Power Cell' amount = '1' slotPosX = '2' slotPosY = '1' />
                              </Output>
                          </ProductionChain>
                        </Component> ";

        XmlReader reader = new XmlTextReader(new StringReader(inputXML));
        reader.Read();
                
        BuildableComponent component = ProjectPorcupine.Buildable.Components.BuildableComponent.Deserialize(reader);

        Assert.NotNull(component);
        Assert.AreEqual("Workshop", component.Type);
    }
    
    [Test]
    public void TestWorkshopSerialization()
    {
        Workshop fi = new Workshop();        
        fi.PossibleProductions = new System.Collections.Generic.List<Workshop.ProductionChain>();
        Workshop.ProductionChain chain1 = new Workshop.ProductionChain()
            {
            Name = "Iron smelting", ProcessingTime = 4.0f
            };
        chain1.Input = new System.Collections.Generic.List<Workshop.Item>();
        chain1.Input.Add(new Workshop.Item()
            {
            ObjectType = "Raw Iron", Amount = 3, SlotPosX = 0, SlotPosY = 0
            });
        chain1.Output = new System.Collections.Generic.List<Workshop.Item>();
        chain1.Output.Add(new Workshop.Item()
            {
            ObjectType = "Steel Plate", Amount = 3, SlotPosX = 2, SlotPosY = 0
            });

        fi.PossibleProductions.Add(chain1);

        Workshop.ProductionChain chain2 = new Workshop.ProductionChain()
            {
            Name = "Copper smelting", ProcessingTime = 3.0f
        };
        chain2.Input = new System.Collections.Generic.List<Workshop.Item>();
        chain2.Input.Add(new Workshop.Item()
            {
            ObjectType = "Raw Copper", Amount = 3, SlotPosX = 0, SlotPosY = 0
            });
        chain2.Output = new System.Collections.Generic.List<Workshop.Item>();
        chain2.Output.Add(new Workshop.Item()
            {
            ObjectType = "Copper Wire", Amount = 6, SlotPosX = 2, SlotPosY = 0
            });

        fi.PossibleProductions.Add(chain2);

        fi.UsedAnimation = new BuildableComponent.UsedAnimations()
        {
            Idle = "idle",
            Running = "running"
        };
               
        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(Workshop) });

        serializer.Serialize(writer, fi);        

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("Workshop.xml", writer.ToString());
        
        // deserialize
        Workshop dfi = (Workshop)serializer.Deserialize(sr);
        
        Assert.NotNull(dfi);
        Assert.AreEqual("Raw Iron", dfi.PossibleProductions[0].Input[0].ObjectType);
        Assert.AreEqual("running", dfi.UsedAnimation.Running);
    }

    [Test]
    public void TestGasConnectionSerialization()
    {
        GasConnection gasCon = new GasConnection();

        gasCon.Provides = new System.Collections.Generic.List<GasConnection.GasInfo>()
        {
            new GasConnection.GasInfo()
            {
                Gas = "O2",
                Rate = 0.16f,
                MaxLimit = 0.2f
            },
            new GasConnection.GasInfo()
            {
                Gas = "N2",
                Rate = 0.16f,
                MaxLimit = 0.8f
            }
        };

        gasCon.UsedAnimation = new BuildableComponent.UsedAnimations()
        {
            Idle = "idle",
            Running = "running"
        };

        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(GasConnection) });

        serializer.Serialize(writer, gasCon);

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("GasConnection.xml", writer.ToString());

        // deserialize
        GasConnection desGasConn = (GasConnection)serializer.Deserialize(sr);

        Assert.NotNull(desGasConn);

        Assert.AreEqual(2, desGasConn.Provides.Count);
        Assert.AreEqual("O2", desGasConn.Provides[0].Gas);
    }
}
