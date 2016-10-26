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
    /// Passes everything after the ':' (cuts out the ending ":" though if it exists).
    /// </summary>
    [MoonSharpUserData]
    public class StringCommand : CoreCommand
    {
        public StringCommand(string title, ConsoleMethod method) : base(title, method)
        {
        }

        public StringCommand(string title, ConsoleMethod method, string helpText) : base(title, method, helpText)
        {
        }

        public StringCommand(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod)
        {
        }

        public StringCommand(ConsoleMethod method) : base(method)
        {
        }

        public StringCommand(ConsoleMethod method, string helpText) : base(method, helpText)
        {
        }

        public StringCommand(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod)
        {
        }

        public delegate void ConsoleMethod(string arg);

        protected override object[] ParseArguments(string message)
        {
            try
            {
                return new object[] { message.Trim() };
            }
            catch (Exception e)
            {
                Debug.ULogErrorChannel("DevConsole", e.ToString());
                return new object[] { };
            }
        }
    }
}