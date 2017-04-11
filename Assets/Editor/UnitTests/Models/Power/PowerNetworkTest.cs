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

public class PowerNetworkTest
{
    private PowerNetwork powerNetwork;
    private HashSet<Grid> powerGrids;

    [SetUp]
    public void Init()
    {
        powerNetwork = new PowerNetwork();
        Type livePowerSystemType = typeof(PowerNetwork);
        FieldInfo field = livePowerSystemType.GetField("powerGrids", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        powerGrids = field.GetValue(powerNetwork) as HashSet<Grid>;
        Assert.IsNotNull(powerGrids);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        powerNetwork.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        Assert.IsTrue(powerNetwork.PlugIn(new MockConnection()));
        Assert.AreEqual(1, powerGrids.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        MockConnection connection = new MockConnection();
        Assert.IsTrue(powerNetwork.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        Grid grid;
        Assert.IsTrue(powerNetwork.IsPluggedIn(connection, out grid));
        Assert.IsNotNull(grid);
    }

    [Test]
    public void UnplugTest()
    {
        MockConnection connection = new MockConnection();
        Grid grid = new Grid();
        Assert.IsTrue(powerNetwork.PlugIn(connection, grid));
        Assert.AreEqual(1, powerGrids.Count);
        powerNetwork.Unplug(connection);
        Assert.AreEqual(0, grid.ConnectionCount);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        powerNetwork.PlugIn(powerProducer);
        powerNetwork.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        powerNetwork.Update(1.0f);
        Assert.IsTrue(powerNetwork.HasPower(powerProducer));
        Assert.IsTrue(powerNetwork.HasPower(firstPowerConsumer));
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        MockConnection secondPowerConsumer = new MockConnection { InputRate = 30.0f };
        powerNetwork.PlugIn(powerProducer);
        powerNetwork.PlugIn(firstPowerConsumer);
        powerNetwork.PlugIn(secondPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        powerNetwork.Update(1.0f);
        Assert.IsFalse(powerNetwork.HasPower(powerProducer));
        Assert.IsFalse(powerNetwork.HasPower(firstPowerConsumer));
        Assert.IsFalse(powerNetwork.HasPower(secondPowerConsumer));
    }

    [Test]
    public void UpdateIntervalTest()
    {
        MockConnection powerProducer = new MockConnection { OutputRate = 50.0f };
        MockConnection firstPowerConsumer = new MockConnection { InputRate = 30.0f };
        powerNetwork.PlugIn(powerProducer);
        powerNetwork.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        powerNetwork.Update(0.2f);
        Assert.IsFalse(powerNetwork.HasPower(powerProducer));
        Assert.IsFalse(powerNetwork.HasPower(firstPowerConsumer));

        powerNetwork.Update(0.2f);
        Assert.IsFalse(powerNetwork.HasPower(powerProducer));
        Assert.IsFalse(powerNetwork.HasPower(firstPowerConsumer));

        powerNetwork.Update(0.2f);
        powerNetwork.Update(0.2f);
        powerNetwork.Update(0.2f);
        Assert.IsTrue(powerNetwork.HasPower(powerProducer));
        Assert.IsTrue(powerNetwork.HasPower(firstPowerConsumer));
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
