#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using DeveloperConsole.CommandTypes;
using DeveloperConsole.Interfaces;

namespace DeveloperConsole
{
    public static class Errors
    {
        public static string ParametersNotInFormat(CommandBase command)
        {
            return "The entered parameters do not conform to the types (in order): " + command.Parameters;
        }

        public static string ParametersMissing(CommandBase command)
        {
            return "The entered parameters do not conform to the types (in order): " + command.Parameters;
        }

        public static string UnknownError(CommandBase command)
        {
            return "An execute error as occured, this could be the method raising an error (or causing an error).  We could not locate the specific error however.";
        }
    }
}