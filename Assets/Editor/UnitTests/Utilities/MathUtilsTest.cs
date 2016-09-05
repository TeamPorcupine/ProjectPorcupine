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

public class MathUtilsTest 
{
    /* 
     * ==================================
     * MathUtilities.AreEqual() ,tested on floats and doubles
     * ==================================
     */

    // inputs one varible into AreEqual as both inputs
    [Test]
    public void AreEqual_AgainstSelf()
    {
        double test_double = 0.999;
        Assert.True(MathUtilities.AreEqual(test_double, test_double));

        float test_float = 0.999f;
        Assert.True(MathUtilities.AreEqual(test_float, test_float));
    }

    // inputs a varible and a copy into AreEqual as  inputs
    [Test]
    public void AreEqual_AgainstCopy()
    {
        double test_double_a = 9.999;
        double test_double_b = test_double_a;
        Assert.True(MathUtilities.AreEqual(test_double_a, test_double_b));

        float test_float_a = 9.999f;
        float test_float_b = test_float_a;
        Assert.True(MathUtilities.AreEqual(test_float_a, test_float_b));
    }

    // inputs two varibles of the same value into AreEqual as inputs
    [Test]
    public void AreEqual_AgainstSameValue()
    {
        double test_double_a = 1.0001;
        double test_double_b = 1.0001;
        Assert.True(MathUtilities.AreEqual(test_double_a, test_double_b));

        float test_float_a = 1.0001f;
        float test_float_b = 1.0001f;
        Assert.True(MathUtilities.AreEqual(test_float_a, test_float_b));
    }

    // inputs two varibles , where the first is smaller than the second, into AreEqual as inputs
    [Test]
    public void AreEqual_AgainstLargerValue()
    {
        double test_double_a = 0.12345;
        double test_double_b = 2.98765;
        Assert.False(MathUtilities.AreEqual(test_double_a, test_double_b));

        float test_float_a = 0.12345f;
        float test_float_b = 2.98765f;
        Assert.False(MathUtilities.AreEqual(test_float_a, test_float_b));
    }

    // inputs two varibles , where the first is larger than the second, into AreEqual as inputs
    [Test]
    public void AreEqual_AgainstSmallerValue()
    {
        double test_double_a = 3.1415;
        double test_double_b = 0.1415;
        Assert.False(MathUtilities.AreEqual(test_double_a, test_double_b));

        float test_float_a = 3.1415f;
        float test_float_b = 0.1415f;
        Assert.False(MathUtilities.AreEqual(test_float_a, test_float_b));
    }

    /*
     * =================================
     * IsZero
     * =================================
     */

    // passes int, float and double zero's into IsZero
    [Test]
    public void IsZero()
    {
        Assert.True(MathUtilities.IsZero(0));
        Assert.True(MathUtilities.IsZero(0.0));
        Assert.True(MathUtilities.IsZero(0f));
    }

    // passes non zero values into IsZero
    [Test]
    public void IsZero_NotZero()
    {
        Assert.False(MathUtilities.IsZero(1.0));
        Assert.False(MathUtilities.IsZero(1f));
    
        Assert.False(MathUtilities.IsZero(-1.0));
        Assert.False(MathUtilities.IsZero(-1f));

        Assert.False(MathUtilities.IsZero(double.Epsilon));
        Assert.False(MathUtilities.IsZero(float.Epsilon));
    }

    /*
     * =================================
     * Clamp
     * =================================
     */

    // tests passing a value that is between min and max
    [Test]
    public void Clamp_InRange()
    {
        Assert.AreEqual(0, MathUtilities.Clamp(0, -1, 1));

        Assert.AreEqual(0.1, MathUtilities.Clamp(0.1, -1.1, 1.1));

        Assert.AreEqual(0.05f, MathUtilities.Clamp(0.05f, 0f, 0.1f));

        Assert.AreEqual('m', MathUtilities.Clamp('m', 'a', 'z'));

        Assert.AreEqual("test", MathUtilities.Clamp("test", "aaaa", "zzzz"));
    }

    // tests passing in min
    [Test]
    public void Clamp_Min()
    {
        Assert.AreEqual(-1, MathUtilities.Clamp(-1, -1, 1));

        Assert.AreEqual(-1.1, MathUtilities.Clamp(-1.1, -1.1, 1.1));

        Assert.AreEqual(0f, MathUtilities.Clamp(0f, 0f, 0.1f));

        Assert.AreEqual('a', MathUtilities.Clamp('a', 'a', 'z'));

        Assert.AreEqual("aaaa", MathUtilities.Clamp("aaaa", "aaaa", "zzzz"));
    }

    // tests passing in max
    [Test]
    public void Clamp_Max()
    {
        Assert.AreEqual(1, MathUtilities.Clamp(1, -1, 1));

        Assert.AreEqual(1.1, MathUtilities.Clamp(1.1, -1.1, 1.1));

        Assert.AreEqual(0.1f, MathUtilities.Clamp(0.1f, 0f, 0.1f));

        Assert.AreEqual('z', MathUtilities.Clamp('z', 'a', 'z'));

        Assert.AreEqual("zzzz", MathUtilities.Clamp("zzzz", "aaaa", "zzzz"));
    }

    // tests passing in a value less than min
    [Test]
    public void Clamp_Lower()
    {
        Assert.AreEqual(-1, MathUtilities.Clamp(-100, -1, 1));

        Assert.AreEqual(1.1, MathUtilities.Clamp(1.1, -1.1, 1.1));

        Assert.AreEqual(0f, MathUtilities.Clamp(-100f, 0f, 0.1f));

        Assert.AreEqual('b', MathUtilities.Clamp('a', 'b', 'z'));

        Assert.AreEqual("abcd", MathUtilities.Clamp("aaaa", "abcd", "zzzz"));
    }

    // tests passing in a value more than max
    [Test]
    public void Clamp_Higher()
    {
        Assert.AreEqual(1, MathUtilities.Clamp(100, -1, 1));

        Assert.AreEqual(1.1, MathUtilities.Clamp(99.9, -1.1, 1.1));

        Assert.AreEqual(0.1f, MathUtilities.Clamp(0.5f, 0f, 0.1f));

        Assert.AreEqual('y', MathUtilities.Clamp('z', 'a', 'y'));

        Assert.AreEqual("yyyy", MathUtilities.Clamp("zzzz", "aaaa", "yyyy"));
    }
}
