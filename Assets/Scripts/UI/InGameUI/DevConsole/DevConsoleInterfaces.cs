#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

namespace DeveloperConsole.Interfaces
{
    public interface ICommandHelpMethod
    {
        HelpMethod HelpMethod
        {
            get;
        }
    }

    public interface ICommandDescription
    {
        string DescriptiveText
        {
            get;
        }

        /// <summary>
        /// Should return all the parameter types (C# format so Int16/Int32 so on...) instead of names
        /// Also should have a ',' between each type.
        /// </summary>
        string Parameters
        {
            get;
        }

        /// <summary>
        /// The title/name for the command.
        /// </summary>
        string Title
        {
            get;
        }
    }

    public interface ICommandCSharp : ICommandRunnable
    {
        /// <summary>
        /// The method to be called.
        /// </summary>
        Delegate Method
        {
            get;
        }
    }

    public interface ICommandRunnable
    {
        /// <summary>
        /// Execute the method.
        /// </summary>
        /// <param name="arguments"> Arguments to parse.</param>
        void ExecuteCommand(string arguments);
    }

    public interface ICommandLUA : ICommandRunnable
    {
        string FunctionName
        {
            get;
        }
    }
}