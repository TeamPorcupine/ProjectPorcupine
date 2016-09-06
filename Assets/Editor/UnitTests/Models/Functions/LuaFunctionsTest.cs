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
public class LuaFunctionsTest 
{
    private LuaFunctions functions;

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

    [SetUp]
    public void Init()
    {
        functions = new LuaFunctions();
    }

    [Test]
    public void Test_LoadScript()
    {
        // Load our good lua file, shouldn't get any errors and shouldnt raise any expetions 
        functions.LoadScriptFromText(testCode1, "testCode1");
    }

    [Test]
    [ExpectedException(typeof(System.ArgumentNullException))]
    public void Test_LoadScript_Null()
    {
        // Try loading a file from the path null
        functions.LoadScriptFromText(null, "");
    }

    [Test]
    public void Test_LoadScript_BadLua_NoEnd()
    {
        // FIXME: use mocking to verify whats going on
        // Bad Lua Code is caught in side the method from this level we don't know if an error happened
        functions.LoadScriptFromText(testCode2, "testCode2");
    }

    [Test]
    public void Test_CallFunction()
    {
        // Test a function that dosent return anything (void c# , nil/nan Lua)
        functions.LoadScriptFromText(testCode1, "testCode1");
        DynValue value = functions.Call("test_func0");
        Assert.AreEqual(true, value.IsNilOrNan());
    }

    [Test]
    public void Test_CallFunction_ReturnString()
    {
        // Test a function that returns a string
        functions.LoadScriptFromText(testCode1, "testCode1");
        DynValue value = functions.Call("test_func1");
        Assert.AreEqual("test_func1_returns", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputString_ReturnInput()
    {
        // Test a function that returns the String passed to it
        functions.LoadScriptFromText(testCode1, "testCode1");
        DynValue value = functions.Call("test_func2", "inputted value");
        Assert.AreEqual("inputted value", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputInts_ReturnSum()
    {
        // Test passing more than one input
        functions.LoadScriptFromText(testCode1, "testCode1");
        DynValue value = functions.Call("test_func3", 4, 7);
        Assert.AreEqual(11, (int)value.CastToNumber());
    }

    // TODO: unit tests for LuaFunctions.RegisterGlobal
}
