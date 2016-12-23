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
    /// A core class for Internal Commands.
    /// </summary>
    [MoonSharpUserData]
    public class InternalCommand : CommandBase, ICommandInternal
    {
        /// <summary>
        /// Standard with title, method, and help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpText"> The help text to display.</param>
        public InternalCommand(string title, Method method, string descriptiveText, Type[] typeInfo = null, string[] parameterNames = null, string defaultValue = "")
        {
            this.Title = title;
            this.Method = method;
            this.DescriptiveText = descriptiveText;
            this.DefaultValue = defaultValue;
            this.TypeInfo = typeInfo;
            this.ParameterNames = parameterNames;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpMethod"> The help method to execute.</param>
        public InternalCommand(string title, Method method, HelpMethod helpMethod, Type[] typeInfo = null, string[] parameterNames = null, string defaultValue = "") : this(title, method, defaultValue)
        {
            this.HelpMethod = helpMethod;
        }

        /// <summary>
        /// Standard with title, method, and help text.
        /// </summary>
        /// <param name="title"> The title for the command.</param>
        /// <param name="method"> The command to execute.</param>
        /// <param name="helpText"> The help text to display.</param>
        /// <param name="tags"> Just tags for help function. </param>
        public InternalCommand(string title, Method method, string descriptiveText, string[] tags, Type[] typeInfo = null, string[] parameterNames = null, string defaultValue = "") : this(title, method, descriptiveText, typeInfo, parameterNames, defaultValue)
        {
            this.Tags = tags;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text.
        /// </summary>
        /// <param name="title"> The title for the command. </param>
        /// <param name="method"> The command to execute. </param>
        /// <param name="helpMethod"> The help method to execute. </param>
        /// <param name="tags"> Just tags for help function. </param>
        public InternalCommand(string title, Method method, HelpMethod helpMethod, string[] tags, Type[] typeInfo = null, string[] parameterNames = null, string defaultValue = "") : this(title, method, helpMethod, typeInfo, parameterNames, defaultValue)
        {
            this.Tags = tags;
        }

        /// <summary>
        /// Get all the parameters for this function.
        /// </summary>
        /// <returns> a string of all the parameters with a comma between them.</returns>
        public override string Parameters
        {
            get
            {
                string list = string.Empty;

                if (TypeInfo == null && ParameterNames != null)
                {
                    list = string.Join(", ", ParameterNames);
                }
                else if (TypeInfo != null && ParameterNames == null)
                {
                    list = string.Join(", ", TypeInfo.Select(x => x.Name).ToArray());
                }
                else if (TypeInfo == null && ParameterNames == null)
                {
                    return list;
                }
                else
                {
                    for (int i = 0; i < Math.Max(TypeInfo.Length, ParameterNames.Length); i++)
                    {
                        if (TypeInfo.Length >= i)
                        {
                            list += " " + TypeInfo[i].Name;
                        }

                        if (ParameterNames.Length >= i)
                        {
                            list += " " + ParameterNames[i];
                        }

                        if (i + 1 < Math.Max(TypeInfo.Length, ParameterNames.Length))
                        {
                            list += ",";
                        }
                    }
                }

                // Trim that first space
                return list.TrimStart();
            }

            protected set
            {
                return;
            }
        }

        public Method Method { get; protected set; }

        public Type[] TypeInfo { get; protected set; }

        public string[] ParameterNames { get; protected set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="arguments"> Passes the arguments.</param>
        public void ExecuteCommand(string arguments)
        {
            try
            {
                Method(ParseArguments(arguments));
            }
            catch (Exception e)
            {
                DevConsole.LogError(Errors.UnknownError(this));
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
            }
        }

        protected override object[] ParseArguments(string args)
        {
            object[] convertedArgs = new object[] { };

            try
            {
                string[] arguments = RegexToStandardPattern(args);
                convertedArgs = new object[arguments.Length];

                // If TypeInfo null then no new parameters to pass (we'll we will pass an array of strings, which could be empty)
                if (TypeInfo == null)
                {
                    return arguments;
                }

                for (int i = 0; i < TypeInfo.Length; i++)
                {
                    // Guard to make sure we don't actually go overboard
                    if (arguments.Length > i)
                    {
                        // This just wraps then unwraps, works quite fast actually, since its a easy wrap/unwrap.
                        convertedArgs[i] = GetValueType<object>(arguments[i], TypeInfo[i]);
                    }
                    else
                    {
                        // No point running through the rest, 
                        // this means 'technically' you could have 100 parameters at the end (not tested)
                        // However, that may break for other reasons
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
            }

            return convertedArgs;
        }
    }
}