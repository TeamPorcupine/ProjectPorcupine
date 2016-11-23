#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using MoonSharp.Interpreter;
using NUnit.Framework;

public class CSharpFunctionsTest
{
    private CSharpFunctions functions;

    // The test code
    private string testCode1 = @"
    public static class TestClass1
    {
        public static double test_func0()
        {
            return 42.0;
        }
    }
        ";

    private string testFurniture1 = @"
    public static class FurnitureFunctions
    {
        public static string PowerCellPress_StatusInfo(Furniture furniture)
        {     
            float perc = 0f;    

            return string.Format('Status: {0}%', perc);
        }
    }
    ";

    [SetUp]
    public void Init()
    {
        functions = new CSharpFunctions();
    }

    [Test]
    public void Test_LoadScript()
    {
        // Try loading a good Lua Code
        bool result = functions.LoadScript(testCode1, "TestClass1");
        Assert.AreEqual(true, result);
    }

    [Test]
    public void Test_CallFunction()
    {
        functions.LoadScript(testCode1, "TestClass1");
        double value = functions.Call<double>("test_func0");
        Assert.AreEqual(42.0, value);
    }

    [Test]
    public void Test_CallFunctionFurniture()
    {
        testFurniture1 = testFurniture1.Replace("'", "\"");
        functions.LoadScript(testFurniture1, "FurnitureFunctions");
        string value = functions.Call<string>("PowerCellPress_StatusInfo", new Furniture());
        Assert.IsTrue(value.Contains("Status"));
    }

    [Test]
    public void Test_CallFunctionDynValue()
    {
        functions.LoadScript(testCode1, "TestClass1");
        DynValue value = functions.Call("test_func0");
        Assert.AreEqual(42.0, value.Number);
    }
}