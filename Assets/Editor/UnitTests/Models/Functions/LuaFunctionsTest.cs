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
        // Try loading a good Lua Code
        bool result = functions.LoadScript(testCode1, "testCode1");
        Assert.AreEqual(true, result);
    }

    [Test]
    [ExpectedException(typeof(System.NullReferenceException))]
    public void Test_LoadScript_Null()
    {
        // Try loading a Lua Code from a null text
        functions.LoadScript(null, string.Empty);
    }

    [Test]
    public void Test_LoadScript_Empty()
    {
        // Try loading a Lua Code from an empty text. Everything should be ok, but there won't be functions
        bool result = functions.LoadScript(string.Empty, string.Empty);
        Assert.AreEqual(true, result);
    }

    [Test]
    public void Test_LoadScript_BadLua_NoEnd()
    {
        // Try loading bad Lua Code. The Lua Interpreter will send an exception and the script won't be loaded
        bool result = functions.LoadScript(testCode2, "testCode2");
        Assert.AreEqual(false, result);
    }

    [Test]
    public void Test_CallFunction()
    {
        // Test a function that dosent return anything (void c# , nil/nan Lua)
        functions.LoadScript(testCode1, "testCode1");
        DynValue value = functions.Call("test_func0");
        Assert.AreEqual(true, value.IsNilOrNan());
    }

    [Test]
    public void Test_CallFunction_ReturnString()
    {
        // Test a function that returns a string
        functions.LoadScript(testCode1, "testCode1");
        DynValue value = functions.Call("test_func1");
        Assert.AreEqual("test_func1_returns", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputString_ReturnInput()
    {
        // Test a function that returns the String passed to it
        functions.LoadScript(testCode1, "testCode1");
        DynValue value = functions.Call("test_func2", "inputted value");
        Assert.AreEqual("inputted value", value.CastToString());
    }

    [Test]
    public void Test_CallFunction_InputInts_ReturnSum()
    {
        // Test passing more than one input
        functions.LoadScript(testCode1, "testCode1");
        DynValue value = functions.Call("test_func3", 4, 7);
        Assert.AreEqual(11, (int)value.CastToNumber());
    }

    // TODO: unit tests for LuaFunctions.RegisterGlobal
    // TODO: unit tests for LuaFunctions.CallWithInstance
}
