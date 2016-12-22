#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Linq;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// A core class for CSharp Commands.
    /// </summary>
    [MoonSharpUserData]
    public class CSharpCommand : CommandBase, ICommandBase
    {
        /// <summary>
        /// Standard with title and a method.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        public CSharpCommand(string title, Delegate method)
        {
            this.Title = title;
            this.Method = method;
        }

        /// <summary>
        /// Standard with title, method, and help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpText"> The help text to display.</param>
        public CSharpCommand(string title, Delegate method, string descriptiveText) : this(title, method)
        {
            this.DescriptiveText = descriptiveText;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpMethod"> The help method to execute.</param>
        public CSharpCommand(string title, Delegate method, HelpMethod helpMethod) : this(title, method)
        {
            this.HelpMethod = helpMethod;
        }

        /// <summary>
        /// Uses reflection to get title.
        /// </summary>
        /// <param name="method"> The command to execute.</param>
        public CSharpCommand(Delegate method) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method)
        {
        }

        /// <summary>
        /// Uses reflection to get title then passes the helpMethod.
        /// </summary>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpText"> The help text to display.</param>
        public CSharpCommand(Delegate method, string descriptiveText) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, descriptiveText)
        {
        }

        /// <summary>
        /// Uses reflection to get title then passes the delegate for helpmethod.
        /// </summary>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpMethod"> The help method to execute.</param>
        public CSharpCommand(Delegate method, HelpMethod helpMethod) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, helpMethod)
        {
        }

        /// <summary>
        /// Get all the parameters for this function.
        /// </summary>
        /// <returns> a string of all the parameters with a comma between them.</returns>
        public override string Parameters
        {
            get
            {
                return string.Join(", ", Method.Method.GetParameters().Select(x => x.Name + ": " + x.ParameterType.Name).ToArray());
            }

            protected set
            {
                return;
            }
        }

        public Delegate Method { get; protected set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="arguments"> Passes the arguments.</param>
        public void ExecuteCommand(string arguments)
        {
            try
            {
                Method.Method.Invoke(Method.Target, ParseArguments(arguments));
            }
            catch (Exception e)
            {
                // Debug Error
                DevConsole.LogError(Errors.ExecuteConsoleError.Description(this));
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
            }
        }

        protected override object[] ParseArguments(string args)
        {
            return new object[] { };
        }
    }
}