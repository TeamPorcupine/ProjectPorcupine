using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DeveloperConsole.CommandTypes
{
    public abstract class CommandBase
    {
        /// <summary>
        /// The title/name of the command
        /// </summary>
        public string title
        {
            get; private set;
        }

        /// <summary>
        /// The help text to display
        /// </summary>
        public string helpText
        {
            get; private set;
        }

        public string getParameters()
        {
            return string.Join(", ", method.Method.GetParameters().Select(x => x.ParameterType.Name).ToArray());
        }

        /// <summary>
        /// The help method to call (instead of displaying help text)
        /// </summary>
        public delegate void HelpMethod();

        HelpMethod helpMethod;
        Delegate method;

        /// <summary>
        /// Standard with title and a method
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        public CommandBase(string title, Delegate method)
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
        public CommandBase(string title, Delegate method, string helpText) : this(title, method)
        {
            this.helpText = helpText;
        }
        /// <summary>
        /// Standard but uses a delegate method for help text
        /// </summary>
        /// <param name="title"> The title for the command </param>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpMethod"> The help method to execute </param>
        //
        public CommandBase(string title, Delegate method, HelpMethod helpMethod) : this(title, method)
        {
            this.helpMethod = helpMethod;
        }

        /// <summary>
        /// Uses reflection to get title
        /// </summary>
        /// <param name="method"> The command to execute </param>
        public CommandBase(Delegate method) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method) { }
        /// <summary>
        /// Uses reflection to get title then passes the helpMethod
        /// </summary>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpText"> The help text to display </param>
        public CommandBase(Delegate method, string helpText) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, helpText) { }
        /// <summary>
        /// Uses reflection to get title then passes the delegate for helpmethod
        /// </summary>
        /// <param name="method"> The command to execute </param>
        /// <param name="helpMethod"> The help method to execute </param>
        public CommandBase(Delegate method, HelpMethod helpMethod) : this(method.Method.DeclaringType.Name + "." + method.Method.Name, method, helpMethod) { }

        /// <summary>
        /// Execute the method
        /// </summary>
        /// <param name="arguments"> Arguments to parse </param>
        public void ExecuteCommand(string arguments)
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

        /// <summary>
        /// Parse the arguments
        /// </summary>
        /// <param name="arguments"> Arguments to parse </param>
        /// <returns> the parsed arguments </returns>
        protected abstract object[] ParseArguments(string arguments);

        /// <summary>
        /// Execute help method/show help text
        /// </summary>
        public virtual void ShowHelp()
        {
            if (helpMethod != null)
            {
                helpMethod();
            }
            else
            {
                DevConsole.Log("<color=blue>Command Info:</color> " + (helpText == null ? "<color=red>There's no help for this command</color>" : helpText));
            }
        }

        #region converters

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public string[] SplitAndTrim(string arguments, char atCharacter = ',')
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
                DevConsole.LogError(Errors.TypeConsoleError.Description(this));
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

        #endregion
    }
}
