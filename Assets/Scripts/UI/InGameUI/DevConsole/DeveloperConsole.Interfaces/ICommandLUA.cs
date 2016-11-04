﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

namespace DeveloperConsole.Interfaces
{
    public interface ICommandLUA : ICommandRunnable
    {
        /// <summary>
        /// The function name to call.
        /// </summary>
        string FunctionName
        {
            get;
        }
    }
}
