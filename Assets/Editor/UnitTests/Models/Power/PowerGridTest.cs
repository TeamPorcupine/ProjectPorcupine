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

public class PowerGridTest
{
    private Grid grid;
    private HashSet<IPlugable> connections;

    [SetUp]
    public void Init()
    {
        grid = new Grid();
        Type powerGridType = typeof(Grid);
        FieldInfo field = powerGridType.GetField("connections", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        connections = field.GetValue(grid) as HashSet<IPlugable>;
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
        grid.PlugIn(new MockConnection());
        Assert.AreEqual(1, connections.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        MockConnection connection = new MockConnection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        Assert.IsTrue(grid.IsPluggedIn(connection));
    }

    [Test]
    public void UnplugTest()
    {
        MockConnection connection = new MockConnection();
        grid.PlugIn(connection);
        Assert.AreEqual(1, connections.Count);
        grid.Unplug(connection);
        Assert.AreEqual(0, connections.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        Assert.AreEqual(2, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection secondPowerConsumer = new MockConnection { InputRate = 30.0f };
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
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection accumulator = new MockConnection { OutputRate = 10.0f, AccumulatorCapacity = 100.0f, InputRate = 10.0f };

        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(3, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(10.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(20.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(30.0f));
    }

    [Test]
    public void DischargeAccumulatorsEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 30.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection firstAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100f };
        MockConnection secondAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100f };
        MockConnection thirdAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        grid.PlugIn(thirdAccumulator);
        Assert.AreEqual(4, connections.Count);

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedAmount.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedAmount.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(20.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(20.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedAmount.AreEqual(20.0f));

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
        MockConnection powerProducer = new MockConnection { OutputRate = 30.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection firstAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100.0f };
        MockConnection secondAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        Assert.AreEqual(3, connections.Count);

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedAmount.AreEqual(30.0f));
    }

    [Test]
    public void ChargeAccumulatorsCapacityTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 20.0f };
        MockConnection accumulator = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, AccumulatorCapacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(15.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(30.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(40.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(40.0f));
    }

    [Test]
    public void DischargeAccumulatorsCapacityTest()
    {
        MockConnection powerConsumer = new MockConnection { InputRate = 4.0f };
        MockConnection accumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100.0f, AccumulatedAmount = 15.0f };
        grid.PlugIn(powerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(11.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(7.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(3.0f));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedAmount.AreEqual(3.0f));
    }

    [Test]
    public void ChargeAccumulatorsInputRateTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 20.0f };
        MockConnection accumulator1 = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, AccumulatorCapacity = 40.0f };
        MockConnection accumulator2 = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, AccumulatorCapacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(3, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(15.0f), string.Format("Expected {0} Actual {1}", 15.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(30.0f), string.Format("Expected {0} Actual {1}", 30.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(10.0f), string.Format("Expected {0} Actual {1}", 10.0f, accumulator2.AccumulatedAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(20.0f), string.Format("Expected {0} Actual {1}", 20.0f, accumulator2.AccumulatedAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(35.0f), string.Format("Expected {0} Actual {1}", 35.0f, accumulator2.AccumulatedAmount));
    }

    [Test]
    public void DischargeAccumulatorsOutputRateTest()
    {
        MockConnection powerConsumer1 = new MockConnection { InputRate = 5.0f };
        MockConnection powerConsumer2 = new MockConnection { InputRate = 5.0f };
        MockConnection powerConsumer3 = new MockConnection { InputRate = 5.0f };
        MockConnection accumulator1 = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100.0f, AccumulatedAmount = 10.0f };
        MockConnection accumulator2 = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, AccumulatorCapacity = 100.0f, AccumulatedAmount = 10.0f };
        grid.PlugIn(powerConsumer1);
        grid.PlugIn(powerConsumer2);
        grid.PlugIn(powerConsumer3);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(5, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedAmount));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator1.AccumulatedAmount.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.AccumulatedAmount));
        Assert.IsTrue(accumulator2.AccumulatedAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.AccumulatedAmount));
    }

    private class MockConnection : IPlugable
    {
        public event Action Reconnecting;

        public float AccumulatedAmount { get; set; }

        public float AccumulatorCapacity { get; set; }

        public float InputRate { get; set; }

        public bool IsAccumulator
        {
            get { return AccumulatorCapacity > 0f; }
        }

        public bool IsConsumer
        {
            get { return InputRate > 0f && !IsAccumulator; }
        }

        public bool IsEmpty
        {
            get { return AccumulatedAmount == 0f; }
        }

        public bool IsFull
        {
            get { return AccumulatedAmount.AreEqual(AccumulatorCapacity); }
        }

        public bool IsProducer
        {
            get { return OutputRate > 0f && !IsAccumulator; }
        }

        public float OutputRate { get; set; }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }
    }
}
