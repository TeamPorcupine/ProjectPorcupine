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

namespace DeveloperConsole.CommandTypes
{
    [MoonSharpUserData]
    public sealed class InvokeCommand : CommandBase, ICommandInvoke
    {
        /// <summary>
        /// Standard with title and a method.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="functionName"> The command to execute.</param>
        public InvokeCommand(string title, string functionName) : this()
        {
            this.Title = title;
            this.FunctionName = functionName;
        }

        /// <summary>
        /// Standard with title, method, and help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="functionName"> The command to execute.</param>
        /// <param name="descriptiveText"> The help text to display.</param>
        public InvokeCommand(string title, string functionName, string descriptiveText) : this(title, functionName)
        {
            this.DescriptiveText = descriptiveText;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpFunctionName"> The help method to execute.</param>
        public InvokeCommand(string title, string functionName, string descriptiveText, string helpFunctionName, string parameters) : this(title, functionName, descriptiveText)
        {
            HelpFunctionName = helpFunctionName;
            Parameters = parameters;
        }

        private InvokeCommand()
        {
            HelpMethod = delegate
            {
                if (HelpFunctionName == string.Empty)
                {
                    DevConsole.Log("<color=yellow>Command Info:</color> " + ((DescriptiveText == string.Empty) ? " < color=red>There's no help for this command</color>" : DescriptiveText));
                    return;
                }

                try
                {
                    FunctionsManager.DevConsole.CallWithError(HelpFunctionName);
                }
                catch (Exception e)
                {
                    DevConsole.LogError(e.Message);
                }
            };
        }

        public string FunctionName
        {
            get; private set;
        }

        public string HelpFunctionName
        {
            get; private set;
        }

        public override string Parameters
        {
            get; protected set;
        }

        public void ExecuteCommand(string arguments)
        {
            try
            {
                FunctionsManager.DevConsole.CallWithError(FunctionName, ParseArguments(arguments));
            }
            catch (Exception e)
            {
                DevConsole.LogError(e.Message);
            }
        }

        protected override object[] ParseArguments(string arguments)
        {
            try
            {
                object[] args = RegexToStandardPattern(arguments);
                return args;
            }
            catch (Exception e)
            {
                Debug.ULogErrorChannel("DevConsole", e.ToString());
            }

            return new object[] { };
        }
    }
}