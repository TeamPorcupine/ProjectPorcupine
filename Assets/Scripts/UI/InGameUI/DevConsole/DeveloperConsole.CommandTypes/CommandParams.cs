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

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// 2 parameter command.
    /// </summary>
    [MoonSharpUserData]
    public class CommandParams : CSharpCommand
    {
        public CommandParams(string title, ConsoleMethod method) : base(title, method)
        {
        }

        public CommandParams(string title, ConsoleMethod method, string helpText) : base(title, method, helpText)
        {
        }

        public CommandParams(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod)
        {
        }

        public CommandParams(ConsoleMethod method) : base(method)
        {
        }

        public CommandParams(ConsoleMethod method, string helpText) : base(method, helpText)
        {
        }

        public CommandParams(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod)
        {
        }

        public delegate void ConsoleMethod(params object[] parameters);

        protected override object[] ParseArguments(string message)
        {
            try
            {
                string[] args = RegexToStandardPattern(message);
                return args;
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
                return new object[] { };
            }
        }
    }
}