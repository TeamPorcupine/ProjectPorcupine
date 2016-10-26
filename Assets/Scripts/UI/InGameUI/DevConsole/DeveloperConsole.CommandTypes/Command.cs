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
    [MoonSharpUserData]
    public class Command : CoreCommand
    {
        #region StandardConstructors

        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        public Command(string title, ConsoleMethod method) : base(title, method)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        /// <param title="helpText"></param>
        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        /// <param title="helpMethod"></param>
        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod)
        {
        }

        #endregion

        #region FunctionConstructors

        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        public Command(ConsoleMethod method) : base(method)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        /// <param title="helpText"></param>
        public Command(ConsoleMethod method, string helpText) : base(method, helpText)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        /// <param title="helpMethod"></param>
        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod)
        {
        }

        #endregion

        /// <summary>
        /// Parameterless command.
        /// </summary>
        public delegate void ConsoleMethod();

        /// <summary>
        /// This type of command has no arguments and is just a function call.
        /// </summary>
        /// <param title="arguments"> Doesn't matter since it'll always return an empty array.</param>
        /// <returns> Always returns an empty array.</returns>
        protected override object[] ParseArguments(string arguments)
        {
            return new object[] { };
        }
    }
}