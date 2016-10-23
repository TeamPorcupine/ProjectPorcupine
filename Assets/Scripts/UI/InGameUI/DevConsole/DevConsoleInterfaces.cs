using UnityEngine;
using System.Collections;
using MoonSharp;
using System;
using MoonSharp.Interpreter;

namespace DeveloperConsole
{
    public delegate void HelpMethod();
}

namespace DeveloperConsole.Interfaces
{
    public interface ICommandHelpMethod
    {
        HelpMethod helpMethod
        {
            get;
        }
    }

    public interface ICommandDescription
    {
        string descriptiveText
        {
            get;
        }

        /// <summary>
        /// Should return all the parameter types (C# format so Int16/Int32 so on...) instead of names
        /// Also should have a ',' between each type
        /// </summary>
        string parameters
        {
            get;
        }

        /// <summary>
        /// The title/name for the command
        /// </summary>
        string title
        {
            get;
        }
    }

    public interface ICommandCSharp : ICommandRunnable
    {
        /// <summary>
        /// The method to be called
        /// </summary>
        Delegate method
        {
            get;
        }
    }

    public interface ICommandRunnable
    {
        /// <summary>
        /// Execute the method
        /// </summary>
        /// <param name="arguments"> Arguments to parse </param>
        void ExecuteCommand(string arguments);
    }

    public interface ICommandLUA : ICommandRunnable
    {
        string functionName
        {
            get;
        }
    }
}
