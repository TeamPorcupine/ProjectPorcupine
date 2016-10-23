using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;

namespace DeveloperConsole.CommandTypes
{
    /*
        void ExecuteCommand(string arguments)
        {
            try
            {
                method.Method.Invoke(method.Target, ParseArguments(arguments));
            }
            catch (Exception e)
            {
                // Debug Error
                DevConsole.LogError(Errors.ExecuteConsoleError.Description(this));
                throw e;
            }
        }
    */

    /// <summary>
    /// A command base that all commands derive from
    /// </summary> 
    [MoonSharpUserData]
    public class CommandBase : ICommandDescription, ICommandHelpMethod
    {
        public string descriptiveText
        {
            get; protected set;
        }

        public virtual string parameters
        {
            get; protected set;
        }

        public string title
        {
            get; protected set;
        }

        public HelpMethod helpMethod
        {
            get; protected set;
        }

        /// <summary>
        /// Parse the arguments
        /// </summary>
        /// <param name="arguments"> Arguments to parse </param>
        /// <returns> the parsed arguments </returns>
        protected virtual object[] ParseArguments(string arguments)
        {
            return new object[] { };
        }

        /// <summary>
        /// Splits at character then trims ends and start
        /// </summary>
        /// <param name="arguments"> The string to split and trim </param>
        /// <param name="atCharacter"> What character to split at </param>
        /// <returns></returns>
        protected string[] SplitAndTrim(string arguments, char atCharacter = ',')
        {
            List<string> args = new List<string>();

            foreach (string arg in arguments.Split(atCharacter))
            {
                args.Add(arg.Trim());
            }

            return args.ToArray();
        }

        /// <summary>
        /// Get the value type of the argument
        /// </summary>
        /// <typeparam name="T"> the type of the argument </typeparam>
        /// <param name="arg"> the argument to find the value type </param>
        /// <returns> The type of the argument given </returns>
        /// <exception cref="Exception"> Throws exception if arg is not type T, SHOULD BE CAUGHT by command&ltT0...&gt </exception>
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
                DevConsole.LogError(Errors.TypeConsoleError.Description(commandBase: this));
                throw e;
            }
        }

        /// <summary>
        /// Converts the value to a boolean via Int, Bool, and String Parsers
        /// </summary>
        /// <param name="value"> The value to convert </param>
        /// <param name="result"> The resulting boolean </param>
        /// <returns> True if the conversion was successful </returns>
        protected bool ValueToBool(string value, out bool result)
        {
            bool boolResult = (result = false);
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
                    result = ((intResult == 1) ? true : false);
                }
                else
                    return false;
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

    [MoonSharpUserData]
    public sealed class LUACommand : CommandBase, ICommandLUA
    {
        public string functionName
        {
            get; private set;
        }

        public string helpFunctionName
        {
            get; private set;
        }

        public void runLUAHelp()
        {
            try
            {
                FunctionsManager.DevConsole.Call_Unsafe(helpFunctionName);
            }
            catch (Exception e)
            {
                DevConsole.LogError(e.Message);
            }
        }

        public void ExecuteCommand(string arguments)
        {
            try
            {
                FunctionsManager.DevConsole.Call_Unsafe(functionName, ParseArguments(arguments));
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
                string[] args = SplitAndTrim(arguments);
                return args;
            }
            catch (Exception e)
            {
                Debug.ULogErrorChannel("DevConsole", e.ToString());
            }

            return new object[] { };
        }

        public LUACommand()
        {
            helpMethod = runLUAHelp;
        }

        /// <summary>
        /// Standard with title and a method
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="functionName"> The command to execute </param>
        public LUACommand(string title, string functionName) : this()
        {
            this.title = title;
            this.functionName = functionName;
        }

        /// <summary>
        /// Standard with title, method, and help text
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="functionName"> The command to execute </param>
        /// <param name="descriptiveText"> The help text to display </param>
        public LUACommand(string title, string functionName, string descriptiveText) : this(title, functionName)
        {
            this.descriptiveText = descriptiveText;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpFunctionName"> The help method to execute </param>
        //
        public LUACommand(string title, string functionName, string descriptiveText, string helpFunctionName, string parameters) : this(title, functionName, descriptiveText)
        {
            this.helpFunctionName = helpFunctionName;
            this.parameters = parameters;
        }
    }

    /// <summary>
    /// A core command for the core code, its in CSharp
    /// </summary>
    [MoonSharpUserData]
    public abstract class CoreCommand : CommandBase, ICommandCSharp
    {
        /// <summary>
        /// The method to call
        /// </summary>
        public Delegate method { get; private set; }

        /// <summary>
        /// Get all the parameters for this function
        /// </summary>
        /// <returns> a string of all the parameters with a comma between them </returns>
        public override string parameters
        {
            get
            {
                return string.Join(", ", method.Method.GetParameters().Select(x => x.ParameterType.Name).ToArray());
            }
        }

        /// <summary>
        /// Standard with title and a method
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        public CoreCommand(string title, Delegate method)
        {
            this.title = title;
            this.method = method;
        }

        /// <summary>
        /// Standard with title, method, and help text
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpText"> The help text to display </param>
        public CoreCommand(string title, Delegate method, string descriptiveText) : this(title, method)
        {
            this.descriptiveText = descriptiveText;
        }

        /// <summary>
        /// Standard but uses a delegate method for help text
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpMethod"> The help method to execute </param>
        //
        public CoreCommand(string title, Delegate method, HelpMethod helpMethod) : this(title, method)
        {
            this.helpMethod = helpMethod;
        }

        /// <summary>
        /// Uses reflection to get title
        /// </summary>
        /// <param name="method"> The command to execute </param>
        public CoreCommand(Delegate method) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method) { }

        /// <summary>
        /// Uses reflection to get title then passes the helpMethod
        /// </summary>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpText"> The help text to display </param>
        public CoreCommand(Delegate method, string descriptiveText) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, descriptiveText) { }

        /// <summary>
        /// Uses reflection to get title then passes the delegate for helpmethod
        /// </summary>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpMethod"> The help method to execute </param>
        public CoreCommand(Delegate method, HelpMethod helpMethod) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, helpMethod) { }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments"> Passes the arguments </param>
        public void ExecuteCommand(string arguments)
        {
            try
            {
                method.Method.Invoke(method.Target, ParseArguments(arguments));
            }
            catch (Exception e)
            {
                // Debug Error
                DevConsole.LogError(Errors.ExecuteConsoleError.Description((ICommandDescription)this));
                Debug.ULogErrorChannel("DevConsole", e.ToString());
            }
        }
    }
}
