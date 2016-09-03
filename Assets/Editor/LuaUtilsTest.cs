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
using UnityEngine;

public class LuaUtilsTest 
{
    private string testCode1 = @"
        function test_func0()
            return
        end

        function test_func1()
            return 'test_func1_returns'
        end

        ";

    private string testCode2 = @"
        function test_func2()
            return
        
        "; 

    private string testCode1Path = Application.streamingAssetsPath + "/LuaUtilsTest1.lua";
    private string testCode2Path = Application.streamingAssetsPath + "/LuaUtilsTest2.lua";

    [SetUp]
    public void Init()
    {
        System.IO.File.WriteAllText(testCode1Path, testCode1);
        System.IO.File.WriteAllText(testCode2Path, testCode2);
    }

    [Test]
    public void Test_LoadScript()
    {
        LuaUtilities.LoadScriptFromFile(testCode1Path);
    }

    [Test]
    [ExpectedException(typeof(System.IO.FileNotFoundException))]
    public void Test_LoadScript_NoFile()
    {
        LuaUtilities.LoadScriptFromFile(testCode1Path + "DoesNotExist.txt");
    }

    [Test]
    [ExpectedException(typeof(System.ArgumentNullException))]
    public void Test_LoadScript_Null()
    {
        LuaUtilities.LoadScriptFromFile(null);
    }

    [Test]
    public void Test_LoadScript_BadLua_NoEnd()
    {
        // bad Lua Code is caught in side the method
        // from this level we don't know if an error happened
        LuaUtilities.LoadScriptFromFile(testCode2Path);
    }

    [Test]
    public void Test_CallFunction()
    {
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func0");
        Assert.AreEqual(true, value.IsNilOrNan());
    }

    [Test]
    public void Test_CallFunction_ReturnString()
    {
        LuaUtilities.LoadScriptFromFile(testCode1Path);
        MoonSharp.Interpreter.DynValue value = LuaUtilities.CallFunction("test_func1");
        Assert.AreEqual("test_func1_returns", value.CastToString());
    }

    [TearDown]
    public void DeInit()
    {
        System.IO.File.Delete(testCode1Path);
        System.IO.File.Delete(testCode2Path);
    }
}
