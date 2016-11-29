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

namespace DeveloperConsole.Errors
{
    public static class ParameterMissingConsoleError
    {
        public static string Description(CommandBase command)
        {
            return "Missing a parameter, the required parameters are: " + command.Parameters;
        }
    }
}