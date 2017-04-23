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

public class PowerGridTest
{
    private PowerGrid powerGrid;
    private HashSet<PowerRelated> gridHashSet;

    [SetUp]
    public void Init()
    {
        powerGrid = new PowerGrid();
        Type powerGridType = typeof(PowerGrid);
        FieldInfo field = powerGridType.GetField("powerGrid", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field);
        gridHashSet = field.GetValue(powerGrid) as HashSet<PowerRelated>;
        Assert.IsNotNull(gridHashSet);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PlugInArgumentNullException()
    {
        powerGrid.PlugIn(null);
    }

    [Test]
    public void PlugInTest()
    {
        powerGrid.PlugIn(new PowerRelated());
        Assert.AreEqual(1, gridHashSet.Count);
    }

    [Test]
    public void IsPluggedInTest()
    {
        PowerRelated powerRelated = new PowerRelated();
        powerGrid.PlugIn(powerRelated);
        Assert.AreEqual(1, gridHashSet.Count);
        Assert.IsTrue(powerGrid.IsPluggedIn(powerRelated));
    }

    [Test]
    public void UnplugTest()
    {
        PowerRelated powerRelated = new PowerRelated();
        powerGrid.PlugIn(powerRelated);
        Assert.AreEqual(1, gridHashSet.Count);
        powerGrid.Unplug(powerRelated);
        Assert.AreEqual(0, gridHashSet.Count);
    }

    [Test]
    public void UpdateEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 50.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        powerGrid.PlugIn(powerProducer);
        powerGrid.PlugIn(firstPowerConsumer);
        Assert.AreEqual(2, gridHashSet.Count);
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
    }

    [Test]
    public void UpdateNotEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 50.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        PowerRelated secondPowerConsumer = new PowerRelated { InputRate = 30.0f };
        powerGrid.PlugIn(powerProducer);
        powerGrid.PlugIn(firstPowerConsumer);
        powerGrid.PlugIn(secondPowerConsumer);
        Assert.AreEqual(3, gridHashSet.Count);
        powerGrid.Update();
        Assert.IsFalse(powerGrid.IsOperating);
    }

    [Test]
    public void ChargeAccumulatorsTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 50.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        PowerRelated accumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        powerGrid.PlugIn(powerProducer);
        powerGrid.PlugIn(firstPowerConsumer);
        powerGrid.PlugIn(accumulator);
        Assert.AreEqual(3, gridHashSet.Count);
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(10.0f));
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(20.0f));
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(accumulator.AccumulatedPower.AreEqual(30.0f));
    }

    [Test]
    public void DischargeAccumulatorsEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 30.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        PowerRelated firstAccumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        PowerRelated secondAccumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        PowerRelated thirdAccumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        powerGrid.PlugIn(powerProducer);
        powerGrid.PlugIn(firstAccumulator);
        powerGrid.PlugIn(secondAccumulator);
        powerGrid.PlugIn(thirdAccumulator);
        Assert.AreEqual(4, gridHashSet.Count);

        powerGrid.Update();
        powerGrid.Update();
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        powerGrid.PlugIn(firstPowerConsumer);
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(30.0f));

        powerGrid.Unplug(powerProducer);
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(20.0f));
        Assert.IsTrue(thirdAccumulator.AccumulatedPower.AreEqual(20.0f));

        powerGrid.Update();
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.IsEmpty);
        Assert.IsTrue(secondAccumulator.IsEmpty);
        Assert.IsTrue(thirdAccumulator.IsEmpty);

        powerGrid.Update();
        Assert.IsFalse(powerGrid.IsOperating);
    }

    [Test]
    public void DischargeAccumulatorsNotEnoughPowerTest()
    {
        PowerRelated powerProducer = new PowerRelated { OutputRate = 30.0f };
        PowerRelated firstPowerConsumer = new PowerRelated { InputRate = 30.0f };
        PowerRelated firstAccumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        PowerRelated secondAccumulator = new PowerRelated { InputRate = 10.0f, OutputRate = 10.0f, Capacity = 100.0f };
        powerGrid.PlugIn(powerProducer);
        powerGrid.PlugIn(firstAccumulator);
        powerGrid.PlugIn(secondAccumulator);
        Assert.AreEqual(3, gridHashSet.Count);

        powerGrid.Update();
        powerGrid.Update();
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        powerGrid.PlugIn(firstPowerConsumer);
        powerGrid.Update();
        Assert.IsTrue(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));

        powerGrid.Unplug(powerProducer);
        powerGrid.Update();
        Assert.IsFalse(powerGrid.IsOperating);
        Assert.IsTrue(firstAccumulator.AccumulatedPower.AreEqual(30.0f));
        Assert.IsTrue(secondAccumulator.AccumulatedPower.AreEqual(30.0f));
    }
}
