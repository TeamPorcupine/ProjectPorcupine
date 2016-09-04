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
     * Compare an object against it's self
     * Objects should be considered duplicates and so return value will be based of equalValueAtEnd
     * ==================================
     */

    [Test]
    public void AreEqual_AgainstSelf()
    {
        double test_int = 0.999;

        Assert.True(MathUtilities.AreEqual(test_int, test_int));
    }
    [Test]
    public void AreEqual_AgainstCopy()
    {
        double test_int_a = 9.999;
        double test_int_b = test_int_a;

        Assert.True(MathUtilities.AreEqual(test_int_a, test_int_b));
    }
    [Test]
    public void AreEqual_AgainstSameValue()
    {
        double test_int_a = 1.0001;
        double test_int_b = 1.0001;

        Assert.True(MathUtilities.AreEqual(test_int_a, test_int_b));
    }
    [Test]
    public void AreEqual_AgainstLargerValue()
    {
        double test_int_a = 0.12345;
        double test_int_b = 2.98765;

        Assert.False(MathUtilities.AreEqual(test_int_a, test_int_b));
    }

    [Test]
    public void AreEqual_AgainstSmallerValue()
    {
        double test_int_a = 3.1415;
        double test_int_b = 0.1415;

        Assert.False(MathUtilities.AreEqual(test_int_a, test_int_b));
    }
}
