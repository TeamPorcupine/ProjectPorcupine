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
        Workshop workshop = new Workshop();        
        workshop.PossibleProductions = new System.Collections.Generic.List<Workshop.ProductionChain>();
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

        workshop.PossibleProductions.Add(chain1);

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

        workshop.PossibleProductions.Add(chain2);

        workshop.ParamsDefinitions = new Workshop.WorkShopParameterDefinitions();
                       
        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(Workshop) });

        serializer.Serialize(writer, workshop);        

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("Workshop.xml", writer.ToString());
        
        // deserialize
        Workshop deserializedWorkshop = (Workshop)serializer.Deserialize(sr);
        
        Assert.NotNull(deserializedWorkshop);
        Assert.AreEqual("Raw Iron", deserializedWorkshop.PossibleProductions[0].Input[0].ObjectType);
    }

    [Test]
    public void TestGasConnectionSerialization()
    {
        GasConnection gasConnection = new GasConnection();

        gasConnection.Provides = new System.Collections.Generic.List<GasConnection.GasInfo>()
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

        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(GasConnection) });

        serializer.Serialize(writer, gasConnection);

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("GasConnection.xml", writer.ToString());

        // deserialize
        GasConnection deserializedGasConnection = (GasConnection)serializer.Deserialize(sr);

        Assert.NotNull(deserializedGasConnection);

        Assert.AreEqual(2, deserializedGasConnection.Provides.Count);
        Assert.AreEqual("O2", deserializedGasConnection.Provides[0].Gas);
    }

    [Test]
    public void TestPowerConnectionSerialization()
    {
        PowerConnection accumulator = new PowerConnection
        {
            Provides = new PowerConnection.Info() { Rate = 10.0f, Capacity = 100.0f },
            Requires = new PowerConnection.Info()
            {
                ParamConditions = new System.Collections.Generic.List<BuildableComponent.ParameterCondition>()
                {
                    new BuildableComponent.ParameterCondition()
                    {
                        Condition = BuildableComponent.ConditionType.IsGreaterThanZero,
                        ParameterName = "cur_processed_inv"
                    }
                }
            },
            ParamsDefinitions = new PowerConnection.PowerConnectionParameterDefinitions()           
        };

        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(PowerConnection) });

        serializer.Serialize(writer, accumulator);

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("PowerConnection.xml", writer.ToString());

        // deserialize
        PowerConnection deserializedPowerConnection = (PowerConnection)serializer.Deserialize(sr);

        Assert.NotNull(deserializedPowerConnection);

        Assert.AreEqual(10f, deserializedPowerConnection.Provides.Rate);
        Assert.AreEqual(0f, deserializedPowerConnection.Requires.Rate);
        Assert.AreEqual(1, deserializedPowerConnection.Requires.ParamConditions.Count);
    }

    [Test]
    public void TestVisualsSerialization()
    {
        Visuals visuals = new Visuals()
        {
            UsedAnimations = new System.Collections.Generic.List<BuildableComponent.UseAnimation>()
            {
                new BuildableComponent.UseAnimation()
                {
                    Name = "idle",
                    Requires = new BuildableComponent.ParameterConditions()
                    {
                        ParamConditions = new System.Collections.Generic.List<BuildableComponent.ParameterCondition>()
                        {
                            new BuildableComponent.ParameterCondition()
                            {
                                Condition = BuildableComponent.ConditionType.IsZero,
                                ParameterName = "cur_processed_inv"
                            }
                        }
                    }
                },
                new BuildableComponent.UseAnimation()
                {
                    Name = "running",
                    Requires = new BuildableComponent.ParameterConditions()
                    {
                        ParamConditions = new System.Collections.Generic.List<BuildableComponent.ParameterCondition>()
                        {
                            new BuildableComponent.ParameterCondition()
                            {
                                Condition = BuildableComponent.ConditionType.IsGreaterThanZero,
                                ParameterName = "cur_processed_inv"
                            }
                        }
                    }
                }
            }
        };

        // serialize
        StringWriter writer = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(BuildableComponent), new Type[] { typeof(Visuals) });

        serializer.Serialize(writer, visuals);

        StringReader sr = new StringReader(writer.ToString());

        // if you want to dump file to disk for visual check, uncomment this
        ////File.WriteAllText("Animator.xml", writer.ToString());

        // deserialize
        Visuals deserializedAnimator = (Visuals)serializer.Deserialize(sr);

        Assert.NotNull(deserializedAnimator);        
    }
}
