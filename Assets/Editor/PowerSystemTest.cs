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

public class PowerSystemTest
{
    private Syster syster;
    private HashSet<Grid> powerGrids;

    [SetUp]
    public void Init()
    {
        syster = new Syster();
        Type livePowerSystemType = typeof(Syster);
        FieldInfo field = livePowerSystemType.GetField("powerGrids", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        powerGrids = field.GetValue(syster) as HashSet<Grid>;
        Assert.IsNotNull(powerGrids);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        syster.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        Assert.IsTrue(syster.PlugIn(new Connection()));
        Assert.AreEqual(1, powerGrids.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(syster.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        Grid grid;
        Assert.IsTrue(syster.IsPluggedIn(connection, out grid));
        Assert.IsNotNull(grid);
    }

    [Test]
    public void UnplugTest()
    {
        Connection connection = new Connection();
        Assert.IsTrue(syster.PlugIn(connection));
        Assert.AreEqual(1, powerGrids.Count);
        syster.Unplug(connection);
        Assert.AreEqual(1, powerGrids.Count);

        syster.Update(1.0f);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        syster.PlugIn(powerProducer);
        syster.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        syster.Update(1.0f);
        Assert.IsTrue(syster.HasPower(powerProducer));
        Assert.IsTrue(syster.HasPower(firstPowerConsumer));
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        Connection secondPowerConsumer = new Connection { InputRate = 30.0f };
        syster.PlugIn(powerProducer);
        syster.PlugIn(firstPowerConsumer);
        syster.PlugIn(secondPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        syster.Update(1.0f);
        Assert.IsFalse(syster.HasPower(powerProducer));
        Assert.IsFalse(syster.HasPower(firstPowerConsumer));
        Assert.IsFalse(syster.HasPower(secondPowerConsumer));
    }

    [Test]
    public void UpdateIntervalTest()
    {
        Connection powerProducer = new Connection { OutputRate = 50.0f };
        Connection firstPowerConsumer = new Connection { InputRate = 30.0f };
        syster.PlugIn(powerProducer);
        syster.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);

        syster.Update(0.2f);
        Assert.IsFalse(syster.HasPower(powerProducer));
        Assert.IsFalse(syster.HasPower(firstPowerConsumer));

        syster.Update(0.2f);
        Assert.IsFalse(syster.HasPower(powerProducer));
        Assert.IsFalse(syster.HasPower(firstPowerConsumer));

        syster.Update(0.2f);
        syster.Update(0.2f);
        syster.Update(0.2f);
        Assert.IsTrue(syster.HasPower(powerProducer));
        Assert.IsTrue(syster.HasPower(firstPowerConsumer));
    }
}
