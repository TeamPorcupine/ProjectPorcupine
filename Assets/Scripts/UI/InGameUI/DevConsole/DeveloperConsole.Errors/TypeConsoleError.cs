﻿#region License
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
    public static class TypeConsoleError
    {
        public static string Description(ICommandDescription command)
        {
            return "The entered parameters do not conform to the types (in order): " + command.Parameters;
        }

        public static string Description(CommandBase commandBase)
        {
            ICommandDescription command = (ICommandDescription)commandBase;

            if (command != null)
            {
                return "The entered parameters do not conform to the types (in order): " + command.Parameters;
            }
            else
            {
                return "The entered parameters do not conform to the types required";
            }
        }
    }
}