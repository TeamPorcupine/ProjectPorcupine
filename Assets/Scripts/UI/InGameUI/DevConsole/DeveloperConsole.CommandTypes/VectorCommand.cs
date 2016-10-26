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
    /// A vector method for obtaining a vector from user input.
    /// </summary>
    /// <typeparam name="T"> Should be of type Vector2, 3, or 4.</typeparam>
    [MoonSharpUserData]
    public class VectorCommand<T> : CoreCommand
    {
        public VectorCommand(string title, ConsoleMethod method) : base(title, method)
        {
        }

        public VectorCommand(string title, ConsoleMethod method, string helpText) : base(title, method, helpText)
        {
        }

        public VectorCommand(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod)
        {
        }

        public VectorCommand(ConsoleMethod method) : base(method)
        {
        }

        public VectorCommand(ConsoleMethod method, string helpText) : base(method, helpText)
        {
        }

        public VectorCommand(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod)
        {
        }

        public delegate void ConsoleMethod(T vector);

        protected override object[] ParseArguments(string arguments)
        {
            string[] args = SplitAndTrim(arguments);

            switch (args.Length)
            {
                case 2:
                    try
                    {
                        Vector2 vector = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
                        return new object[] { vector };
                    }
                    catch
                    {
                        DevConsole.LogError(Errors.TypeConsoleError.Description((ICommandDescription)this));
                        Debug.ULogErrorChannel("DevConsole", "The entered value is not a valid Vector2 value");
                        return new object[] { };
                    }

                case 3:
                    try
                    {
                        Vector3 vector = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
                        return new object[] { vector };
                    }
                    catch
                    {
                        DevConsole.LogError(Errors.TypeConsoleError.Description((ICommandDescription)this));
                        Debug.ULogErrorChannel("DevConsole", "The entered value is not a valid Vector3 value");
                        return new object[] { };
                    }

                case 4:
                    try
                    {
                        Vector4 vector = new Vector4(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        return new object[] { vector };
                    }
                    catch
                    {
                        DevConsole.LogError(Errors.TypeConsoleError.Description((ICommandDescription)this));
                        Debug.ULogErrorChannel("DevConsole", "The entered value is not a valid Vector4 value");
                        return new object[] { };
                    }

                default:
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description((ICommandDescription)this));
                    Debug.ULogErrorChannel("DevConsole", "The entered value is not a valid Vector value");
                    return new object[] { };
            }
        }
    }
}