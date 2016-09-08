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
        Assert.IsTrue(powerNetwork.PlugIn(new Connection()));
        Assert.AreEqual(1, powerGrids.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(powerNetwork.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        Grid grid;
        Assert.IsTrue(powerNetwork.IsPluggedIn(connection, out grid));
        Assert.IsNotNull(grid);
    }

    [Test]
    public void UnplugTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(powerNetwork.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        powerNetwork.Unplug(connection);
        Assert.AreEqual(1, powerGrids.Count);

        powerNetwork.Update(1.0f);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
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
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection secondPowerConsumer = new Connection { InputRate = 30.0f };
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
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
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
}
