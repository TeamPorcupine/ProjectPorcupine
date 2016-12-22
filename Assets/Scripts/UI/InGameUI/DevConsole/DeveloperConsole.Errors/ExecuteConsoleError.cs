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
    public static class ExecuteConsoleError
    {
        public static string Description(CommandBase command)
        {
            return "An execute error as occured, this could be due to a type error or parameter missing error; or could be the method raising an error (or causing an error).";
        }
    }
}