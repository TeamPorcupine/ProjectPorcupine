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
using Power;

public class PowerGridTest
{
    private Grid grid;
    private HashSet<Connection> connections;

    [SetUp]
    public void Init()
    {
        grid = new Grid();
        Type powerGridType = typeof(Grid);
        FieldInfo field = powerGridType.GetField("connections", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        connections = field.GetValue(grid) as HashSet<Connection>;
        Assert.IsNotNull(connections);
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
        grid.PlugIn(new Connection());
        Assert.AreEqual(1, connections.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        Connection connection = new Connection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        Assert.IsTrue(grid.IsPluggedIn(connection));
    }

    [Test]
    public void UnplugTest()
    {
        Connection connection = new Connection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        grid.Unplug(connection);
        Assert.AreEqual(0, connections.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        Assert.AreEqual(2, connections.Count);
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection secondPowerConsumer = new Connection { InputRate = 30.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        grid.PlugIn(secondPowerConsumer);
        Assert.AreEqual(3, connections.Count);
        grid.Update();
        Assert.IsFalse(grid.IsOperating);
    }

    [Test]
    public void ChargeAccumulatorsTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection accumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(3, connections.Count);
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(10.0f));
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(20.0f));
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(30.0f));
    }

    [Test]
    public void DischargeAccumulatorsEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 30.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection firstAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        Connection secondAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        Connection thirdAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        grid.PlugIn(thirdAccumulator);
        Assert.AreEqual(4, connections.Count);

        grid.Update();
        grid.Update();
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(20.0f));

        grid.Update();
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.IsEmpty);
        Assert.IsTrue(secondAccumulator.IsEmpty);
        Assert.IsTrue(thirdAccumulator.IsEmpty);

        grid.Update();
        Assert.IsFalse(grid.IsOperating);
    }

    [Test]
    public void DischargeAccumulatorsNotEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 30.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection firstAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        Connection secondAccumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        Assert.AreEqual(3, connections.Count);

        grid.Update();
        grid.Update();
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Update();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Update();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    }
}
