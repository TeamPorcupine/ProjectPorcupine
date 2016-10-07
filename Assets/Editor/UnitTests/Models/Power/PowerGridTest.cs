#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectPorcupine.PowerNetwork;
using ProjectPorcupine.Buildable.Components;

public class PowerGridTest
{
    private Grid grid;
    private HashSet<PowerConnection> connections;
    PowerNetwork powerNetwork;

    [SetUp]
    public void Init()
    {
        grid = new Grid();
        Type powerGridType = typeof(Grid);
        FieldInfo field = powerGridType.GetField("connections", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        connections = field.GetValue(grid) as HashSet<PowerConnection>;
        Assert.IsNotNull(connections);

        powerNetwork = new PowerNetwork();
        //powerNetwork.GetType().GetProperty("PowerNetwork").SetValue(World.Current, powerNetwork, null);

        //World.Current
        //World.Current.SetPrivatePropertyValue("PowerNetwork", powerNetwork);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        grid.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        grid.PlugIn(new PowerConnection());
        Assert.AreEqual(1, connections.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        PowerConnection connection = new PowerConnection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        Assert.IsTrue(grid.IsPluggedIn(connection));
    }

    [Test]
    public void UnplugTest()
    {
        PowerConnection connection = new PowerConnection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        grid.Unplug(connection);
        Assert.AreEqual(0, connections.Count);
    }

    //[Test]
    //public void UpdateEnoughPowerTest()
    //{
    //    PowerConnection powerProducer = new PowerConnection { Provides = new PowerConnection.Info() { Rate = 50.0f } };
    //    PowerConnection firstPowerConsumer = new PowerConnection { Requires = new PowerConnection.Info() { Rate = 30.0f } };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(firstPowerConsumer);
    //    Assert.AreEqual(2, connections.Count);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //}

    //[Test]
    //public void UpdateNotEnoughPowerTest()
    //{
    //    PowerConnection powerProducer = new PowerConnection { Provides = new PowerConnection.Info() { Rate = 50.0f } };
    //    PowerConnection firstPowerConsumer = new PowerConnection { Requires = new PowerConnection.Info() { Rate = 30.0f } };
    //    PowerConnection secondPowerConsumer = new PowerConnection { Requires = new PowerConnection.Info() { Rate = 30.0f } };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(firstPowerConsumer);
    //    grid.PlugIn(secondPowerConsumer);
    //    Assert.AreEqual(3, connections.Count);
    //    grid.Tick();
    //    Assert.IsFalse(grid.IsOperating);
    //}

    //[Test]
    //public void ChargeAccumulatorsTest()
    //{
    //    Furniture f1 = new Furniture();
    //    Furniture f2 = new Furniture();
    //    Furniture f3 = new Furniture();

    //    PowerConnection powerProducer = new PowerConnection { Provides = new PowerConnection.Info() { Rate = 50.0f } };
    //    PowerConnection firstPowerConsumer = new PowerConnection { Requires = new PowerConnection.Info() { Rate = 30.0f } };
    //    PowerConnection accumulator = new PowerConnection
    //    {
    //        Provides = new PowerConnection.Info() { Rate = 10.0f, Capacity = 100.0f },
    //        Requires = new PowerConnection.Info() { Rate = 10.0f }
    //    };

    //    powerProducer.Initialize(f1);
    //    firstPowerConsumer.Initialize(f2);
    //    accumulator.Initialize(f3);

    //    //grid.PlugIn(powerProducer);
    //    //grid.PlugIn(firstPowerConsumer);
    //    //grid.PlugIn(accumulator);
    //    Assert.AreEqual(3, connections.Count);
    //    //grid.Tick();
    //    powerNetwork.Update(1.01f);
    //    //Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(10.0f));
    //    powerNetwork.Update(1.01f);
    //    //grid.Tick();
    //    //Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(20.0f));
    //    powerNetwork.Update(1.01f);
    //    //grid.Tick();
    //    //Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(30.0f));
    //}

    //[Test]
    //public void DischargeAccumulatorsEnoughPowerTest()
    //{
    //    PowerConnection powerProducer = new PowerConnection { Provides = new PowerConnection.Info() { Rate = 30.0f } };
    //    PowerConnection firstPowerConsumer = new PowerConnection { Requires = new PowerConnection.Info() { Rate = 30.0f } };
    //    PowerConnection firstAccumulator = new PowerConnection {
    //        Requires = new PowerConnection.Info() { Rate = 10.0f },
    //        Provides = new PowerConnection.Info() { Rate = 10.0f, Capacity = 100f }
    //    };
    //    PowerConnection secondAccumulator = new PowerConnection
    //    {
    //        Requires = new PowerConnection.Info() { Rate = 10.0f },
    //        Provides = new PowerConnection.Info() { Rate = 10.0f, Capacity = 100f }
    //    };
    //    PowerConnection thirdAccumulator = new PowerConnection
    //    {
    //        Requires = new PowerConnection.Info() { Rate = 10.0f },
    //        Provides = new PowerConnection.Info() { Rate = 10.0f, Capacity = 100f }
    //    };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(firstAccumulator);
    //    grid.PlugIn(secondAccumulator);
    //    grid.PlugIn(thirdAccumulator);
    //    Assert.AreEqual(4, connections.Count);

    //    grid.Tick();
    //    grid.Tick();
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

    //    grid.PlugIn(firstPowerConsumer);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

    //    grid.Unplug(powerProducer);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(20.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(20.0f));
    //    Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(20.0f));

    //    grid.Tick();
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.IsEmpty);
    //    Assert.IsTrue(secondAccumulator.IsEmpty);
    //    Assert.IsTrue(thirdAccumulator.IsEmpty);

    //    grid.Tick();
    //    Assert.IsFalse(grid.IsOperating);
    //}

    //[Test]
    //public void DischargeAccumulatorsNotEnoughPowerTest()
    //{
    //    Connection powerProducer = new PowerConnection { Provides = new PowerConnection.PowerInfo() { Rate = 30.0f } };
    //    Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
    //    Connection firstAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
    //    Connection secondAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(firstAccumulator);
    //    grid.PlugIn(secondAccumulator);
    //    Assert.AreEqual(3, connections.Count);

    //    grid.Tick();
    //    grid.Tick();
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

    //    grid.PlugIn(firstPowerConsumer);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

    //    grid.Unplug(powerProducer);
    //    grid.Tick();
    //    Assert.IsFalse(grid.IsOperating);
    //    Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
    //    Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    //}

    //[Test]
    //public void ChargeAccumulatorsCapacityTest()
    //{
    //    Connection powerProducer = new PowerConnection { Provides = new PowerConnection.PowerInfo() { Rate = 20.0f } };
    //    Connection accumulator = new Connection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(accumulator);
    //    Assert.AreEqual(2, connections.Count);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(15.0f));
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(30.0f));
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(40.0f));
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(40.0f));
    //}

    //[Test]
    //public void DischargeAccumulatorsCapacityTest()
    //{
    //    PowerConnection powerConsumer = new PowerConnection { InputRate = 4.0f };
    //    PowerConnection accumulator = new PowerConnection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 15.0f };
    //    grid.PlugIn(powerConsumer);
    //    grid.PlugIn(accumulator);
    //    Assert.AreEqual(2, connections.Count);

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(11.0f));

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(7.0f));

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(3.0f));

    //    grid.Tick();
    //    Assert.IsFalse(grid.IsOperating);
    //    Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(3.0f));
    //}

    //[Test]
    //public void ChargeAccumulatorsInputRateTest()
    //{
    //    PowerConnection powerProducer = new PowerConnection { Provides = new PowerConnection.PowerInfo() { Rate = 20.0f } };
    //    PowerConnection accumulator1 = new PowerConnection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
    //    PowerConnection accumulator2 = new PowerConnection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
    //    grid.PlugIn(powerProducer);
    //    grid.PlugIn(accumulator1);
    //    grid.PlugIn(accumulator2);
    //    Assert.AreEqual(3, connections.Count);
    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(15.0f), string.Format("Expected {0} Actual {1}", 15.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(30.0f), string.Format("Expected {0} Actual {1}", 30.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(10.0f), string.Format("Expected {0} Actual {1}", 10.0f, accumulator2.AccumulatedPower));

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(20.0f), string.Format("Expected {0} Actual {1}", 20.0f, accumulator2.AccumulatedPower));

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(35.0f), string.Format("Expected {0} Actual {1}", 35.0f, accumulator2.AccumulatedPower));
    //}

    //[Test]
    //public void DischargeAccumulatorsOutputRateTest()
    //{
    //    Connection powerConsumer1 = new Connection { InputRate = 5.0f };
    //    Connection powerConsumer2 = new Connection { InputRate = 5.0f };
    //    Connection powerConsumer3 = new Connection { InputRate = 5.0f };
    //    Connection accumulator1 = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 10.0f };
    //    Connection accumulator2 = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 10.0f };
    //    grid.PlugIn(powerConsumer1);
    //    grid.PlugIn(powerConsumer2);
    //    grid.PlugIn(powerConsumer3);
    //    grid.PlugIn(accumulator1);
    //    grid.PlugIn(accumulator2);
    //    Assert.AreEqual(5, connections.Count);

    //    grid.Tick();
    //    Assert.IsTrue(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));

    //    grid.Tick();
    //    Assert.IsFalse(grid.IsOperating);
    //    Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedPower));
    //    Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));
    //}
}
