#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Text.RegularExpressions;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;
using System.Linq;

using Microsoft.CSharp;
using System.CodeDom.Compiler;

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

            // Parse the parameters
            // We are using regex since we only want a very specific part of the parameters
            // This is relatively fast, (actually quite fast compared to other methods and is in start)
            // Something like text: String, value: Int, on: Bool
            string regexExpression = @"\s*.*?\:(.*?)\s*(?:\,|$)";

            // This will just get the types
            string[] parameterTypes = Regex.Matches(parameters, regexExpression)
                .Cast<Match>()
                .Where(m => m.Groups.Count >= 2 && m.Groups[1].Value != string.Empty)
                .Select(m => m.Groups[1].Value.Trim())
                .ToArray();

            Type[] types = new Type[parameterTypes.Length];

            // Okay now we want to split it at the ':' and get second array
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i] == string.Empty)
                {
                    types[i] = typeof(object);
                }
                else
                {
                    // This is just to have a safety, that may work in some cases??  Better than nothing I guess
                    // Could try to remove, but in most cases won't be part of DevConsole, till you open, or it starts.
                    try
                    {
                        types[i] = GetFriendlyType(parameterTypes[i]);
                    }
                    catch (Exception e)
                    {
                        types[i] = typeof(object);
                        Debug.ULogErrorChannel("DevConsole", e.Message);
                    }
                }
            }

            // Assign to 'global'
            Types = types;
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

        public Type[] Types
        {
            get; private set;
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

        // This should work for all types
        public static Type GetFriendlyType(string friendlyName)
        {
            // Do a first convert then try other
            Type tryType = Type.GetType(friendlyName + ", UnityEngine", false, true);

            Debug.LogWarning(tryType);
            if (tryType != null)
            {
                return tryType;
            }

            // From http://stackoverflow.com/questions/16984005/convert-c-friendly-type-name-to-actual-type-int-typeofint
            // Really annoying way to do it
            var provider = new CSharpCodeProvider();

            var pars = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            string code = "public class TypeFullNameGetter"
                        + "{"
                        + "     public override string ToString()"
                        + "     {"
                        + "         return typeof(" + friendlyName.ToLower() + ").FullName;"
                        + "     }"
                        + "}";

            var comp = provider.CompileAssemblyFromSource(pars, new[] { code });

            if (comp.Errors.Count > 0)
            {
                foreach (CompilerError error in comp.Errors)
                {
                    Debug.LogWarning(error.ErrorText);
                }

                return null;
            }

            object fullNameGetter = comp.CompiledAssembly.CreateInstance("TypeFullNameGetter");
            string fullName = fullNameGetter.ToString();
            return Type.GetType(fullName);
        }

        protected override object[] ParseArguments(string arguments)
        {
            try
            {
                string[] args = RegexToStandardPattern(arguments);
                object[] convertedArgs = new object[args.Length];

                // Time to use args and the type pattern
                if (Types.Length == 0)
                {
                    return args;
                }
                else
                {
                    for (int i = 0; i < Types.Length; i++)
                    {
                        // Guard to make sure we don't actually go overboard
                        if (args.Length > i)
                        {
                            // Now we can do a convert
                            // THIS WORKS FOR SOME BIZARRE REASON
                            // Probably due to nice objects, but I'm scared to optimise since it works hehe
                            convertedArgs[i] = GetValueType<object>(args[i], Types[i]);
                        }
                        else
                        {
                            // No point running through the rest, 
                            // this means technically you could have 100 parameters at the end
                            // However, that may break for other reasons
                            break;
                        }
                    }

                    // Isn't this a nice method :D
                    return convertedArgs;
                }
            }
            catch (Exception e)
            {
                Debug.ULogErrorChannel("DevConsole", e.ToString());
            }

            return new object[] { };
        }
    }
}