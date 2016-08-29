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
        grid.Tick();
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
        grid.Tick();
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
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(10.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(20.0f));
        grid.Tick();
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

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(20.0f));

        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.IsEmpty);
        Assert.IsTrue(secondAccumulator.IsEmpty);
        Assert.IsTrue(thirdAccumulator.IsEmpty);

        grid.Tick();
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

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    }

    [Test]
    public void ChargeAccumulatorsCapacityTest()
    {
        Connection powerProducer = new Connection { OutputRate = 20.0f };
        Connection accumulator = new Connection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(15.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(30.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(40.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(40.0f));
    }

    [Test]
    public void DischargeAccumulatorsCapacityTest()
    {
        Connection powerConsumer = new Connection { InputRate = 4.0f };
        Connection accumulator = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 15.0f };
        grid.PlugIn(powerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(11.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(7.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(3.0f));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(3.0f));
    }

    [Test]
    public void ChargeAccumulatorsInputRateTest()
    {
        Connection powerProducer = new Connection { OutputRate = 20.0f };
        Connection accumulator1 = new Connection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
        Connection accumulator2 = new Connection { InputRate = 15.0f, OutputRate = 10.0f, Capacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(3, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(15.0f), string.Format("Expected {0} Actual {1}", 15.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(30.0f), string.Format("Expected {0} Actual {1}", 30.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(10.0f), string.Format("Expected {0} Actual {1}", 10.0f, accumulator2.AccumulatedPower));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(20.0f), string.Format("Expected {0} Actual {1}", 20.0f, accumulator2.AccumulatedPower));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(35.0f), string.Format("Expected {0} Actual {1}", 35.0f, accumulator2.AccumulatedPower));
    }

    [Test]
    public void DischargeAccumulatorsOutputRateTest()
    {
        Connection powerConsumer1 = new Connection { InputRate = 5.0f };
        Connection powerConsumer2 = new Connection { InputRate = 5.0f };
        Connection powerConsumer3 = new Connection { InputRate = 5.0f };
        Connection accumulator1 = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 10.0f };
        Connection accumulator2 = new Connection { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f, AccumulatedPower = 10.0f };
        grid.PlugIn(powerConsumer1);
        grid.PlugIn(powerConsumer2);
        grid.PlugIn(powerConsumer3);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(5, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedPower.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedPower));
        Assert.IsTrue(accumulator2.AccumulatedPower.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedPower));
    }
}
