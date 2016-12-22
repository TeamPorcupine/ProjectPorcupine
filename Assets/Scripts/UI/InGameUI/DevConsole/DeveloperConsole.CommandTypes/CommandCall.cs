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

        public delegate void ConsoleMethod();

        protected override object[] ParseArguments(string args)
        {
            return new object[] { };
        }
    }
}