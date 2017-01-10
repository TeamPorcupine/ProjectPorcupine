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
using System.Reflection;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using UnityEngine;

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// A command base that all commands derive from.
    /// </summary> 
    [MoonSharpUserData]
    public abstract class CommandBase
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
        public abstract string Parameters
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

        public string[] Tags
        {
            get; protected set;
        }

        public string DefaultValue
        {
            get; protected set;
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

            string mutableArgs = arguments;
            string constantsPattern = @"(?:\'(.*?)\')";

            mutableArgs = Regex.Replace(arguments, constantsPattern, MatchEval, RegexOptions.IgnoreCase);

            string pattern = @"\s*((?:\[.*?\])|(?:[^,]*))\s*";

            MatchCollection result = Regex.Matches(mutableArgs, pattern);

            return result
                .Cast<Match>()
                .Select(m => m.Value.Trim())
                .Where(m => m != string.Empty)
                .ToArray();
        }

        protected string MatchEval(Match match)
        {
            if (match.Groups.Count < 2)
            {
                return string.Empty;
            }

            World world;
            bool worldSuccess = ModUtils.GetCurrentWorld(out world);

            switch (match.Groups[1].Value.ToLower())
            {
                case "center":
                case "centre":
                    if (worldSuccess)
                    {
                        Tile t = world.GetCenterTile();
                        return "[" + t.X + ", " + t.Y + ", " + t.Z + "]";
                    }

                    break;
                case "mousePos":
                    Vector3 mousePos = Input.mousePosition;
                    return "[" + mousePos.x + ", " + mousePos.y + ", " + mousePos.z + "]";
                case "timeScale":
                    return (TimeManager.Instance != null) ? TimeManager.Instance.TimeScale.ToString() : string.Empty;
                case "pi":
                    return Mathf.PI.ToString();
                default:
                    DevConsole.LogWarning("You entered an constant identifier that doesn't exist?  Check spelling.");
                    break;
            }

            return string.Empty;
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
        /// Get the value type of the argument.
        /// </summary>
        /// <typeparam name="T"> The type of the argument. </typeparam>
        /// <param name="arg"> The argument to find the value type. </param>
        /// <returns> The type of the argument given. </returns>
        /// <exception cref="Exception"> Throws exception if arg is not type T, SHOULD BE CAUGHT by command. </exception>
        protected T GetValueType<T>(string arg, Type typeVariable = null)
        {
            Type typeOfT;

            if (typeVariable != null)
            {
                typeOfT = typeVariable;
            }
            else
            {
                typeOfT = typeof(T);
            }

            try
            {
                T returnValue;
                if (typeof(bool) == typeOfT)
                {
                    // I'm wanting a boolean
                    bool result;
                    if (ValueToBool(arg, out result))
                    {
                        returnValue = (T)(object)result;
                    }
                    else
                    {
                        throw new Exception("The entered value is not a valid " + typeOfT + " value");
                    }
                }
                else if (typeOfT == typeof(string))
                {
                    return (T)(object)arg.Trim('"');
                }
                else if (arg.Contains('['))
                {
                    arg = arg.Trim().Trim('[', ']');

                    string pattern = @"\,?((?:\"".*?\"")|(?:[^\,]*))\,?";

                    string[] args = Regex.Matches(arg, pattern)
                        .Cast<Match>()
                        .Where(m => m.Groups.Count >= 2 && m.Groups[1].Value != string.Empty)
                        .Select(m => m.Groups[1].Value.Trim().Trim('"'))
                        .ToArray();

                    // This is a list because then we can go parameters.Count to get the current 'non nil' parameters
                    List<object> parameters = new List<object>();

                    ConstructorInfo[] possibleConstructors = typeOfT.GetConstructors().Where(x => x.GetParameters().Length == args.Length).ToArray();
                    ConstructorInfo chosenConstructor = null;

                    for (int i = 0; i < possibleConstructors.Length; i++)
                    {
                        parameters = new List<object>();
                        ParameterInfo[] possibleParameters = possibleConstructors[i].GetParameters();

                        for (int j = 0; j < possibleParameters.Length; j++)
                        {
                            try
                            {
                                if (possibleParameters[j].ParameterType == typeof(string))
                                {
                                    parameters.Add(args[j]);
                                }
                                else
                                {
                                    parameters.Add(Convert.ChangeType(args[j], possibleParameters[j].ParameterType));
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }

                        if (parameters.Count == possibleParameters.Length)
                        {
                            // We have all our parameters
                            chosenConstructor = possibleConstructors[i];
                            break;
                        }
                    }

                    if (chosenConstructor == null)
                    {
                        throw new Exception("The entered value is not a valid " + typeOfT + " value");
                    }
                    else
                    {
                        returnValue = (T)chosenConstructor.Invoke(parameters.ToArray());
                    }
                }
                else
                {
                    returnValue = (T)Convert.ChangeType(arg, typeOfT);
                }

                return returnValue;
            }
            catch (Exception e)
            {
                DevConsole.LogError(Errors.ParametersNotInFormat(this));
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