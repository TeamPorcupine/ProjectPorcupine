#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;
using NUnit.Framework;

public class HeadlineGeneratorTest
{
    private HeadlineGenerator gen;
    private bool stringPrinted;

    private string testHeadlineXml = @"
<Headlines>
    <Headline>The CEO of Quillcorp, has announced that it main and only shareholder is still the main and only shareholder, Quill18.</Headline>
    <Headline>Notice: Quillcorp has placed an embargo on ""Chairs"", any Quillcorp Basic Utility Station found in possession of this illegal contraband will be fined 2 million Quillbucks.</Headline>
</Headlines>";

    [SetUp]
    public void SetUp()
    {
        if (PrototypeManager.Headline == null)
        {
            new PrototypeManager();
        }

        PrototypeManager.Headline.LoadPrototypes(testHeadlineXml);

        gen = new HeadlineGenerator();
        gen.UpdatedHeadline += StringPrinted;
        stringPrinted = false;
    }

    public void StringPrinted(string headline)
    {
        UnityDebugger.Debugger.Log(headline);
        stringPrinted = true;
    }

    [Test]
    public void T01_1s_No_Generation()
    {
        stringPrinted = false;
        Scheduler.Scheduler.Current.Update(1);
        Assert.AreEqual(false, stringPrinted);
    }

    [Test]
    public void T02_100s_Generated()
    {
        stringPrinted = false;
        Scheduler.Scheduler.Current.Update(100);
        Assert.AreEqual(true, stringPrinted);
    }

    [Test]
    public void T03_Add_Instant_Generated()
    {
        stringPrinted = false;
        gen.AddHeadline("This is a test", true);
        Assert.AreEqual(true, stringPrinted);
    }

    [Test]
    public void T04_Add_Headline_False()
    {
        stringPrinted = false;
        gen.AddHeadline("This is a test", false);
        Assert.AreEqual(false, stringPrinted);
    }
}