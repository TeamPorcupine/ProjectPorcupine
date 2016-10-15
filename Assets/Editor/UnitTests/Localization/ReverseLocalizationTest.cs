#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using NUnit.Framework;
using ProjectPorcupine.Localization;

public class ReverseLocalizationTest
{
    [Test]
    public void ReverseText()
    {
        string testString = "testing";
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, "gnitset");
    }

    [Test]
    public void ReverseTextWithParameter()
    {
        string testString = "I am {0} yrs old";
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, "dlo sry {0} ma I");
    }

    [Test]
    public void ReverseTextWithMultipleParameters()
    {
        string testString = "{0} + {1} equals {2}";
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, "{2} slauqe {1} + {0}");
    }

    [Test]
    public void ReverseTextWithMultipleDigitParameter()
    {
        string testString = "I am {10} yrs old";
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, "dlo sry {10} ma I");
    }

    [Test]
    public void ReverseNullText()
    {
        string testString = null;
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, null);
    }

    [Test]
    public void ReverseEmptyText()
    {
        string testString = string.Empty;
        string result = LocalizationTable.ReverseString(testString);
        Assert.AreEqual(result, string.Empty);
    } 
}
