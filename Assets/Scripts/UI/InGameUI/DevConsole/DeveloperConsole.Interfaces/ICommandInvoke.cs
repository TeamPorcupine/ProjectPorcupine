#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

namespace DeveloperConsole.Interfaces
{
    public interface ICommandInvoke : ICommandRunnable
    {
        /// <summary>
        /// The function name to call.
        /// </summary>
        string FunctionName
        {
            get;
        }

        /// <summary>
        /// The help function name to call.
        /// </summary>
        string HelpFunctionName
        {
            get;
        }
    }
}
