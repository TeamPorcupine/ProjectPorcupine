#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using MoonSharp.Interpreter;
using NUnit.Framework;
using UnityEngine;

[MoonSharpUserData]
public class LuaUtilsTest 
{
    // The test code
    private string testCode1 = @"
        function test_func0()
            return
        end

        function test_func1()
            return 'test_func1_returns'
        end

        function test_func2(input)
            return input
        end
        
        function test_func3(inputa, inputb)
            return inputa + inputb
        end
        ";

    // Test Code with missing 'end' i.e. bad lua
    private string testCode2 = @"
        function test_func2()
            return
        
        "; 

    // File paths where our test code will be saved
    private string testCode1Path = Application.streamingAssetsPath + "/LuaUtilsTest1.lua";
    private string testCode2Path = Application.streamingAssetsPath + "/LuaUtilsTest2.lua";

    [SetUp]
    public void Init()
    {
        // Write both test scripts to file.
        System.IO.File.WriteAllText(testCode1Path, testCode1);
        System.IO.File.WriteAllText(testCode2Path, testCode2);
    }

    [Test]
    public void Test_LoadScript()
    {
        // Load our good lua file, shouldn't get any errors and shouldnt raise any expetions 
        LuaUtilities.LoadScriptFromFile(testCode1Path);
    }

    [Test]
    [ExpectedException(typeof(System.IO.FileNotFoundException))]
    public void Test_LoadScript_NoFile()
    {
        // Try loading some file that dosent exist , expect to get an exeption about no file found
        LuaUtilities.LoadScriptFromFile(testCode1Path + "DoesNotExist.txt");
    }

    [Test]
    [ExpectedException(typeof(System.ArgumentNullException))]
    public void Test_LoadScript_Null()
    {
        // Try loading a file from the path null
        LuaUtilities.LoadScriptFromFile(null);
    }

    [Test]
    public void Test_LoadScript_BadLua_NoEnd()
    {
        // FIXME: use mocking to verify whats going on
        // Bad Lua Code is caught in side the method from this level we don't know if an error happened
        LuaUtilities.LoadScriptFromFile(testCode2Path);
    }

    [Test]
    public void Test_CallFunction()
    {
        // Test a function that dosent return anything (void c# , nil/nan Lua)
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func0");
        Assert.AreEqual(true, value.IsNilOrNan());
    }

    [Test]
    public void Test_CallFunction_ReturnString()
    {
        // Test a function that returns a string
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func1");
        Assert.AreEqual("test_func1_returns", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputString_ReturnInput()
    {
        // Test a function that returns the String passed to it
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func2", "inputted value");
        Assert.AreEqual("inputted value", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputInts_ReturnSum()
    {
        // Test passing more than one input
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func3", 4, 7);
        Assert.AreEqual(11, (int)value.CastToNumber());
    }

    // TODO: unit tests for LuaUtilities.RegisterGlobal
    [TearDown]
    public void DeInit()
    {
        // remove the two files we saved with our test code in
        System.IO.File.Delete(testCode1Path);
        System.IO.File.Delete(testCode2Path);
    }
}
