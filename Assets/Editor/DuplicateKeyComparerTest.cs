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

public class DuplicateKeyComparerTest 
{
    /* 
     * ==================================
     * Compare an object against it's self
     * Objects should be considered duplicates
     * ==================================
     */

    [Test]
    public void Compare_AgainstSelf()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.AreEqual(1, dkc_int.Compare(test_int, test_int));
    }

    [Test]
    public void Compare_AgainstSelf_equalValueAtEnd_False()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.AreEqual(1, dkc_int.Compare(test_int, test_int));
    }

    [Test]
    public void Compare_AgainstSelf_equalValueAtEnd_True()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.AreEqual(-1, dkc_int.Compare(test_int, test_int));
    }

    /*
     * ==================================
     * Compare an object against a copy of it's self
     * Objects should be considered duplicates
     * ==================================
     */

    [Test]
    public void Compare_AgainstCopy()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.AreEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstCopy_equalValueAtEnd_False()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.AreEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstCopy_equalValueAtEnd_True()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.AreEqual(-1, dkc_int.Compare(test_int_a, test_int_b));
    }

    /*
     * ==================================
     * Compare an object against another object with the same value
     * Objects should be considered duplicates
     * ==================================
     */

    [Test]
    public void Compare_AgainstSameValue()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.AreEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstSameValue_equalValueAtEnd_False()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.AreEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstSameValue_equalValueAtEnd_True()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.AreEqual(-1, dkc_int.Compare(test_int_a, test_int_b));
    }

    /*
     * ==================================
     * Compare an object against another object with a larger value
     * Objects should --NOT-- be considered duplicates
     * ==================================
     */

    [Test]
    public void Compare_AgainstLargerValue()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        Debug.Log("a = " + 1 + "\t b = " + 2);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.AreNotEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstLargerValue_equalValueAtEnd_Flase()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        Debug.Log("a = " + 1 + "\t b = " + 2);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.AreNotEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstLargerValue_equalValueAtEnd_True()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        Debug.Log("a = " + 1 + "\t b = " + 2);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.AreNotEqual(-1, dkc_int.Compare(test_int_a, test_int_b));
    }

    /*
     * ==================================
     * Compare an object against another object with a smaller value
     * Objects should --NOT-- be considered duplicates
     * ==================================
     */

    [Test]
    public void Compare_AgainstSmallerValue()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        Debug.Log("a = " + 2 + "\t b = " + 1);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.AreNotEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstSmallerValue_equalValueAtEnd_False()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        Debug.Log("a = " + 2 + "\t b = " + 1);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.AreNotEqual(1, dkc_int.Compare(test_int_a, test_int_b));
    }

    [Test]
    public void Compare_AgainstSmallerValue_equalValueAtEnd_True()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        Debug.Log("a = " + 2 + "\t b = " + 1);
        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.AreNotEqual(-1, dkc_int.Compare(test_int_a, test_int_b));
    }
}
