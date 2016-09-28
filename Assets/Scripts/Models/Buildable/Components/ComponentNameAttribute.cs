﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

namespace ProjectPorcupine.Buildable.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentNameAttribute : Attribute
    {
        public readonly string ComponentName;
        
        public ComponentNameAttribute(string componentName)  
        {
            this.ComponentName = componentName;
        }
    }
}