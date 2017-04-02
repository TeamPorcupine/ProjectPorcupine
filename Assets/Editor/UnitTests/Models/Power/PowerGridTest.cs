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
    private HashSet<IPluggable> connections;

    [SetUp]
    public void Init()
    {
        grid = new Grid();
        Type powerGridType = typeof(Grid);
        FieldInfo field = powerGridType.GetField("connections", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        connections = field.GetValue(grid) as HashSet<IPluggable>;
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
        MockConnection accumulator = new MockConnection { OutputRate = 10.0f, StorageCapacity = 100.0f, InputRate = 10.0f };

        grid.PlugIn(powerProducer);
        grid.PlugIn(firstPowerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(3, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(10.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(20.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(30.0f));
    }

    [Test]
    public void DischargeAccumulatorsEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 30.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection firstAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100f };
        MockConnection secondAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100f };
        MockConnection thirdAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        grid.PlugIn(thirdAccumulator);
        Assert.AreEqual(4, connections.Count);

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.StoredAmount.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.StoredAmount.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(20.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(20.0f));
        Assert.IsTrue(thirdAccumulator.StoredAmount.AreEqual(20.0f));

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
        MockConnection firstAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100.0f };
        MockConnection secondAccumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(firstAccumulator);
        grid.PlugIn(secondAccumulator);
        Assert.AreEqual(3, connections.Count);

        grid.Tick();
        grid.Tick();
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(30.0f));

        grid.PlugIn(firstPowerConsumer);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(30.0f));

        grid.Unplug(powerProducer);
        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(firstAccumulator.StoredAmount.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.StoredAmount.AreEqual(30.0f));
    }

    [Test]
    public void ChargeAccumulatorsCapacityTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 20.0f };
        MockConnection accumulator = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, StorageCapacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(15.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(30.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(40.0f));
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(40.0f));
    }

    [Test]
    public void DischargeAccumulatorsCapacityTest()
    {
        MockConnection powerConsumer = new MockConnection { InputRate = 4.0f };
        MockConnection accumulator = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100.0f, StoredAmount = 15.0f };
        grid.PlugIn(powerConsumer);
        grid.PlugIn(accumulator);
        Assert.AreEqual(2, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(11.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(7.0f));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(3.0f));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator.StoredAmount.AreEqual(3.0f));
    }

    [Test]
    public void ChargeAccumulatorsInputRateTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 20.0f };
        MockConnection accumulator1 = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, StorageCapacity = 40.0f };
        MockConnection accumulator2 = new MockConnection { InputRate = 15.0f, OutputRate = 10.0f, StorageCapacity = 40.0f };
        grid.PlugIn(powerProducer);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(3, connections.Count);
        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(15.0f), string.Format("Expected {0} Actual {1}", 15.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.StoredAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(30.0f), string.Format("Expected {0} Actual {1}", 30.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(10.0f), string.Format("Expected {0} Actual {1}", 10.0f, accumulator2.StoredAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(20.0f), string.Format("Expected {0} Actual {1}", 20.0f, accumulator2.StoredAmount));

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(40.0f), string.Format("Expected {0} Actual {1}", 40.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(35.0f), string.Format("Expected {0} Actual {1}", 35.0f, accumulator2.StoredAmount));
    }

    [Test]
    public void DischargeAccumulatorsOutputRateTest()
    {
        MockConnection powerConsumer1 = new MockConnection { InputRate = 5.0f };
        MockConnection powerConsumer2 = new MockConnection { InputRate = 5.0f };
        MockConnection powerConsumer3 = new MockConnection { InputRate = 5.0f };
        MockConnection accumulator1 = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100.0f, StoredAmount = 10.0f };
        MockConnection accumulator2 = new MockConnection { InputRate = 10.0f, OutputRate = 10.0f, StorageCapacity = 100.0f, StoredAmount = 10.0f };
        grid.PlugIn(powerConsumer1);
        grid.PlugIn(powerConsumer2);
        grid.PlugIn(powerConsumer3);
        grid.PlugIn(accumulator1);
        grid.PlugIn(accumulator2);
        Assert.AreEqual(5, connections.Count);

        grid.Tick();
        Assert.IsTrue(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.StoredAmount));

        grid.Tick();
        Assert.IsFalse(grid.IsOperating);
        Assert.IsTrue(accumulator1.StoredAmount.AreEqual(0.0f), string.Format("Expected {0} Actual {1}", 0.0f, accumulator1.StoredAmount));
        Assert.IsTrue(accumulator2.StoredAmount.AreEqual(5.0f), string.Format("Expected {0} Actual {1}", 5.0f, accumulator2.StoredAmount));
    }

    private class MockConnection : IPluggable
    {
        public event Action Reconnecting;

        public float StoredAmount { get; set; }

        public float StorageCapacity { get; set; }

        public float InputRate { get; set; }

        public bool IsStorage
        {
            get { return StorageCapacity > 0f; }
        }

        public bool IsConsumer
        {
            get { return InputRate > 0f && !IsStorage; }
        }

        public bool IsEmpty
        {
            get { return StoredAmount == 0f; }
        }

        public bool IsFull
        {
            get { return StoredAmount.AreEqual(StorageCapacity); }
        }

        public string UtilityType 
        { 
            get 
            { 
                return "Power";
            }
        }

        public string SubType 
        {
            get 
            {
                return string.Empty;
            }

            set
            {
            }
        }

        public bool IsProducer
        {
            get { return OutputRate > 0f && !IsStorage; }
        }

        public float OutputRate { get; set; }

        public bool InputCanVary { get; set; }

        public bool OutputCanVary { get; set; }

        public bool OutputIsNeeded { get; set; }

        public bool AllRequirementsFulfilled
        {
            get { return true; }
        }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }
    }
}
