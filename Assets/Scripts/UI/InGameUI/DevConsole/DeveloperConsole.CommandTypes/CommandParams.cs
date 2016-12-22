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
        public CommandParams(string title, ConsoleMethod method, string helpText, string defaultValue = "") : base(title, method, helpText, defaultValue)
        {
        }

        public CommandParams(string title, ConsoleMethod method, HelpMethod helpMethod, string defaultValue = "") : base(title, method, helpMethod, defaultValue)
        {
        }

        public CommandParams(string title, ConsoleMethod method, string descriptiveText, string[] tags) : this(title, method, descriptiveText)
        {
            this.Tags = tags;
        }

        public CommandParams(string title, ConsoleMethod method, HelpMethod helpMethod, string[] tags) : this(title, method, helpMethod)
        {
            this.Tags = tags;
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