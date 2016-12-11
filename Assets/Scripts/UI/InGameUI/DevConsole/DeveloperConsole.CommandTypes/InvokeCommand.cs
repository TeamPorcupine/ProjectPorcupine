#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

using DeveloperConsole.Interfaces;
using Microsoft.CSharp;
using MoonSharp.Interpreter;

namespace DeveloperConsole.CommandTypes
{
    /// <summary>
    /// Invoke some code from either C# Function Manager or LUA Function Manager.
    /// </summary>
    [MoonSharpUserData]
    public sealed class InvokeCommand : CommandBase, ICommandInvoke
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="title"> The title of the command. </param>
        /// <param name="functionName"> The function name of the command (can be C# or LUA). </param>
        /// <param name="descriptiveText"> The text that describes this command. </param>
        /// <param name="helpFunctionName"> The help function name of the command (can be C# or LUA). </param>
        /// <param name="parameters"> The parameters ( should be invokeable format of (using [any character];)? type variableName, ... ). </param>
        public InvokeCommand(string title, string functionName, string descriptiveText, string helpFunctionName, string parameters)
        {
            Title = title;
            FunctionName = functionName;
            DescriptiveText = descriptiveText;
            HelpFunctionName = helpFunctionName;

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

            // If the parameters contains a ';' then it'll exclude the 'using' statement.
            // Just makes the declaration help look nicer.
            if (parameters.Contains(';'))
            {
                int indexOfSemiColon = parameters.IndexOf(';') + 1;

                if (parameters.Length > indexOfSemiColon)
                {
                    Parameters = parameters.Substring(indexOfSemiColon).Trim();
                }
                else
                {
                    // Something weird happened here so we are just setting it to ''
                    // This will only happen if the semi colon is the last element in the string
                    Parameters = string.Empty;

                    UnityDebugger.Debugger.LogWarning("DevConsole", "Parameters had a semicolon as a last character this is an illegal string.");
                }
            }
            else
            {
                // No ';' exists so free to just copy it over
                // We can't just do a substring cause it'll return an error and this isn't safely done
                Parameters = parameters;
            }

            // Parse the parameters
            // We are using regex since we only want a very specific part of the parameters
            // This is relatively fast, (actually quite fast compared to other methods and is in start)
            // Something like text: String, value: Int, on: Bool
            // Old Regex: \s*(.*?\;)?.*?\:(.*?)\s*(?:\,|$)
            // Koosemoose's Regex: /using\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)/
            // My Adjustments to Koosemoose's Regex (optimises for groups not needed): (using\s+[^\s]+)?\s*([^\s]+)\s+[^\s]+
            // Note: Regex would be faster than using a for loop, cause it would require a lot of splits, and other heavily costing operations.
            string regexExpression = @"(using\s+[^\s]+)?\s*([^\s]+)\s+[^\s]+";

            // This will just get the types
            string[] parameterTypes = Regex.Matches(parameters, regexExpression)
                .Cast<Match>()
                .Where(m => m.Groups.Count >= 2 && m.Groups[2].Value != string.Empty)
                .Select(m => (m.Groups[1].Value != string.Empty ? m.Groups[1].Value.Trim() : "using System;") + m.Groups[2].Value.Trim())
                .ToArray();

            Type[] types = new Type[parameterTypes.Length];

            // Now we just cycle through and get types
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i] == string.Empty)
                {
                    types[i] = typeof(object);
                }
                else
                {
                    // This is just to have a safety, that may trigger in some cases??  Better than nothing I guess
                    // Could try to remove, but in most cases won't be part of DevConsole, till you open, or it starts.
                    try
                    {
                        // We just split, since its a decently appropriate solution.
                        string[] parameterSections = parameterTypes[i].Split(';');

                        types[i] = GetFriendlyType(parameterSections[1], parameterSections[0]);
                    }
                    catch (Exception e)
                    {
                        // This means invalid type, so we set it to object.
                        // This in most cases is fine, just means that when you call it, 
                        // it won't work (unless the type is object)
                        types[i] = typeof(object);
                        UnityDebugger.Debugger.LogError("DevConsole", e.Message);
                    }
                }
            }

            // Assign array
            Types = types;
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

        /// <summary>
        /// This returns the type from a friendly name (like Int instead of System.Int32).
        /// Note: it can also accept non-friendly names and they will work too.
        /// </summary>
        /// <param name="friendlyName"> The friendly name of the type. </param>
        /// <param name="namespaces"> The namespace in which this type exists. </param>
        /// <returns></returns>
        public Type GetFriendlyType(string friendlyName, string namespaces)
        {
            // This first bit was made up by me,
            // just to make the whole process a little faster (this should run in 99% of cases)
            // This is a HUGE optimisation, and should work 99% of the time.
            // It speeds up this process by more than 4x (even though this is done in start this helps)
            Type tryType = Type.GetType(friendlyName + ", " + namespaces, false, true);

            if (tryType != null)
            {
                return tryType;
            }

            // From http://stackoverflow.com/questions/16984005/convert-c-friendly-type-name-to-actual-type-int-typeofint
            // Really annoying way to do it
            // We could maybe cache multiple together????
            var provider = new CSharpCodeProvider();

            var pars = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            // It just compiles some code then runs it
            // Yes, its very ugly (and slow) but it was the only way I could allow friendly/types in namespaces
            // That don't conform to the optimisation above
            string code =
                namespaces + ";\n"
                + @" public class TypeFullNameGetter {public override string ToString(){"
                + "return typeof(" + friendlyName.ToLower() + ").FullName;"
                + " }}";

            var comp = provider.CompileAssemblyFromSource(pars, new[] { code });

            if (comp.Errors.Count > 0)
            {
                // We don't care about the errors (they entered the type wrong)
                return null;
            }

            object fullNameGetter = comp.CompiledAssembly.CreateInstance("TypeFullNameGetter");
            string fullName = fullNameGetter.ToString();
            return Type.GetType(fullName);
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
                string[] args = RegexToStandardPattern(arguments);
                object[] convertedArgs = new object[args.Length];

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
                            // This just wraps then unwraps, works quite fast actually, since its a easy wrap/unwrap.
                            convertedArgs[i] = GetValueType<object>(args[i], Types[i]);
                        }
                        else
                        {
                            // No point running through the rest, 
                            // this means 'technically' you could have 100 parameters at the end (not tested)
                            // However, that may break for other reasons
                            break;
                        }
                    }

                    return convertedArgs;
                }
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogError("DevConsole", e.ToString());
            }

            return new object[] { };
        }
    }
}