#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using NUnit.Framework;

public class ModUtilsTest 
{
    /*
     * ModUtils.Clamp01
     */

    // tests passing in a value between zero and one
    [Test]
    public void Test_Clamp01_InRange() 
    {
        float value_1 = 0.12345f;
        Assert.AreEqual(value_1, ModUtils.Clamp01(value_1));

        float value_2 = 0.9999f;
        Assert.AreEqual(value_2, ModUtils.Clamp01(value_2));

        float value_3 = float.Epsilon;
        Assert.AreEqual(value_3, ModUtils.Clamp01(value_3));

        float value_4 = 1f - float.Epsilon;
        Assert.AreEqual(value_4, ModUtils.Clamp01(value_4));
    }

    // tests passing in values that are out of range
    [Test]
    public void Test_Clamp01_OutOfRange() 
    {
        float value_1 = 1.0001f;
        float ans_1 = ModUtils.Clamp01(value_1);

        Assert.AreNotEqual(value_1, ans_1);
        Assert.GreaterOrEqual(ans_1, 0f);
        Assert.LessOrEqual(ans_1, 1f);

        float value_2 = -0.0001f;
        float ans_2 = ModUtils.Clamp01(value_2);

        Assert.AreNotEqual(value_2, ans_2);
        Assert.GreaterOrEqual(ans_2, 0f);
        Assert.LessOrEqual(ans_2, 1f);
    }

    /*
     * ModUtils.FloorToInt
     */

    [Test]
    public void Test_FloorToInt()
    {
        float value_1 = 1.0001f;
        int ans_1 = ModUtils.FloorToInt(value_1);

        Assert.AreEqual(1, ans_1);

        float value_2 = 99.999f;
        int ans_2 = ModUtils.FloorToInt(value_2);

        Assert.AreEqual(99, ans_2);

        float value_3 = 50f;
        int ans_3 = ModUtils.FloorToInt(value_3);

        Assert.AreEqual(50, ans_3);
    }

    /*
     * ModUtils.Round
     */

    [Test]
    public void Test_Round()
    {
        float value_1 = 1.000142f;
        float ans_1 = ModUtils.Round(value_1, 5);

        Assert.AreEqual(1.00014f, ans_1);

        float value_2 = 9.9998f;
        float ans_2 = ModUtils.Round(value_2, 2);

        Assert.AreEqual(10f, ans_2);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Test_Round_NegitiveDigits()
    {
        float value_2 = 9.9998f;
        ModUtils.Round(value_2, -2);
    }
}
