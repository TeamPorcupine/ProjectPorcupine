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
using ProjectPorcupine.Power;

public class PowerSystemTest
{
    private ProjectPorcupine.Power.System system;
    private HashSet<Grid> powerGrids;

    [SetUp]
    public void Init()
    {
        system = new ProjectPorcupine.Power.System();
        Type livePowerSystemType = typeof(ProjectPorcupine.Power.System);
        FieldInfo field = livePowerSystemType.GetField("powerGrids", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        powerGrids = field.GetValue(system) as HashSet<Grid>;
        Assert.IsNotNull(powerGrids);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        system.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        Assert.IsTrue(system.PlugIn(new Connection()));
        Assert.AreEqual(1, powerGrids.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(system.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        Grid grid;
        Assert.IsTrue(system.IsPluggedIn(connection, out grid));
        Assert.IsNotNull(grid);
    }

    [Test]
    public void UnplugTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(system.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        system.Unplug(connection);
        Assert.AreEqual(1, powerGrids.Count);

        system.Update(1.0f);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        system.PlugIn(powerProducer);
        system.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        system.Update(1.0f);
        Assert.IsTrue(system.HasPower(powerProducer));
        Assert.IsTrue(system.HasPower(firstPowerConsumer));
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection secondPowerConsumer = new Connection { InputRate = 30.0f };
        system.PlugIn(powerProducer);
        system.PlugIn(firstPowerConsumer);
        system.PlugIn(secondPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        system.Update(1.0f);
        Assert.IsFalse(system.HasPower(powerProducer));
        Assert.IsFalse(system.HasPower(firstPowerConsumer));
        Assert.IsFalse(system.HasPower(secondPowerConsumer));
    }

    [Test]
    public void UpdateIntervalTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        system.PlugIn(powerProducer);
        system.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        system.Update(0.2f);
        Assert.IsFalse(system.HasPower(powerProducer));
        Assert.IsFalse(system.HasPower(firstPowerConsumer));

        system.Update(0.2f);
        Assert.IsFalse(system.HasPower(powerProducer));
        Assert.IsFalse(system.HasPower(firstPowerConsumer));

        system.Update(0.2f);
        system.Update(0.2f);
        system.Update(0.2f);
        Assert.IsTrue(system.HasPower(powerProducer));
        Assert.IsTrue(system.HasPower(firstPowerConsumer));
    }
}
