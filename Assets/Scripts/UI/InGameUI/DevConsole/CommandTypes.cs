using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace DeveloperConsole.CommandTypes
{
    public class Command : CommandBase
    {
        /// <summary>
        /// Parameterless command
        /// </summary>
        public delegate void ConsoleMethod();

        #region StandardConstructors
        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        public Command(string title, ConsoleMethod method) : base(title, method) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        /// <param title="helpText"></param>
        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="title"></param>
        /// <param title="method"></param>
        /// <param title="helpMethod"></param>
        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod) { }

        #endregion

        #region FunctionConstructors
        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        public Command(ConsoleMethod method) : base(method) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        /// <param title="helpText"></param>
        public Command(ConsoleMethod method, string helpText) : base(method, helpText) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param title="method"></param>
        /// <param title="helpMethod"></param>
        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod) { }

        #endregion

        /// <summary>
        /// This type of command has no arguments and is just a function call.
        /// </summary>
        /// <param title="arguments"> doesn't matter since it'll always return an empty array </param>
        /// <returns> always returns an empty array </returns>
        protected override object[] ParseArguments(string arguments)
        {
            return new object[] { };
        }
    }

    /// <summary>
    /// 1 parameter command
    /// </summary>
    public class Command<T0> : CommandBase
    {

        public delegate void ConsoleMethod(T0 arg0);

        public Command(string title, ConsoleMethod method) : base(title, method) { }
        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText) { }
        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod) { }
        public Command(ConsoleMethod method) : base(method) { }
        public Command(ConsoleMethod method, string helpText) : base(method, helpText) { }
        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod) { }

        protected override object[] ParseArguments(string args)
        {
            try
            {
                if (args.Length > 0)
                {
                    return new object[] { GetValueType<T0>(args) };
                }
                else
                {
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description(this));
                    throw new Exception("Command Missing Parameter");
                }
            }
            catch (Exception e)
            {
                //DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                throw e;
            }
        }
    }

    /// <summary>
    /// 2 parameter command
    /// </summary>
    public class Command<T0, T1> : CommandBase
    {

        public delegate void ConsoleMethod(T0 arg0, T1 arg1);

        public Command(string title, ConsoleMethod method) : base(title, method) { }
        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText) { }
        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod) { }
        public Command(ConsoleMethod method) : base(method) { }
        public Command(ConsoleMethod method, string helpText) : base(method, helpText) { }
        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod) { }

        protected override object[] ParseArguments(string message)
        {
            try
            {
                string[] args = SplitAndTrim(message);
                if (args.Length == 3)
                {
                    return new object[] { GetValueType<T0>(args[0]), GetValueType<T1>(args[1]) };
                }
                else
                {
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description(this));
                    throw new Exception("Command Missing Parameter");
                }
            }
            catch (Exception e)
            {
                //DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                throw e;
            }
        }
    }

    /// <summary>
    /// 3 parameter command
    /// </summary>
    public class Command<T0, T1, T2> : CommandBase
    {
        public delegate void ConsoleMethod(T0 arg0, T1 arg1, T2 arg2);

        public Command(string title, ConsoleMethod method) : base(title, method) { }
        public Command(string title, ConsoleMethod method, string helpText) : base(title, method, helpText) { }
        public Command(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod) { }
        public Command(ConsoleMethod method) : base(method) { }
        public Command(ConsoleMethod method, string helpText) : base(method, helpText) { }
        public Command(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod) { }

        protected override object[] ParseArguments(string message)
        {
            try
            {
                string[] args = SplitAndTrim(message);
                if (args.Length == 3)
                {
                    return new object[] { GetValueType<T0>(args[0]), GetValueType<T1>(args[1]), GetValueType<T2>(args[2]) };
                }
                else
                {
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description(this));
                    throw new Exception("Command Missing Parameter");
                }
            }
            catch (Exception e)
            {
                //DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                throw e;
            }
        }
    }

    /// <summary>
    /// A vector method for obtaining a vector from user input
    /// </summary>
    /// <typeparam name="T"> Should be of type Vector2, 3, or 4 </typeparam>
    public class VectorCommand<T> : CommandBase
    {
        public delegate void ConsoleMethod(T vector);

        public VectorCommand(string title, ConsoleMethod method) : base(title, method) { }
        public VectorCommand(string title, ConsoleMethod method, string helpText) : base(title, method, helpText) { }
        public VectorCommand(string title, ConsoleMethod method, HelpMethod helpMethod) : base(title, method, helpMethod) { }
        public VectorCommand(ConsoleMethod method) : base(method) { }
        public VectorCommand(ConsoleMethod method, string helpText) : base(method, helpText) { }
        public VectorCommand(ConsoleMethod method, HelpMethod helpMethod) : base(method, helpMethod) { }

        protected override object[] ParseArguments(string arguments)
        {
            string[] args = base.SplitAndTrim(arguments);

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
                        DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                        throw new ArgumentException("The entered value is not a valid Vector2 value");
                    }
                case 3:
                    try
                    {
                        Vector3 vector = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
                        return new object[] { vector };
                    }
                    catch
                    {
                        DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                        throw new ArgumentException("The entered value is not a valid Vector3 value");
                    }
                case 4:
                    try
                    {
                        Vector4 vector = new Vector4(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        return new object[] { vector };
                    }
                    catch
                    {
                        DevConsole.LogError(Errors.TypeConsoleError.Description(this));
                        throw new ArgumentException("The entered value is not a valid Vector4 value");
                    }
                default:
                    DevConsole.LogError(Errors.ParameterMissingConsoleError.Description(this));
                    throw new ArgumentException("The entered value is not a valid Vector value");
            }
        }
    }
}