#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using System.Xml;
using NUnit.Framework;

public class HeadlineGeneratorTest
{
    private HeadlineGenerator gen;
    private bool stringPrinted;

    [SetUp]
    public void SetUp()
    {
        string filePath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Headlines.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);
        XmlDocument doc = new XmlDocument();
        doc.Load(new StringReader(furnitureXmlText));
        gen = new HeadlineGenerator(doc.SelectSingleNode("Headlines"));
        gen.updatedHeadline+=StringPrinted;
        stringPrinted = false;
    }

    public void StringPrinted(string headline)
    {
        Debug.Log(headline);
        stringPrinted = true;
    }

    [Test]
    public void T01_1s_No_Generation()
    {
        stringPrinted = false;
        gen.Update(1);
        Assert.AreEqual(false, stringPrinted);
    }

    [Test]
    public void T02_10s_Generated()
    {
        stringPrinted = false;
        gen.Update(10);
        Assert.AreEqual(true, stringPrinted);
    }
}