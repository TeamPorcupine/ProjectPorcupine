using UnityEngine;
using System.Collections;
using System;
using DeveloperConsole.CommandTypes;
using DeveloperConsole.Interfaces;

namespace DeveloperConsole.Errors
{
    public static class TypeConsoleError
    {
        public static string Description(ICommandDescription command)
        {
            return "The entered parameters do not conform to the types (in order): " + command.parameters;
        }

        public static string Description(CommandBase commandBase)
        {
            ICommandDescription command = (ICommandDescription)commandBase;

            if (command != null)
            {
                return "The entered parameters do not conform to the types (in order): " + command.parameters;
            }
            else
            {
                return "The entered parameters do not conform to the types required";
            }
        }
    }

    public static class ParameterMissingConsoleError
    {
        public static string Description(ICommandDescription command)
        {
            return "Missing a parameter, the required parameters are: " + command.parameters;
        }

        public static string Description(CommandBase commandBase)
        {
            ICommandDescription command = (ICommandDescription)commandBase;

            if (command != null)
            {
                return "Missing a parameter, the required parameters are: " + command.parameters;
            }
            else
            {
                return "Missing a parameter";
            }
        }
    }

    public static class ExecuteConsoleError
    {
        public static string Description(ICommandDescription command)
        {
            return "An execute error as occured, this could be due to a type error or parameter missing error; or could be the method raising an error (or causing an error).";
        }

        public static string Description(CommandBase commandBase)
        {
            ICommandDescription command = (ICommandDescription)commandBase;

            if (command != null)
            {
                return "An execute error as occured, this could be due to a type error or parameter missing error; or could be the method raising an error (or causing an error).";
            }
            else
            {
                return "An execute error as occured, this could be due to a type error or parameter missing error; or could be the method raising an error (or causing an error).";
            }
        }
    }
}
