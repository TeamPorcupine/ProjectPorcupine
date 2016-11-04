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
using System.Linq;
using System.Text.RegularExpressions;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;
using UnityEngine;

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
            return RegexToStandardPattern(arguments);
        }

        /// <summary>
        /// Regexs the character set properly, should always be called instead of you trying to do it yourself.
        /// </summary>
        /// <param name="arguments"> The string to split and trim.</param>
        /// <param name="atCharacter"> What character to split at.</param>
        protected string[] RegexToStandardPattern(string arguments)
        {
            // To kinds of arguments
            // The first is a simple argument such as '1' or 'A'
            // The second is a class argument such as [x, y, z] or [Bob hates Jim, and Jim hates Bob]
            // If we find a second then we don't break up the arguments within that second we pass it as a SINGLE argument, 
            // else we break them into multiple

            // E.G. Example1 ( 1, 2, [x, y, z] ) would return an array of [ 1, 2, [x, y, z] ]

            /*
                What we are saying is:

                [x, y, z] then normal

                so we match [...] + (,[...])* or NOT',' + (,NOT',')*

                [...] | NOT',' then check for (,[...]|NOT',')*
            */

            string pattern = @"\s*((?:\[.*?\])|(?:[^,]*))\s*";

            MatchCollection result = Regex.Matches(arguments, pattern);

            // Regex IS slower then a for loop, 
            // but in this case its better because it increases readabilty and we AREN'T focusing on speed
            // because this is a developer tool so having it run 100 ms slower isn't a problem that we care about
            return result
                .Cast<Match>()
                .Select(m => m.Value.Trim())
                .ToArray();
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
                    // I'm wanting a boolean
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
                else if (typeof(Vector2) == typeof(T))
                {
                    // I'm wanting a vector 2
                    Vector2 result;
                    if (ValueToVector2(arg, out result))
                    {
                        returnValue = (T)(object)result;
                    }
                    else
                    {
                        throw new Exception("The entered value is not a valid " + typeof(T) + " value");
                    }
                }
                else if (typeof(Vector3) == typeof(T))
                {
                    // I'm wanting a vector 3
                    Vector3 result;
                    if (ValueToVector3(arg, out result))
                    {
                        returnValue = (T)(object)result;
                    }
                    else
                    {
                        throw new Exception("The entered value is not a valid " + typeof(T) + " value");
                    }
                }
                else if (typeof(Vector4) == typeof(T))
                {
                    // I'm wanting a vector 4
                    Vector4 result;
                    if (ValueToVector4(arg, out result))
                    {
                        returnValue = (T)(object)result;
                    }
                    else
                    {
                        throw new Exception("The entered value is not a valid " + typeof(T) + " value");
                    }
                }
                else if (typeof(string) == typeof(T))
                {
                    // I'm wanting a string
                    return (T)(object)arg.Trim().TrimEnd(']').TrimStart('[');
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

        /// <summary>
        /// Converts the value to a vector via Int, and String Parsers, the string should be of type [3, 2, 1].
        /// </summary>
        /// <param name="value"> The value to convert.</param>
        /// <param name="result"> The resulting vector2.</param>
        /// <returns> True if the conversion was successful.</returns>
        protected bool ValueToVector2(string value, out Vector2 result)
        {
            // Value should be in format [x, y]
            // Therefore we can conclude that if it doesn't just return an empty vector

            // We are using regex because its reliable then using some loops
            string pattern = @"\[(\d+(?:\.\d+)?),(\d+(?:\.\d+)?)\]";

            // Because we don't care about spaces in vectors
            Match matches = Regex.Match(value.Replace(" ", string.Empty), pattern);

            if (matches.Groups.Count == 3)
            {
                // Matches what we want so do our magic
                result = new Vector2(float.Parse(matches.Groups[1].Value), float.Parse(matches.Groups[2].Value));

                // Return true
                return true;
            }

            result = new Vector2();

            // Else it doesn't so return false
            return false;
        }

        /// <summary>
        /// Converts the value to a vector via Int, and String Parsers, the string should be of type [3, 2, 1].
        /// </summary>
        /// <param name="value"> The value to convert.</param>
        /// <param name="result"> The resulting vector2.</param>
        /// <returns> True if the conversion was successful.</returns>
        protected bool ValueToVector3(string value, out Vector3 result)
        {
            // Value should be in format [x, y]
            // Therefore we can conclude that if it doesn't just return an empty vector

            // We are using regex because its reliable then using some loops
            string pattern = @"\[(\d+(?:\.\d+)?),(\d+(?:\.\d+)?),(\d+(?:\.\d+)?)\]";

            // We don't care about spaces
            Match matches = Regex.Match(value.Replace(" ", string.Empty), pattern);

            if (matches.Groups.Count == 4)
            {
                // Matches what we want so do our magic
                result = new Vector3(float.Parse(matches.Groups[1].Value), float.Parse(matches.Groups[2].Value), float.Parse(matches.Groups[3].Value));

                // Return true
                return true;
            }

            result = new Vector3();

            // Else it doesn't so return false
            return false;
        }

        /// <summary>
        /// Converts the value to a vector via Int, and String Parsers, the string should be of type [3, 2, 1].
        /// </summary>
        /// <param name="value"> The value to convert.</param>
        /// <param name="result"> The resulting vector2.</param>
        /// <returns> True if the conversion was successful.</returns>
        protected bool ValueToVector4(string value, out Vector4 result)
        {
            // Value should be in format [x, y]
            // Therefore we can conclude that if it doesn't just return an empty vector

            // We are using regex because its reliable then using some loops

            // Because we don't care about spaces in vectors
            string pattern = @"\[(\d+(?:\.\d+)?),(\d+(?:\.\d+)?),(\d+(?:\.\d+)?),(\d+(?:\.\d+)?)\]";

            Match matches = Regex.Match(value.Replace(" ", string.Empty), pattern);

            if (matches.Groups.Count == 5)
            {
                // Matches what we want so do our magic
                result = new Vector4(float.Parse(matches.Groups[1].Value), float.Parse(matches.Groups[2].Value), float.Parse(matches.Groups[3].Value), float.Parse(matches.Groups[4].Value));

                // Return true
                return true;
            }

            result = new Vector4();

            // Else it doesn't so return false
            return false;
        }
    }
}