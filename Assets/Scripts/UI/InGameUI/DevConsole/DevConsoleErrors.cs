using UnityEngine;
using System.Collections;
using System;
using DeveloperConsole.CommandTypes;

namespace DeveloperConsole.Errors
{
    public static class TypeConsoleError
    {
        public static string Description(CommandBase command)
        {
            return "The entered parameters do not conform to the types (in order): " + command.getParameters();
        }
    }

    public static class ParameterMissingConsoleError
    {
        public static string Description(CommandBase command)
        {
            return "Missing a parameter, the required parameters are: " + command.getParameters();
        }
    }

    public static class ExecuteConsoleError
    {
        public static string Description(CommandBase command)
        {
            return "An execute error as occured, this could be due to a type error or parameter missing error; or could be the method raising an error (or causing an error).";
        }
    }
}
