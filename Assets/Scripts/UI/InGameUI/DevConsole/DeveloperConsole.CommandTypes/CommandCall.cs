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
    /// 1 parameter command.
    /// </summary>
    [MoonSharpUserData]
    public class Command : CSharpCommand
    {
        public Command(string title, ConsoleMethod method, string helpText, string defaultValue = "") : base(title, method, helpText, defaultValue)
        {
        }

        public Command(string title, ConsoleMethod method, HelpMethod helpMethod, string defaultValue = "") : base(title, method, helpMethod, defaultValue)
        {
        }

        public Command(string title, ConsoleMethod method, string helpText, string[] tags) : this(title, method, helpText)
        {
            this.Tags = tags;
        }

        public Command(string title, ConsoleMethod method, HelpMethod helpMethod, string[] tags) : this(title, method, helpMethod)
        {
            this.Tags = tags;
        }

        public delegate void ConsoleMethod();

        protected override object[] ParseArguments(string args)
        {
            return new object[] { };
        }
    }
}