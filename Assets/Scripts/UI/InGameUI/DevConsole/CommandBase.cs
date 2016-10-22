using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using DeveloperConsole.Interfaces;

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
    public abstract class CommandBase
    {
        /// <summary>
        /// Parse the arguments
        /// </summary>
        /// <param name="arguments"> Arguments to parse </param>
        /// <returns> the parsed arguments </returns>
        protected abstract object[] ParseArguments(string arguments);

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

    public abstract class LUACommand : CommandBase, ICommandDescription, ICommandHelpMethod, ICommandRunnable, ICommandLUA
    {
        public string descriptiveText
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string functionName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HelpMethod helpMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string parameters
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string title
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void ExecuteCommand(string arguments)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A core command for the core code, its in CSharp
    /// </summary>
    public abstract class CoreCommand : CommandBase, ICommandDescription, ICommandRunnable, ICommandCSharp, ICommandHelpMethod
    {
        /// <summary>
        /// Text that describes this command
        /// </summary>
        public string descriptiveText { get; private set; }

        /// <summary>
        /// The method to call
        /// </summary>
        public Delegate method { get; private set; }

        /// <summary>
        /// The title of command
        /// </summary>
        public string title { get; private set; }

        /// <summary>
        /// Get all the parameters for this function
        /// </summary>
        /// <returns> a string of all the parameters with a comma between them </returns>
        public string parameters
        {
            get
            {
                return string.Join(", ", method.Method.GetParameters().Select(x => x.ParameterType.Name).ToArray());
            }
        }

        /// <summary>
        /// To call to display help
        /// </summary>
        public HelpMethod helpMethod
        {
            get; private set;
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
                DevConsole.LogError(Errors.ExecuteConsoleError.Description(command: (ICommandDescription)this));
                throw e;
            }
        }
    }
}
