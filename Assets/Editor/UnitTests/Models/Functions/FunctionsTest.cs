#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

public class FunctionsTest
{
    private CSharpFunctions csharpFunctions;
    private LuaFunctions luaFunctions;

    private string testCSharp = @"
    public static class FurnitureFunctions
    {
        public static string PowerCellPress_StatusInfo(Furniture furniture)
        {     
            float perc = 0f;    

            return string.Format('Status: {0:0}%', perc);
        }
    }
    ";

    // The test code
    private string testLUA = @"
        function PowerGenerator_FuelInfo(furniture)
   
            local perc = 0	

            return 'Status: ' .. string.format('%.1f', perc) .. '%'
        end
    ";

    [SetUp]
    public void Init()
    {
        csharpFunctions = new CSharpFunctions();
        luaFunctions = new LuaFunctions();
    }

    [Test]
    public void CompareLUAvsCSharp()
    {
        bool resultCSharp = csharpFunctions.LoadScript(ReplaceQuotes(testCSharp), "FurnitureFunctions");
        bool resultLua = luaFunctions.LoadScript(ReplaceQuotes(testLUA), "PowerGen");

        Assert.IsTrue(resultCSharp);
        Assert.IsTrue(resultLua);

        int iterations = 1000;

        List<string> cache = new List<string>(iterations * 2);

        Stopwatch sw1 = new Stopwatch();
        sw1.Start();
        for (int i = 0; i < iterations; i++)
        {
            cache.Add(csharpFunctions.Call<string>("PowerCellPress_StatusInfo", new Furniture()));
        }

        sw1.Stop();

        Stopwatch sw2 = new Stopwatch();
        sw2.Start();
        for (int i = 0; i < iterations; i++)
        {
            cache.Add(luaFunctions.Call<string>("PowerGenerator_FuelInfo", new Furniture()));
        }

        sw2.Stop();

        UnityDebugger.Debugger.Log(string.Format("Iterations: {0}", cache.Count / 2));
        UnityDebugger.Debugger.Log(string.Format("CSharp calls: {0} ms", sw1.ElapsedMilliseconds));
        UnityDebugger.Debugger.Log(string.Format("LUA calls: {0} ms", sw2.ElapsedMilliseconds));
    }

    private string ReplaceQuotes(string text)
    {
        return text.Replace("'", "\"");
    }
}
