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

public class LivePowerSystemTest
{
    private LivePowerSystem livePowerSystem;
    private HashSet<PowerGrid> powerGrids;

    [SetUp]
    public void Init()
    {
        livePowerSystem = new LivePowerSystem();
        Type livePowerSystemType = typeof(LivePowerSystem);
        FieldInfo field = livePowerSystemType.GetField("powerGrids", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        powerGrids = field.GetValue(livePowerSystem) as HashSet<PowerGrid>;
        Assert.IsNotNull(powerGrids);
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        livePowerSystem.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        Assert.IsTrue(livePowerSystem.PlugIn(new PowerRelated()));
        Assert.AreEqual(1, powerGrids.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        PowerRelated powerRelated = new PowerRelated();
        Assert.IsTrue(livePowerSystem.PlugIn(powerRelated));
        Assert.AreEqual(1, powerGrids.Count);
        PowerGrid powerGrid;
        Assert.IsTrue(livePowerSystem.IsPluggedIn(powerRelated, out powerGrid));
        Assert.IsNotNull(powerGrid);
    }

    [Test]
    public void UnplugTest()
    {
        PowerRelated powerRelated = new PowerRelated();
        Assert.IsTrue(livePowerSystem.PlugIn(powerRelated));
        Assert.AreEqual(1, powerGrids.Count);
        livePowerSystem.Unplug(powerRelated);
        Assert.AreEqual(1, powerGrids.Count);
        livePowerSystem.Update();
        Assert.AreEqual(0, powerGrids.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 50.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        livePowerSystem.PlugIn(powerProducer);
        livePowerSystem.PlugIn(firstPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);
        livePowerSystem.Update();
        Assert.IsTrue(livePowerSystem.HasPower(powerProducer));
        Assert.IsTrue(livePowerSystem.HasPower(firstPowerConsumer));
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 50.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        PowerRelated secondPowerConsumer = new PowerRelated { InputRate = 30.0f };
        livePowerSystem.PlugIn(powerProducer);
        livePowerSystem.PlugIn(firstPowerConsumer);
        livePowerSystem.PlugIn(secondPowerConsumer);
        Assert.AreEqual(1, powerGrids.Count);
        livePowerSystem.Update();
        Assert.IsFalse(livePowerSystem.HasPower(powerProducer));
        Assert.IsFalse(livePowerSystem.HasPower(firstPowerConsumer));
        Assert.IsFalse(livePowerSystem.HasPower(secondPowerConsumer));
    }
}
