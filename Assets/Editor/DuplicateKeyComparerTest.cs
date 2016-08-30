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
     * Objects should be considered duplicates and so return value will be based of equalValueAtEnd
     * ==================================
     */

    [Test]
    public void Compare_AgainstSelf()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.Greater(dkc_int.Compare(test_int, test_int), 0);
    }

    [Test]
    public void Compare_AgainstSelf_equalValueAtEnd_False()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.Greater(dkc_int.Compare(test_int, test_int), 0);
    }

    [Test]
    public void Compare_AgainstSelf_equalValueAtEnd_True()
    {
        // test on int
        int test_int = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.Less(dkc_int.Compare(test_int, test_int), 0);
    }

    /*
     * ==================================
     * Compare an object against a copy of it's self
     * Objects should be considered duplicates and so return value will be based of equalValueAtEnd
     * ==================================
     */

    [Test]
    public void Compare_AgainstCopy()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstCopy_equalValueAtEnd_False()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstCopy_equalValueAtEnd_True()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = test_int_a;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.Less(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    /*
     * ==================================
     * Compare an object against another object with the same value
     * Objects should be considered duplicates and so return value will be based of equalValueAtEnd
     * ==================================
     */

    [Test]
    public void Compare_AgainstSameValue()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstSameValue_equalValueAtEnd_False()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstSameValue_equalValueAtEnd_True()
    {
        // test on int
        int test_int_a = 1;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.Less(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    /*
     * ==================================
     * Compare an object against another object with a larger value
     * Objects should --NOT-- be considered duplicates and so return value should be based on which is larger
     * ==================================
     */

    [Test]
    public void Compare_AgainstLargerValue()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.Less(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstLargerValue_equalValueAtEnd_Flase()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.Less(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstLargerValue_equalValueAtEnd_True()
    {
        // test on two int's with a < b
        int test_int_a = 1;
        int test_int_b = 2;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.Less(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    /*
     * ==================================
     * Compare an object against another object with a smaller value
     * Objects should --NOT-- be considered duplicates and so return value should be based on which is larger
     * ==================================
     */

    [Test]
    public void Compare_AgainstSmallerValue()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>();
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstSmallerValue_equalValueAtEnd_False()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(false);
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    [Test]
    public void Compare_AgainstSmallerValue_equalValueAtEnd_True()
    {
        // test on two int's with a < b
        int test_int_a = 2;
        int test_int_b = 1;

        DuplicateKeyComparer<int> dkc_int = new DuplicateKeyComparer<int>(true);
        Assert.Greater(dkc_int.Compare(test_int_a, test_int_b), 0);
    }

    /*
     * ==================================
     * null against null
     * Expect a null refrence exeption
     * ==================================
     */

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void Compare_NullAgainstNull()
    {
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>();
        dkc_int.Compare(null, null);
    }

    /*
     * ==================================
     * value against null
     * value should be considered grater than null so result should be positive
     * ==================================
     */

    [Test]
    public void Compare_ValueAgainstNull()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>();
        Assert.Greater(dkc_int.Compare(test_string, null), 0);
    }

    [Test]
    public void Compare_ValueAgainstNull_equalValueAtEnd_False()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>(false);
        Assert.Greater(dkc_int.Compare(test_string, null), 0);
    }

    [Test]
    public void Compare_ValueAgainstNull_equalValueAtEnd_True()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>(true);
        Assert.Greater(dkc_int.Compare(test_string, null), 0);
    }

    /*
     * ==================================
     * null against value
     * Expect null refrence exeptions
     * ==================================
     */

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void Compare_NullAgainstValue()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>();
        dkc_int.Compare(null, test_string);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void Compare_NullAgainstValue_equalValueAtEnd_False()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>(false);
        dkc_int.Compare(null, test_string);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void Compare_NullAgainstValue_equalValueAtEnd_True()
    {
        string test_string = "test";
        DuplicateKeyComparer<string> dkc_int = new DuplicateKeyComparer<string>(true);
        dkc_int.Compare(null, test_string);
    }
}
