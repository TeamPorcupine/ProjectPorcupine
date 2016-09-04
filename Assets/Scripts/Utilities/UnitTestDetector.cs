#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Reflection;

/// <summary>
/// Detect if we are running as part of a unit test.
/// This is DIRTY and should only be used if absolutely necessary 
/// as its usually a sign of bad design.
/// </summary>    
public static class UnitTestDetector
{
    private static bool runningFromNUnit;

    static UnitTestDetector()
    {
        foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assem.FullName.ToLowerInvariant().StartsWith("nunit.framework"))
            {
                runningFromNUnit = true;
                break;
            }
        }
    }

    public static bool IsRunningFromUnitTest
    {
        get { return runningFromNUnit; }
    }
}