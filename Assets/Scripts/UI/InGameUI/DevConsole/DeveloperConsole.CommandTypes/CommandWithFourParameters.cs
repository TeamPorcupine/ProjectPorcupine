#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;
using UnityEngine;

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// 3 parameter command.
    /// </summary>
    [MoonSharpUserData]
    public class Command<T0, T1, T2, T3> : CSharpCommand
    {
        public Command(string title, ConsoleMethod method) : base(title, method)
        {
        }

        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText)
        {
        }

        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod)
        {
        }

        public Command(ConsoleMethod method) : base(method)
        {
        }

        public Command(ConsoleMethod method, string helpText) : base(method, helpText)
        {
        }

        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod)
        {
        }

        public delegate void ConsoleMethod(T0 arg0, T1 arg1, T2 arg2, T3 arg3);

        protected override object[] ParseArguments(string message)
        {
            try
            {
                string[] args = RegexToStandardPattern(message);
                if (args.Length == 4)
                {
                    return new object[] { GetValueType<T0>(args[0]), GetValueType<T1>(args[1]), GetValueType<T2>(args[2]), GetValueType<T3>(args[3]) };
                }
                else
                {
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description(this));
                    throw new Exception("Command Missing Parameter");
                }
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
                return new object[] { };
            }
        }
    }
}