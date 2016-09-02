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
   
    private string testCode = @"
        function test_func()
            return
        end
        ";
    private string testFilePath = Application.streamingAssetsPath + "/LuaUtilsTest.lua";

    [SetUp]
    public void Init()
    {
        System.IO.File.WriteAllText(testFilePath,testCode);
    }

    [Test]
    public void Test_LoadScript()
    {
        LuaUtilities.LoadScriptFromFile(testFilePath);
    }

    [Test]
    [ExpectedException(typeof(System.IO.FileNotFoundException))]
    public void Test_LoadScript_NoFile()
    {
        LuaUtilities.LoadScriptFromFile(testFilePath + "DoesNotExist.txt");
    }

    [Test]
    [ExpectedException(typeof(System.ArgumentNullException))]
    public void Test_LoadScript_Null()
    {
        LuaUtilities.LoadScriptFromFile(null);
    }

    [TearDown]
    public void DeInit()
    {
        System.IO.File.Delete(testFilePath);
    }

}
