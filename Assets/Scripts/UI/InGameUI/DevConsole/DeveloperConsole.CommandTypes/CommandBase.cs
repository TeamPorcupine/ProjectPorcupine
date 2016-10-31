#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// A command base that all commands derive from.
    /// </summary> 
    [MoonSharpUserData]
    public class CommandBase
    {
        /// <summary>
        /// Text describing the command.
        /// </summary>
        public string DescriptiveText
        {
            get; protected set;
        }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public virtual string Parameters
        {
            get; protected set;
        }

        /// <summary>
        /// The title of the command.
        /// </summary>
        public string Title
        {
            get; protected set;
        }

        /// <summary>
        /// The help method to call.
        /// </summary>
        public HelpMethod HelpMethod
        {
            get; protected set;
        }

        /// <summary>
        /// Parse the arguments.
        /// </summary>
        /// <param name="arguments"> Arguments to parse.</param>
        /// <returns> The parsed arguments.</returns>
        protected virtual object[] ParseArguments(string arguments)
        {
            return new object[] { };
        }

        /// <summary>
        /// Splits at character then trims ends and start.
        /// </summary>
        /// <param name="arguments"> The string to split and trim.</param>
        /// <param name="atCharacter"> What character to split at.</param>
        protected string[] SplitAndTrim(string arguments, char character = ',')
        {
            List<string> args = new List<string>();

            foreach (string arg in arguments.Split(character))
            {
                args.Add(arg.Trim());
            }

            return args.ToArray();
        }

        /// <summary>
        /// Get the value type of the argument.
        /// </summary>
        /// <typeparam name="T"> the type of the argument.</typeparam>
        /// <param name="arg"> the argument to find the value type.</param>
        /// <returns> The type of the argument given.</returns>
        /// <exception cref="Exception"> Throws exception if arg is not type T, SHOULD BE CAUGHT by command&ltT0...&gt.</exception>
        protected T GetValueType<T>(string arg)
        {
            try
            {
                T returnValue;
                if (typeof(bool) == typeof(T))
                {
                    bool result;
                    if (ValueToBool(arg, out result))
                    {
                        returnValue = (T)(object)result;
                    }
                    else
                    {
                        throw new Exception("The entered value is not a valid " + typeof(T) + " value");
                    }
                }
                else
                {
                    returnValue = (T)Convert.ChangeType(arg, typeof(T));
                }

                return returnValue;
            }
            catch (Exception e)
            {
                DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                throw e;
            }
        }

        /// <summary>
        /// Converts the value to a boolean via Int, Bool, and String Parsers.
        /// </summary>
        /// <param name="value"> The value to convert.</param>
        /// <param name="result"> The resulting boolean.</param>
        /// <returns> True if the conversion was successful.</returns>
        protected bool ValueToBool(string value, out bool result)
        {
            bool boolResult = result = false;
            int intResult = 0;

            if (bool.TryParse(value, out boolResult))
            {
                // Try Bool Parser
                result = boolResult;
            }
            else if (int.TryParse(value, out intResult))
            {
                // Try Int Parser
                if (intResult == 1 || intResult == 0)
                {
                    result = (intResult == 1) ? true : false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Try String Parser
                string stringResult = value.ToLower().Trim();
                if (stringResult.Equals("yes") || stringResult.Equals("y"))
                {
                    result = true;
                }
                else if (stringResult.Equals("no") || stringResult.Equals("n"))
                {
                    result = false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}